using System.Collections.Generic;
using UnityEngine;

public class ScannedMarker : MonoBehaviour
{
    public List<Transform> DMSpawnPositions;
    public List<Transform> PlayerSpawnPositions;
    public List<Character> SpawnedCharacterList;
    public GameObject CharParent;
    public UIManager UIManager;
    private MainController MainController;

    private int DMSpawnCounter = 0;
    private int PlayerSpawnCounter = 0;
    private bool firstTime = true;

    private void Awake()
    {
        SpawnedCharacterList = new();
        MainController = FindObjectOfType<MainController>();
    }

    private void OnEnable()
    {
        if (!firstTime)
        {
            UIManager.DmOverlay.SetActive(true);
            UIManager.OpenDmPanel(); 
            MainController.ScanCanvas.SetActive(false);
        }
        else
        {
            firstTime = false;
        }
    }

    public Transform GetNextSpawnPosition(bool player)
    {
        if (DMSpawnCounter >= DMSpawnPositions.Count)
            DMSpawnCounter = 0;

        if (PlayerSpawnCounter >= PlayerSpawnPositions.Count)
            PlayerSpawnCounter = 0;

        if (player)
            return PlayerSpawnPositions[PlayerSpawnCounter++];
        else
            return DMSpawnPositions[DMSpawnCounter++];
    }

    public void AddCharToList(Character c)
    {
        SpawnedCharacterList.Add(c);
        c.gameObject.transform.SetParent(CharParent.transform);
    }

    public void MoveChar(Character c, Vector3 pos)
    {

    }
}