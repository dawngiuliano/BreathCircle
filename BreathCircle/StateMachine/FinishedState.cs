using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// Wim Hof 完成状态 — 显示训练完成
    /// </summary>
    public class FinishedState : IState
    {
        private double _elapsed;
        private const double DisplaySeconds = 3.0; // 显示3秒
        private readonly string _message;

        public BreathStage Stage => BreathStage.Finished;
        public double DurationSeconds => DisplaySeconds;
        public bool IsComplete { get; private set; }
        public string CenterText => _message;

        /// <summary>
        /// 创建完成状态
        /// </summary>
        /// <param name="message">显示消息</param>
        public FinishedState(string message = "完成！")
        {
            _message = message;
        }

        public void Enter()
        {
            IsComplete = false;
            _elapsed = 0;
        }

        public void Update(double elapsed)
        {
            _elapsed = elapsed;
            if (elapsed >= DisplaySeconds)
                IsComplete = true;
        }

        public void Exit() { }
    }
}
