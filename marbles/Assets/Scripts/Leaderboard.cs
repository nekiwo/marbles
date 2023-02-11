using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public GameObject EntryObj;
    public GameObject LeaderboardObj;
    public GameObject ScrollObj;
    public InputField NameInput;
    public GameObject RecordWindow;
    public GameObject RecordButton;
    public GameObject SubmitScore;
    public GameObject LoadingCircle;

    public Sprite MobileIcon;
    public Sprite DesktopIcon;

    private class RecordEntry
    {
        public int score;
        public string name;
        public string platform;
    }

    private class Status
    {
        public string result;
    }

    public IEnumerator LoadLeaderboard()
    {
        UnityWebRequest request = UnityWebRequest.Get(Menu.LBHost + "GetScores");
        request.SetRequestHeader("Accept", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            List<RecordEntry> ParsedEntries = JsonConvert.DeserializeObject<List<RecordEntry>>(request.downloadHandler.text);

            for (int i = 0; i < ParsedEntries.Count; i++)
            {
                RecordEntry entry = ParsedEntries[i];
                StartCoroutine(RenderEntry(i, entry.score.ToString(), entry.name, entry.platform));
            }

            Destroy(EntryObj);
            LoadingCircle.SetActive(false);
        }
    }

    IEnumerator RenderEntry(int place, string score, string name, string platform)
    {
        GameObject entry = GameObject.Instantiate(EntryObj);
        entry.transform.SetParent(LeaderboardObj.transform);

        yield return new WaitForEndOfFrame();

        entry.transform.Find("place").GetComponent<Text>().text = "#" + (place + 1).ToString();
        entry.transform.Find("score").GetComponent<Text>().text = score;
        entry.transform.Find("name").GetComponent<Text>().text = name;

        ScrollObj.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 1);

        if (platform == "mobile")
        {
            entry.transform.Find("platform").GetComponent<Image>().sprite = MobileIcon;
        } else
        {
            entry.transform.Find("platform").GetComponent<Image>().sprite = DesktopIcon;
        }
    }

    public void PostEntryButton()
    {
        StartCoroutine(PostEntry(GameScript.score, NameInput.text, GameScript.GamePlatform));
    }

    IEnumerator PostEntry(int score, string name, string platform)
    {
        WWWForm form = new WWWForm();
        form.AddField("score", score);
        form.AddField("name", name);
        form.AddField("platform", platform);

        UnityWebRequest request = UnityWebRequest.Post(Menu.LBHost + "SetScore", form);
        request.SetRequestHeader("Accept", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Status status = JsonConvert.DeserializeObject<Status>(request.downloadHandler.text);
            
            if (status.result == "success")
            {
                RecordWindow.SetActive(false);
                RecordButton.SetActive(false);
            } else
            {
                //error
            }
        }
    }

    public void ToggleWindow()
    {
        if (RecordWindow.activeSelf)
        {
            RecordWindow.SetActive(false);
        } else
        {
            RecordWindow.SetActive(true);
        }
    }

    public void HideButton()
    {
        if (NameInput.text == "")
        {
            SubmitScore.SetActive(false);
        } else
        {
            SubmitScore.SetActive(true);
        }
    }
}