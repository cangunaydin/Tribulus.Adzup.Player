using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tribulus.Adzup.Player.Shared.PeriodicTask
{
    public abstract class BackgroundTask
    {
        private readonly PeriodicTimer _timer;
        private CancellationTokenSource _cts = new();
        private Task? _timerTask;

        protected BackgroundTask(TimeSpan timeInterval)
        {
            _timer = new PeriodicTimer(timeInterval);
        }
        public void Start()
        {
            CreateNewCts();
            _timerTask = DoWorkMainAsync();
        }
        private void CreateNewCts()
        {
            if (_cts==null)
            {
                _cts = new();
            }
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
            _cts = null;
        }
    }
}
