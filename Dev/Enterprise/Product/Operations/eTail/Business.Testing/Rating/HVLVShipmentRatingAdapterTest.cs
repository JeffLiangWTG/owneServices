using System;
using System.Linq;
using Enterprise.eTail.Business.Rating;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVShipmentRatingAdapterTest : ShipmentRatingSupportTest
	{
		public void TestMergeCharges()
		{
			var adpter = GetAdapter();
			AssertEquals("MergeCharges should be HLSMerge.", MergeChargeOptions.HLSMerge, adpter.MergeCharges);
		}

		public void TestJobServices()
		{
			var adpter = GetAdapter();

			AssertEquals("JobServices count should be 4.", 4, adpter.JobServices.Count);
			AssertEquals("JobServices should have 2 Overweight Penalty service info objects.", 2, adpter.JobServices.Count(c => c.ServiceCode == ChargeCodeSubGroupList.OverweightPenalty));
			AssertEquals("JobServices should have 2 Overweight Surcharge service info objects.", 2, adpter.JobServices.Count(c => c.ServiceCode == ChargeCodeSubGroupList.OverweightSurcharge));
		}

		public void TestMeasures()
		{
			var adpter = GetAdapter();
			var measures = (RateableMeasureSet)adpter.RateableMeasures;
			foreach (MeasureType measureType in Enum.GetValues(typeof(MeasureType)))
			{
				AssertEquals("Measures should have nothing: " + measureType, false, measures.HasMeasureType(measureType));
			}
		}

		public void TestShouldRemoveCharge()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_ChargeSubGroup = ChargeCodeSubGroupList.OverweightPenalty;
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode1.AC_IsGroupageCharge = true;

			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_ChargeSubGroup = ChargeCodeSubGroupList.OverweightSurcharge;
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode2.AC_IsGroupageCharge = true;

			var chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode3.AC_ChargeSubGroup = ChargeCodeSubGroupList.Cod;
			chargeCode3.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			chargeCode3.AC_IsGroupageCharge = true;

			var adpter = GetAdapter();

			AssertEquals("ChargeCode is null.", true, adpter.ShouldRemoveCharge(null));
			AssertEquals("ChargeSubGroup is Overweight Penalty.", false, adpter.ShouldRemoveCharge(chargeCode1));
			AssertEquals("ChargeSubGroup is Overweight Surcharge.", false, adpter.ShouldRemoveCharge(chargeCode2));
			AssertEquals("ChargeSubGroup is not Overweight Penalty and Surcharge.", true, adpter.ShouldRemoveCharge(chargeCode3));
		}

		HVLVShipmentRatingAdapter GetAdapter()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var provider = new HVLVRatingAdapterProvider(shipment);
			var adpter = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue).OfType<HVLVShipmentRatingAdapter>().FirstOrDefault();

			return adpter;
		}

		public override void TestAdapterTypeAndID()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var provider = new HVLVRatingAdapterProvider(shipment);
			var adapter = provider.GetAdapters(null, AutoRateOptions.AutorateRevenue).OfType<HVLVShipmentRatingAdapter>().FirstOrDefault();
			AssertEquals(AdapterType.HVLVShipment, adapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, adapter.OperationalJobCode);
		}
	}
}
