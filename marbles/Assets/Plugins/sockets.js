mergeInto(LibraryManager.library, {
    WebGLInitiate: function (host) {
        window.socket = io(UTF8ToString(host));

        window.socket.onAny((name, ...data) => {
            //console.log("on", name, data[0]);
            window.unityInstance.SendMessage("WS", "SocketIOCall", name + "|" + JSON.stringify(data[0]));
        });
    },
    WebGLEmit: function (name, data) {
        let ParsedName = UTF8ToString(name);
        let ParsedData = UTF8ToString(data);

        try {
            ParsedData = JSON.parse(ParsedData);
        } catch (error) {}

        //console.log("emit", ParsedName, ParsedData);
        window.socket.emit(ParsedName, ParsedData);
    },
    SetStorage: function (name, data) {
        localStorage.setItem(name, data);
    },
    GetStorage: function (name) {
        const data = localStorage.getItem(name);
        if (data == null) {
            return "";
        }
        return data;
    },
});