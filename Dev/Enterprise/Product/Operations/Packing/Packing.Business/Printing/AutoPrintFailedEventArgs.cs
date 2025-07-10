using System;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class AutoPrintFailedEventArgs : EventArgs
	{
		public AutoPrintFailedEventArgs(ZString message, bool canContinueWithManualPrint)
		{
			Message = message;
			CanContinueWithManualPrint = canContinueWithManualPrint;
		}

		public readonly ZString Message;
		public readonly bool CanContinueWithManualPrint;
	}
}
