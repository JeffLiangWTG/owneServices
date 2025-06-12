using System;
using System.Threading;

namespace OcmPoc.Utils
{
	public class ConsoleCancelHandler : IDisposable, IConsoleCancelHandler
	{
		CancellationTokenSource cts;
		ManualResetEventSlim disposeEvent;

		public ConsoleCancelHandler()
		{
			cts = new CancellationTokenSource();

			Console.CancelKeyPress += SignalCancellation;
			AppDomain.CurrentDomain.ProcessExit += SignalCancellation;
		}

		public CancellationToken Token => cts.Token;

		void SignalCancellation(object sender, object eventArgs)
		{
			disposeEvent = new ManualResetEventSlim();

			Console.WriteLine($"Cancellation event received");

			if (eventArgs is ConsoleCancelEventArgs cancelEventArgs)
			{
				cancelEventArgs.Cancel = true;
			}

			cts.Cancel();

			disposeEvent.Wait();
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Console.CancelKeyPress -= SignalCancellation;
					AppDomain.CurrentDomain.ProcessExit -= SignalCancellation;

					cts.Dispose();
					disposeEvent?.Set();
				}
				
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
