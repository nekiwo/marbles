using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Linq;

public class WebSockets : MonoBehaviour
{
    public static SocketIOUnity socket;

    public static bool IsWebGL;
    public static string WSHost = "http://localhost:3000";

    public static ICollection<Event> events;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        events = new List<Event>();
        IsWebGL = Application.platform == RuntimePlatform.WebGLPlayer;

        if (IsWebGL)
        {
            //WSHost = Application.absoluteURL.Substring(0, Application.absoluteURL.LastIndexOf("/"));
            WSHost = Application.absoluteURL.Replace("marbles/", "");
        }

        Debug.Log(WSHost);
    }

    public class Event
    {
        public string name;
        public Action<WSResponse> callback;

        public Event(string name, Action<WSResponse> callback)
        {
            this.name = name;
            this.callback = callback;
        }
    }

    public class WSResponse
    {
        public string RawDataText;
        public SocketIOResponse RawDataResp;

        public T GetValue<T>()
        {
            if (IsWebGL)
            {
                T ParsedValue = JsonConvert.DeserializeObject<T>(RawDataText);
                return ParsedValue;
            }
            else
            {
                T ParsedValue = RawDataResp.GetValue<T>();
                return ParsedValue;
            }
        }

        public WSResponse(object RawData)
        {
            if (RawData.GetType() == typeof(string))
            {
                this.RawDataText = (string)RawData;
            } else
            {
                this.RawDataResp = (SocketIOResponse)RawData;
            }
        }
    }

    [DllImport("__Internal")]
    private static extern void WebGLInitiate(string host);

    public static void Initiate()
    {
        if (IsWebGL)
        {
            WebGLInitiate(WSHost);
        } else
        {
            System.Uri uri = new System.Uri(WSHost);
            socket = new SocketIOUnity(uri);
            socket.JsonSerializer = new NewtonsoftJsonSerializer();
            socket.Connect();
        }

        OnUnityThread("LobbyPing", (data) =>
        {
            if (MPClient.GameCode != null)
            {
                Emit("LobbyPong", MPClient.GameCode);
            }
        });
    }

    [DllImport("__Internal")]
    private static extern void WebGLEmit(string name, string data);

    public static void Emit(string evnt, object data)
    {
        if (IsWebGL)
        {
            string SerializedData = JsonConvert.SerializeObject(data);
            WebGLEmit(evnt, SerializedData);
        }
        else
        {
            socket.Emit(evnt, data);
        }
    }

    public static void OnUnityThread(string evnt, Action<WSResponse> callback)
    {
        if (IsWebGL)
        {
            events.Add(new Event(evnt, callback));
        }
        else
        {
            socket.OnUnityThread(evnt, (data) =>
            {
                callback(new WSResponse(data));
            });
        }
    }

    public static void OnUnityThreadFixed(string evnt, Action<WSResponse> callback)
    {
        if (IsWebGL)
        {
            events.Add(new Event(evnt, callback));
        }
        else
        {
            socket.unityThreadScope = SocketIOUnity.UnityThreadScope.FixedUpdate;
            socket.OnUnityThread(evnt, (data) =>
            {
                callback(new WSResponse(data));
            });
        }
    }

    public void SocketIOCall(string data)
    {
        string[] SplitData = data.Split("|");
        string name = SplitData[0];
        string text = SplitData[1];

        foreach (Event evnt in events.ToList())
        {
            if (evnt.name == name)
            {
                evnt.callback(new WSResponse(text));
            }
        }
    }


    [DllImport("__Internal")]
    public static extern void SetStorage(string name, string data);

    [DllImport("__Internal")]
    public static extern string GetStorage(string name);
}