// quick demo client - not production code, just enough to prove the hub works

let currentUserId = null;
let currentConversationId = null;
let otherUserId = null;
let lastMessageTime = null;
let connection = null;

function startConnection(userId) {
    currentUserId = userId;

    // userId goes in the query string, that's what ChatHub.GetUserId() reads
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chatHub?userId=${encodeURIComponent(userId)}`)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 15000]) // retry schedule
        .build();

    registerHandlers();

    connection.start()
        .then(() => console.log("connected as", userId))
        .catch(err => console.error("connection failed:", err));
}

function registerHandlers() {

    connection.onreconnecting(() => {
        document.getElementById("status").innerText = "Reconnecting...";
    });

    connection.onreconnected(() => {
        document.getElementById("status").innerText = "Connected";
        if (currentConversationId && lastMessageTime) {
            connection.invoke("GetMissedMessages", currentConversationId, lastMessageTime);
        }
    });

    connection.onclose(() => {
        document.getElementById("status").innerText = "Disconnected";
    });

    connection.on("ReceiveMessage", (message) => {
        appendMessage(message);
        lastMessageTime = message.sentAt;

        if (message.senderId !== currentUserId) {
            incrementBadge();
            connection.invoke("MarkAsRead", message.messageId, currentUserId);
        }
    });

    connection.on("MissedMessages", (messages) => {
        messages.forEach(m => appendMessage(m));
        if (messages.length > 0) {
            lastMessageTime = messages[messages.length - 1].sentAt;
        }
    });

    connection.on("MessageRead", (messageId, readAt) => {
        const el = document.querySelector(`[data-msg-id='${messageId}']`);
        if (el) el.querySelector(".read-status").innerText = "Read";
    });

    connection.on("UserOnline", (userId) => {
        if (userId === otherUserId) document.getElementById("peer-status").innerText = "Online";
    });

    connection.on("UserOffline", (userId) => {
        if (userId === otherUserId) document.getElementById("peer-status").innerText = "Offline";
    });

    connection.on("MessageDelivered", (message) => {
        appendMessage(message);
        lastMessageTime = message.sentAt;
    });

} // end registerHandlers

function sendMessage() {
    const input = document.getElementById("msg-input");
    const text = input.value.trim();
    if (!text) return;

    connection.invoke("SendMessage", currentConversationId, currentUserId, otherUserId, text)
        .catch(err => console.error(err));

    input.value = "";
}

async function sendFile(fileInput) {
    const file = fileInput.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    const res = await fetch("/api/chat/upload", { method: "POST", body: formData });
    if (!res.ok) {
        alert("upload failed: " + await res.text());
        return;
    }
    const data = await res.json();

    await connection.invoke("SendFileMessage", currentConversationId, currentUserId, otherUserId,
        data.fileUrl, data.fileName, data.fileSize);
}

function appendMessage(message) {
    const container = document.getElementById("messages");
    const div = document.createElement("div");
    div.className = "message";
    div.dataset.msgId = message.messageId;

    let contentHtml = "";
    if (message.fileUrl) {
        contentHtml = `<a href="${message.fileUrl}" target="_blank">${message.fileName}</a>`;
    } else {
        contentHtml = message.content;
    }

    div.innerHTML = `
        <span class="sender">${message.senderId}:</span>
        <span class="content">${contentHtml}</span>
        <span class="read-status">${message.isRead ? "Read" : ""}</span>
    `;
    container.appendChild(div);
    container.scrollTop = container.scrollHeight;
}

function incrementBadge() {
    const badge = document.getElementById("unread-badge");
    badge.style.display = "inline-block";
    badge.innerText = (parseInt(badge.innerText || "0") + 1).toString();
}