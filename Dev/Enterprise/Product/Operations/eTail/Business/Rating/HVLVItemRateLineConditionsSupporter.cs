using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business
{
	public class HVLVItemRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public HVLVItemRateLineConditionsSupporter(HVLVItem item)
			: base(item)
		{
		}

		public HVLVItem Item => ObjectToWrap as HVLVItem;

		protected override OrgHeader GetArrivalCFS() => Item.Shipment?.ArrivalConsol?.ArrivalUnpackCFSTransport;

		protected override OrgHeader GetControllingAgent() => Item.ETailer;

		protected override OrgHeader GetDepartureCFS() => Item.Shipment?.DepartureConsol?.DeparturePackCFSTransport;

		protected override OrgHeader GetExportBroker() => Item.ETailer;

		protected override bool GetHasDangerousGoods() => Item.Consignment.HVC_IsHazardous || Item.DGCodes.Any();

		protected override OrgHeader GetImportBroker() => Item.ETailer;

		protected override OrgHeader GetReceivingAgent() => Item.Consignment.LastMileCarrier;

		protected override OrgHeader GetSendingAgent() => Item.Consignment.LastMileCarrier;
	}
}
