using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public MainController Main;
    public GameObject SpawnPanel;
    public GameObject EffectPanel;
    public GameObject DmOverlay;
    public GameObject Buttons;
    private Char? ToSpawn; //for spawning
    private Char Selected; // for effects
    private Char Target; // for effects

    public void SetButtons(bool on)
    {
        Buttons.SetActive(on);
    }

    public void CloseEffectPanel()
    {
        EffectPanel.SetActive(false);
        SetButtons(true);
    }

    public void OpenEffectPanel()
    {
        EffectPanel.SetActive(true);
        SpawnPanel.SetActive(false);
        SetButtons(false);
    }

    public void CloseDmPanel()
    {
        SpawnPanel.SetActive(false);
        SetButtons(true);
    }

    public void OpenDmPanel()
    {
        SpawnPanel.SetActive(true);
        EffectPanel.SetActive(false);
        SetButtons(false);
    }

    public void ChooseCharToSpawn(string s)
    {
        if (s == "elf")
            ToSpawn = Char.Elf;

        if (s == "goblin")
            ToSpawn = Char.Goblin;

        if (s == "tiefling")
            ToSpawn = Char.Tiefling;
    }

    public void SpawnCharToBoard()
    {
        bool b;

        if (ToSpawn == Char.Goblin)
            b = false;
        else
            b = true;

        Transform t = Main.ScannedMarker.GetNextSpawnPosition(b);
        Main.SpawnCharOnBoard(ToSpawn.GetValueOrDefault(), t, b);
        ToSpawn = null;
    }

    public void StartButton()
    {
        Main.ToJoinSession();
    }

    public void ToScan()
    {
        Main.ToScan();
    }
}