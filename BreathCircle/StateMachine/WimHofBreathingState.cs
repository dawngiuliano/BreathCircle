using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// Wim Hof 快速呼吸状态 — 30次快速深呼吸，圆圈快速缩放
    /// </summary>
    public class WimHofBreathingState : IState
    {
        private readonly int _breathCount;
        private readonly double _inhaleSeconds;
        private readonly double _exhaleSeconds;
        private readonly double _breathCycleSeconds;

        public BreathStage Stage => BreathStage.WimHofBreathing;
        public double DurationSeconds => _breathCount * _breathCycleSeconds;
        public bool IsComplete { get; private set; }
        public string CenterText
        {
            get
            {
                int currentBreath = Math.Min(_breathCount, (int)(_totalElapsed / _breathCycleSeconds) + 1);
                int remaining = _breathCount - currentBreath + 1;
                return remaining.ToString();
            }
        }

        /// <summary>当前是第几次呼吸（1-based），供外部显示详情</summary>
        public string DetailText
        {
            get
            {
                int currentBreath = Math.Min(_breathCount, (int)(_totalElapsed / _breathCycleSeconds) + 1);
                return $"第 {currentBreath}/{_breathCount} 次";
            }
        }

        private double _totalElapsed;

        /// <summary>
        /// 创建Wim Hof快速呼吸状态
        /// </summary>
        /// <param name="breathCount">呼吸次数（默认30）</param>
        /// <param name="inhaleSeconds">每次吸气时长（秒）</param>
        /// <param name="exhaleSeconds">每次呼气时长（秒）</param>
        public WimHofBreathingState(int breathCount = 30, double inhaleSeconds = 1.0, double exhaleSeconds = 1.0)
        {
            _breathCount = breathCount;
            _inhaleSeconds = inhaleSeconds;
            _exhaleSeconds = exhaleSeconds;
            _breathCycleSeconds = inhaleSeconds + exhaleSeconds;
        }

        public void Enter()
        {
            IsComplete = false;
            _totalElapsed = 0;
        }

        public void Update(double elapsed)
        {
            _totalElapsed = elapsed;
            if (elapsed >= DurationSeconds)
                IsComplete = true;
        }

        /// <summary>
        /// 获取当前呼吸阶段的进度（0→1），用于动画缩放
        /// </summary>
        public double GetPhaseProgress()
        {
            double phaseElapsed = _totalElapsed % _breathCycleSeconds;
            if (IsInhaling)
                return Math.Min(phaseElapsed / _inhaleSeconds, 1.0);
            else
                return Math.Min((phaseElapsed - _inhaleSeconds) / _exhaleSeconds, 1.0);
        }

        /// <summary>
        /// 当前是否在吸气（true=吸气，false=呼气）
        /// </summary>
        public bool IsInhaling
        {
            get
            {
                double phaseElapsed = _totalElapsed % _breathCycleSeconds;
                return phaseElapsed < _inhaleSeconds;
            }
        }

        /// <summary>
        /// 当前呼吸次数（1-based）
        /// </summary>
        public int CurrentBreath => Math.Min(_breathCount, (int)(_totalElapsed / _breathCycleSeconds) + 1);

        /// <summary>
        /// 总呼吸次数
        /// </summary>
        public int TotalBreaths => _breathCount;

        public void Exit() { }
    }
}
