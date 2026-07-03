using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zxs.UI
{
    public interface IUIViewOwner
    {
        void HideView(ViewBase instance);
        void PopPopup();
    }
    
    public class UIManager : MonoBehaviour, IUIViewOwner
    {
        private static UIManager _instance;
        public static  UIManager Instance => _instance;

        [SerializeField] private Transform _topRoot;
        [SerializeField] private Transform _middleRoot;
        [SerializeField] private Transform _bottomRoot;

        private List<ViewBase> _activeViews = new();
        private List<ViewBase> _popupStack  = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public void ShowView(ViewBase prefab, UIViewLayer layer)
        {
            if (prefab == null) return;
            ViewBase instance = Instantiate(prefab, GetParent(layer));
            instance.Owner = this;
            instance.OnShow();
            _activeViews.Add(instance);
        }

        public void HideView(ViewBase instance)
        {
            if (instance == null) return;
            if (_activeViews.Contains(instance))
            {
                instance.OnHide();
                _activeViews.Remove(instance);
                Destroy(instance.gameObject);
            }
        }

        public void PushPopup(ViewBase prefab, UIViewLayer layer)
        {
            if (prefab == null) return;
            ViewBase instance = Instantiate(prefab, GetParent(layer));
            instance.Owner = this;
            instance.OnShow();
            _popupStack.Add(instance);
        }

        public void PopPopup()
        {
            if (_popupStack.Count == 0) return;
            int    index    = _popupStack.Count - 1;
            ViewBase instance = _popupStack[index];
            _popupStack.RemoveAt(index);
            instance.OnHide();
            Destroy(instance.gameObject);
        }

        private Transform GetParent(UIViewLayer layer)
        {
            return layer switch
            {
                UIViewLayer.Bottom => _bottomRoot,
                UIViewLayer.Middle => _middleRoot,
                UIViewLayer.Top    => _topRoot,
                _                  => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
            };
        }
    }

    public enum UIViewLayer
    {
        Bottom,
        Middle,
        Top
    }
}