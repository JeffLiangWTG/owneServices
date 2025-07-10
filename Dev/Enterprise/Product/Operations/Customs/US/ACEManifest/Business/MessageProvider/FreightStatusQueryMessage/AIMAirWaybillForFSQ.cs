using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMAirWaybillForFSQ : IAIMAirWaybill
	{
		public AIMAirWaybillForFSQ(AsycudaBill bill, AsycudaArrivalHeader arrivalHeader)
		{
			this.bill = Argument.NotNull(bill, "bill");
			this.arrivalHeader = arrivalHeader;
		}

		readonly AsycudaBill bill;
		readonly AsycudaArrivalHeader arrivalHeader;

		public ZString AirWaybillPrefix => GetFormattedMAWB().Left(3);
		public ZString AWBSerialNumber => GetFormattedMAWB().SubstringSafe(3, 8);
		public ZString HAWBNumber => bill.IsChildMasterBill ? ZString.Empty : bill.ABL_BillNumber.Left(12);
		public ZString PartArrivalReference => arrivalHeader?.ATH_Reference ?? ZString.Empty;

		ZString GetFormattedMAWB()
		{
			return bill.Header?.AMA_MasterBill.KeepAlphanumericCharacters() ?? ZString.Empty;
		}

		public ZString PackageTrackingIdentifier => ZString.Empty;
		public ZBool IsMasterAirWaybill => false;
	}
}
