using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMAirWaybill : IAIMAirWaybill
	{
		public AIMAirWaybill(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "bill");
		}

		readonly protected AsycudaBill bill;

		public ZString AirWaybillPrefix => GetFormattedMAWB().Left(3);
		public ZString AWBSerialNumber => GetFormattedMAWB().SubstringSafe(3, 8);
		public ZBool IsMasterAirWaybill => IsMasterAirWaybillCore;
		protected virtual ZBool IsMasterAirWaybillCore => false;
		public ZString HAWBNumber => bill.IsChildMasterBill ? ZString.Empty : bill.ABL_BillNumber.Left(12);
		public ZString PackageTrackingIdentifier => ZString.Empty;
		public ZString PartArrivalReference => ZString.Empty;

		ZString GetFormattedMAWB()
		{
			return bill.Header?.AMA_MasterBill.KeepAlphanumericCharacters() ?? ZString.Empty;
		}
	}
}
