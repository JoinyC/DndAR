using UnityEngine;

public class UIManager : MonoBehaviour
{
    public MainController Main;

    public void StartButton()
    {
        Main.ToScan();
    }
}