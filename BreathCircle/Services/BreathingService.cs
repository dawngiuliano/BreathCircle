using BreathCircle.Helpers;
using BreathCircle.Models;
using BreathCircle.StateMachine;

namespace BreathCircle.Services
{
    /// <summary>
    /// 呼吸核心服务 — 协调整体呼吸过程、状态机转换和动画计算
    /// </summary>
    public class BreathingService
    {
        private readonly StateContext _stateContext = new();
        private readonly StopwatchTimer _timer = new();
        private readonly AnimationService _animation;
        private BreathMode _currentMode = BreathMode.BoxBreathing;

        // 精确计时：记录每个状态开始时的 Stopwatch 毫秒数
        private double _stateStartTimeMs;

        // Wim Hof 状态追踪
        private int _wimHofCurrentRound;
        private int _wimHofTotalRounds;
        private WimHofSettings _wimHofSettings = new();
        private bool _wimHofShowingRoundPause;
        private bool _isPaused;

        /// <summary>Wim Hof 可调参数（外部设置）</summary>
        public WimHofSettings WimHofSettings
        {
            get => _wimHofSettings;
            set { _wimHofSettings = value ?? new(); }
        }

        // 事件
        public event Action? StateChanged;
        public event Action? ModeChanged;

        // 公开属性（供 ViewModel 绑定）
        public BreathStage CurrentStage => _stateContext.CurrentState?.Stage ?? BreathStage.Idle;
        public BreathMode CurrentMode => _currentMode;
        public bool IsRunning => _timer.IsRunning;
        public bool IsPaused => _isPaused;
        public double CircleScale => _animation.CircleScale;
        public double GlowOpacity => _animation.GlowOpacity;
        public string CenterText => _isPaused ? "已暂停" : (_stateContext.CurrentState?.CenterText ?? "");

        /// <summary>Wim Hof 当前轮次（1-4），0表示未开始</summary>
        public int WimHofCurrentRoundDisplay => _wimHofCurrentRound + 1;

        /// <summary>Wim Hof 是否正在显示轮次暂停</summary>
        public bool IsWimHofRoundPause => _wimHofShowingRoundPause;

        /// <summary>吸气时长（秒）</summary>
        public double InhaleDuration => GetModeInhaleSeconds();
        /// <summary>呼气时长（秒）</summary>
        public double ExhaleDuration => GetModeExhaleSeconds();
        /// <summary>屏息时长（秒）</summary>
        public double HoldDuration => GetModeHoldSeconds();

        public BreathingService(AnimationService animation)
        {
            _animation = animation;
            _stateContext.StateChanged += OnInternalStateChanged;
            ChangeState(new IdleState());
        }

        /// <summary>
        /// 每帧更新（由 CompositionTarget.Rendering 驱动）。
        /// 使用 Stopwatch 真实时间，不依赖帧间隔。
        /// </summary>
        public void Update()
        {
            if (!_timer.IsRunning) return;

            // 用 Stopwatch 真实经过时间，无漂移
            double elapsedSec = (_timer.ElapsedMilliseconds - _stateStartTimeMs) / 1000.0;
            if (elapsedSec < 0) elapsedSec = 0;

            _stateContext.UpdateAbsolute(elapsedSec);

            var state = _stateContext.CurrentState;
            if (state == null) return;

            UpdateAnimation(state);

            if (state.IsComplete)
                TransitionToNextState();
        }

        /// <summary>
        /// 开始呼吸
        /// </summary>
        public void Start()
        {
            if (_currentMode == BreathMode.WimHof)
            {
                StartWimHof();
            }
            else
            {
                StartNormalMode();
            }
        }

        /// <summary>
        /// 暂停 — 保留当前进度，冻结动画
        /// </summary>
        public void Pause()
        {
            _timer.Stop();
            _isPaused = true;
            NotifyStateChanged();
        }

        /// <summary>
        /// 继续 — 从暂停位置恢复
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
            _timer.Start();
            NotifyStateChanged();
        }

