using UnityEngine;
using UnityEngine.UI;

namespace UnityModules
{
    public class ScrollViewOptimizeItemView : MonoBehaviour
    {
        public Text NameText;

        public void Refresh(string name)
        {
            this.NameText.text = name;
        }
    }
}