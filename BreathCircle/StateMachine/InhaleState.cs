using BreathCircle.Helpers;
using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// 吸气状态 — 圆圈从小变大
    /// </summary>
    public class InhaleState : IState
    {
        private readonly double _durationSeconds;

        public BreathStage Stage => BreathStage.Inhale;
        public double DurationSeconds => _durationSeconds;
        public bool IsComplete { get; private set; }
        public string CenterText => "吸气";

        /// <summary>
        /// 创建吸气状态
        /// </summary>
        /// <param name="durationSeconds">吸气持续时长（秒）</param>
        public InhaleState(double durationSeconds)
        {
            _durationSeconds = durationSeconds;
        }

        public void Enter()
        {
            IsComplete = false;
        }

        public void Update(double elapsed)
        {
            if (elapsed >= _durationSeconds)
                IsComplete = true;
        }

        /// <summary>
        /// 获取当前缓动进度（用于动画：0→1 表示从小到大）
        /// </summary>
        public double GetProgress(double elapsed)
        {
            double progress = Math.Min(elapsed / _durationSeconds, 1.0);
            return EaseFunctions.EaseInOutSine(progress);
        }

        public void Exit() { }
    }
}
