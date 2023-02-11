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
        localStorage.setItem(UTF8ToString(name), UTF8ToString(data));
        console.log("set", UTF8ToString(name), UTF8ToString(data))
    },
    GetStorage: function (name) {
        const data = localStorage.getItem(UTF8ToString(name));
        console.log("get", UTF8ToString(name), data);

        let result = "";
        if (data != null) {
            result = data;
        }

        let bufferSize = lengthBytesUTF8(result) + 1;
        let buffer = _malloc(bufferSize);
        stringToUTF8(result, buffer, bufferSize);
        return buffer;
    },
});