using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityModules.Zxs.UI
{
    public interface IUIViewOwner
    {
        void HideView(UIView instance);
        void PopPopup();
    }
    
    public class UIManager : MonoBehaviour, IUIViewOwner
    {
        private static UIManager _instance;
        public static  UIManager Instance => _instance;

        [SerializeField] private Transform _topRoot;
        [SerializeField] private Transform _middleRoot;
        [SerializeField] private Transform _bottomRoot;

        private List<UIView> _activeViews = new();
        private List<UIView> _popupStack  = new();

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

        public void ShowView(UIView prefab, UIViewLayer layer)
        {
            if (prefab == null) return;
            UIView instance = Instantiate(prefab, GetParent(layer));
            instance.Owner = this;
            instance.OnShow();
            _activeViews.Add(instance);
        }

        public void HideView(UIView instance)
        {
            if (instance == null) return;
            if (_activeViews.Contains(instance))
            {
                instance.OnHide();
                _activeViews.Remove(instance);
                Destroy(instance.gameObject);
            }
        }

        public void PushPopup(UIView prefab, UIViewLayer layer)
        {
            if (prefab == null) return;
            UIView instance = Instantiate(prefab, GetParent(layer));
            instance.Owner = this;
            instance.OnShow();
            _popupStack.Add(instance);
        }

        public void PopPopup()
        {
            if (_popupStack.Count == 0) return;
            int    index    = _popupStack.Count - 1;
            UIView instance = _popupStack[index];
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