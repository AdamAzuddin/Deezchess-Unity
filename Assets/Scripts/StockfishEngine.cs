using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishEngine
{
    
    public string GetBestMove(string fen, int depth)
    {
        Process stockfish = new Process();
        string stockfishPath = Path.Combine(Application.streamingAssetsPath, "stockfish-windows-x86-64-avx2.exe");
        if (!File.Exists(stockfishPath))
        {
            Debug.Log("Error: Stockfish executable not found at " + stockfishPath);
            return "";
        }
        stockfish.StartInfo.FileName = stockfishPath;
        stockfish.StartInfo.RedirectStandardInput = true;
        stockfish.StartInfo.RedirectStandardOutput = true;
        stockfish.StartInfo.UseShellExecute = false;
        stockfish.StartInfo.CreateNoWindow = true;
        stockfish.Start();

        // Initialize Stockfish
        stockfish.StandardInput.WriteLine("uci");
        stockfish.StandardInput.Flush();

        // Wait for "uciok"
        while (!stockfish.StandardOutput.EndOfStream)
        {
            string line = stockfish.StandardOutput.ReadLine();
            if (line == "uciok") break;
        }

        // Send FEN position
        stockfish.StandardInput.WriteLine($"position fen {fen}");
        stockfish.StandardInput.Flush();

        // Start search
        stockfish.StandardInput.WriteLine($"go depth {depth}");
        stockfish.StandardInput.Flush();

        // Read best move
        string bestMoveStr = "";
        while (!stockfish.StandardOutput.EndOfStream)
        {
            string line = stockfish.StandardOutput.ReadLine();
            if (line.StartsWith("bestmove"))
            {
                bestMoveStr = line.Split(' ')[1]; // Extract move
                break;
            }
        }
        Debug.Log("Best move from stockfish based on fen string "+fen+" is "+bestMoveStr);

        stockfish.Close();
        return bestMoveStr;
    }
}
