var username = document.getElementById("username");
var connectButton = document.getElementById("connectButton");
var stateLabel = document.getElementById("stateLabel");
var sendMessage = document.getElementById("sendMessage");
var sendButton = document.getElementById("sendButton");
var messagesLog = document.getElementById("messagesLog");
var closeButton = document.getElementById("closeButton");
var connID = document.getElementById("connIDLabel");

connectButton.onclick = function () {
    stateLabel.innerHTML = "Attempting to connect...";
    var socketUrl = "ws://localhost:5000?username=" + username.value;
    socket = new WebSocket(socketUrl);
    socket.onopen = function (event) {
        updateState();
        messagesLog.innerHTML += '<tr>' +
            '<td colspan="3" class="messagesLog-data">Connection opened</td>' +
            '</tr>';
    };
    socket.onclose = function (event) {
        updateState();
        messagesLog.innerHTML += '<tr>' +
            '<td colspan="3" class="messagesLog-data">Connection closed. Code: ' + htmlEscape(event.code) + '. Reason: ' + htmlEscape(event.reason) + '</td>' +
            '</tr>';
    };
    socket.onerror = updateState;
    socket.onmessage = function (event) {
        var data = JSON.parse(event.data);
        console.log(data);
        if (data.username == username.value) {
            messagesLog.innerHTML += '<tr>' +
                '<td><span>You:</span> ' + htmlEscape(data.message) + ' </td>';
        } else {
            messagesLog.innerHTML += '<tr>' +
                '<td><span>' + htmlEscape(data.username) + ':</span> ' + htmlEscape(data.message) + ' </td>';
        }
        isConnID(data.message);
    };

};

closeButton.onclick = function () {
    if (!socket || socket.readyState !== WebSocket.OPEN) {
        alert("socket not connected");
    }
    socket.close(1000, "Closing from client");
};
sendButton.onclick = function () {
    if (!socket || socket.readyState !== WebSocket.OPEN) {
        alert("socket not connected");
    }
    var data = sendMessage.value;
    socket.send(data);
    sendMessage.value = "";
};

function isConnID(str) {
    if (str.substring(0, 7) == "ConnID:")
        connID.innerHTML = "ConnID: " + str.substring(8, 45);
}

function constructJSONPayload() {
    return JSON.stringify({
        "From": connID.innerHTML.substring(8, connID.innerHTML.length),
        "Message": sendMessage.value
    });
}

function htmlEscape(str) {
    return str.toString()
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

function updateState() {
    function disable() {
        sendMessage.disabled = true;
        sendButton.disabled = true;
        closeButton.disabled = true;
    }
    function enable() {
        sendMessage.disabled = false;
        sendButton.disabled = false;
        closeButton.disabled = false;
    }
    username.disabled = true;
    connectButton.disabled = true;
    if (!socket) {
        disable();
    } else {
        switch (socket.readyState) {
            case WebSocket.CLOSED:
                stateLabel.innerHTML = "Closed";
                connID.innerHTML = "ConnID: N/a";
                disable();
                username.disabled = false;
                connectButton.disabled = false;
                break;
            case WebSocket.CLOSING:
                stateLabel.innerHTML = "Closing...";
                disable();
                break;
            case WebSocket.CONNECTING:
                stateLabel.innerHTML = "Connecting...";
                disable();
                break;
            case WebSocket.OPEN:
                stateLabel.innerHTML = "Open";
                enable();
                break;
            default:
                stateLabel.innerHTML = "Unknown WebSocket State: " + htmlEscape(socket.readyState);
                disable();
                break;
        }
    }
}