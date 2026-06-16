namespace BreathCircle.Models
{
    /// <summary>
    /// 呼吸会话数据（V1 预留扩展）
    /// </summary>
    public class BreathSession
    {
        /// <summary>会话开始时间</summary>
        public DateTime StartTime { get; set; }

        /// <summary>呼吸模式</summary>
        public BreathMode Mode { get; set; }

        /// <summary>完成的轮次数</summary>
        public int CompletedRounds { get; set; }

        /// <summary>总练习时长（秒）</summary>
        public double TotalDurationSeconds { get; set; }
    }
}
