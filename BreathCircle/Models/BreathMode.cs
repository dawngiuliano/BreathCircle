namespace BreathCircle.Models
{
    /// <summary>
    /// 呼吸模式枚举
    /// </summary>
    public enum BreathMode
    {
        /// <summary>盒式呼吸：吸4-屏4-呼4-屏4</summary>
        BoxBreathing,

        /// <summary>4-7-8呼吸法：吸4-屏7-呼8</summary>
        FourSevenEight,

        /// <summary>共鸣呼吸：吸5-呼5（无屏息）</summary>
        Resonance,

        /// <summary>生理性叹息：吸2-屏2-呼8</summary>
        PhysiologicalSigh,

        /// <summary>Wim Hof呼吸法</summary>
        WimHof
    }
}
