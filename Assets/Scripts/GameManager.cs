using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SFB;
using System.IO;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class GameManager : MonoBehaviour
{
    public bool isWhiteToMove = true;
    public bool whiteCanShortCastle = true;
    public bool whiteCanLongCastle = true;
    public bool blackCanShortCastle = true;
    public bool blackCanLongCastle = true;
    public GameObject gameOverPopup;
    public GameObject pawnPromotionPopup;
    public GameObject NoConnectionPopup;
    public GameObject resultTextGameObject;
    public GameObject PGNUploadPopup;

    public GameObject promotionRook;
    public GameObject promotionKnight;
    public GameObject promotionBishop;
    public GameObject promotionQueen;
    public Sprite wR, wN, wB, wQ;
    public Sprite bR, bN, bB, bQ;

    public Camera mainCamera;

    private Text resultText;

    private Dictionary<int, Square> squareDictionary;
    private int piecesLayer;

    public int pawnPromotionSquareIndex;

    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private Text uploadOutputText;
    private List<GameObject> hiddenPieces = new List<GameObject>();
    private List<GameObject> hiddenSquares = new List<GameObject>();
    public InputField playerNameInputField;
    public Button uploadButton;

    protected string botPath;

    void Start()
    {
        // ✅ Check if API server is connected
        StartCoroutine(CheckServerAvailability(isConnected =>
        {
            if (!isConnected)
            {
                ShowNoConnectionPopup();
                return;
            }

            // ✅ Server is connected — continue initializing
            GameObject[] allSquareObjects = GameObject.FindGameObjectsWithTag("Square");
            piecesLayer = LayerMask.NameToLayer("Pieces");
            squareDictionary = new Dictionary<int, Square>();

            if (resultTextGameObject != null)
            {
                resultText = resultTextGameObject.GetComponent<Text>();
            }

            foreach (GameObject squareObject in allSquareObjects)
            {
                Square square = squareObject.GetComponent<Square>();
                if (square != null)
                {
                    squareDictionary[square.index] = square;
                }
            }
            Debug.Log("Game initialized successfully.");
        }));

        playerNameInputField.onValueChanged.AddListener(ValidatePlayerName);
        ValidatePlayerName(playerNameInputField.text); // Initial check
    }

    private void ValidatePlayerName(string value)
    {
        bool isValid = !string.IsNullOrWhiteSpace(value);

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL-specific workaround
    uploadButton.interactable = isValid;

    var group = uploadButton.GetComponent<CanvasGroup>() ?? uploadButton.gameObject.AddComponent<CanvasGroup>();
    group.blocksRaycasts = isValid;
#else
        // Normal behavior for other platforms
        uploadButton.interactable = isValid;
#endif
    }
    public void TestClick()
    {
        Debug.Log("Clicked");
    }

    public void ShowNoConnectionPopup()
    {
        Piece[] pieces = FindObjectsOfType<Piece>();
        foreach (Piece piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        Square[] squares = FindObjectsOfType<Square>();
        foreach (Square square in squares)
        {
            Destroy(square.gameObject);
        }
        NoConnectionPopup.SetActive(true);
    }

    public void RetryConnection()
    {
        Debug.Log("Retrying connection...");
        StartCoroutine(CheckServerAvailability(isConnected =>
        {
            if (!isConnected)
            {
                Debug.Log("Still no connection.");
            }
            else
            {
                Debug.Log("Server connected after retry. Restarting...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }));
    }


    IEnumerator CheckServerAvailability(System.Action<bool> onResult)
    {
        UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:8000/health");
        request.timeout = 5;

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.Log("Server not reachable: " + request.error);
            onResult?.Invoke(false);
        }
        else
        {
            Debug.Log("Server is reachable!");
            onResult?.Invoke(true);
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void UploadPGNFile(Text outputText)
    {
        uploadOutputText = outputText;
        UploadFile(gameObject.name, "OnFileUpload", ".pgn", false);
    }

    // Called from browser
    public void OnFileUpload(string url)
    {
        StartCoroutine(ReadAndUploadFile(url));
    }

#else
    public void UploadPGNFile(Text outputText)
    {
        uploadOutputText = outputText;
        var paths = StandaloneFileBrowser.OpenFilePanel("Select PGN File", "", "pgn", false);
        if (paths.Length > 0)
        {
            string path = paths[0];

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                uploadOutputText.text = "File too large! Maximum size is 5MB.";
                return;
            }

            StartCoroutine(ReadAndUploadFile("file://" + path, path));
        }
    }
#endif

    private IEnumerator ReadAndUploadFile(string fileUrl, string filePath = null)
    {

        string playerName = playerNameInputField.text;
        Debug.Log("Creating bot for " + playerName);
        UnityWebRequest fileRequest = UnityWebRequest.Get(fileUrl);
        yield return fileRequest.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
        if (fileRequest.result != UnityWebRequest.Result.Success)
#else
    if (fileRequest.isNetworkError || fileRequest.isHttpError)
#endif
        {
            uploadOutputText.text = "Failed to read file: " + fileRequest.error;
            yield break;
        }

        byte[] fileData = fileRequest.downloadHandler.data;

        if (fileData.Length > MaxFileSizeBytes)
        {
            uploadOutputText.text = "File too large! Maximum size is 5MB.";
            yield break;
        }

        uploadOutputText.text = "Uploading...";

        WWWForm form = new WWWForm();
        string fileName = filePath != null ? Path.GetFileName(filePath) : "uploaded.pgn";
        form.AddField("playerName", playerName);
        form.AddBinaryData("file", fileData, fileName, "text/plain");

        UnityWebRequest uploadRequest = UnityWebRequest.Post("http://localhost:8000/pgn_upload", form);
        uploadRequest.downloadHandler = new DownloadHandlerBuffer(); // Ensure we get full binary response
        yield return uploadRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (uploadRequest.result != UnityWebRequest.Result.Success)
#else
    if (uploadRequest.isNetworkError || uploadRequest.isHttpError)
#endif
        {
            uploadOutputText.text = "Upload Failed: " + uploadRequest.error;
            Debug.Log("Upload Failed: " + uploadRequest.error);
        }
        else
        {
            byte[] zipFileData = uploadRequest.downloadHandler.data;

            // Save received .zip file
            string zipSavePath = Path.Combine(Application.persistentDataPath, "bot.zip");
            File.WriteAllBytes(zipSavePath, zipFileData);

            uploadOutputText.text = "Upload Success. Zip file saved at: " + zipSavePath;
            Debug.Log("Upload Success. Zip file saved at: " + zipSavePath);

            // Extract zip file
            string extractPath = Path.Combine(Application.persistentDataPath, "bot_output");

            // Ensure directory exists
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            // Extract ZIP
            ZipFile.ExtractToDirectory(zipSavePath, extractPath, true);

            Debug.Log("Zip extracted to: " + extractPath);
            uploadOutputText.text += "\nZip extracted to: " + extractPath;
            botPath = extractPath;
            File.Delete(zipSavePath);
        }
    }

    public Square GetSquareByIndex(int targetIndex)
    {
        if (squareDictionary.TryGetValue(targetIndex, out Square square))
        {
            return square;
        }

        Debug.LogError("Square with index " + targetIndex + " not found!");
        return null;
    }

    public void ShowGameOver(string text)
    {
        Piece[] pieces = FindObjectsOfType<Piece>();
        foreach (Piece piece in pieces)
        {
            Destroy(piece.gameObject);
        }
        Square[] squares = FindObjectsOfType<Square>();
        foreach (Square square in squares)
        {
            Destroy(square.gameObject);
        }
        if (resultText != null)
        {
            resultText.text = text;
        }
        gameOverPopup.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPopup.SetActive(false);
    }



    public void ShowPGNUploaderPopup()
    {
        hiddenPieces.Clear();
        hiddenSquares.Clear();

        Piece[] pieces = FindObjectsOfType<Piece>();
        foreach (Piece piece in pieces)
        {
            hiddenPieces.Add(piece.gameObject);
            piece.gameObject.SetActive(false);
        }

        Square[] squares = FindObjectsOfType<Square>();
        foreach (Square square in squares)
        {
            hiddenSquares.Add(square.gameObject);
            square.gameObject.SetActive(false);
        }
        PGNUploadPopup.SetActive(true);
    }

    public void HidePGNUploaderPopup()
    {
        foreach (GameObject piece in hiddenPieces)
        {
            if (piece != null) piece.SetActive(true);
        }

        foreach (GameObject square in hiddenSquares)
        {
            if (square != null) square.SetActive(true);
        }

        PGNUploadPopup.SetActive(false);

        // Optional: clear again if you don’t plan to reuse lists
        hiddenPieces.Clear();
        hiddenSquares.Clear();
    }



    public void ShowPawnPromotionPopup(Piece.PieceColor color)
    {

        if (color == Piece.PieceColor.White)
        {
            promotionRook.GetComponent<Button>().image.sprite = wR;
            promotionKnight.GetComponent<Button>().image.sprite = wN;
            promotionBishop.GetComponent<Button>().image.sprite = wB;
            promotionQueen.GetComponent<Button>().image.sprite = wQ;
        }
        else
        {
            promotionRook.GetComponent<Button>().image.sprite = bR;
            promotionRook.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionKnight.GetComponent<Button>().image.sprite = bN;
            promotionKnight.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionBishop.GetComponent<Button>().image.sprite = bB;
            promotionBishop.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
            promotionQueen.GetComponent<Button>().image.sprite = bQ;
            promotionQueen.GetComponent<PieceToPromoteWithButton>().pieceColor = Piece.PieceColor.Black;
        }

        // Show the popup
        pawnPromotionPopup.SetActive(true);

        // Hide the "Pieces" layer from the main camera
        mainCamera.cullingMask &= ~(1 << piecesLayer);
    }

    public void HidePawnPromotionPopup()
    {
        // Show the "Pieces" layer again in the main camera
        mainCamera.cullingMask |= 1 << piecesLayer;
        pawnPromotionPopup.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        NoConnectionPopup.SetActive(false);
        HidePawnPromotionPopup();
        HideGameOver();
    }

}
