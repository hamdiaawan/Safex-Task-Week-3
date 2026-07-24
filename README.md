# SafeX Chat - Real-Time Messaging Module (Project 2, Group 51)

Real-time chat between Companies and Interns, built with ASP.NET Core + SignalR + EF Core + SQL Server.

## How to run

1. Update the connection string in `appsettings.json` to point at your SQL Server instance.
2. Run migrations:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
3. `dotnet run`
4. Open `https://localhost:xxxx/index.html` in two different browser tabs (or one normal + one incognito) to simulate two users chatting.

## How offline / reconnect / ordering was handled

**Offline detection**
Every socket connect/disconnect updates a row in `UserConnections` (`IsOnline`, `LastSeen`). `OnDisconnectedAsync` in `ChatHub` marks the user offline and broadcasts `UserOffline` so the other party's UI updates immediately. When a message is sent to someone who's offline, the hub still saves it to the DB as normal and fires a `PendingNotification` event — that's the hook point for the email notification.

**Reconnect**
SignalR's client has built-in `withAutomaticReconnect()`, so short network blips are retried automatically with the interval schedule `[0, 2, 5, 10, 15]` seconds. The tricky part is what happens *between* disconnect and reconnect — any messages sent during that window would otherwise be lost on the client. To handle that:
- The client tracks `lastMessageTime` (timestamp of the last message it actually rendered).
- On `onreconnected`, it calls `GetMissedMessages(conversationId, lastMessageTime)` on the hub, which pulls anything from the DB newer than that timestamp and sends it back as a batch (`MissedMessages` event).
- This means the source of truth is always the database, not the socket — the socket is just a delivery mechanism. If it drops, nothing is lost because messages are persisted synchronously before being pushed out.

**Message ordering**
Client-side timestamps aren't trusted for ordering (clock drift, network delay) — every message gets its `SentAt` from the server (`DateTime.UtcNow` at the point of insert), and the DB has a composite index on `(ConversationId, SentAt)`. History is always queried and displayed sorted by that server timestamp, never by arrival order over the socket.

## Known limitations / next steps
- Auth is stubbed (`userId` passed via query string) — needs to be swapped for real ASP.NET Identity / JWT once the auth module from another teammate is ready, so I don't duplicate that work.
- Email notification is not wired up yet (background job / SMTP), only the `PendingNotification` hook exists — will add if time allows per the task scope.
- Group chat is out of scope, this is 1-to-1 company↔intern only.
