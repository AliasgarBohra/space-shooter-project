# 🚀 Space Shooter (WebGL Multiplayer)

A multiplayer space shooter game built with **Unity 6** and **Photon Fusion 2**, playable directly in the browser.  
The game is hosted here:  
[Play Now](https://aliasgarbohra.github.io/space-shooter/)

---

## 🎮 How to Play

1. Open the game in your browser.  
2. Enter or share a **match link** with a friend:
   - A match link looks like this:  
     ```
     https://aliasgarbohra.github.io/space-shooter/?matchId=abc123&playerId=player1&opponentId=player2
     ```
   - Share it with your opponent so they can join directly.  
3. Defeat your opponent by shooting enemy ships while avoiding hits.  
4. The winner is reported back to the host page via `postMessage`.

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