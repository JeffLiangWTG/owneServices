using System;
using CargoWise.Common;

namespace Enterprise.Packing.Business
{
	public class ExceptionEventArgs : EventArgs
	{
		public ExceptionEventArgs(Exception exception)
		{
			Exception = Argument.NotNull(exception, nameof(exception));
		}

		public Exception Exception { get; }
	}
}
