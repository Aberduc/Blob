using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingPlayer : MonoBehaviour
{
    private float m_inputX;
    private float m_inputZ;
    private bool m_inputJump;

    public float m_jumpForce = 10f;

    public float m_airMoveForce = 0.2f;
    public float m_groundMoveForce = 0.5f;
    public float m_squashedMoveForce = 0.3f;

    public float m_radius = 0.5f;
    public float m_skinSphereRadius = 0.075f;

    public GameObject m_shadowProjector;

    /// <summary>
    /// The prefab of a skin sphere
    /// </summary>
    public Rigidbody m_skinSphere;
    public float m_skinSpringForce = 100f;
    public float m_centerSpringForce = 10f;
    public float m_centerSpringForceSquashed = 1f;

    private Dictionary<Vector2Int, Rigidbody> m_skinSpheres;
    /// <summary>
    /// the coordinates of the skin spheres in the void in a non squashed situation
    /// </summary>
    private Dictionary<Vector2Int, Vector3> m_skinOptimalCoords;

    private Dictionary<JointIndex, SpringJoint> m_springJoints;
    private Dictionary<Vector2Int, SpringJoint> m_centerSprings;

    private bool m_squash;
    private float m_squashRatio = 0; //0 : not squashed at all, 1 -> totally squashed
    public float m_squashTime = 0.5f;
    public float m_stretchTime = 0.1f;

    public float m_slopeMinY = 0.8f;
    private bool m_grounded = false;
    private Vector3 m_up = Vector3.up;

    private Vector3 m_groundNormal;

    public int m_skinSphereRow = 9;
    public int m_spherePerRow = 9;

    public float m_cubeMoveForce = 125f;

    private Vector3 m_centroid = Vector3.zero;
    private List<CubeCollision> m_cubesCollisions;

    private Mesh m_mesh;

    private Rigidbody m_coreSphere;

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        this.gameObject.AddComponent<ComputerInput>().m_player = this;
#elif UNITY_ANDROID
        this.gameObject.AddComponent<AccelerometerInput>().m_player = this;
#else
        this.gameObject.AddComponent<ComputerInput>().m_player = this;  
#endif

        FillCoordDictionnary();
        FillSphereDictionaries();
        CreateSpringJoints();

        Instantiate(m_shadowProjector, transform);

        GenerateMesh();

        m_cubesCollisions = new List<CubeCollision>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(m_coreSphere.transform.position, m_up * 5, m_grounded ? Color.green : Color.red);

        if (m_inputJump)
        {
            m_squash = true;
        }
        else
        {
            if (m_grounded && m_squash)
            {
                Jump();
            }

            m_squash = false;
        }

        UpdateMesh();
    }

    private void Jump()
    {
        foreach (Rigidbody sphere in m_skinSpheres.Values)
        {
            sphere.AddForce(Vector3.up * m_jumpForce * m_squashRatio, ForceMode.VelocityChange);
        }
        m_coreSphere.AddForce(Vector3.up * m_jumpForce * m_squashRatio, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (m_cubesCollisions.Count > 0)
        {
            float totalForce = m_cubeMoveForce * m_squashRatio * Vector3.ProjectOnPlane(m_cubesCollisions[0].cube.position - m_centroid, m_up).magnitude;

            foreach (CubeCollision collision in m_cubesCollisions)
            {
                Vector3 contactPointLocal = collision.contactPoint - m_centroid;
                Vector3 localForce = (totalForce / (m_skinSphereRow * m_spherePerRow)) * Vector3.Cross(Vector3.Cross(m_up, contactPointLocal), collision.normal).normalized;
                collision.cube.AddForceAtPosition(localForce, collision.contactPoint);

                //Debug.DrawRay(collision.contactPoint, localForce * 5, Color.magenta);
            }
        }

        m_cubesCollisions.Clear();

        if (m_groundNormal == Vector3.zero)
        {
            m_up = Vector3.up;
            m_grounded = false;
        }
        else
        {
            m_up = m_groundNormal.normalized;
            m_grounded = true;
        }
        m_groundNormal = Vector3.zero;


        Squash();


        m_centroid = Vector3.zero;
        
        float moveForce = m_grounded ? m_squashedMoveForce + (m_groundMoveForce - m_squashedMoveForce) * (1 - m_squashRatio) : m_airMoveForce;

        Vector3 force = new Vector3(m_inputX * moveForce, 0, m_inputZ * moveForce);
        if (m_up != Vector3.up)
            force = Quaternion.FromToRotation(Vector3.up, m_up) * force;
        force -= Vector3.ProjectOnPlane(Physics.gravity, m_up) * Time.fixedDeltaTime;

        foreach (Rigidbody sphere in m_skinSpheres.Values)
        {
            sphere.WakeUp();
            sphere.AddForce(force, ForceMode.VelocityChange);

            m_centroid += sphere.position;
        }
        m_coreSphere.AddForce(force, ForceMode.VelocityChange);

        m_centroid = m_centroid / (m_skinSphereRow * m_spherePerRow);
        //Debug.DrawRay(m_coreSphere.transform.position, force * 5, Color.blue);
    }


    public void OnSkinCollision(Collision collision)
    {
        Vector3 normal = Vector3.zero;
        Vector3 contactPoint = Vector3.zero;
        foreach (ContactPoint point in collision.contacts)
        {
            normal += point.normal;
            contactPoint += point.point;
        }
        normal.Normalize();

        if (normal.y > m_slopeMinY)
        {
            m_groundNormal += normal;
        }

        if (collision.contacts.Length > 0 && collision.gameObject.tag == "Cube" && normal.y < 0.1)
        {
            contactPoint = contactPoint / collision.contacts.Length;
            m_cubesCollisions.Add(new CubeCollision
            {
                normal = normal,
                contactPoint = contactPoint,
                cube = collision.rigidbody
            });
        }
    }

    private class CubeCollision
    {
        public Vector3 normal;
        public Vector3 contactPoint;

        public Rigidbody cube;
    }





    /***************************
     ********* SQUASH **********
     ***************************/

    private void Squash()
    {
        if (m_squash)
        {
            m_squashRatio += Time.fixedDeltaTime * 1 / m_squashTime;
            if (m_squashRatio > 1)
                m_squashRatio = 1;
        }
        else
        {
            m_squashRatio -= Time.fixedDeltaTime * 1 / m_stretchTime;
            if (m_squashRatio < 0)
                m_squashRatio = 0;
        }

        SpringJoint currentSpringJoint;

        foreach (JointIndex index in m_springJoints.Keys)
        {
            currentSpringJoint = m_springJoints[index];

            float distance = (ComputeGoalPosition(index.sphere2) - ComputeGoalPosition(index.sphere1)).magnitude;
            currentSpringJoint.anchor = Vector3.zero;
            currentSpringJoint.connectedAnchor = Vector3.zero;
            currentSpringJoint.minDistance = distance;
            currentSpringJoint.maxDistance = distance;

            currentSpringJoint.spring = m_skinSpringForce;
        }

        foreach (Vector2Int index in m_centerSprings.Keys)
        {
            currentSpringJoint = m_centerSprings[index];

            currentSpringJoint.connectedAnchor = Vector3.zero;
            currentSpringJoint.anchor = -ComputeGoalPosition(index);

            currentSpringJoint.spring = (m_centerSpringForceSquashed + (m_centerSpringForce - m_centerSpringForceSquashed) * (1 - m_squashRatio));
        }
    }

    /// <summary>
    /// Compute the localPosition a skin sphere should be at this fixed frame
    /// </summary>
    /// <param name="index">index of the skin sphere you want to compute the position of</param>
    /// <returns></returns>
    private Vector3 ComputeGoalPosition(Vector2Int index)
    {
        return (Vector3.Dot(m_skinOptimalCoords[index], transform.InverseTransformVector(m_up).normalized) * (1 - m_squashRatio) * transform.InverseTransformVector(m_up).normalized + Vector3.ProjectOnPlane(m_skinOptimalCoords[index], transform.InverseTransformVector(m_up).normalized) * (1 + m_squashRatio * 0.75f)) * m_radius;
    }

    private class JointIndex
    {
        public Vector2Int sphere1;
        public Vector2Int sphere2;
    }





    /***************************
     ********* INIT  ***********
     ***************************/

    private void FillSphereDictionaries()
    {
        m_skinSpheres = new Dictionary<Vector2Int, Rigidbody>();

        m_coreSphere = Instantiate(m_skinSphere, transform.position, Quaternion.Euler(0, 0, 0), transform);
        m_coreSphere.freezeRotation = false;
        m_coreSphere.gameObject.tag = "Core";
        m_coreSphere.gameObject.name = "SkinSphere_core";
        m_coreSphere.GetComponent<SphereCollider>().radius = m_skinSphereRadius;
        m_coreSphere.GetComponent<SkinSphere>().m_player = this;

        //so that the core's position is also the gameObject's position
        gameObject.GetComponent<ConfigurableJoint>().connectedBody = m_coreSphere;

        foreach (Vector2Int index in m_skinOptimalCoords.Keys)
        {
            Rigidbody currentSphere = Instantiate(m_skinSphere, transform.position + m_skinOptimalCoords[index] * 0.1f, Quaternion.Euler(0,0,0), transform );
            currentSphere.gameObject.name = "SkinSphere_" + index.x + "," + index.y;
            currentSphere.GetComponent<SphereCollider>().radius = m_skinSphereRadius;
            currentSphere.GetComponent<SkinSphere>().m_player = this;

            m_skinSpheres.Add(index, currentSphere);
        }
    }

    private void CreateSpringJoints()
    {
        m_springJoints = new Dictionary<JointIndex, SpringJoint>();
        m_centerSprings = new Dictionary<Vector2Int, SpringJoint>();

        Rigidbody currentSphere;
        Rigidbody currentNeighboor;

        foreach (Vector2Int index in m_skinSpheres.Keys)
        {
            currentSphere = m_skinSpheres[index];
            foreach (Vector2Int neighboor in GetNeighboors(index))
            {
                currentNeighboor = m_skinSpheres[neighboor];

                SpringJoint currentSpringJoint = currentSphere.gameObject.AddComponent<SpringJoint>();
                currentSpringJoint.connectedBody = currentNeighboor;
                Vector3 anchorPos = (currentNeighboor.transform.localPosition - currentSphere.transform.localPosition) / 2;
                currentSpringJoint.anchor = anchorPos;
                currentSpringJoint.autoConfigureConnectedAnchor = false;
                currentSpringJoint.connectedAnchor = -anchorPos;
                currentSpringJoint.spring = m_skinSpringForce;
                currentSpringJoint.tolerance = 0;

                m_springJoints.Add(new JointIndex
                {
                    sphere1 = index,
                    sphere2 = neighboor
                }, currentSpringJoint);
            }

            SpringJoint currentCenterSpringJoint = currentSphere.gameObject.AddComponent<SpringJoint>();
            currentCenterSpringJoint.connectedBody = m_coreSphere;
            currentCenterSpringJoint.anchor = -currentSphere.transform.localPosition / 2;
            currentCenterSpringJoint.autoConfigureConnectedAnchor = false;
            currentCenterSpringJoint.connectedAnchor = currentSphere.transform.localPosition / 2;
            currentCenterSpringJoint.spring = m_centerSpringForce;
            currentCenterSpringJoint.tolerance = 0;

            m_centerSprings.Add(index, currentCenterSpringJoint);

            currentSphere.mass = 1f / m_skinSpheres.Count;
        }
    }

    /// <summary>
    /// Only returns the skin spheres that are connected to this one with a spring joint starting from this sphere
    /// </summary>
    /// <param name="index">index of the sphere you want the neigboors of</param>
    /// <returns></returns>
    private List<Vector2Int> GetNeighboors(Vector2Int index)
    {
        List<Vector2Int> neighboors = new List<Vector2Int>();

        neighboors.Add(new Vector2Int(index.x, (index.y + 1) % m_spherePerRow)); // the next sphere in the same row
        if (index.x != 0)
        {
            //add the two spheres down one row
            neighboors.Add(new Vector2Int(index.x - 1, index.y));
            neighboors.Add(new Vector2Int(index.x - 1, (index.y + (index.x % 2 == 0 ? m_spherePerRow - 1 : 1)) % m_spherePerRow));
        }
        
        return neighboors;
    }


    //Vector2Int : an int from 0 to m_skinSphereRow representing the row (0 is at the bottom), and a second int representing the angle from right
    private void FillCoordDictionnary()
    {
        m_skinOptimalCoords = new Dictionary<Vector2Int, Vector3>();

        for (int i = 0; i < m_skinSphereRow; i++)
        {
            float phi = (180f / m_skinSphereRow) * (i + 0.5f) - 90;
            for (int j = 0; j < m_spherePerRow; j++)
            {
                float theta = (360f / m_spherePerRow) * (j + 0.5f * i % 2);
                m_skinOptimalCoords.Add(new Vector2Int(i, j), Quaternion.Euler(0, theta, phi) * Vector3.right * m_radius);
            }
        }
    }






    /***************************
     ********* MESH  ***********
     ***************************/

    private void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[m_skinSpheres.Count];
        int[] triangles = new int[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 +  2*(m_spherePerRow - 2) * 3];

        foreach (Vector2Int index in m_skinSpheres.Keys)
        {
            vertices[index.x * m_spherePerRow + index.y] = m_skinSpheres[index].transform.localPosition * (1 + m_skinSphereRadius);

            if (index.x != 0)
            {
                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y)] = index.x * m_spherePerRow + index.y;
                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y) + 1] = (index.x - 1) * m_spherePerRow + ((index.y + (index.x % 2)) % m_spherePerRow);
                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y) + 2] = index.x * m_spherePerRow + ((index.y + 1) % m_spherePerRow);

                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y) + 3] = index.x * m_spherePerRow + index.y;
                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y) + 4] = (index.x - 1) * m_spherePerRow + ((index.y + (index.x % 2) + m_spherePerRow - 1) % m_spherePerRow);
                triangles[6 * ((index.x - 1) * m_spherePerRow + index.y) + 5] = (index.x - 1) * m_spherePerRow + ((index.y + (index.x % 2)) % m_spherePerRow);
            }
        }

        for(int i = 0; i < m_spherePerRow - 2; i++)
        {
            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i] = 0 * m_spherePerRow + 0;
            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i + 1] = 0 * m_spherePerRow + i + 2;
            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i + 2] = 0 * m_spherePerRow + i + 1;

            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i + 3] = (m_skinSphereRow - 1) * m_spherePerRow + 0;
            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i + 4] = (m_skinSphereRow - 1) * m_spherePerRow + i + 1;
            triangles[2 * m_spherePerRow * (m_skinSphereRow - 1) * 3 + 6 * i + 5] = (m_skinSphereRow - 1) * m_spherePerRow + i + 2;

        }

        m_mesh = new Mesh();

        m_mesh.vertices = vertices;
        m_mesh.triangles = triangles;
        m_mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = m_mesh;
    }

    private void UpdateMesh()
    {
        Vector3[] vertices = new Vector3[m_skinSpheres.Count];

        foreach (Vector2Int index in m_skinSpheres.Keys)
        {
            vertices[index.x * m_spherePerRow + index.y] = m_skinSpheres[index].transform.localPosition + (m_skinSpheres[index].transform.localPosition - transform.InverseTransformPoint(m_centroid)).normalized * m_skinSphereRadius;
        }

        m_mesh.vertices = vertices;

        GetComponent<MeshFilter>().mesh = m_mesh;

        GetComponent<MeshRenderer>().shadowCastingMode = m_grounded || (!m_inputJump && m_squash)? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
    }






    /***************************
     ********* INPUT ***********
     ***************************/

    public void SetInput(Vector2 input, bool inputJump)
    {
        Vector2 clampedInput = Vector2.ClampMagnitude(input, 1f);
        m_inputX = clampedInput.x;
        m_inputZ = clampedInput.y;

        m_inputJump = inputJump;
    }
}
