using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// Wim Hof 屏息保持状态 — 圆保持最小尺寸，发光，显示目标倒计时
    /// </summary>
    public class WimHofRetentionState : IState
    {
        private readonly double _targetSeconds;  // 目标屏息时间
        private readonly double _capSeconds;      // 安全上限
        private double _elapsed;

        public BreathStage Stage => BreathStage.WimHofRetention;
        public double DurationSeconds => _capSeconds;
        public bool IsComplete { get; private set; }

        /// <summary>显示距目标剩余秒数</summary>
        public string CenterText
        {
            get
            {
                double remaining = _targetSeconds - _elapsed;
                if (remaining <= 0)
                    return "0s";
                return $"{(int)Math.Ceiling(remaining)}s";
            }
        }

        /// <summary>
        /// 创建Wim Hof屏息状态
        /// </summary>
        /// <param name="targetSeconds">目标屏息时长（秒），到达后自动结束</param>
        /// <param name="capSeconds">安全上限（秒），绝对不超过此值</param>
        public WimHofRetentionState(double targetSeconds, double capSeconds)
        {
            _targetSeconds = targetSeconds;
            _capSeconds = capSeconds;
        }

        public void Enter()
        {
            IsComplete = false;
            _elapsed = 0;
        }

        public void Update(double elapsed)
        {
            _elapsed = elapsed;
            // 到达目标时间自动完成，或到达上限强制完成
            if (elapsed >= _targetSeconds || elapsed >= _capSeconds)
                IsComplete = true;
        }

        public double GetGlowIntensity()
        {
            return 0.3 + 0.25 * (1 + Math.Sin(_elapsed * 0.8));
        }

        public double ElapsedSeconds => _elapsed;

        public void Exit() { }
    }
}
