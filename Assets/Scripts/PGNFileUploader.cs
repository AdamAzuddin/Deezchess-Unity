using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Networking;
using System.IO;

[RequireComponent(typeof(Button))]
public class PGNFileUploader : MonoBehaviour, IPointerDownHandler
{
    public Text output;
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".pgn", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(ReadAndUploadFile(url));
    }
#else
    public void OnPointerDown(PointerEventData eventData) { }

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select PGN File", "", "pgn", false);
        if (paths.Length > 0)
        {
            string path = paths[0];

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                output.text = "File too large! Maximum size is 5MB.";
                return;
            }

            StartCoroutine(ReadAndUploadFile("file://" + path, path));
        }
    }
#endif

    private IEnumerator ReadAndUploadFile(string fileUrl, string filePath = null)
    {
        UnityWebRequest fileRequest = UnityWebRequest.Get(fileUrl);
        yield return fileRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (fileRequest.result != UnityWebRequest.Result.Success)
#else
    if (fileRequest.isNetworkError || fileRequest.isHttpError)
#endif
        {
            output.text = "Failed to read file: " + fileRequest.error;
            yield break;
        }

        byte[] fileData = fileRequest.downloadHandler.data;

        if (fileData.Length > MaxFileSizeBytes)
        {
            output.text = "File too large! Maximum size is 5MB.";
            yield break;
        }

        output.text = "Uploading...";

        // 👇 Create proper multipart form
        WWWForm form = new WWWForm();

        string fileName = filePath != null ? Path.GetFileName(filePath) : "uploaded.pgn";
        form.AddBinaryData("file", fileData, fileName, "text/plain");

        UnityWebRequest uploadRequest = UnityWebRequest.Post("https://deezchess-api.onrender.com/pgn_upload", form);

        yield return uploadRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (uploadRequest.result != UnityWebRequest.Result.Success)
#else
    if (uploadRequest.isNetworkError || uploadRequest.isHttpError)
#endif
        {
            output.text = "Upload Failed: " + uploadRequest.error;
            Debug.Log("Upload Failed: " + uploadRequest.error);
        }
        else
        {
            output.text = "Upload Success: " + uploadRequest.downloadHandler.text;
            Debug.Log("Upload Success: " + uploadRequest.downloadHandler.text);
        }
    }

}
