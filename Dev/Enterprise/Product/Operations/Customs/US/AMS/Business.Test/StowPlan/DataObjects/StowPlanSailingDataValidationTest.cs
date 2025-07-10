using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class StowPlanSailingDataValidationTest : TestCaseWithFactory
	{
		public void TestCheckDeparture()
		{
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.Departure = "USLAX";
			AssertHasErrorContaining(sailingData.DepartureInfo, ListValidation.InvalidCodeError);
			sailingData.Departure = "AUMEL";
			AssertNoErrorContaining(sailingData.DepartureInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckArrivalTime()
		{
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.ArrivalTime = ZDateTime.Empty;
			AssertHasErrorContaining(sailingData.ArrivalTimeInfo, MandatoryValidation.MustBeEntered);
			sailingData.ArrivalTime = ZDateTime.Today.AddDays(20);
			AssertNoErrorContaining(sailingData.ArrivalTimeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckArrival()
		{
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.Arrival = ZString.Empty;
			AssertHasErrorContaining(sailingData.ArrivalInfo, MandatoryValidation.MustBeEntered);
			sailingData.Arrival = "XXXXX";
			AssertHasErrorContaining(sailingData.ArrivalInfo, ListValidation.InvalidCodeError);
			AssertNoErrorContaining(sailingData.ArrivalInfo, MandatoryValidation.MustBeEntered);
			sailingData.Arrival = "USLAX";
			AssertNoErrorContaining(sailingData.ArrivalInfo, ListValidation.InvalidCodeError);
		}

		public void TestNoBillsMsg()
		{
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.Validation.ValidateAll();
			AssertHasRowMessageError(sailingData, string.Format(StowPlanSailingDataValidation.NoBillsMsg, "USLAX"));

			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			sailingData.RebuildShipmentsCollection();
			sailingData.RebuidIssues();
			AssertNoRowMessageError(sailingData, string.Format(StowPlanSailingDataValidation.NoBillsMsg, "USLAX"));
		}

		JobVoyage voyage;
		protected override void SetUp()
		{
			base.SetUp();
			voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_A_DEP = ZDateTime.Today.AddDays(10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_A_ARV = ZDateTime.Today.AddDays(20);
			voyage.GenerateSailings();
		}
	}
}
