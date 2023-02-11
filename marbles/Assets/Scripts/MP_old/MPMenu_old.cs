using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MPMenu_old : MonoBehaviour
{
    public Transform marbles;
    public GameObject PreviousButton;
    public GameObject NextButton;
    public TextMeshProUGUI PlaceText;
    public GameObject confetti;

    private GameObject target = null;
    private int TargetIndex = 0;

    private void Start()
    {
        PlaceText.text = "You're #" + (System.Math.Max(0, MPClient_old.TotalAlive)).ToString() + " Place!";

        if (MPClient_old.TotalAlive == 1)
        {
            PreviousButton.SetActive(false);
            NextButton.SetActive(false);

            confetti.transform.position = new Vector3(
                Camera.main.transform.position.x + 3,
                Camera.main.transform.position.y + 3,
                Camera.main.transform.position.z
            );
            ParticleSystem system = confetti.GetComponent<ParticleSystem>();
            system.Clear();
            system.Play();

            //StartCoroutine(RestartMatch());
        } else
        {
            SpectatorNext();
        }
    }

    void Update()
    {
        if (target != null)
        {
            Rigidbody TargetBody = target.GetComponent<Rigidbody>();

            Camera.main.transform.position = new Vector3(
                target.transform.position.x - 4,
                target.transform.position.y + 2.5f,
                target.transform.position.z - TargetBody.velocity.z / 100
            );

            float TargetFoV = Mathf.Max(50, 40 + TargetBody.velocity.x / 2.5f);
            Camera.main.fieldOfView += (TargetFoV - Camera.main.fieldOfView) * 0.1f;
        }
    }

    public class PlayerLeaveInfo
    {
        public string id;
        public string code;

        public PlayerLeaveInfo()
        {
            this.id = MPClient_old.ClientID;
            this.code = MPClient_old.GameCode;
        }
    }

    public void MenuButton()
    {
        WebSockets.Emit("leave", new PlayerLeaveInfo());
        SceneManager.LoadScene("menu", LoadSceneMode.Single);
    }

    public void SpectatorPrevious()
    {
        Spectate(-1);
    }

    public void SpectatorNext()
    {
        Spectate(1);
    }

    void Spectate(int change)
    {
        if (marbles.childCount != 1)
        {
            TargetIndex = (TargetIndex + change) % (marbles.childCount - 1);
            target = marbles.GetChild(TargetIndex).gameObject;
        }
    }

    IEnumerator RestartMatch()
    {
        yield return new WaitForSeconds(5);
        WebSockets.Emit("restart", MPClient_old.GameCode);
    }
}
