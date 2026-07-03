using UnityEngine;

namespace UnityModules.Zxs.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public class ViewBase : MonoBehaviour
    {
        public CanvasGroup   canvasGroup;
        public RectTransform rectTransform;
        public IUIViewOwner  Owner;

        public virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public void HideView()
        {
            Owner.HideView(this);
        }

        public void PopPopup()
        {
            Owner.PopPopup();
        }
    }
}