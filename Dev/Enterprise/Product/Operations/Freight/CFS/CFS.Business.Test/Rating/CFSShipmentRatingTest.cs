using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentRatingTest : TestCaseWithFactory
	{
		public void TestJobDatesProvider()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			AssertType<CFSShipmentJobDatesProvider>(shipment.RatingAdapter.JobDatesProvider);
		}

		public void TestAdapterTypeAndId()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			AssertEquals(AdapterType.CFSShipment, shipment.RatingAdapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, shipment.RatingAdapter.OperationalJobCode);
		}

		public void TestAutoRatingFreightMode()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(FreightMode.SEA, ratingAdapter.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals(FreightMode.ROA, ratingAdapter.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Rail;
			AssertEquals(FreightMode.RAI, ratingAdapter.FreightMode);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals(FreightMode.AIR, ratingAdapter.FreightMode);
		}

		public void TestAutoRatingTime()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			var adapter = shipment.RatingAdapter;

			var fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Today;
			fumigation.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			AssertEquals(2d, adapter.JobServices.Time(chargeCode).Span.TotalHours);
		}

		public void TestContainerSpecialServices_HasContainerStorageCommencesDateSet_EnablesContainerStorageStorageService()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			chargeCode.AC_ChargeSubGroup = Constants.FreightServiceType.Codes.Fumigation;

			var shipment = Factory.NewWithValidTestData<CFSShipment>();

			var fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Today;
			fumigation.ES_Duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2);

			var adapter = shipment.RatingAdapter;

			AssertEquals(2d, adapter.JobServices.FindServices(chargeCode).FirstOrDefault().ServiceDuration.TotalHours);
		}
	}
}
