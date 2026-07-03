using UnityEngine;
using UnityEngine.UI;

namespace Zxs.Extension
{
    public class UIScreenAdapter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private CanvasScaler  _canvasScaler;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasScaler  = FindObjectOfType<CanvasScaler>();
            float scaledWidth  = Screen.width  / _canvasScaler.scaleFactor;
            float scaledHeight = Screen.height / _canvasScaler.scaleFactor;
            _rectTransform.sizeDelta = new Vector2(scaledWidth, scaledHeight);
        }
    }
}