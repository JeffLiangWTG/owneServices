using Xtrade.Administration;
using Xtrade.Core;

namespace XT.Internal.API
{
	public class StatusObserver : IWaitStatus
	{
		public void SetDoneMsg(XtString doneMessage) { }

		public void SetStatusMsg(XtString message) { }

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing) { }
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
