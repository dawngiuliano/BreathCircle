namespace BreathCircle.Helpers
{
    /// <summary>
    /// 缓动函数库 — 统一使用 EaseInOutSine 实现平滑呼吸动画
    /// </summary>
    public static class EaseFunctions
    {
        /// <summary>
        /// EaseInOutSine: 在 [0, 1] 区间内平滑加速和减速
        /// scale = -(cos(π * t) - 1) / 2
        /// </summary>
        /// <param name="t">进度值，范围 [0, 1]</param>
        /// <returns>缓动后的值，范围 [0, 1]</returns>
        public static double EaseInOutSine(double t)
        {
            if (t <= 0) return 0;
            if (t >= 1) return 1;
            return -(Math.Cos(Math.PI * t) - 1) / 2;
        }

        /// <summary>
        /// 在 [min, max] 区间内插值
        /// </summary>
        public static double Lerp(double min, double max, double easeProgress)
        {
            return min + (max - min) * easeProgress;
        }

        /// <summary>
        /// 计算带缓动的缩放值（用于圆呼吸动画）
        /// </summary>
        public static double CalculateScale(double progress, double minScale, double maxScale)
        {
            double eased = EaseInOutSine(progress);
            return Lerp(minScale, maxScale, eased);
        }
    }
}
