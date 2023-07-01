using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Char
{
    Goblin, Elf, Tiefling
}

public enum AppState
{
    Setup, PreScan, Live
}

public class MainController : MonoBehaviour
{
    public List<GameObject> CharacterPrefabs;
    public AppState AppState;
    public GameObject SetupCanvas;
    public GameObject ScanCanvas;
    public GameObject LiveCanvas;

    private List<Character> SpawnedChars;

    private void Awake()
    {
        AppState = AppState.Setup;
        Setup();
    }

    private void Setup()
    {
        SetupCanvas.SetActive(true);
        ScanCanvas.SetActive(false);
        LiveCanvas.SetActive(false);
        SpawnedChars = new List<Character>();
    }

    public void ToScan()
    {
        SetupCanvas.SetActive(false);
        ScanCanvas.SetActive(true);
    }

    public void SpawnChar(Transform parent, Char c)
    {
        if(ScanCanvas.activeInHierarchy)
            ScanCanvas.SetActive(false);

        var prefab = CharacterPrefabs.Where(x => x.GetComponent<Character>().ThisChar == c).FirstOrDefault();
        var t = Instantiate(prefab, parent);

        if(t != null)
            SpawnedChars.Add(prefab.GetComponent<Character>());

        if (!LiveCanvas.activeInHierarchy)
            LiveCanvas.SetActive(true);
    }
}