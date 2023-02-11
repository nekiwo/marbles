const express = require("express");
const multer  = require("multer");
const upload = multer();
const bodyParser = require("body-parser");
const fs = require("fs");

const app = express();
const http = require("http").createServer(app);
var cors = require("cors")
const io = require("socket.io")(http);

const publicdir = __dirname + "/src";
app.use(express.static(publicdir, {extensions:["html"]}));
app.use(bodyParser.urlencoded({extended:false}));
app.use(bodyParser.json());
app.use(cors());
app.set("trust proxy", true);

app.get("/Marbles/GetScores", function(req, res) {
    fs.readFile("scores.json", function (error, data) {
        if (error) throw error;
        res.json(JSON.parse(data));
    })
});

app.post("/Marbles/SetScore", upload.none(), function(req, res) {
    fs.readFile("scores.json", function (error, data) {
        if (error) throw error;

        let JsonData = JSON.parse(data);
        let score = parseInt(req.body.score);

        let NameExists = false;
        let NameIndex;
        JsonData.some((entry, i) => {
            if (entry.name == req.body.name) {
                NameExists = true;
                NameIndex = i;
            }
            return NameExists;
        });

        if (NameExists) {
            if (JsonData[NameIndex].score < score) {
                JsonData[NameIndex].score = score;
                JsonData[NameIndex].platform = req.body.platform;
            }
        } else {
            JsonData.push({
                "score": score,
                "name": req.body.name,
                "platform": req.body.platform
            });
        }

        JsonData = SortScores(JsonData);

        fs.writeFile("scores.json", JSON.stringify(JsonData), (error) => {
            if (error) throw error;
            res.json({"result": "success"});
        })
    })
});

let SortScores = (scores) => {
    return scores.sort((a, b) => {
        return b.score - a.score;
    });
}

let GenNewCode = () => {
    let result = "";

    for (let i = 0; i < 4; i++) {
        result += "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[Math.floor(Math.random() * 36)];
    }

    return result;
}

app.use(function(_req, res) {
    res.status(404).send("404 :(");
});

let AllGames = {}

/*"CODE": {
    "players": [
        {
            "id": "ID",
            "name": "NAME",
            "afk": false
        }
    ],
    "ready": [
        "ID"
    ]
}*/

var recievedLobbyPong = [];
io.on("connection", function(socket) {
    console.log("new connection");

    socket.on("test", function() {
        console.log("test");
        socket.emit("test", {});
    });

    socket.on("join", function(data) {
        let SplitData = data;//.split(",");
        let room = SplitData[0];
        let ClientID = SplitData[1];
        let name = SplitData[2];
        let isHost = SplitData[3];

        console.log("new join at", SplitData)

        if (AllGames[room] == null) {
            if (isHost == "false") {
                io.to(socket.id).emit("NoGame", {});
                console.log("no game")
                return;
            } else {
                AllGames[room] = {};
                AllGames[room].players = [];
                AllGames[room].ready = false; // ready test

                io.emit("CreatedLobby", {
                    name: room,
                    count: 1
                });
            }
        }

        if (AllGames[room].players.find(({id}) => id === ClientID) == undefined) {
            AllGames[room].players.push({
                "id": ClientID,
                "name": name,
                "afk": 2556057600000,
                "dead": false
            });

            socket.join(room);
            io.to(room).emit("NewJoin", name);
            io.emit("UpdateLobby", {
                name: room,
                count: AllGames[room].players.length
            });
        }
    });

    socket.on("leave", function(data) {
        console.log("leave at ", data.room)

        if (AllGames[data.room] != null) {
            AllGames[data.room].players.filter(item => item !== data.id);

            if (AllGames[data.room].players.length == 0) {
                delete AllGames[data.room];
                io.emit("RemovedLobby", data.room);
            }
        }

        socket.leave(data.room);
    });

    socket.on("LobbyList", function() {
        console.log("LobbyList")

        // clean lobbies
        for(let room in AllGames) {
            io.to(room).emit("LobbyPing", {});
        }

        setTimeout(() => {
            console.log("recievedLobbyPong ", recievedLobbyPong)

            for(let room in AllGames) {
                if (recievedLobbyPong.includes(room)) {
                    io.to(socket.id).emit("CreatedLobby", {
                        name: room,
                        count: AllGames[room].players.length
                    });
                } else {
                    delete AllGames[room];
                    io.emit("RemovedLobby", room);
                    
                    console.log("deleted " + room)
                }
            }

            recievedLobbyPong = [];
        }, 2000);
    });

    socket.on("LobbyPong", function(room) {
        console.log("LobbyPong ", room)

        recievedLobbyPong.push(room);
        console.log("recievedLobbyPong", recievedLobbyPong)
    });

    socket.on("start", function(room) {
        console.log("Game started at", room)

        io.to(room).emit("GameStarted", {});
        io.emit("RemovedLobby", room);
    });

    /*socket.on("ready", function(data) {
        let SplitData = data//.split(",");
        let room = SplitData[0];
        let ClientID = SplitData[1];
        console.log("new ready at", SplitData)
        if (AllGames[room].ready == null) {
            AllGames[room].ready = [];
        }
        
        if (AllGames[room].ready.indexOf(ClientID) == -1) {
            AllGames[room].ready.push(ClientID);
        }
        if (AllGames[room].players.length == AllGames[room].ready.length) {
            console.log("Game loaded at", room)
            console.log(AllGames[room].players)
            io.to(room).emit("GameLoaded", AllGames[room].players);
        }
    });*/

    socket.on("ready", function(data) {
        let room = data[0];

        if (AllGames[room] != null) {
            if (!AllGames[room].ready) {
                AllGames[room].ready = true;
            } else {
                console.log("Game loaded at", room)
                io.to(room).emit("GameLoaded", AllGames[room].players);
            }
        }
    });

    socket.on("PhysChange", function(data) {
        //console.log(data)
        io.to(data.room).emit("PhysChange", data);
        //AllGames[data.room].players.find(({id}) => id === data.name).afk = Date.now();
    });

    socket.on("NewPlatform", function(data) {
        //console.log(data)
        io.to(data.room).emit("NewPlatform", data);
    });

    socket.on("DeleteLastPlatform", function(room) {
        //console.log(data)
        io.to(room).emit("DeleteLastPlatform", {});
    });

    socket.on("death", function(data) {
        console.log("death", data)
        io.to(data.room).emit("death", data);
        AllGames[data.room].players.find(({id}) => id === data.name).dead = true;
    });

    socket.on("restart", function(room) {
        console.log("restart: " + room)
        io.to(room).emit("restart", {});
    });

    socket.on("ping", function(data) {
        io.to(socket.id).emit("pong", {});

        let time = Date.now();
        //let player = AllGames[data.room].players.find(({id}) => id === data.id);
        AllGames[data.room].players.forEach(player => {
            if (player != undefined) {
                if (time - player.afk < 8000 && !player.dead) {
                    console.log(player.id, time - player.afk, player.afk)

                    if (player.id === data.id) {
                        player.afk = time;
                    }
                } else {
                    console.log(time - player.afk, [time - player.afk < 8000, !player.dead])
                    console.log("death AFK", player.id)
        
                    player.dead = true;
                    io.to(data.room).emit("death", {
                        room: data.room,
                        name: player.id,
                        PosX: 0,
                        PosY: 0,
                        PosZ: 0
                    });
                }
            }
        });
    });
});

const PORT = process.env.PORT || 3000;
http.listen(PORT, function() {
    console.log("Server is running on PORT:", PORT);
});