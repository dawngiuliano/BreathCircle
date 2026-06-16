using BreathCircle.Models;

namespace BreathCircle.StateMachine
{
    /// <summary>
    /// 空闲状态 — 等待用户开始
    /// </summary>
    public class IdleState : IState
    {
        public BreathStage Stage => BreathStage.Idle;
        public double DurationSeconds => 0; // 无限等待
        public bool IsComplete => false;     // 需要用户手动触发
        public string CenterText => "准备";

        public void Enter() { }
        public void Update(double elapsed) { }
        public void Exit() { }
    }
}
