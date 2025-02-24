using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StockfishEngine
{
    public static (int, int) UciMoveToBitboardIndices(string uciMove)
    {
        if(uciMove=="0-0"){
            return (0,0);
        }
        else if (uciMove=="0-0-0"){
            return (1,1);
        }
        if (uciMove.Length != 4)
            return (-1, -1);

        string fromSquare = uciMove.Substring(0, 2);
        string toSquare = uciMove.Substring(2, 2);

        int fromIndex = UciSquareToBitboardIndex(fromSquare);
        int toIndex = UciSquareToBitboardIndex(toSquare);

        return (fromIndex, toIndex);
    }

    // Dummy function (you need to implement this based on how your bitboard indices are mapped)
    public static int UciSquareToBitboardIndex(string square)
    {
        // Example logic: Convert "e2" to bitboard index
        if (square.Length != 2)
            return -1;

        char file = square[0]; // 'a' to 'h'
        char rank = square[1]; // '1' to '8'

        if (file < 'a' || file > 'h' || rank < '1' || rank > '8')
            return -1;

        return (rank - '1') * 8 + (file - 'a'); // Convert to 0-63 index
    }

    public (int, int) GetBestMove(string fen, int depth)
    {
        Process stockfish = new Process();
        string stockfishPath = Path.Combine(Application.streamingAssetsPath, "stockfish-windows-x86-64-avx2.exe");
        if (!File.Exists(stockfishPath))
        {
            Debug.Log("Error: Stockfish executable not found at " + stockfishPath);
            return (0, 0);
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
        (int, int) bestMove = UciMoveToBitboardIndices(bestMoveStr);
        // (0,0) = short castle
        // (1,1) = long castle

        stockfish.Close();
        return bestMove;
    }
}
