using UnityEngine;


//not used THOUGH - nobody needs this
public class RenderTextureScaler : MonoBehaviour
{
    public float targetAspect = 9f / 16f; // Portrait aspect
    public Camera referenceCamera;

    private float lastCamAspect = -1f;

    void LateUpdate()
    {
        if (referenceCamera == null)
        {
            Debug.LogWarning("Reference Camera not assigned.");
            return;
        }

        float currentAspect = referenceCamera.aspect;

        if (Mathf.Approximately(currentAspect, lastCamAspect))
            return; // Skip if no change

        lastCamAspect = currentAspect;

        float camHeight = referenceCamera.orthographicSize * 2f;
        float camWidth = camHeight * currentAspect;

        float fitWidth = camHeight * targetAspect;
        float fitHeight = camWidth / targetAspect;

        float finalWidth, finalHeight;

        if (fitWidth <= camWidth)
        {
            finalWidth = fitWidth;
            finalHeight = camHeight;
        }
        else
        {
            finalWidth = camWidth;
            finalHeight = fitHeight;
        }

        transform.localScale = new Vector3(finalWidth, finalHeight, 1f);
    }
}
