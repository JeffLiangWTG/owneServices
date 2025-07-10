using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMAirWaybillForArrival : AIMAirWaybill
	{
		public AIMAirWaybillForArrival(AsycudaArrivalHeader arrivalHeader, AsycudaBill bill) : base(bill)
		{
			this.arrivalHeader = Argument.NotNull(arrivalHeader, "arrivalHeader");
		}
		readonly AsycudaArrivalHeader arrivalHeader;
		protected override ZBool IsMasterAirWaybillCore => bill.IsChildMasterBill && !arrivalHeader.ATH_Reference.IsEmpty;
	}
}
