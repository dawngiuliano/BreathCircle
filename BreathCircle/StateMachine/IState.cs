using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// 状态接口 — 所有呼吸状态必须实现
    /// </summary>
    public interface IState
    {
        /// <summary>进入状态时调用</summary>
        void Enter();

        /// <summary>每帧更新，elapsed 为当前状态已持续时间（秒）</summary>
        void Update(double elapsed);

        /// <summary>退出状态时调用</summary>
        void Exit();

        /// <summary>当前阶段</summary>
        BreathStage Stage { get; }

        /// <summary>当前状态的总持续时长（秒），0 表示无限</summary>
        double DurationSeconds { get; }

        /// <summary>当前是否已完成（到达时长或条件满足）</summary>
        bool IsComplete { get; }

        /// <summary>显示在圆中央的文字</summary>
        string CenterText { get; }
    }
}
