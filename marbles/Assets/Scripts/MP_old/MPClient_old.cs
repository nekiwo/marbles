using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MPClient_old : MonoBehaviour
{
    public static string GameCode;
    public static string ClientID;
    public static string ClientName = "DefaultName";
    public static bool IsHost;
    public static bool GameStarted;
    public static int TotalAlive;

    public GameObject MarbleObj;
    public GameObject LoadingScreen;
    public Volume GV;

    public Transform marbles;
    public GameObject platforms;
    public GameObject InitPlatform;
    public GameObject particles;
    public GameObject MPDeathMenuObj;
    public TextMeshProUGUI PlayersAlive;

    private DepthOfField blur;

    public GameObject PingText;
    private System.Diagnostics.Stopwatch watch;
    private long ping;

    public GameObject FinishLine;

    public static ICollection<GameObject> RenderedPlatforms;

    private List<MPInteract_old.Death> DeathQueue;

    public class Platform
    {
        public string code;
        public int index;
        public float PosX;
        public float PosY;
        public float PosZ;

        public Platform(int index, Vector3 Pos)
        {
            this.code = GameCode;
            this.index = index;
            this.PosX = Pos.x;
            this.PosY = Pos.y;
            this.PosZ = Pos.z;
        }
    }

    public class Player
    {
        public string id;
        public string name;
    }

    public class PlayerJoin
    {
        public string code;
        public string id;
        public string name;
        public bool isHost;

        public PlayerJoin(bool host)
        {
            this.code = GameCode;
            this.id = ClientID;
            this.name = ClientName;
            this.isHost = host;
        }
    }

    void Start()
    {
        DeathQueue = new List<MPInteract_old.Death>();
        GameStarted = false;

        if (IsHost)
        {
            gameObject.AddComponent<MPHost_old>();
            gameObject.GetComponent<MPHost_old>().marbles = marbles;
            gameObject.GetComponent<MPHost_old>().platforms = platforms;
            gameObject.GetComponent<MPHost_old>().InitPlatform = InitPlatform;
        }

        FinishLine.transform.position = new Vector3(
            100 * MPHost_old.RaceLength,
            22 - 3 * MPHost_old.RaceLength,
            0
        );

        InvokeRepeating("MeasurePing", 3, 5);
        WebSockets.OnUnityThread("pong", (data) =>
        {
            ping = watch.ElapsedMilliseconds;
            watch.Reset();
            PingText.GetComponent<Text>().text = ping + "ms";
        });

        StartCoroutine(SendReady());

        WebSockets.OnUnityThread("GameLoaded", (data) =>
        {
            Debug.Log("GameLoaded");
            GameStarted = true;

            LoadingScreen.SetActive(false);
            GV.profile.TryGet(out DepthOfField blur);
            blur.active = false;

            Player[] players = data.GetValue<Player[]>();
            for (int i = 0; i < players.Length; i++)
            {
                string MarbleId = players[i].id;

                GameObject MarbleCopy = GameObject.Instantiate(MarbleObj);
                MarbleCopy.name = MarbleId;
                MarbleCopy.transform.Find("Canvas").Find("nametag").GetComponent<TextMeshProUGUI>().text = players[i].name;
                MarbleCopy.transform.SetParent(marbles);
                MarbleCopy.transform.position = new Vector3(
                    -10,
                    23 + i,
                    0
                );

                if (ClientID == MarbleId)
                {
                    MarbleCopy.AddComponent<MPMovement_old>();
                    MarbleCopy.GetComponent<MPMovement_old>().marbles = marbles;

                    MarbleCopy.AddComponent<MPInteract_old>();
                    MarbleCopy.GetComponent<MPInteract_old>().particles = particles;
                    MarbleCopy.GetComponent<MPInteract_old>().MPDeathMenuObj = MPDeathMenuObj;
                }
            }

            TotalAlive = marbles.transform.childCount;
            PlayersAlive.text = "Players Alive: " + System.Math.Max(0, TotalAlive).ToString();

            RenderedPlatforms = new List<GameObject>();
            RenderedPlatforms.Add(InitPlatform);

            if (IsHost)
            {
                gameObject.GetComponent<MPHost_old>().InitPlatforms();
            }

            WebSockets.OnUnityThread("NewPlatform", (data) =>
            {
                if (!IsHost)
                {
                    Debug.Log("NewPlatform");

                    Platform platform = data.GetValue<Platform>();

                    GameObject RandomPlatform = platforms.transform.GetChild(platform.index).gameObject;
                    GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
                    PlatformCopy.transform.position = new Vector3(
                        platform.PosX,
                        platform.PosY,
                        platform.PosZ
                    );

                    RenderedPlatforms.Add(PlatformCopy);
                }
            });

            WebSockets.OnUnityThread("DeleteLastPlatform", (data) =>
            {
                if (!IsHost)
                {
                    Destroy(RenderedPlatforms.First());
                    RenderedPlatforms.Remove(RenderedPlatforms.First());
                }
            });

            WebSockets.OnUnityThread("death", (data) =>
            {
                DeathQueue.Add(data.GetValue<MPInteract_old.Death>());
            });
        });

        WebSockets.OnUnityThread("restart", (data) =>
        {
            SceneManager.LoadScene("multiplayer", LoadSceneMode.Single);
        });
    }

    private void Update()
    {
        if (DeathQueue.Count > 0)
        {
            Debug.Log("COUNT " + DeathQueue.Count.ToString());

            //foreach (MPInteract.Death death in DeathQueue)
            for (int i = 0; i < DeathQueue.Count; i++)
            {
                MPInteract_old.Death death = DeathQueue[i];

                if (death.id != ClientID)
                {
                    Debug.Log("death");

                    GameObject ParticlesCopy = GameObject.Instantiate(particles);
                    ParticlesCopy.transform.position = new Vector3(
                        death.pos[0],
                        death.pos[1],
                        death.pos[2]
                    );
                    ParticleSystem system = ParticlesCopy.GetComponent<ParticleSystem>();
                    system.Clear();
                    system.Simulate(system.main.duration);
                    system.Play();

                    try
                    {
                        //marbles.Find(death.id).gameObject.SetActive(false);
                        Destroy(marbles.Find(death.id).gameObject);
                    } catch
                    {

                    }
                    Debug.Log("DEAD " + death.id);

                    if (TotalAlive == 1)
                    {
                        marbles.Find(ClientID).GetComponent<MPInteract_old>().DeathAnimation(true);
                    }
                }
                else if (!marbles.Find(ClientID).GetComponent<MPInteract_old>().IsDead)
                {
                    marbles.Find(ClientID).GetComponent<MPInteract_old>().DeathAnimation(true);
                }

                PlayersAlive.text = "Players Alive: " + System.Math.Max(0, TotalAlive).ToString();

                DeathQueue.RemoveAt(i);
                TotalAlive--;
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < marbles.childCount; i++)
        {
            Rigidbody rb = marbles.GetChild(i).gameObject.GetComponent<Rigidbody>();

            if (rb.velocity.x < MPMovement_old.MaxVelocity)
            {
                rb.AddForce(MPMovement_old.RollSpeedMultiplier, 0, 0);
            }
        }
    }

    IEnumerator SendReady()
    {
        yield return new WaitForSeconds(3);
        WebSockets.Emit("ready", GameCode);

        yield return new WaitForSeconds(3);
        if (!GameStarted)
        {
            Menu.OnStartErrorMsg = "The lobby has been closed because the other player left";

            WebSockets.Emit("leave", new MPMenu_old.PlayerLeaveInfo());
            SceneManager.LoadScene("menu", LoadSceneMode.Single);
        }
    } 

    void MeasurePing()
    {
        watch = System.Diagnostics.Stopwatch.StartNew();
        WebSockets.Emit("ping", new MPMenu_old.PlayerLeaveInfo());
    }
}