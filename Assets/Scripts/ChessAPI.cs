using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

public class ChessAPI : MonoBehaviour
{
    private const string apiUrl = "http://127.0.0.1:8000/legal_moves"; // Change if deployed

    public IEnumerator GetLegalMovesFromIndex(int currentIndex, string fen, System.Action<List<int>> onResult)
    {
        List<int> legalMovesTo = new List<int>();

        // Construct the request body
        var requestBody = new { fen = fen };
        string jsonBody = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get legal moves: {request.error}");
                onResult?.Invoke(legalMovesTo); // return empty list
            }
            else
            {
                // Parse response
                var response = JsonConvert.DeserializeObject<LegalMovesResponse>(request.downloadHandler.text);
                foreach (var move in response.legal_moves_bitboard)
                {
                    int from = move[0];
                    int to = move[1];

                    if (from == currentIndex)
                    {
                        legalMovesTo.Add(to);
                    }
                }

                Debug.Log($"There are {legalMovesTo.Count} legal moves from index {currentIndex} for fen: {fen}");
                onResult?.Invoke(legalMovesTo);
            }
        }
    }

    // Helper class for JSON deserialization
    [System.Serializable]
    public class LegalMovesResponse
    {
        public List<List<int>> legal_moves_bitboard;
    }


    public IEnumerator GetTotalLegalMovesCount(string fen, System.Action<int> onResult)
    {
        var requestBody = new { fen = fen };
        string jsonBody = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to get legal moves: {request.error}");
                onResult?.Invoke(0); // fallback
            }
            else
            {
                var response = JsonConvert.DeserializeObject<LegalMovesResponse>(request.downloadHandler.text);
                onResult?.Invoke(response.legal_moves_bitboard.Count);
            }
        }
    }

}
