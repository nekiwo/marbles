using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MPHost_old : MonoBehaviour
{
    public Transform marbles;
    public GameObject platforms;
    public GameObject InitPlatform;

    private int ChunkNum = 5;
    public static int RaceLength = 3;

    private int TotalPlatforms = 0;
    private IEnumerable<MarbleObj> SortedMarbles;
    public static List<GameObject> RenderedPlatforms;

    class MarbleObj
    {
        public string name;
        public float dist;

        public MarbleObj(string name, float dist)
        {
            this.name = name;
            this.dist = dist;
        }
    }

    void Start()
    {
        RenderedPlatforms = new List<GameObject>();

        InvokeRepeating("SortMarbles", 0, 0.5f);
    }

    void SortMarbles()
    {
        if (MPClient_old.GameStarted)
        {
            MarbleObj[] MarblesBuffer = new MarbleObj[marbles.childCount];

            for (int i = 0; i < marbles.childCount; i++)
            {
                GameObject m = marbles.GetChild(i).gameObject;
                MarblesBuffer[i] = new MarbleObj(m.name, m.transform.position.x);
            }

            SortedMarbles = MarblesBuffer.OrderBy(m => m.dist);

            if (TotalPlatforms == RaceLength)
            {
                //foreach (MarbleObj m in SortedMarbles)
                for (int i = 0; i < SortedMarbles.Count(); i++)
                {
                    GameObject mObj = marbles.Find(SortedMarbles.ElementAt(i).name).gameObject;
                    WebSockets.Emit("death", new MPInteract_old.Death(mObj.transform.position, mObj.name));
                }
            }

            if (SortedMarbles.Last().dist > RenderedPlatforms[RenderedPlatforms.Count - ChunkNum + 1].transform.position.x + 20)
            {
                AddPlatform();
            }

            if (SortedMarbles.First().dist > RenderedPlatforms[1].transform.position.x + 20)
            {
                DeletePlatform();
            }
        }
    }

    void AddPlatform()
    {
        TotalPlatforms++;
        Debug.Log(TotalPlatforms);

        int ChoosenPlatform = Random.Range(0, platforms.transform.childCount);
        GameObject RandomPlatform = platforms.transform.GetChild(ChoosenPlatform).gameObject;
        GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
        PlatformCopy.transform.position = new Vector3(
            RenderedPlatforms[^1].transform.position.x + 100,
            RenderedPlatforms[^1].transform.position.y - 3,
            Random.Range(-2, 2)
        );

        WebSockets.Emit("NewPlatform", new MPClient_old.Platform(
            ChoosenPlatform,
            PlatformCopy.transform.position
        ));

        RenderedPlatforms.Add(PlatformCopy);
    }

    void DeletePlatform()
    {
        GameObject ToBeDestroyed = RenderedPlatforms[0];
        Destroy(ToBeDestroyed);
        RenderedPlatforms.RemoveAt(0);

        WebSockets.Emit("DeleteLastPlatform", MPClient_old.GameCode);
    }

    public void InitPlatforms()
    {
        Debug.Log("InitPlatforms");

        RenderedPlatforms.Add(InitPlatform);
        for (int i = 0; i < ChunkNum - 1; i++)
        {
            int ChoosenPlatform = Random.Range(0, platforms.transform.childCount);
            GameObject RandomPlatform = platforms.transform.GetChild(ChoosenPlatform).gameObject;
            GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
            PlatformCopy.transform.position = new Vector3(
                RenderedPlatforms[RenderedPlatforms.Count - 1].transform.position.x + 100,
                RenderedPlatforms[RenderedPlatforms.Count - 1].transform.position.y - 3,
                Random.Range(-2, 2)
            );

            RenderedPlatforms.Add(PlatformCopy);

            WebSockets.Emit("NewPlatform", new MPClient_old.Platform(
                ChoosenPlatform,
                PlatformCopy.transform.position
            ));
        }
    }

    /* checklist
     * 
     * When game starts:
     * -* host sends HTTP request to create new MP server
     * -* website server copies content of multiplayer.js and creates a new file with a special code
     * -* website server sends back the special code
     * -* host connects to the special code WS
     * -* clients connect to the same WS using the code
     * -* when host sends "start" HTTP request to game server, it sends "start" WS message to everyone
     * -* each player loads "multiplayer" scene without starting the game
     * -* each player sends "ready" WS message when the scene loaded
     * -* when everyone sent "ready", game server sends "start2" WS message
     * - game starts for each player
     * 
     * Game Client sends every time input is pressed:
     * - their position + velocity
     * 
     * Game Servers constantly send to everyone:
     * - position + velocity of all marbles (each client then fixes positions + velocities of the marbles)
     * 
     * Game Servers check all positions, generate random platforms/PUs, and periodically send to everyone:
     * - platforms + their positions
     * - PUs + their positions
     * 
     */
}