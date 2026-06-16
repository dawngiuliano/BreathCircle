using BreathCircle.Helpers;

namespace BreathCircle.Services
{
    /// <summary>
    /// 动画服务 — 计算圆的缩放值和发光值
    /// </summary>
    public class AnimationService
    {
        /// <summary>最小圆尺寸（90/150 = 0.6）</summary>
        public const double MinCircleRatio = 0.6;

        /// <summary>最大圆尺寸（150/150 = 1.0）</summary>
        public const double MaxCircleRatio = 1.0;

        /// <summary>当前圆缩放比例 [0.5, 1.0]</summary>
        public double CircleScale { get; private set; } = MinCircleRatio;

        /// <summary>发光透明度 [0, 1]</summary>
        public double GlowOpacity { get; private set; } = 0;

        /// <summary>
        /// 根据呼吸进度计算缩放值
        /// </summary>
        /// <param name="progress">进度 [0, 1]</param>
        /// <param name="growing">true=从小变大(吸气), false=从大变小(呼气)</param>
        public void UpdateScale(double progress, bool growing)
        {
            double eased = EaseFunctions.EaseInOutSine(progress);
            if (growing)
                CircleScale = MinCircleRatio + (MaxCircleRatio - MinCircleRatio) * eased;
            else
                CircleScale = MaxCircleRatio - (MaxCircleRatio - MinCircleRatio) * eased;
        }

        /// <summary>
        /// 设置固定的缩放值（用于保持状态）
        /// </summary>
        /// <param name="atMax">true=最大尺寸, false=最小尺寸</param>
        public void SetFixedScale(bool atMax)
        {
            CircleScale = atMax ? MaxCircleRatio : MinCircleRatio;
        }

        /// <summary>
        /// 更新发光值（屏息阶段的呼吸辉光效果）
        /// </summary>
        /// <param name="elapsedSeconds">当前状态已持续时间</param>
        /// <param name="active">是否激活发光</param>
        public void UpdateGlow(double elapsedSeconds, bool active)
        {
            if (!active)
            {
                GlowOpacity = 0;
                return;
            }
            // 在 0.3~0.8 之间振荡
            GlowOpacity = 0.3 + 0.25 * (1 + Math.Sin(elapsedSeconds * 1.5));
        }

        /// <summary>
        /// 停用发光
        /// </summary>
        public void DisableGlow()
        {
            GlowOpacity = 0;
        }

        /// <summary>
        /// 重置动画到默认状态
        /// </summary>
        public void Reset()
        {
            CircleScale = MinCircleRatio;
            GlowOpacity = 0;
        }
    }
}
