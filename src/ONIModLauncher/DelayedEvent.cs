using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ONIModLauncher
{
    public class DelayedEvent
    {
        private readonly TimeSpan _delay;
        private readonly Timer _timer;

        public event EventHandler DelayFinished;

        public DelayedEvent(TimeSpan delay)
        {
            _delay = delay;
            _timer = new Timer(_delay.TotalMilliseconds);
			_timer.Elapsed += Timer_Elapsed;
            _timer.AutoReset = false;
        }

        public void Start()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			DelayFinished?.Invoke(this, EventArgs.Empty);
		}
    }
}
