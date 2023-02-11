using UnityEngine;

public class Movement : MonoBehaviour
{
    public static float RollSpeedMultiplier = 16;
    public static float SideSpeedMultiplier = 40;
    public static float MaxVelocity = 60;

    public static bool shake = false;
    public static float ShakeMag = 0;

    public GameObject marble;
    public Rigidbody MarbleBody;
    public GameObject particles;

    private Vector3 CachedVelocity;

    void Start()
    {
        MarbleBody.AddForce(1500, 0, 0);
        shake = false;
    }

    void Update()
    {
        if (!shake)
        {
            Camera.main.transform.position = new Vector3(
                marble.transform.position.x - 4,
                marble.transform.position.y + 2.5f,
                marble.transform.position.z - MarbleBody.velocity.z / 100
            );
        } else
        {
            Camera.main.transform.position = new Vector3(
                marble.transform.position.x - 4 + rng(),
                marble.transform.position.y + 2.5f + rng(),
                marble.transform.position.z - MarbleBody.velocity.z / 100 + rng()
            );
        }

        float TargetFoV = Mathf.Max(50, 40 + MarbleBody.velocity.x / 2.5f);
        Camera.main.fieldOfView += (TargetFoV - Camera.main.fieldOfView) * 0.1f;
    }

    private void FixedUpdate()
    {
        if (MarbleBody.velocity.magnitude < 3)
        {
            MarbleBody.AddForce(RollSpeedMultiplier + GameScript.score / 3, 0, 0);
        } else if (MarbleBody.velocity.x < MaxVelocity)
	    {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, new Vector3(
                MarbleBody.velocity.y,
                -MarbleBody.velocity.x,
                0
            ) * 100);

            if (Physics.Raycast(ray, out hit))
            {
                float ForceMultiplier = RollSpeedMultiplier + GameScript.score / 3;
                MarbleBody.AddForce(
                    ForceMultiplier * hit.normal.y,
                    ForceMultiplier * -hit.normal.x,
                    0
                );
            }
        }

        if (Input.GetMouseButton(0))
        {
            float MousePosX = Input.mousePosition.x;
            float ScreenWidth = Screen.width;

            if (MousePosX < ScreenWidth / 2)
            {
                Steer(1);
            } else
            {
                Steer(-1);
            }
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
        {
            Steer(1);
            GameScript.GamePlatform = "desktop";
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Steer(-1);
        }

        void Steer(float dir)
        {
            MarbleBody.AddForce(0, 0, SideSpeedMultiplier * dir);
        }

        CachedVelocity = MarbleBody.velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CachedVelocity.magnitude - MarbleBody.velocity.magnitude > 20 && !collision.gameObject.CompareTag("red"))
        {
            particles.transform.position = new Vector3(
                collision.contacts[0].point.x + 2,
                collision.contacts[0].point.y + 0.1f,
                collision.contacts[0].point.z
            );

            ParticleSystem system = particles.GetComponent<ParticleSystem>();
            system.Clear();
            system.Simulate(system.main.duration);
            system.Play();

            StartCoroutine(Interact.Shake(0.2f, 0.3f));
        }
    }

    float rng()
    {
        return Random.Range(-ShakeMag, ShakeMag);
    }
}