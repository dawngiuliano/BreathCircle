namespace BreathCircle.Models
{
    /// <summary>
    /// 呼吸阶段枚举
    /// </summary>
    public enum BreathStage
    {
        /// <summary>空闲</summary>
        Idle,

        /// <summary>吸气</summary>
        Inhale,

        /// <summary>呼气</summary>
        Exhale,

        /// <summary>屏息/保持</summary>
        Hold,

        /// <summary>Wim Hof: 轮次开始</summary>
        RoundStart,

        /// <summary>Wim Hof: 快速呼吸中</summary>
        WimHofBreathing,

        /// <summary>Wim Hof: 屏息保持</summary>
        WimHofRetention,

        /// <summary>Wim Hof: 恢复呼吸</summary>
        WimHofRecovery,

        /// <summary>已完成</summary>
        Finished
    }
}
