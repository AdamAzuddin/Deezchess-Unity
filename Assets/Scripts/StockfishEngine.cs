using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;


public class StockfishEngine
{

    public IEnumerator GetBestMove(string fen, string binFilePath, string configFilePath, System.Action<string> callback)
    {
        string url = "http://127.0.0.1:8000/get_bot_move";

        if (!File.Exists(binFilePath) || !File.Exists(configFilePath))
        {
            Debug.LogError("Missing bin or config file.");
            callback("");
            yield break;
        }

        // Read binary files into byte arrays
        byte[] binData = File.ReadAllBytes(binFilePath);
        byte[] configData = File.ReadAllBytes(configFilePath);

        // Build multipart form
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("bin_file", binData, Path.GetFileName(binFilePath), "application/octet-stream"),
            new MultipartFormFileSection("config_file", configData, Path.GetFileName(configFilePath), "text/plain"),
            new MultipartFormDataSection("fen", fen)
        };

        UnityWebRequest request = UnityWebRequest.Post(url, formData);

        // Send request
        yield return request.SendWebRequest();

        // Handle response
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            UciMoveResponse response = JsonUtility.FromJson<UciMoveResponse>(responseText);
            Debug.Log("Best move from server: " + response.uci_move);
            callback(response.uci_move);
        }
        else
        {
            Debug.LogError("Request failed: " + request.error);
            callback("");
        }
    }

    // JSON structure for response
    [System.Serializable]
    public class UciMoveResponse
    {
        public string uci_move;
    }
}
