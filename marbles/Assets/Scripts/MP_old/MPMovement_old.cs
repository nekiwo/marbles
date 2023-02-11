using UnityEngine;

public class MPMovement_old : MonoBehaviour
{
    public static float RollSpeedMultiplier = 18;
    public static float SideSpeedMultiplier = 40;
    public static float MaxVelocity = 55;

    public Transform marbles;

    private Rigidbody MarbleBody;

    private class Marble
    {
        public string name;
        public string code;
        public float[] position;
        public float[] velocity;

        public Marble(string name, string code, float[] position, float[] velocity)
        {
            this.name = name;
            this.code = code;
            this.position = position;
            this.velocity = velocity;
        }
    }

    void Start()
    {
        MarbleBody = GetComponent<Rigidbody>();

        WebSockets.OnUnityThreadFixed("PhysChange", (data) =>
        {
            Marble m = data.GetValue<Marble>();

            if (m.name != MPClient_old.ClientID)
            {
                GameObject NotClientMarble = marbles.Find(m.name).gameObject;
                Rigidbody NotClientBody = NotClientMarble.GetComponent<Rigidbody>();

                NotClientMarble.transform.position = new Vector3(
                    m.position[0],
                    m.position[1],
                    m.position[2]
                );

                NotClientBody.velocity = new Vector3(
                    m.velocity[0],
                    m.velocity[1],
                    m.velocity[2]
                );
            }
        });
    }

    void Update()
    {
        Camera.main.transform.position = new Vector3(
            transform.position.x - 4,
            transform.position.y + 2.5f,
            transform.position.z - MarbleBody.velocity.z / 100
        );

        float TargetFoV = Mathf.Max(50, 40 + MarbleBody.velocity.x / 2.5f);
        Camera.main.fieldOfView += (TargetFoV - Camera.main.fieldOfView) * 0.1f;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            float MousePosX = Input.mousePosition.x;
            float ScreenWidth = Screen.width;

            if (MousePosX < ScreenWidth / 2)
            {
                Steer(1);
            }
            else
            {
                Steer(-1);
            }
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            Steer(1);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            Steer(-1);
        }

        void Steer(float dir)
        {
            MarbleBody.AddForce(0, 0, SideSpeedMultiplier * dir);

            WebSockets.Emit("PhysChange", new Marble(
                MPClient_old.ClientID,
                MPClient_old.GameCode,
                new float[] {
                    MarbleBody.position.x,
                    MarbleBody.position.y,
                    MarbleBody.position.z,
                },
                new float[] {
                    MarbleBody.velocity.x,
                    MarbleBody.velocity.y,
                    MarbleBody.velocity.z,
                }
            ));
        }
    }
}