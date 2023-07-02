using Auki.ConjureKit;
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
    public GameObject JoinSessionCanvas;
    public GameObject ScanCanvas;
    public GameObject LiveCanvas;
    public ConjureScript Conjurer;
    public ScannedMarker ScannedMarker;

    public List<Character> SpawnedChars;

    private void Awake()
    {
        AppState = AppState.Setup;
        Setup();
    }

    private void Setup()
    {
        SetupCanvas.SetActive(true);
        JoinSessionCanvas.SetActive(false);
        ScanCanvas.SetActive(false);
        LiveCanvas.SetActive(false);
        SpawnedChars = new List<Character>();
    }

    public void ToJoinSession()
    {
        SetupCanvas.SetActive(false);
        JoinSessionCanvas.SetActive(true);
    }

    public void ToScan()
    {
        JoinSessionCanvas.SetActive(false);
        ScanCanvas.SetActive(true);
    }

    public void SpawnCharOnBoard(Char c, Transform pos, bool player)
    {
        if (ScanCanvas.activeInHierarchy)
            ScanCanvas.SetActive(false);

        var prefab = CharacterPrefabs.Where(x => x.GetComponent<Character>().ThisChar == c).FirstOrDefault();

        if (player)
        {
            var t = Instantiate(prefab, pos.position, pos.rotation);
            ScannedMarker.AddCharToList(t.GetComponent<Character>());
        }
        else
        {
            var t = Instantiate(prefab, pos.position, Quaternion.identity);
            t.transform.rotation = new Quaternion(0, 180, 0, 0);
            ScannedMarker.AddCharToList(t.GetComponent<Character>());
        }
    }

    public void SpawnChar(Entity entity, Transform parent, Char c)
    {
        if (ScanCanvas.activeInHierarchy)
            ScanCanvas.SetActive(false);

        var prefab = CharacterPrefabs.Where(x => x.GetComponent<Character>().ThisChar == c).FirstOrDefault();
        var t = Instantiate(prefab, parent);

        if (t != null)
            SpawnedChars.Add(prefab.GetComponent<Character>());

        if (!LiveCanvas.activeInHierarchy)
            LiveCanvas.SetActive(true);
    }
}