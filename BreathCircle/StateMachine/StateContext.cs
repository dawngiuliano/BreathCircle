namespace BreathCircle.StateMachine
{
    /// <summary>
    /// 状态上下文 — 管理当前状态和转换
    /// </summary>
    public class StateContext
    {
        private IState? _currentState;

        /// <summary>当前状态</summary>
        public IState? CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState?.Exit();
                    _currentState = value;
                    _currentState?.Enter();
                    StateChanged?.Invoke(value);
                }
            }
        }

        /// <summary>状态变化事件</summary>
        public event Action<IState?>? StateChanged;

        /// <summary>当前总经过时间</summary>
        public double ElapsedInState { get; private set; }

        /// <summary>
        /// 执行状态转换
        /// </summary>
        public void ChangeState(IState? state)
        {
            ElapsedInState = 0;
            CurrentState = state;
        }

        /// <summary>
        /// 使用绝对经过时间更新（精确，无漂移）
        /// </summary>
        public void UpdateAbsolute(double absoluteElapsedSeconds)
        {
            ElapsedInState = absoluteElapsedSeconds;
            _currentState?.Update(absoluteElapsedSeconds);
        }

        /// <summary>
        /// 每帧更新当前状态（delta 累积模式，有漂移风险）
        /// </summary>
        public void Update(double deltaSeconds)
        {
            ElapsedInState += deltaSeconds;
            _currentState?.Update(ElapsedInState);
        }

        /// <summary>
        /// 重置上下文
        /// </summary>
        public void Reset()
        {
            _currentState?.Exit();
            _currentState = null;
            ElapsedInState = 0;
        }
    }
}
