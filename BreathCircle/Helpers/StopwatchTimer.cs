using System.Diagnostics;

namespace BreathCircle.Helpers
{
    /// <summary>
    /// 基于 Stopwatch 的计时器封装 — 唯一时间源，避免时间漂移
    /// </summary>
    public class StopwatchTimer
    {
        private readonly Stopwatch _stopwatch = new();
        private bool _isRunning;

        /// <summary>
        /// 自 Start() 以来的总经过秒数
        /// </summary>
        public double ElapsedSeconds => _stopwatch.Elapsed.TotalSeconds;

        /// <summary>
        /// 自 Start() 以来的总经过毫秒数
        /// </summary>
        public double ElapsedMilliseconds => _stopwatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// 计时器是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 启动计时器
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
            _isRunning = true;
        }

        /// <summary>
        /// 暂停计时器（保留已累计的时间）
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
            _isRunning = false;
        }

        /// <summary>
        /// 重置并停止计时器
        /// </summary>
        public void Reset()
        {
            _stopwatch.Reset();
            _isRunning = false;
        }

        /// <summary>
        /// 重置并立即开始计时
        /// </summary>
        public void Restart()
        {
            _stopwatch.Restart();
            _isRunning = true;
        }
    }
}
