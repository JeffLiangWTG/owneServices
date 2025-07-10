using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Consol
{
	public class ConsolRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public ConsolRateLineConditionsSupporter(CommonConsol objectToWrap) : base(objectToWrap) { }

		CommonConsol Consol
		{
			get { return ObjectToWrap as CommonConsol; }
		}

		protected override OrgHeader GetDepartureCFS()
		{
			return Consol.PackDepotAddress != null ? Consol.PackDepotAddress.Header : null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			return Consol.UnpackDepotAddress != null ? Consol.UnpackDepotAddress.Header : null;
		}

		protected override OrgHeader GetExportBroker()
		{
			return null;
		}

		protected override OrgHeader GetImportBroker()
		{
			return null;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return Consol.SendingForwarder;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return Consol.ReceivingForwarder;
		}

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return Consol.Shipments.Cast<CommonShipment>().Any(s => s.OuterPackLines.Cast<PackLine>().Any(p => p.UNDGs.Any()));
		}
	}
}
