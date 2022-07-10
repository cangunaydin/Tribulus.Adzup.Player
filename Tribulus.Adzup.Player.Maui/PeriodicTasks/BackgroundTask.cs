using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tribulus.Adzup.Player.Maui.PeriodicTasks
{
    public abstract class BackgroundTask
    {
        private readonly PeriodicTimer _timer;
        private readonly CancellationTokenSource _cts = new();
        private Task? _timerTask;

        protected BackgroundTask(TimeSpan timeInterval)
        {
            _timer = new PeriodicTimer(timeInterval);
        }
        public void Start()
        {
            _timerTask = DoWorkMainAsync();
        }
        public async Task DoWorkMainAsync()
        {
            try
            {
                while (await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    await DoWorkAsync();
                }
            }
            catch (OperationCanceledException)
            {

            }
        }
        public abstract Task DoWorkAsync();
        public async Task StopAsync()
        {
            if (_timerTask is null)
                return;

            _cts.Cancel();
            await _timerTask;
            _cts.Dispose();
        }
    }
}
