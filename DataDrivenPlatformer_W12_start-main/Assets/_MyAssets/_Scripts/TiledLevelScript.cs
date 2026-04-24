using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

public class TiledLevelScript : MonoBehaviour
{
    [SerializeField] private Tilemap[] tileMaps;
    [SerializeField] private TileBase[] tileBases;
    [SerializeField] private char[] tileKeys;
    [SerializeField] private char[] tileObstacles;

    [Header("Spawned Objects")]
    [SerializeField] private GameObject checkpointParent;
    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private GameObject finishPrefab;
    [SerializeField] private GameObject ringParent;
    [SerializeField] private GameObject ringPrefab;

    private int rows; // Y-axis.

    private string levelText;
    private string tileDataText;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadLevel());
    }

    private IEnumerator LoadLevel()
    {
        LoadAndSortTileBases();

        yield return StartCoroutine(LoadTextFile("TileData.txt", result => tileDataText = result));

        if (string.IsNullOrEmpty(tileDataText))
        {
            Debug.LogError("TileData.txt could not be loaded.");
            yield break;
        }

        yield return StartCoroutine(LoadTextFile(LevelSettings.SelectedLevel, result => levelText = result));
        if (string.IsNullOrEmpty(levelText))
        {
            Debug.LogError("Level could not be loaded.");
            yield break;
        }

        try
        {
            string[] tileDataLines = SplitLines(tileDataText);

            if (tileDataLines.Length < 2)
            {
                throw new Exception("TileData.txt must contain at least two lines.");
            }

            string line = tileDataLines[0];
            tileKeys = line.ToCharArray();

            line = tileDataLines[1];
            tileObstacles = line.ToCharArray();

            GetRowsAndColumns();

            string[] levelLines = SplitLines(levelText);

            for (int row = 1; row < rows + 1; row++)
            {
                line = levelLines[row - 1];

                for (int col = 0; col < line.Length; col++)
                {
                    char c = line[col];

                    if (c == '*' || c == ' ') continue;
                    else if (c == 'C')
                    {
                        SpawnGameObject(checkpointPrefab, checkpointParent.transform, col, -row);
                        continue;
                    }
                    else if (c == 'F')
                    {
                        SpawnGameObject(finishPrefab, checkpointParent.transform, col, -row);
                        continue;
                    }
                    else if (c == '0')
                    {
                        SpawnGameObject(ringPrefab, ringParent.transform, col, -row);
                        continue;
                    }

                    int charIndex = Array.IndexOf(tileKeys, c);
                    if (charIndex == -1) throw new Exception("Index not found.");

                    if (Array.IndexOf(tileObstacles, c) > -1)
                    {
                        SetTile(0, charIndex, col, row);
                    }
                    else
                    {
                        SetTile(1, charIndex, col, row);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private IEnumerator LoadTextFile(string fileName, Action<string> onLoaded)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        using (UnityWebRequest req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to load " + fileName + ": " + req.error);
                onLoaded?.Invoke(null);
                yield break;
            }

            onLoaded?.Invoke(req.downloadHandler.text);
        }
    }

    private string[] SplitLines(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    private void SpawnGameObject(GameObject pf, Transform par, int c, int r)
    {
        GameObject inst = Instantiate(pf, new Vector3((c + 0.5f), (r + 0.5f), 0f), Quaternion.identity);
        inst.transform.parent = par.transform;
    }

    private void GetRowsAndColumns()
    {
        // Read all lines from the loaded level text.
        string[] lines = SplitLines(levelText);

        // Check if any lines were read.
        if (lines.Length == 0) return; // Early exit if no lines.

        rows = lines.Length; // Number of rows is number of elements, or lines in the file.
    }

    private void LoadAndSortTileBases()
    {
        tileBases = Resources.LoadAll<TileBase>("TileBases");
        // In Array.Sort, first param is what we're sorting, and second param is how we're sorting.
        Array.Sort(tileBases, (x, y) => ExtractNumber(x.name).CompareTo( ExtractNumber(y.name) ) );
    }

    private int ExtractNumber(string name) // name is the TileBase name. "Tiles_10"
    {
        return Int32.Parse(new string(name.Where(Char.IsDigit).ToArray()));
    }

    private void SetTile(int tileMapIndex, int charIndex, int col, int row)
    {
        // Check all tilemaps to see if there's a manually-painted tile there.
        foreach (Tilemap tilemap in tileMaps)
        {
            if (tilemap.HasTile(new Vector3Int(col, -row, 0))) return;
        }
        // If no tile, then set the tile in the desired tilemap.
        tileMaps[tileMapIndex].SetTile(new Vector3Int(col, -row, 0), tileBases[charIndex]);
    }
}
