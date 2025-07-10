using System;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class WorkSheetLegLinkEventArgs : EventArgs
	{
		public WorkSheetLegLinkEventArgs(CommonCartageLeg cartageLeg)
		{
			CartageLeg = cartageLeg;
		}

		public CommonCartageLeg CartageLeg { get; private set; }
	}
}
