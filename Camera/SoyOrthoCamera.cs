using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//not used THOUGH - don't bother
public class SoyOrthoCamera : MonoBehaviour
{
    private float targetAspectRatio => 9f / 16f;
    private float currentAspectRatio;
    private Vector2 resolution;

    private void OnEnable()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        UpdateOrthoCameraRect();
    }
 
    private void Update ()
    {
        if (Screen.width != resolution.x || Screen.height != resolution.y)
        {
            Debug.Log("Change in resolution detected");
            UpdateOrthoCameraRect();
        }
    }

    private void UpdateOrthoCameraRect()
    {
        // Debug.Log("Updating Orthographic size");
        resolution.x = Screen.width;
        resolution.y = Screen.height;
        currentAspectRatio = resolution.x / resolution.y;

        float scaleheight = currentAspectRatio / targetAspectRatio;

        if (scaleheight < 1f)
        {
            GetComponent<Camera>().rect = new Rect(0f, (1f - scaleheight) / 2f, 1f, scaleheight);
        }
        else
        {
            float scalewidth = 1f / scaleheight;
            GetComponent<Camera>().rect = new Rect((1f - scalewidth) / 2f, 0f, scalewidth, 1f);
        }

       GetComponent<Camera>().orthographicSize = 5f;
    }
}
