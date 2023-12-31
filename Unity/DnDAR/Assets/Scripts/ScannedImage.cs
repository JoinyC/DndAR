using UnityEngine;

public class ScannedImage : MonoBehaviour
{
    public Char ThisChar;
    private bool firstTime = true;
    public ConjureScript conjurer;

    private void OnEnable()
    {
        if (!firstTime)
        {
            conjurer.AddCharEntity(transform.parent, ThisChar);
        }
        else
        {
            firstTime = false;
        }
    }
}