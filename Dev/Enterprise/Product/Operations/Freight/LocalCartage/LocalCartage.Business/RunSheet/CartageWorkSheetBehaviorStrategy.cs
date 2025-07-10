using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CartageWorkSheetBehaviorStrategy : CommonWorkSheetBehaviorStrategy
	{
		public CartageWorkSheetBehaviorStrategy()
		{
		}

		public override void SetDefaultValues(CommonWorkSheet workSheet)
		{
			workSheet.EY_StartTime = ZDateTime.Today;
			workSheet.EY_EndTime = workSheet.EY_StartTime.EndOfDay().AddSeconds(-59);
		}

		public override void VehicleAttached(CommonWorkSheet workSheet)
		{
			if (workSheet.EY_GS_NKTruckDriver.IsEmpty && workSheet.Truck != null && !workSheet.Truck.RQ_GS_NKPreferredDriver.IsEmpty)
			{
				workSheet.EY_GS_NKTruckDriver = workSheet.Truck.RQ_GS_NKPreferredDriver;
			}
		}

		public override DocumentSupporter DocumentSupporter(CommonWorkSheet workSheet)
		{
			return new CartageWorkSheetDocumentSupporter(workSheet);
		}
	}
}