        /// <summary>
        /// 完全停止并重置到空闲
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
            _isPaused = false;
            _wimHofShowingRoundPause = false;
            ChangeState(new IdleState());
            _animation.Reset();
            NotifyStateChanged();
        }

        /// <summary>
        /// 切换：准备→开始→暂停→继续→暂停→...
        /// </summary>
        public void Toggle()
        {
            if (!IsRunning && !_isPaused)
            {
                // 空闲 → 开始
                Start();
            }
            else if (IsRunning)
            {
                // 运行中 → 暂停
                Pause();
            }
            else if (_isPaused)
            {
                // 暂停中 → 继续
                Resume();
            }
        }

        /// <summary>
        /// 切换到下一个模式
        /// </summary>
        public void NextMode()
        {
            bool wasRunning = IsRunning;
            Stop();
            int modeCount = Enum.GetValues<BreathMode>().Length;
            _currentMode = (BreathMode)(((int)_currentMode + 1) % modeCount);
            ModeChanged?.Invoke();
            if (wasRunning) Start();
        }

        /// <summary>
        /// 设置指定模式
        /// </summary>
        public void SetMode(BreathMode mode)
        {
            bool wasRunning = IsRunning;
            Stop();
            _currentMode = mode;
            ModeChanged?.Invoke();
            if (wasRunning) Start();
        }

        /// <summary>
        /// 指定模式是否在运行中
        /// </summary>
        public bool IsModeRunning(BreathMode mode) => IsRunning && _currentMode == mode;

        // ===== 私有方法 =====

        private void StartNormalMode()
        {
            _timer.Restart();
            TransitionToNextState();
            NotifyStateChanged();
        }

        private void StartWimHof()
        {
            _wimHofCurrentRound = 0;
            _wimHofTotalRounds = _wimHofSettings.Rounds;
            _wimHofShowingRoundPause = true;
            _timer.Restart();

            ChangeState(new WimHofBreathingState(
                _wimHofSettings.BreathCount,
                _wimHofSettings.InhaleSeconds,
                _wimHofSettings.ExhaleSeconds));
            NotifyStateChanged();
        }

        private void TransitionToNextState()
        {
            var prevState = _stateContext.CurrentState;
            var prevStage = prevState?.Stage ?? BreathStage.Idle;

            if (_currentMode == BreathMode.WimHof)
            {
                TransitionWimHof(prevStage);
            }
            else
            {
                TransitionNormalMode(prevStage);
            }
        }

        private void TransitionNormalMode(BreathStage prevStage)
        {
            IState nextState;

            switch (_currentMode)
            {
                case BreathMode.BoxBreathing:
                    nextState = NextBoxBreathingStage(prevStage);
                    break;
                case BreathMode.FourSevenEight:
                    nextState = NextFourSevenEightStage(prevStage);
                    break;
                case BreathMode.Resonance:
                    nextState = NextResonanceStage(prevStage);
                    break;
                case BreathMode.PhysiologicalSigh:
                    nextState = NextPhysiologicalSighStage(prevStage);
                    break;
                default:
                    nextState = new IdleState();
                    break;
            }

            ChangeState(nextState);
            NotifyStateChanged();
        }

        private void TransitionWimHof(BreathStage prevStage)
        {
            IState nextState;
            var s = _wimHofSettings;

            switch (prevStage)
            {
                case BreathStage.Idle:
                    nextState = new IdleState();
                    break;

                case BreathStage.WimHofBreathing:
                    // 进入屏息保持：目标=本轮配置时长，上限=目标+60秒缓冲
                    double target = s.GetRetentionForRound(_wimHofCurrentRound);
                    nextState = new WimHofRetentionState(target, target + 60);
                    break;

                case BreathStage.WimHofRetention:
                    // 进入恢复呼吸：深吸气 → 屏息 → 吐气缓冲
                    nextState = new WimHofRecoveryState(2.0, s.RecoveryHoldSeconds, s.RecoveryExhaleSeconds);
                    break;

                case BreathStage.WimHofRecovery:
                    _wimHofCurrentRound++;
                    if (_wimHofCurrentRound < _wimHofTotalRounds)
                    {
                        // 下一轮快速呼吸
                        _wimHofShowingRoundPause = true;
                        nextState = new WimHofBreathingState(
                            s.BreathCount, s.InhaleSeconds, s.ExhaleSeconds);
                    }
                    else
                    {
                        // 全部完成
                        nextState = new FinishedState("训练完成！");
                    }
                    break;

                case BreathStage.Finished:
                    nextState = new IdleState();
                    _wimHofShowingRoundPause = false;
                    break;

                default:
                    nextState = new IdleState();
                    break;
            }

            ChangeState(nextState);
            NotifyStateChanged();
        }

        // 盒式呼吸：吸4 → 屏4 → 呼4 → 屏4
        private IState NextBoxBreathingStage(BreathStage prevStage)
        {
            return prevStage switch
            {
                BreathStage.Idle => new InhaleState(4),
                BreathStage.Inhale => new HoldState(4, true),
                BreathStage.Hold when IsAfterInhaleHold() => new ExhaleState(4),
                BreathStage.Exhale => new HoldState(4, false),
                BreathStage.Hold when !IsAfterInhaleHold() => new InhaleState(4),
                _ => new InhaleState(4),
            };
        }

        // 4-7-8：吸4 → 屏7 → 呼8
        private IState NextFourSevenEightStage(BreathStage prevStage)
        {
            return prevStage switch
            {
                BreathStage.Idle => new InhaleState(4),
                BreathStage.Inhale => new HoldState(7, true),
                BreathStage.Hold => new ExhaleState(8),
                BreathStage.Exhale => new InhaleState(4),
                _ => new InhaleState(4),
            };
        }

        // 共鸣呼吸：吸5 → 呼5
        private IState NextResonanceStage(BreathStage prevStage)
        {
            return prevStage switch
            {
                BreathStage.Idle => new InhaleState(5),
                BreathStage.Inhale => new ExhaleState(5),
                BreathStage.Exhale => new InhaleState(5),
                _ => new InhaleState(5),
            };
        }

        // 生理性叹息：吸2 → 屏2 → 呼8
        private IState NextPhysiologicalSighStage(BreathStage prevStage)
        {
            return prevStage switch
            {
                BreathStage.Idle => new InhaleState(2),
                BreathStage.Inhale => new HoldState(2, true),
                BreathStage.Hold => new ExhaleState(8),
                BreathStage.Exhale => new InhaleState(2),
                _ => new InhaleState(2),
            };
        }

        private bool IsAfterInhaleHold()
        {
            var state = _stateContext.CurrentState;
            if (state is HoldState hold)
                return hold.IsAfterInhale;
            return true;
        }

        private void UpdateAnimation(IState state)
        {
            double elapsed = _stateContext.ElapsedInState;

            switch (state)
            {
                case InhaleState inhale:
                    double inhaleProgress = Math.Min(elapsed / inhale.DurationSeconds, 1.0);
                    _animation.UpdateScale(inhaleProgress, growing: true);
                    _animation.UpdateGlow(elapsed, false);
                    break;

                case ExhaleState exhale:
                    double exhaleProgress = Math.Min(elapsed / exhale.DurationSeconds, 1.0);
                    _animation.UpdateScale(exhaleProgress, growing: false);
                    _animation.UpdateGlow(elapsed, false);
                    break;

                case HoldState hold:
                    _animation.SetFixedScale(hold.IsAfterInhale);
                    _animation.UpdateGlow(elapsed, true);
                    break;

                case WimHofBreathingState wb:
                    double phaseProgress = wb.GetPhaseProgress();
                    _animation.UpdateScale(phaseProgress, growing: wb.IsInhaling);
                    _animation.UpdateGlow(elapsed, false);
                    break;

                case WimHofRetentionState:
                    _animation.SetFixedScale(false); // 屏息时圆最小
                    _animation.UpdateGlow(elapsed, true);
                    break;

                case WimHofRecoveryState wr:
                    if (wr.IsInhaling)
                    {
                        _animation.UpdateScale(wr.GetPhaseProgress(), growing: true);
                    }
                    else if (wr.IsExhaling)
                    {
                        _animation.UpdateScale(wr.GetPhaseProgress(), growing: false);
                    }
                    else
                    {
                        _animation.SetFixedScale(true); // 屏息保持最大
                    }
                    _animation.UpdateGlow(elapsed, true);
                    break;

                default:
                    _animation.SetFixedScale(false);
                    _animation.UpdateGlow(elapsed, false);
                    break;
            }
        }

        private void OnInternalStateChanged(IState? state)
        {
            // 内部状态变化时的处理（可扩展）
        }

        private void ChangeState(IState state)
        {
            _stateStartTimeMs = _timer.ElapsedMilliseconds;
            _stateContext.ChangeState(state);
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        // 模式参数
        private double GetModeInhaleSeconds() => _currentMode switch
        {
            BreathMode.BoxBreathing => 4,
            BreathMode.FourSevenEight => 4,
            BreathMode.Resonance => 5,
            BreathMode.PhysiologicalSigh => 2,
            _ => 4,
        };

        private double GetModeExhaleSeconds() => _currentMode switch
        {
            BreathMode.BoxBreathing => 4,
            BreathMode.FourSevenEight => 8,
            BreathMode.Resonance => 5,
            BreathMode.PhysiologicalSigh => 8,
            _ => 4,
        };

        private double GetModeHoldSeconds() => _currentMode switch
        {
            BreathMode.BoxBreathing => 4,
            BreathMode.FourSevenEight => 7,
            BreathMode.PhysiologicalSigh => 2,
            _ => 0,
        };
    }
}
