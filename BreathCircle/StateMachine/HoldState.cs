using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// 屏息/保持状态 — 圆圈保持最大尺寸，带发光效果
    /// </summary>
    public class HoldState : IState
    {
        private readonly double _durationSeconds;
        private readonly bool _isAfterInhale; // true=吸气后屏息(圆大), false=呼气后屏息(圆小)

        public BreathStage Stage => BreathStage.Hold;
        public double DurationSeconds => _durationSeconds;
        public bool IsComplete { get; private set; }

        public string CenterText
        {
            get
            {
                int remaining = Math.Max(0, (int)Math.Ceiling(_durationSeconds - _elapsed));
                return $"屏息 {remaining}s";
            }
        }

        private double _elapsed;

        /// <summary>
        /// 创建屏息状态
        /// </summary>
        /// <param name="durationSeconds">屏息持续时长（秒）</param>
        /// <param name="isAfterInhale">true=吸气后屏息（圆保持大），false=呼气后屏息（圆保持小）</param>
        public HoldState(double durationSeconds, bool isAfterInhale = true)
        {
            _durationSeconds = durationSeconds;
            _isAfterInhale = isAfterInhale;
        }

        public void Enter()
        {
            IsComplete = false;
            _elapsed = 0;
        }

        public void Update(double elapsed)
        {
            _elapsed = elapsed;
            if (elapsed >= _durationSeconds)
                IsComplete = true;
        }

        /// <summary>
        /// 是否在吸气后屏息（决定圆的大小）
        /// </summary>
        public bool IsAfterInhale => _isAfterInhale;

        /// <summary>
        /// 获取发光强度（使用正弦振荡，产生呼吸般的辉光效果）
        /// </summary>
        public double GetGlowIntensity(double elapsed)
        {
            if (elapsed <= 0) return 0.5;
            // 在0.3~0.8之间波动
            return 0.3 + 0.25 * (1 + Math.Sin(elapsed * 1.5));
        }

        public void Exit() { }
    }
}
