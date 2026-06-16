namespace BreathCircle.Models
{
    /// <summary>
    /// Wim Hof 轮次配置
    /// </summary>
    public class WimHofRound
    {
        /// <summary>轮次编号（1-4）</summary>
        public int RoundNumber { get; set; }

        /// <summary>屏息保持时长（秒）</summary>
        public int RetentionSeconds { get; set; }

        /// <summary>屏息最大上限（秒），到达后自动进入下一阶段</summary>
        public int RetentionCapSeconds { get; set; } = 120;

        /// <summary>恢复呼吸屏息时长（秒）</summary>
        public int RecoveryHoldSeconds { get; set; } = 15;

        /// <summary>快速呼吸次数</summary>
        public int BreathCount { get; set; } = 30;

        /// <summary>创建默认的4轮Wim Hof配置</summary>
        public static WimHofRound[] CreateDefaultRounds()
        {
            return new[]
            {
                new WimHofRound { RoundNumber = 1, RetentionSeconds = 30 },
                new WimHofRound { RoundNumber = 2, RetentionSeconds = 45 },
                new WimHofRound { RoundNumber = 3, RetentionSeconds = 60 },
                new WimHofRound { RoundNumber = 4, RetentionSeconds = 90 },
            };
        }
    }
}
