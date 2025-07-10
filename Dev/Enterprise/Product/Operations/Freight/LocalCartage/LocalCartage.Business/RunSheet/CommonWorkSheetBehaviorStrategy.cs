using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetBehaviorStrategy
	{
		public CommonWorkSheetBehaviorStrategy()
		{
		}

		public virtual void SetDefaultValues(CommonWorkSheet workSheet)
		{
		}

		public virtual void VehicleAttached(CommonWorkSheet workSheet)
		{
		}

		public virtual void VehicleRemoved(CommonWorkSheet workSheet)
		{
		}

		public virtual DocumentSupporter DocumentSupporter(CommonWorkSheet workSheet)
		{
			return null;
		}

		public virtual DocManagerInfo DocManagerInfo(CommonWorkSheet workSheet)
		{
			return new CartageWorkSheetDocManagerInfo(workSheet, Constants.DocManagerCodes.JobCartageRunSheet);
		}
	}
}
