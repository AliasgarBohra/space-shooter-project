# 🚀 Space Shooter (WebGL Multiplayer)

A multiplayer space shooter game built with **Unity 6** and **Photon Fusion 2**, playable directly in the browser.  
The game is hosted here:  
[Play Now](https://aliasgarbohra.github.io/space-shooter/)

---

## 🎮 How to Play

1. Open the game in your browser.  
2. The game works in both Singleplayer and Multiplayer.
3. You can either share a code or directly share the link from your browser, and your friend will join automatically!
4. Enter or share a **match link**:
   - A match link looks like this:  
     ```
     https://aliasgarbohra.github.io/space-shooter/?matchId=abc123&playerId=player1&opponentId=player2
     ```
   - Share it with your opponent so they can join directly.  
5. Defeat your opponent by shooting enemy ships while avoiding hits.  
6. The winner is reported back to the host page via `postMessage`.

---
## Documentation

1. EnemyWaveSpawner.cs: The enemy waves are generated and synchronized using seed based approach! The host generates a random seed and send it through rpc and the host and client will produce deterministic waves!
2. EnemyShip.cs and EnemyMovement.cs: They are blueprint of individual enemy ships!
3. WebGLMatchBootstrap.cs: Its work is to check for URL at start, if it finds match id in url, it will attempt to join the match or create one. It is also used to send messages to browser, like match result, match abort, etc.
4. FusionLauncher.cs: It handles creation and joining of a session, it generates the match id or joining code.
5. GameManager.cs: It's simple use is to handle session data.
6. Player folder contains scripts to handle player ship motion, shoot projectile, etc.
7. GameplayHandler: It handles the gameplay when players are connected, it starts enemy waves, timer, and ends game, give results.

---

## 🔗 Match Links

- Each match has a unique `matchId`.  
- The URL query parameters control who joins which game:
  - `matchId`: Unique identifier for the game session  
  - `playerId`: Current player’s ID  
  - `opponentId`: The matched opponent’s ID  

## 🛠️ Tech Stack

- Unity 6 (WebGL build)
- Photon Fusion 2 (Multiplayer networking)
- C# (Game logic & network management) and JavaScript
- GitHub Pages (Hosting)