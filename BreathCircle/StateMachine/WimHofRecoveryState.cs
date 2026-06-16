using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// Wim Hof 恢复呼吸状态 — 深吸气 → 屏住 → 吐气缓冲
    /// </summary>
    public class WimHofRecoveryState : IState
    {
        private readonly double _inhaleSeconds;
        private readonly double _holdSeconds;
        private readonly double _exhaleSeconds;

        private double _elapsed;
        private enum Phase { Inhale, Hold, Exhale }
        private Phase _phase;

        public BreathStage Stage => BreathStage.WimHofRecovery;
        public double DurationSeconds => _inhaleSeconds + _holdSeconds + _exhaleSeconds;
        public bool IsComplete { get; private set; }

        public string CenterText
        {
            get
            {
                return _phase switch
                {
                    Phase.Inhale => "深吸气",
                    Phase.Hold => $"保持 {(int)Math.Ceiling(_holdSeconds - _phaseElapsed)}s",
                    Phase.Exhale => "缓慢吐气",
                    _ => ""
                };
            }
        }

        private double _phaseElapsed;

        /// <param name="inhaleSeconds">深吸气时长</param>
        /// <param name="holdSeconds">屏息保持时长</param>
        /// <param name="exhaleSeconds">吐气缓冲时长</param>
        public WimHofRecoveryState(double inhaleSeconds = 2.0, double holdSeconds = 15.0, double exhaleSeconds = 3.0)
        {
            _inhaleSeconds = inhaleSeconds;
            _holdSeconds = holdSeconds;
            _exhaleSeconds = exhaleSeconds;
        }

        public void Enter()
        {
            IsComplete = false;
            _elapsed = 0;
            _phaseElapsed = 0;
            _phase = Phase.Inhale;
        }

        public void Update(double elapsed)
        {
            _elapsed = elapsed;

            switch (_phase)
            {
                case Phase.Inhale:
                    _phaseElapsed = elapsed;
                    if (elapsed >= _inhaleSeconds)
                    {
                        _phase = Phase.Hold;
                        _phaseElapsed = elapsed - _inhaleSeconds;
                    }
                    break;

                case Phase.Hold:
                    _phaseElapsed = elapsed - _inhaleSeconds;
                    if (_phaseElapsed >= _holdSeconds)
                    {
                        _phase = Phase.Exhale;
                        _phaseElapsed = elapsed - _inhaleSeconds - _holdSeconds;
                    }
                    break;

                case Phase.Exhale:
                    _phaseElapsed = elapsed - _inhaleSeconds - _holdSeconds;
                    if (_phaseElapsed >= _exhaleSeconds)
                        IsComplete = true;
                    break;
            }
        }

        /// <summary>当前是否在吸气阶段（用于动画：圆放大）</summary>
        public bool IsInhaling => _phase == Phase.Inhale;

        /// <summary>当前是否在吐气阶段（用于动画：圆缩小）</summary>
        public bool IsExhaling => _phase == Phase.Exhale;

        /// <summary>获取当前阶段的进度 [0, 1]</summary>
        public double GetPhaseProgress()
        {
            double phaseDuration = _phase switch
            {
                Phase.Inhale => _inhaleSeconds,
                Phase.Hold => _holdSeconds,
                Phase.Exhale => _exhaleSeconds,
                _ => 1.0
            };
            return Math.Min(_phaseElapsed / phaseDuration, 1.0);
        }

        public void Exit() { }
    }
}
