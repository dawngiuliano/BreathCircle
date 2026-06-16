namespace BreathCircle.Models
{
    /// <summary>
    /// Wim Hof 可调参数
    /// </summary>
    public class WimHofSettings
    {
        /// <summary>练习轮次</summary>
        public int Rounds { get; set; } = 4;

        /// <summary>每轮呼吸次数</summary>
        public int BreathCount { get; set; } = 30;

        /// <summary>每次吸气时长（秒）</summary>
        public double InhaleSeconds { get; set; } = 1.0;

        /// <summary>每次呼气时长（秒）</summary>
        public double ExhaleSeconds { get; set; } = 1.0;

        /// <summary>第1轮屏息时长（秒）</summary>
        public int BaseRetentionSeconds { get; set; } = 30;

        /// <summary>每轮屏息递增（秒），如 5 表示每轮比上一轮多屏息5秒</summary>
        public int RetentionIncrementSeconds { get; set; } = 15;

        /// <summary>恢复阶段：深吸气后屏住时长（秒）</summary>
        public double RecoveryHoldSeconds { get; set; } = 15.0;

        /// <summary>恢复阶段：屏息后的吐气缓冲时长（秒）</summary>
        public double RecoveryExhaleSeconds { get; set; } = 3.0;

        /// <summary>
        /// 获取指定轮次（0-based index）的屏息时长
        /// 第1轮 = Base, 第2轮 = Base+Increment, ...
        /// </summary>
        public int GetRetentionForRound(int roundIndex)
        {
            return BaseRetentionSeconds + roundIndex * RetentionIncrementSeconds;
        }

        /// <summary>
        /// 所有轮次中的最大屏息时长 + 缓冲（用作内部上限）
        /// </summary>
        public int AutoRetentionCap()
        {
            int max = BaseRetentionSeconds + (Rounds - 1) * RetentionIncrementSeconds;
            return max + 60; // 给用户60秒缓冲
        }
    }
}
