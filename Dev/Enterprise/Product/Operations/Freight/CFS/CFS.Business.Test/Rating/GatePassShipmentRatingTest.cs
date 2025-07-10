using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class GatePassShipmentRatingTest : TestCaseWithFactory
	{
		public void TestJobDatesProvider()
		{
			var shipment = Factory.NewWithValidTestData<GatePassShipment>();
			AssertType<GatePassShipmentJobDateProvider>(shipment.RatingAdapter.JobDatesProvider);
		}

		public void TestAdapterTypeAndId()
		{
			var shipment = Factory.NewWithValidTestData<GatePassShipment>();
			AssertEquals(AdapterType.GatePass, shipment.RatingAdapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, shipment.RatingAdapter.OperationalJobCode);
		}

		public void TestAutoRatingJobServices()
		{
			var shipment = Factory.NewWithValidTestData<GatePassShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			Assert(!ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.CustomsHold));
			JobService customsHold = shipment.DocsAndCartage.Services.AddNew();
			customsHold.ES_ServiceCode = Constants.FreightServiceType.Codes.CustomsHold;
			Assert(!ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.CustomsHold));
			customsHold.ES_Completed = ZDateTime.Today;
			Assert(ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.CustomsHold));

			Assert(!ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.QuarantineInspection));
			JobService quarantine = shipment.DocsAndCartage.Services.AddNew();
			quarantine.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			quarantine.ES_Completed = ZDateTime.Today;
			Assert(ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.QuarantineInspection));

			Assert(!ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.Fumigation));
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = ZDateTime.Today;
			Assert(ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, Constants.FreightServiceType.Codes.Fumigation));

			Assert(!ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage));

			var pack = shipment.OuterPackLines.AddNew();
			var container = Factory.New<GatePassContainer>();
			var loadList = shipment.Consols.AddNew();
			loadList.Containers.Add(container);
			container.JC_JX = shipment.JS_JX;
			pack.SetContainer(container.PK);
			container.JC_LCLStorageCommences = ZDateTime.Today.AddDays(-1);
			Assert(ratingAdapter.JobServices.IsEnabled(ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage));
		}

		public void TestAutoRatingTime()
		{
			var shipment = Factory.NewWithValidTestData<GatePassShipment>();
			var loadList = shipment.Consols.AddNew();
			AssertMeasureTime(shipment, 0d);

			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_PackageCount = 20;
			pack.JL_Outturn = 15;
			pack.JL_F3_NKPackType = Constants.PkgUnit.Box;

			var container = loadList.Containers.AddNew();
			container.JC_JX = shipment.JS_JX;
			pack.SetContainer(container.PK);
			container.JC_LCLAvailable = ZDateTime.Today.AddDays(-3);
			container.JC_LCLStorageCommences = ZDateTime.Today;

			AssertMeasureTime(shipment, 1d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 4d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = false;
			shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Today.AddDays(-5);
			shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Today.AddDays(-1);
			AssertMeasureTime(shipment, 2d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 6d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = false;
			shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			container.JC_LCLAvailable = ZDateTime.Today.AddDays(-6);
			container.JC_LCLStorageCommences = ZDateTime.Today.AddDays(-3);
			AssertMeasureTime(shipment, 4d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 7d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = false;
			CommonPickupDeliveryConfirm delivery = shipment.DestinationCFSDepartures.AddNew();
			delivery.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-2);
			AssertMeasureTime(shipment, 2d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 5d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = false;
			delivery.EU_PickupDeliveryTime = ZDateTime.Today.AddDays(-5);
			AssertMeasureTime(shipment, 0d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 0d);

			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = false;
			container.JC_LCLStorageCommences = ZDateTime.Empty;
			container.JC_LCLAvailable = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Today.AddDays(-8);
			Env.Registry.Rating.IncludeCFSFreeStorageDaysInCalculation = true;
			AssertMeasureTime(shipment, 4d);
		}

		void AssertMeasureTime(GatePassShipment shipment, double expectedTotalDays)
		{
			var ratingAdapter = (GatePassShipmentRatingAdapter)shipment.RatingAdapter;
			AssertEquals(expectedTotalDays, ((RateableMeasureSet)ratingAdapter.RateableMeasures).Time.Span.TotalDays);
		}
	}
}
