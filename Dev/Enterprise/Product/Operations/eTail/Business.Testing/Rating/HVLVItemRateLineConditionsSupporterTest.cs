using Enterprise.eTail.Business.Rating;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVItemRateLineConditionsSupporterTest : RateLineConditionsSupporterTest<HVLVItemRatingAdapter, HVLVItemRateLineConditionsSupporter>
	{
		public override void TestHasDangerousGoods()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			var supporter = new HVLVItemRateLineConditionsSupporter(item);

			AssertEquals(false, supporter.HasDangerousGoods);

			consignment.HVC_IsHazardous = true;
			AssertEquals(true, supporter.HasDangerousGoods);

			consignment.HVC_IsHazardous = false;
			AssertEquals(false, supporter.HasDangerousGoods);

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Code = "AA";

			var dgItem = item.UNDGs.AddNew();
			dgItem.DI_DG = substance.PK;

			AssertEquals(true, supporter.HasDangerousGoods);
		}

		protected override HVLVItemRatingAdapter GetInterfacedObject()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			var item = consignment.Items.AddNew();
			return new HVLVItemRatingAdapter(item, consignment, shipment);
		}

		protected override OrgHeader SetArrivalCFS(HVLVItemRatingAdapter adapter)
		{
			return null;
		}

		protected override OrgHeader SetControllingAgent(HVLVItemRatingAdapter adapter)
		{
			return adapter.Parent.Consignment.BookingHeader.BillToParty.Header;
		}

		protected override OrgHeader SetDepartureCFS(HVLVItemRatingAdapter adapter)
		{
			return null;
		}

		protected override OrgHeader SetExportBroker(HVLVItemRatingAdapter adapter)
		{
			return adapter.Parent.Consignment.BookingHeader.BillToParty.Header;
		}

		protected override OrgHeader SetImportBroker(HVLVItemRatingAdapter adapter)
		{
			return adapter.Parent.Consignment.BookingHeader.BillToParty.Header;
		}

		protected override OrgHeader SetReceivingAgent(HVLVItemRatingAdapter adapter)
		{
			return SetConsignmentLastMileCarrier(adapter.Parent.Consignment);
		}

		protected override OrgHeader SetSendingAgent(HVLVItemRatingAdapter adapter)
		{
			return SetConsignmentLastMileCarrier(adapter.Parent.Consignment);
		}

		OrgHeader SetConsignmentLastMileCarrier(HVLVConsignment consignment)
		{
			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;
			return lastMileCarrier;
		}
	}
}
