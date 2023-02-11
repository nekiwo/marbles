using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class Menu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public GameObject ChoiceMenu;
    public GameObject HostMenu;
    public GameObject JoinMenu;
    public TextMeshProUGUI CodeText;
    public Slider slider;
    public InputField LBHostInput;
    public TMP_InputField CodeInput;
    public TextMeshProUGUI JoinMessage;
    public GameObject JoinList;
    public GameObject JoinListScroll;
    public GameObject JoinNameTemplate;
    public InputField UsernameInput;
    public GameObject HostError;
    public Text version;
    public PopUpError PopUp;
    public static string OnStartErrorMsg = "";

    public GameObject LobbyList;
    public GameObject LobbyListScroll;
    public GameObject LobbyEntryTemplate;
    public GameObject LobbyListLoading;

    public GameObject MPMenuBtn;
    public GameObject MPMenuLoading;

    public static string LBHost = "http://localhost:3000/marbles/";
    private float MinSens = 0;
    private float MaxSens = 80;

    private Coroutine listLoad;

    void Start()
    {
        version.text = Application.version;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string StoredName = WebSockets.GetStorage("ClientName");
            Debug.Log(StoredName);
            if (StoredName == "")
            {
                WebSockets.SetStorage("ClientName", "DefaultName");
                StoredName = "DefaultName";
            }
            MPClient.ClientName = StoredName;

            string StoredMultiplier = WebSockets.GetStorage("SideSpeedMultiplier");
            Debug.Log(StoredMultiplier);
            if (StoredMultiplier == "")
            {
                WebSockets.SetStorage("SideSpeedMultiplier", 0.5f.ToString());
                StoredMultiplier = 0.5f.ToString();
            }
            Movement.SideSpeedMultiplier = MinSens + float.Parse(StoredMultiplier) * MaxSens;
        }

        if (OnStartErrorMsg != "")
        {
            PopUp.SetErrorText(OnStartErrorMsg);
            PopUp.OpenErrorMessage();

            OnStartErrorMsg = "";
        }
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(
            0,
            transform.eulerAngles.y + 2 * Time.deltaTime,
            0
        );
    }

    public void Multiplayer()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            StartCoroutine(OpenMultiplayer());

            MPMenuBtn.SetActive(false);
            MPMenuLoading.SetActive(true);
        } else
        {
            PopUp.SetErrorText("No internet connection");
            PopUp.OpenErrorMessage();
        }
    }

    IEnumerator OpenMultiplayer()
    {
        UnityWebRequest request = UnityWebRequest.Get(WebSockets.WSHost);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            MainMenu.SetActive(false);
            ChoiceMenu.SetActive(true);
        }
        else
        {
            Debug.Log(request.error);

            PopUp.SetErrorText("Servers are offline");
            PopUp.OpenErrorMessage();
        }

        MPMenuBtn.SetActive(true);
        MPMenuLoading.SetActive(false);
    }

    public void Singleplayer()
    {
        SceneManager.LoadScene("singleplayer", LoadSceneMode.Single);
    }

    public void Settings()
    {
        MainMenu.SetActive(false);
        SettingsMenu.SetActive(true);

        LBHostInput.text = LBHost;
        slider.value = (Movement.SideSpeedMultiplier - MinSens) / MaxSens;
        UsernameInput.text = MPClient.ClientName;
    }

    public void CustomLBHostURL()
    {
        LBHost = LBHostInput.text;
    }

    public void SensitivitySlider()
    {
        Movement.SideSpeedMultiplier = MinSens + slider.value * MaxSens;
        WebSockets.SetStorage("SideSpeedMultiplier", slider.value.ToString());
    }

    public void CustomUsername()
    {
        MPClient.ClientName = UsernameInput.text;
        WebSockets.SetStorage("ClientName", MPClient.ClientName);
    }

    public void OpenHostMenu()
    {
        ChoiceMenu.SetActive(false);
        HostMenu.SetActive(true);

        WebSockets.Initiate();

        CodeText.text = "Wait...";
        foreach (Transform plr in JoinList.transform)
        {
            if (plr.gameObject.name != JoinNameTemplate.name)
            {
                Destroy(plr.gameObject);
            }
        }

        listLoad = StartCoroutine(WaitForWSInit());

        WebSockets.OnUnityThread("NewJoin", (data) =>
        {
            string JoinName = data.GetValue<string>();

            GameObject JoinNameCopy = GameObject.Instantiate(JoinNameTemplate);
            JoinNameCopy.GetComponent<Text>().text = JoinName;
            JoinNameCopy.transform.SetParent(JoinList.transform);

            RectTransform originalRect = JoinNameTemplate.GetComponent<RectTransform>();
            RectTransform rect = JoinNameCopy.GetComponent<RectTransform>();

            rect.position = new Vector2(
                originalRect.position.x,
                originalRect.position.y - rect.sizeDelta.y * (JoinList.transform.childCount - 1)
            );

            rect.offsetMin = new Vector2(0, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0, rect.offsetMax.y);

            JoinListScroll.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);
        });

        WebSockets.OnUnityThread("GameStarted", (data) =>
        {
            Debug.Log("Game Started");
            SceneManager.LoadScene("multiplayer", LoadSceneMode.Single);
        });

        WebSockets.OnUnityThread("RemovedLobby", (data) =>
        {
            string code = data.GetValue<string>();

            if (code == MPClient.GameCode)
            {
                PopUp.SetErrorText("Your lobby was deleted for being AFK");
                PopUp.OpenErrorMessage();

                SettingsMenu.SetActive(false);
                ChoiceMenu.SetActive(false);
                HostMenu.SetActive(false);
                JoinMenu.SetActive(false);

                MainMenu.SetActive(true);
            }
        });
    }

    IEnumerator WaitForWSInit()
    {
        yield return new WaitForSeconds(2);

        MPClient.GameCode = GenCode();
        MPClient.ClientID = GenCode();
        MPClient.IsHost = true;
        CodeText.text = "Code: " + MPClient.GameCode;

        WebSockets.Emit("join", new MPClient.PlayerJoin(true));
        Debug.Log("join " + MPClient.GameCode);
    }

    private string GenCode()
    {
        string result = "";

        for (int i = 0; i < 4; i++)
        {
            result += "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[Random.Range(0, 36)];
        }

        return result;
    }

    public void HostStart()
    {
        if (JoinList.transform.childCount > 2)
        {
            WebSockets.Emit("start", MPClient.GameCode);
        } else
        {
            PopUp.SetErrorText("Not Enough Players");
            PopUp.OpenErrorMessage();
        }
    }

    class Lobby
    {
        public string name;
        public int count;
    }

    public void OpenJoinMenu()
    {
        ChoiceMenu.SetActive(false);
        JoinMenu.SetActive(true);

        foreach (Transform lobby in LobbyList.transform)
        {
            if (lobby.gameObject.name != LobbyEntryTemplate.name)
            {
                Destroy(lobby.gameObject);
            }
        }

        LobbyListLoading.SetActive(true);

        WebSockets.Initiate();
        listLoad = StartCoroutine(LoadLobbyList());

        WebSockets.OnUnityThread("CreatedLobby", (data) =>
        {
            Lobby lobby = data.GetValue<Lobby>();

            if (LobbyList.transform.Find(lobby.name) == null)
            {
                GameObject lobbyEntryCopy = GameObject.Instantiate(LobbyEntryTemplate);
                lobbyEntryCopy.name = lobby.name;
                lobbyEntryCopy.transform.Find("code").GetComponent<Text>().text = lobby.name;
                lobbyEntryCopy.transform.Find("player count").GetComponent<Text>().text = lobby.count.ToString() + "/20";
                lobbyEntryCopy.transform.SetParent(LobbyList.transform);

                RectTransform originalRect = LobbyEntryTemplate.GetComponent<RectTransform>();
                RectTransform rect = lobbyEntryCopy.GetComponent<RectTransform>();

                rect.position = new Vector2(
                    originalRect.position.x,
                    originalRect.position.y - rect.sizeDelta.y * (LobbyList.transform.childCount - 1)
                );

                rect.offsetMin = new Vector2(0, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0, rect.offsetMax.y);

                LobbyListScroll.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);


                LobbyListLoading.SetActive(false);
            }
        });

        WebSockets.OnUnityThread("UpdateLobby", (data) =>
        {
            Lobby lobby = data.GetValue<Lobby>();

            Transform entry = LobbyList.transform.Find(lobby.name);
            entry.Find("player count").gameObject.GetComponent<Text>().text = lobby.count.ToString() + "/20";
        });

        WebSockets.OnUnityThread("RemovedLobby", (data) =>
        {
            string code = data.GetValue<string>();

            Debug.Log(code);

            Transform entry = LobbyList.transform.Find(code);
            if (entry != null)
            {
                //Destroy(entry);
                entry.Find("code").gameObject.GetComponent<Text>().text = "[Started]";
            }
        });
    }

    IEnumerator LoadLobbyList()
    {
        yield return new WaitForSeconds(2);

        WebSockets.Emit("LobbyList", "");
        Debug.Log("join " + MPClient.GameCode);
    }

    private bool IsJoined = false;
    public void JoinGame()
    {
        if (!IsJoined)
        {
            IsJoined = true;

            MPClient.GameCode = CodeInput.text.ToUpper();
            MPClient.ClientID = GenCode();
            MPClient.IsHost = false;
            JoinMessage.text = "Game will start soon...";

            WebSockets.Emit("join", new MPClient.PlayerJoin(false));

            WebSockets.OnUnityThread("NoGame", (data) =>
            {
                JoinMessage.text = "No games found";
                IsJoined = false;
            });

            WebSockets.OnUnityThread("GameStarted", (data) =>
            {
                Debug.Log("Game Started");
                SceneManager.LoadScene("multiplayer", LoadSceneMode.Single);
            });
        }
    }

    public void GoBack()
    {
        if (listLoad != null)
        {
            StopCoroutine(listLoad);
        }

        if (MPClient.ClientName == "")
        {
            MPClient.ClientName = "DefaultName";
        }

        if (MPClient.GameCode != null)
        {
            //WebSockets.Emit("leave", new MPMenu.PlayerLeaveInfo());
        }

        SettingsMenu.SetActive(false);
        ChoiceMenu.SetActive(false);
        HostMenu.SetActive(false);
        JoinMenu.SetActive(false);

        MainMenu.SetActive(true);
    }
}