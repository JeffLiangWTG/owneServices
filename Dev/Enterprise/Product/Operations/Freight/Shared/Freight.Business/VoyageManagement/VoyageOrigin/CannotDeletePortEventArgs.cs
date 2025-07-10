using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class CannotDeletePortEventArgs : EventArgs
	{
		public CannotDeletePortEventArgs(ZString reasonForNotAbleToDelete)
			: base()
		{
			ReasonForNotAbleToDelete = reasonForNotAbleToDelete;
		}

		public readonly ZString ReasonForNotAbleToDelete;
	}
}
