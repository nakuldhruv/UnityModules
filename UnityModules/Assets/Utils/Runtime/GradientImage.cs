using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Utils
{
    [AddComponentMenu("UI/Gradient Image")]
    public class GradientImage : Image
    {
        public enum GradientMode
        {
            Horizontal, // 水平渐变
            Vertical,   // 垂直渐变
            Corners     // 四角渐变
        }

        public GradientMode mode = GradientMode.Vertical;

        [Header("Colors")]
        public Color colorTopLeft = Color.white;
        public Color colorTopRight = Color.white;
        public Color colorBottomLeft = Color.black;
        public Color colorBottomRight = Color.black;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // 1. 调用父类方法生成基础的网格（支持 Sprite、Sliced 等）
            base.OnPopulateMesh(vh);

            if (vh.currentVertCount == 0) return;

            // 2. 获取 Rect 范围以便计算顶点在图片中的相对比例 (0~1)
            Rect rect = GetPixelAdjustedRect();

            UIVertex vertex = default;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);

                // 计算顶点在矩形中的归一化位置 (0 到 1)
                float x = (vertex.position.x - rect.xMin) / rect.width;
                float y = (vertex.position.y - rect.yMin) / rect.height;

                // 3. 根据模式计算颜色
                switch (mode)
                {
                    case GradientMode.Horizontal:
                        vertex.color = Color.Lerp(colorTopLeft, colorTopRight, x);
                        break;
                    case GradientMode.Vertical:
                        vertex.color = Color.Lerp(colorBottomLeft, colorTopLeft, y);
                        break;
                    case GradientMode.Corners:
                        // 四角插值：先插值上下两边，再根据垂直位置插值
                        Color colorTop = Color.Lerp(colorTopLeft, colorTopRight, x);
                        Color colorBottom = Color.Lerp(colorBottomLeft, colorBottomRight, x);
                        vertex.color = Color.Lerp(colorBottom, colorTop, y);
                        break;
                }

                // 4. 应用修改后的顶点信息
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}