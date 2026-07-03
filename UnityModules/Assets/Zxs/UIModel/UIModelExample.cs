using System.Collections.Generic;
using UnityEngine;

namespace Zxs.Extension
{
    public class UIModelExample : MonoBehaviour
    {
        [SerializeField] private UIModelRenderer _modelRenderer;

        private void Start()
        {
            var models = new List<UIModel>()
            {
                new UIModel()
                {
                    Name = "Model_Cube",
                    Rotation = new Vector3(0f, 30f, 0f),
                }
            };
            _modelRenderer.SetModels(models);
        }
    }
}