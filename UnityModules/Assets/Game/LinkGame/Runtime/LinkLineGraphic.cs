using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nakul.LinkGame
{
    /// <summary>
    /// 用于绘制连连看连接线的 UI 图形组件。
    /// 通过 OnPopulateMesh 生成带厚度的折线网格，并支持按进度渐进绘制（连线动画）。
    /// </summary>
    public class LinkLineGraphic : MaskableGraphic
    {
        public float Thickness { get; set; } = 10f;

        private readonly List<Vector2> _points = new List<Vector2>();
        private bool _visible;
        private float _progress = 1f;

        public void Show(List<Vector2> points)
        {
            _points.Clear();
            if (points != null)
            {
                _points.AddRange(points);
            }

            _visible = true;
            _progress = 1f;

            // 显示前确保 GameObject 处于激活状态，避免上次隐藏后残留禁用状态
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            SetVerticesDirty();
        }

        public void Hide()
        {
            _visible = false;
            _points.Clear();
            _progress = 0f;

            // 立即清空渲染几何，避免 Canvas 延迟重建导致连线残影
            if (canvasRenderer != null)
            {
                canvasRenderer.Clear();
            }

            SetVerticesDirty();

            // 禁用整个 GameObject，彻底阻止任何延迟重建把旧几何重新画出来
            gameObject.SetActive(false);
        }




        /// <summary>设置连线绘制进度（0~1），用于连线动画。</summary>
        public void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!_visible || _points.Count < 2 || _progress <= 0f)
            {
                return;
            }

            float halfThickness = Mathf.Max(0f, Thickness) * 0.5f;

            float totalLength = 0f;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                totalLength += (_points[i + 1] - _points[i]).magnitude;
            }

            if (totalLength <= 0f)
            {
                return;
            }

            float targetLength = totalLength * _progress;
            float accumulated = 0f;

            for (int i = 0; i < _points.Count - 1; i++)
            {
                Vector2 a = _points[i];
                Vector2 b = _points[i + 1];
                float segLen = (b - a).magnitude;
                if (segLen <= 0f)
                {
                    continue;
                }

                if (accumulated >= targetLength)
                {
                    break;
                }

                Vector2 end = b;
                if (accumulated + segLen > targetLength)
                {
                    float t = (targetLength - accumulated) / segLen;
                    end = Vector2.Lerp(a, b, t);
                }

                Vector2 dir = (end - a).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x) * halfThickness;

                Vector2 p0 = a - normal;
                Vector2 p1 = a + normal;
                Vector2 p2 = end + normal;
                Vector2 p3 = end - normal;

                int baseIndex = vh.currentVertCount;
                vh.AddVert(p0, color, Vector2.zero);
                vh.AddVert(p1, color, Vector2.zero);
                vh.AddVert(p2, color, Vector2.zero);
                vh.AddVert(p3, color, Vector2.zero);

                vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);

                accumulated += segLen;
            }
        }
    }
}
