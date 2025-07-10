#if DEBUG
using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class MockProgressFormForTest : IDisposable
	{
		public string Status { get; private set; } = ZString.Empty;

		public bool IsDisposed { get; private set; }

		public IDisposable Show(string status)
		{
			Status = status;
			return this;
		}

		public void ClearStatus()
		{
			Status = ZString.Empty;
			IsDisposed = false;
		}

		public void Dispose()
		{
			IsDisposed = true;
		}
	}
}
#endif
