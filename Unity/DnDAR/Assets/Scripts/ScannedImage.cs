using UnityEngine;

public class ScannedImage : MonoBehaviour
{
    public Char ThisChar;
    private bool firstTime = true;

    private void OnEnable()
    {
        if (!firstTime)
        {
            var manager = FindObjectOfType<MainController>();
            manager.SpawnChar(transform.parent, ThisChar);
        }
        else
        {
            firstTime = false;
        }
    }
}