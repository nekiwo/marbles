using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MPClient : MonoBehaviour
{
    public static string GameCode;
    public static string ClientID;
    public static string ClientName = "DefaultName";
    public static bool IsHost;
    public static bool GameStarted;

    public GameObject LoadingScreen;
    public Volume GV;
    private DepthOfField blur;

    public GameObject MarbleObj;
    public Transform marbles;
    public GameObject platforms;

    public GameObject FinishLine;
    public GameObject particles;
    public GameObject MPDeathMenuObj;
    public TextMeshProUGUI PlayersAlive;
    public GameObject PingText;

    public class Platform
    {
        public int index;
        public float[] pos;
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
        GameStarted = false;
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
                    MarbleCopy.AddComponent<MPMovement>();
                    MarbleCopy.GetComponent<MPMovement>().marbles = marbles;

                    MarbleCopy.AddComponent<MPInteract>();
                }
            }
        });

        WebSockets.OnUnityThread("AllPlatforms", (data) =>
        {
            Debug.Log("AllPlatforms");

            Platform[] AllPlatforms = data.GetValue<Platform[]>();

            foreach (Platform platform in AllPlatforms)
            {
                GameObject RandomPlatform = platforms.transform.GetChild(platform.index).gameObject;
                GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
                PlatformCopy.transform.position = new Vector3(
                    platform.pos[0],
                    platform.pos[1],
                    platform.pos[2]
                );
            }

            FinishLine.transform.position = new Vector3(
                AllPlatforms.Length * 100,
                22 - AllPlatforms.Length * 3,
                0
            );
        });

        WebSockets.OnUnityThread("ping", (data) =>
        {
            int ping = data.GetValue<int>();
            PingText.GetComponent<Text>().text = ping.ToString() + "ms";
            WebSockets.Emit("pong", GameCode);
        });
    }

    void FixedUpdate()
    {
        for (int i = 0; i < marbles.childCount; i++)
        {
            Rigidbody rb = marbles.GetChild(i).gameObject.GetComponent<Rigidbody>();

            if (rb.velocity.x < MPMovement.MaxVelocity)
            {
                rb.AddForce(MPMovement.RollSpeedMultiplier, 0, 0);
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
            SceneManager.LoadScene("menu", LoadSceneMode.Single);
        }
    }
}