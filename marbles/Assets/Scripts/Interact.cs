using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    public GameObject particles;
    public GameObject LeaderboardObj;
    public GameObject SubmitLeaderboard;
    public GameObject DeathMenu;

    public Image PUImage;
    public Image ring;
    public PhysicMaterial BounceMat;
    public Sprite none;
    public Sprite PU1;
    public Sprite PU2;
    public Sprite PU3;
    public Sprite PU4;
    public Sprite PU5;
    public Sprite PU6;
    public Sprite PU7;

    private bool IsDead = false;
    private int RingTime = 0;
    private bool inv = false;

    private void Start()
    {
        Movement.RollSpeedMultiplier = 15;
        Movement.MaxVelocity = 75;
        Physics.gravity = new Vector3(0, -20, 0);
    }

    private void OnTriggerEnter(Collider collision)
    {
        switch (collision.gameObject.tag)
        {
            case "x2":
                StartCoroutine(Speed(1.4f, 2, PU1));
                ring.fillAmount = 1;
                RingTime = 2;
                Destroy(collision.gameObject);
                break;
            case "x05":
                StartCoroutine(Speed(0.6f, 4, PU2));
                ring.fillAmount = 1;
                RingTime = 4;
                Destroy(collision.gameObject);
                break;
            case "boost":
                StartCoroutine(Speed(1.5f, 3, none));
                ring.fillAmount = 0;
                RingTime = 2;
                break;
            case "zerog":
                StartCoroutine(ZeroG());
                ring.fillAmount = 1;
                RingTime = 3;
                Destroy(collision.gameObject);
                break;
            case "double":
                StartCoroutine(DoublePlatforms());
                ring.fillAmount = 1;
                RingTime = 3;
                Destroy(collision.gameObject);
                break;
            case "inv":
                StartCoroutine(Invincibility());
                ring.fillAmount = 1;
                RingTime = 5;
                Destroy(collision.gameObject);
                break;
            case "bounce":
                StartCoroutine(Bounce());
                ring.fillAmount = 1;
                RingTime = 5;
                Destroy(collision.gameObject);
                break;
            case "jump":
                StartCoroutine(Jump());
                ring.fillAmount = 1;
                RingTime = 3;
                Destroy(collision.gameObject);
                break;
        }
    }

    private void Update()
    {
        if (RingTime > 0)
        {
            ring.fillAmount -= Time.deltaTime / RingTime;
        }

        if (ring.fillAmount < 0)
        {
            RingTime = 0;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("red") && !IsDead)
        {
            if (!inv)
            {
                DeathAnimation();
            } else
            {
                Destroy(collision.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsDead)
        {
            if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return)) && !SubmitLeaderboard.activeSelf)
            {
                SceneManager.LoadScene("singleplayer", LoadSceneMode.Single);
            }
        } else
        {
            if (transform.position.y < GameScript.RenderedPlatforms[GameScript.RenderedPlatforms.Count - 1].transform.position.y - 10)
            {
                DeathAnimation();
            }
        }
    }

    void DeathAnimation()
    {
        IsDead = true;

        gameObject.GetComponent<Movement>().enabled = false;
        Camera.main.GetComponent<GameScript>().enabled = false;

        DeathMenu.SetActive(true);
        StartCoroutine(LeaderboardObj.GetComponent<Leaderboard>().LoadLeaderboard());

        Vector3 DeathPlace = gameObject.transform.position;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        particles.transform.position = DeathPlace;

        ParticleSystem system = particles.GetComponent<ParticleSystem>();
        system.Clear();
        system.Simulate(system.main.duration);
        system.Play();

        StartCoroutine(Shake(0.6f, 0.5f));
    }

    public static IEnumerator Shake(float secs, float mag)
    {
        Movement.shake = true;

        for (float i = 0; i < secs; i += 0.01f)
        {
            Movement.ShakeMag = mag * ((secs - i) / secs);
            yield return new WaitForSeconds(0.01f);
        }

        Movement.shake = false;
    }

    IEnumerator Speed(float multiplier, int time, Sprite img)
    {
        Movement.RollSpeedMultiplier *= multiplier;
        Movement.MaxVelocity *= multiplier;
        PUImage.sprite = img;

        yield return new WaitForSeconds(time);
        
        Movement.RollSpeedMultiplier *= 1 / multiplier;
        Movement.MaxVelocity *= 1 / multiplier;
        PUImage.sprite = none;
    }

    IEnumerator ZeroG()
    {
        Physics.gravity = new Vector3(0, 0, 0);
        PUImage.sprite = PU3;

        yield return new WaitForSeconds(3);

        Physics.gravity = new Vector3(0, -20, 0);
        PUImage.sprite = none;
    }

    IEnumerator DoublePlatforms()
    {
        foreach (GameObject platform in GameScript.RenderedPlatforms)
        {
            GameObject PlatformCopy = GameObject.Instantiate(platform);

            PlatformCopy.transform.position = new Vector3(
                platform.transform.position.x,
                platform.transform.position.y,
                platform.transform.position.z + Random.Range(5, 12)
            );
        }

        PUImage.sprite = PU4;
        yield return new WaitForSeconds(3);
        PUImage.sprite = none;
    }

    IEnumerator Invincibility()
    {
        inv = true;
        PUImage.sprite = PU5;

        yield return new WaitForSeconds(5);

        inv = false;
        PUImage.sprite = none;
    }

    IEnumerator Bounce()
    {
        gameObject.GetComponent<SphereCollider>().material = BounceMat;
        Physics.gravity = new Vector3(0, -10, 0);
        PUImage.sprite = PU6;

        yield return new WaitForSeconds(5);

        gameObject.GetComponent<SphereCollider>().material = null;
        Physics.gravity = new Vector3(0, -20, 0);
        PUImage.sprite = none;
    }

    IEnumerator Jump()
    {
        Rigidbody body = gameObject.GetComponent<Rigidbody>();
        body.velocity = new Vector3(
            body.velocity.x,
            body.velocity.y + 30,
            body.velocity.z
        );

        PUImage.sprite = PU7;
        yield return new WaitForSeconds(3);
        PUImage.sprite = none;
    }
}