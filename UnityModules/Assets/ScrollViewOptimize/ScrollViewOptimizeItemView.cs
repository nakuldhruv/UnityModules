using UnityEngine;
using UnityEngine.UI;

public class ScrollViewOptimizeItemView : MonoBehaviour
{
    public Text NameText;

    public void Refresh(string name)
    {
        this.NameText.text = name;
    }
}