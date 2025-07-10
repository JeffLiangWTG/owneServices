using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanSailingData))]
	class StowPlanSailingDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultFromVoyage()
		{
			var voyage = CreateVoyage("AUMEL", "USLAX", "USLAX", "NZABY");

			var sailingData = new StowPlanSailingData(voyage);
			AssertEquals("USLAX", sailingData.Arrival);
			AssertEquals(ZDateTime.Today.AddDays(2), sailingData.ArrivalTime);
			Assert(sailingData.IsArrivalTimeEstimated);
			AssertEquals("AUMEL", sailingData.Departure);
			AssertEquals(ZDateTime.Today.AddDays(1), sailingData.DepartureTime);
			Assert(sailingData.IsDepartureTimeEstimated);

			voyage = CreateVoyage("AUMEL", "USLAX", "USLAX", "USCHI");
			sailingData = new StowPlanSailingData(voyage);
			sailingData.Arrival = "USLAX";
			AssertEquals("AUMEL", sailingData.Departure);
			sailingData.Arrival = "USCHI";
			AssertEquals("AUMEL", sailingData.Departure);

			voyage = CreateVoyage("USLAX", "AUMEL", "AUMEL", "USCHI");
			sailingData = new StowPlanSailingData(voyage);
			sailingData.Arrival = "USCHI";
			AssertEquals("AUMEL", sailingData.Departure);
			sailingData.Arrival = "USLAX";
			AssertEquals(ZString.Empty, sailingData.Departure);
		}

		JobVoyage CreateVoyage(ZString firstLoad, ZString firstDischarge, ZString secondLoad, ZString secondDischarge)
		{
			var result = Factory.New<JobVoyage>();
			var origin = result.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = firstLoad;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			var destination = result.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = firstDischarge;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(2);
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = secondLoad;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(3);
			destination = result.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = secondDischarge;
			destination.JB_E_ARV = ZDateTime.Today.AddDays(4);
			return result;
		}

		public void TestCreateStowPlanMessage()
		{
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.CreateStowPlanMessage();
			destination.Messages.Load();
			AssertEquals(1, destination.Messages.Count);
			AssertEquals(MessageStatusListSTW.Codes.Sending, destination.StowPlanMessageStatus);
		}

		public void TestIStowPlanSailingDataMemebers()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "ALABTT";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(10);
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(20);
			voyage.GenerateSailings();

			var shipment1 = Factory.New<BillOfLading>();
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment1.JS_JX = voyage.Sailings[0].PK;

			var shipment2 = Factory.New<BillOfLading>();
			shipment2.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment2.JS_JX = voyage.Sailings[0].PK;

			var shipment3 = Factory.New<BillOfLading>();
			shipment3.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment3.JS_JX = voyage.Sailings[0].PK;

			IStowPlanSailingData sailingData = new StowPlanSailingData(voyage);
			((StowPlanSailingData)sailingData).Shipments.OfType<StowPlanShipmentData>().FirstOrDefault().Checked = false;

			AssertEquals("123", sailingData.VoyageNumber);
			AssertNotNull(sailingData.Vessel);
			AssertEquals(destination.JB_RL_NKPortOfDischarge, sailingData.Arrival);
			AssertEquals(origin.JA_RL_NKPortOfLoading, sailingData.Departure);
			AssertEquals(2, sailingData.Shipments.Count());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StowPlanSailingData(voyage);
		}

		JobVoyage voyage;
		VoyageOrigin origin;
		VoyageDestination destination;
		protected override void SetUp()
		{
			base.SetUp();
			voyage = Factory.New<JobVoyage>();
			origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(10);
			destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(20);
			voyage.GenerateSailings();

			var shipment = Factory.New<BillOfLading>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_JX = voyage.Sailings[0].PK;
		}
	}
}
