Canvas Sort Order
第一优先级（跨 Canvas 全局）：Sort Order or Order in Layer
第二优先级（同 Canvas 内部）：Hierarchy 窗口中的上下顺序
第三优先级（特殊渲染模式）：摄像机 Depth（深度）
Inspector 顶部的 Layer “物理层”
负责：摄像机剔除（Culling Mask）物理碰撞（Physics Collision）射线检测（Raycast）

---------------------------------------------------------------------------------------------------------

Mask
Mask 组件必须依附于 Image 组件才能生效。其裁剪行为取决于 Image 是否挂载了图片：
无图片时：默认以 Image 的 矩形边界（Rect Transform） 作为裁剪区域。
有图片时：以图片的 Alpha 通道 作为裁剪形状（透明部分被裁掉，不透明部分保留）。
无论哪种情况，Image.color.a（透明度）仅控制 Image 自身显示，不影响 Mask 对子物体的裁剪结果。