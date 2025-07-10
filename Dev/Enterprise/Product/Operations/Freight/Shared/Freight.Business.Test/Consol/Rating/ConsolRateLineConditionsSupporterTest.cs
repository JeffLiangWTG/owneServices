using System.Linq;
using Enterprise.Freight.Business.Consol;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolRateLineConditionsSupporterTest : RateLineConditionsSupporterTest<ConsolRatingAdapter<CommonConsol>, ConsolRateLineConditionsSupporter>
	{
		protected override OrgHeader SetArrivalCFS(ConsolRatingAdapter<CommonConsol> adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_UnpackDepotAddress = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetDepartureCFS(ConsolRatingAdapter<CommonConsol> adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_PackDepotAddress = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetExportBroker(ConsolRatingAdapter<CommonConsol> adapter)
		{
			return null;
		}

		protected override OrgHeader SetImportBroker(ConsolRatingAdapter<CommonConsol> adapter)
		{
			return null;
		}

		protected override OrgHeader SetReceivingAgent(ConsolRatingAdapter<CommonConsol> adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetSendingAgent(ConsolRatingAdapter<CommonConsol> adapter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			return org;
		}

		protected override OrgHeader SetControllingAgent(ConsolRatingAdapter<CommonConsol> adapter)
		{
			return null;
		}

		protected override ConsolRatingAdapter<CommonConsol> GetInterfacedObject()
		{
			return Consol.GetRatingAdapters().FirstOrDefault() as ConsolRatingAdapter<CommonConsol>;
		}

		public override void TestHasDangerousGoods()
		{
			Consol.Shipments.RemoveAndDeleteAll();
			var shipment = Consol.Shipments.AddNew();
			shipment.OuterPackLines.RemoveAndDeleteAll();
			var supporter = new ShipmentRateLineConditionsSupporter(shipment);

			Assert("Consol's shipment has no dg", !supporter.HasDangerousGoods);

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.UNDGs.AddNew();

			Assert(supporter.HasDangerousGoods);
		}

		CommonConsol Consol
		{
			get { return consol ?? (consol = Factory.NewWithValidTestData<CommonConsol>()); }
		}
		CommonConsol consol;
	}
}
