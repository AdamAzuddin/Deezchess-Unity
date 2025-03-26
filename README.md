# DeezChess Unity  

### 🏆 Play Against Your Own AI!  
DeezChess lets you play chess against a bot that mimics your playstyle! Upload a PGN of your games, and the AI will generate an opening book + play moves based on your past games.  

---  

## 🛠️ Tech Stack  
- **Unity** (Frontend) – Built with WebGL compatibility.  
- **FastAPI** (Backend) – Handles move generation and AI logic.  
- **Stockfish** – Used for move evaluation when no opening moves are found.  
- **Docker** – Backend is containerized for easy deployment.  
- **Render.com** – Used to host the backend API.  

---  

## 🚀 How It Works  
1. Upload a PGN file of your past games.  
2. The backend processes it, generating an **opening book** and **configuration file**.  
3. Unity downloads these files and sends move requests to the backend.  
4. If the move is in the opening book, it's played. Otherwise, Stockfish takes over.  
5. You play against your **own personalized bot!**  

---  

## 📚 Repository Structure  
- `Assets/` – Unity project files.  
- `Scripts/` – C# scripts for game logic & API requests.  
- `Scenes/` – Unity scenes, including the main game UI.  
- `.gitignore` – Standard Unity gitignore settings.  

---  

## 📢 Feedback & Contributions  
This is a **passion project**, and I’d love to hear your thoughts! Feel free to open issues or submit PRs. 😃  
