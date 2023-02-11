using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameScript : MonoBehaviour
{
    public static int score = 0;
    public static string GamePlatform = "mobile";

    public static List<GameObject> RenderedPlatforms;

    public GameObject marble;
    public GameObject platforms;
    public GameObject PowerUps;
    public GameObject InitPlatform;
    public GameObject ScoreObject;
    public GameObject VersionObject;

    private int ChunkNum = 5;

    private float PlayerProgress;
    private GameObject LastPU;

    public Volume GV;
    private DepthOfField blur;

    void Start()
    {
        GV.profile.TryGet(out DepthOfField blur);
        blur.active = false;

        VersionObject.GetComponent<Text>().text = Application.version;

        score = 0;
        RenderedPlatforms = new List<GameObject>();

        InitPlatforms();
    }

    void Update()
    {
        PlayerProgress = marble.transform.position.x;

        if (PlayerProgress > RenderedPlatforms[1].transform.position.x + 20)
        {
            AddPlatform(Random.Range(-2, 2));

            score++;
            Text ScoreText = ScoreObject.GetComponent<Text>();
            ScoreText.text = score.ToString();
        }
    }

    void AddPlatform(int PosZ)
    {
        GameObject RandomPlatform = platforms.transform.GetChild(Random.Range(0, platforms.transform.childCount)).gameObject;
        GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
        PlatformCopy.transform.position = new Vector3(
            RenderedPlatforms[ChunkNum - 1].transform.position.x + 100,
            RenderedPlatforms[ChunkNum - 1].transform.position.y - 3,
            PosZ
        );

        GameObject ToBeDestroyed = RenderedPlatforms[0];
        Destroy(ToBeDestroyed);

        for (int i = 1; i < ChunkNum; i++)
        {
            RenderedPlatforms[i - 1] = RenderedPlatforms[i];
        }

        RenderedPlatforms[ChunkNum - 1] = PlatformCopy;

        if (PlayerProgress % 400 < 100)
        {
            AddPowerUp(RenderedPlatforms[ChunkNum - 3].transform.position);
        }
    }

    void InitPlatforms()
    {
        RenderedPlatforms.Add(InitPlatform);
        for (int i = 0; i < ChunkNum - 1; i++)
        {
            GameObject RandomPlatform = platforms.transform.GetChild(Random.Range(0, platforms.transform.childCount)).gameObject;
            GameObject PlatformCopy = GameObject.Instantiate(RandomPlatform);
            PlatformCopy.transform.position = new Vector3(
                RenderedPlatforms[RenderedPlatforms.Count - 1].transform.position.x + 100,
                RenderedPlatforms[RenderedPlatforms.Count - 1].transform.position.y - 3,
                Random.Range(-2, 2)
            );

            RenderedPlatforms.Add(PlatformCopy);
        }
    }

    void AddPowerUp(Vector3 platform)
    {
        GameObject RandomPU = PowerUps.transform.GetChild(Random.Range(0, PowerUps.transform.childCount)).gameObject;
        GameObject PUCopy = GameObject.Instantiate(RandomPU);

        PUCopy.transform.position = new Vector3(
            platform.x + 48,
            platform.y + 2.5f,
            platform.z
        );

        Destroy(LastPU);
        LastPU = PUCopy;
    }
}