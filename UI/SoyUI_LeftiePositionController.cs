using UnityEngine;

public class SoyUI_LeftiePositionController : MonoBehaviour
{
    void OnEnable()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (ToweraldStatic.isLeftie)
        {
            rt.anchoredPosition = new Vector2(-rt.anchoredPosition.x, rt.anchoredPosition.y);
        }
    }

}
