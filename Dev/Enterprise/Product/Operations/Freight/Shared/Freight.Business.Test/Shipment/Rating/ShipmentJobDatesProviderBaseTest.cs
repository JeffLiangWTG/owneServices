using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class ShipmentJobDatesProviderBaseTest : TestCaseWithFactory
	{
		public void TestVesselDepartureAndArrivalDates()
		{
			var shipmentEstimatedDepartureDate = new DateTime(2012, 8, 11);
			var shipmentEstimatedArrivalDate = new DateTime(2012, 8, 20);
			var voyageOriginEstimatedDepartureDate = new DateTime(2012, 8, 13);
			var voyageOriginActualDepartureDate = new DateTime(2012, 8, 14);
			var voyageOriginEstimatedArrivalDate = new DateTime(2012, 8, 12);
			var voyageOriginActualArrivalDate = new DateTime(2012, 8, 12);
			var voyageDestEstimatedArrivalDate = new DateTime(2012, 8, 18);
			var voyageDestActualArrivalDate = new DateTime(2012, 8, 19);

			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_E_DEP = shipmentEstimatedDepartureDate;
			shipment.JS_E_ARV = shipmentEstimatedArrivalDate;

			var origin = Factory.NewWithValidTestData<VoyageOrigin>();
			origin.JA_E_DEP = voyageOriginEstimatedDepartureDate;
			origin.JA_A_DEP = voyageOriginActualDepartureDate;
			origin.JA_E_ARV = voyageOriginEstimatedArrivalDate;
			origin.JA_A_ARV = voyageOriginActualArrivalDate;
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destn = Factory.NewWithValidTestData<VoyageDestination>();
			destn.JB_E_ARV = voyageDestEstimatedArrivalDate;
			destn.JB_A_ARV = voyageDestActualArrivalDate;
			destn.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destn.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			CommonShipment shipmentReloaded;
			JobDatesProvider<CommonShipment> jobDatesProvider;

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(shipmentEstimatedDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(shipmentEstimatedArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = sailing.PK;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(voyageOriginActualDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(voyageDestActualArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			origin.JA_A_DEP = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(voyageOriginEstimatedDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(voyageDestActualArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			origin.JA_E_DEP = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(shipmentEstimatedDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(voyageDestActualArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			destn.JB_A_ARV = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(shipmentEstimatedDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(voyageDestEstimatedArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			destn.JB_E_ARV = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(shipmentEstimatedDepartureDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(shipmentEstimatedArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			shipment.JS_E_DEP = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(shipmentEstimatedArrivalDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));

			shipment.JS_E_ARV = ZDateTime.Empty;
			Factory.Save();

			shipmentReloaded = factory2.Load<CommonShipment>(shipment.PK);
			jobDatesProvider = new CommonShipmentJobDatesProvider(shipmentReloaded);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));
		}

		[TestDate(2023, 4, 4)]
		public void TestRevenueAutoratingDate()
		{
			var testDate = ZDate.Today.AddDays(2);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.RevenueAutoratingDateOverride));

			shipment.RevenueAutoratingDate = testDate;
			AssertEquals(testDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.RevenueAutoratingDateOverride));
		}
	}
}
