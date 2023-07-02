using UnityEngine;

public class UIManager : MonoBehaviour
{
    public MainController Main;

    public void StartButton()
    {
        Main.ToJoinSession();
    }

    public void ToScan()
    {
        Main.ToScan();
    }

}