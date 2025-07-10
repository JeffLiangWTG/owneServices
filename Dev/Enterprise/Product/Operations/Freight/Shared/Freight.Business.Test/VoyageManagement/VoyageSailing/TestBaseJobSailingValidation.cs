using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobSailing))]
	sealed class BaseJobSailingValidationTest : EnterpriseBusinessObjectTestCase
	{
		#region TestValidateJX_DepotCutOff
		public void TestValidateJX_DepotCutOff()
		{
			Sailing.JX_IsPublished = true;
			Sailing.JX_DepotCutOff = ZDateTime.Empty;
			Origin.JA_CutOff = ZDateTime.Empty;
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoWarnings("CFS is empty, no warning expected", Sailing.JX_DepotCutOffInfo);

			ZDateTime currentDay = ZDateTime.Today;
			Sailing.JX_DepotCutOff = currentDay;
			Origin.JA_CutOff = currentDay;
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoWarnings("CTO cut off is same as CFS cut off, no warning expected", Sailing.JX_DepotCutOffInfo);

			Sailing.JX_DepotCutOff = currentDay;
			Origin.JA_CutOff = currentDay.AddDays(1);
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoWarnings("CTO cut off is after CFS cut off, no warning expected", Sailing.JX_DepotCutOffInfo);

			Origin.JA_CutOff = currentDay.AddDays(-2);
			Sailing.JX_DepotCutOff = currentDay;
			Origin.JA_E_DEP = currentDay.AddDays(-1);
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertHasErrors("CFS cut off is after ETD, error expected", Sailing.JX_DepotCutOffInfo);

			Sailing.JX_DepotCutOff = currentDay.AddDays(-1);
			Origin.JA_E_DEP = currentDay.AddDays(-1);
			Origin.JA_CutOff = currentDay.AddDays(-1);
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoErrors("CFS cut off is same as ETD, no error expected", Sailing.JX_DepotCutOffInfo);

			Sailing.JX_DepotCutOff = currentDay.AddDays(-2);
			Origin.JA_E_DEP = currentDay.AddDays(-1);
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoErrors("CFS cut off is before ETD, no error expected", Sailing.JX_DepotCutOffInfo);
		}

		public void TestValidateJX_DepotCutOffAllowsCutOffToBeSameDayAsETD()
		{
			Sailing.JX_DepotCutOff = ZDateTime.Today.AddHours(10);
			Origin.JA_E_DEP = ZDateTime.Today;
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertNoErrors("CFS cut off is after ETD but on same day - no error expected", Sailing.JX_DepotCutOffInfo);

			Sailing.JX_DepotCutOff = ZDateTime.Today.AddDays(1);
			Origin.JA_E_DEP = ZDateTime.Today;
			Sailing.Validation.ValidateJX_DepotCutOff();
			AssertHasErrors("CFS cut off has later date part than ETD - error expected", Sailing.JX_DepotCutOffInfo);
		}
		#endregion

		#region Test JX_JA_CTOReceivalCommences

		public void TestValidateJX_JA_CTOReceivalCommences()
		{
			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Empty;
			Assert("JX_JA_CTOReceivalCommences is empty, no warning expected", !Sailing.JX_JA_CTOReceivalCommencesInfo.HasWarnings());

			Sailing.Origin.JA_ReceivalCommences = new DateTime(2013, 11, 27);
			Assert("JX_JA_CTOReceivalCommences has valid date, no error expected", !Sailing.JX_JA_CTOReceivalCommencesInfo.HasErrors());
		}

		#endregion

		#region Test JX_DepotReceivalCommences

		public void TestValidateJX_DepotReceivalCommences()
		{
			Sailing.JX_DepotReceivalCommences = ZDateTime.Empty;
			Assert("JX_DepotReceivalCommences is empty, no warning expected", !Sailing.JX_DepotReceivalCommencesInfo.HasWarnings());

			Sailing.JX_DepotReceivalCommences = DateTime.Today;
			Assert("JX_DepotReceivalCommences has valid date, no error expected", !Sailing.JX_DepotReceivalCommencesInfo.HasErrors());
		}

		#endregion

		#region TestValidateJX_DepotStorageDate

		public void TestValidateJX_DepotStorageDate()
		{
			Sailing.JX_DepotAvailabilityDate = ZDateTime.Empty;
			Sailing.JX_DepotStorageDate = ZDateTime.Empty;
			Assert("CFS Storage date is empty, no warning expected", !Sailing.JX_DepotStorageDateInfo.HasWarnings());

			Sailing.JX_DepotAvailabilityDate = ZDateTime.Today.AddDays(1);
			Sailing.JX_DepotStorageDate = ZDateTime.Today;
			Assert("CFS Storage date is before availability date, error expected", Sailing.JX_DepotStorageDateInfo.HasErrors());

			Sailing.JX_DepotAvailabilityDate = ZDateTime.Today;
			Sailing.JX_DepotStorageDate = ZDateTime.Today;
			Assert("CFS Storage date is same as availability date,no error expected", !Sailing.JX_DepotStorageDateInfo.HasErrors());

			Sailing.JX_DepotAvailabilityDate = ZDateTime.Today;
			Sailing.JX_DepotStorageDate = ZDateTime.Today.AddDays(1);
			Assert("CFS Storage date is after availability date, no error expected", !Sailing.JX_DepotStorageDateInfo.HasErrors());
		}

		#endregion

		#region Test JX_OnlineScheduleStatus

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestValidateJX_OnlineScheduleStatus_PartiallyMatched()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				GenerateVoyagesAndSailings();
				Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				Origin.JA_E_DEP = ZDate.Today;
				Destination.JB_E_ARV = ZDate.Today.AddDays(1);

				var sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
				sailing.TryMatchAgainstOnlineFlights();

				Assert("Mocked matcher returns a Partially Matched status so warnings expected.", sailing.JX_OnlineScheduleStatusInfo.HasWarnings());
			}, Constants.FlightScheduleStatus.PartiallyMatched);
		}

		[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
		public void TestValidateJX_OnlineScheduleStatus_Matched()
		{
			OnlineFlightMatchingHelperTest.RunTestWithMockedS8Matcher(() =>
			{
				GenerateVoyagesAndSailings();
				Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
				Origin.JA_E_DEP = ZDate.Today;
				Destination.JB_E_ARV = ZDate.Today.AddDays(1);

				var sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
				sailing.TryMatchAgainstOnlineFlights();

				AssertNoWarnings("Mocked matcher returns a Matched status so no warnings expected.", sailing.JX_OnlineScheduleStatusInfo);
			});
		}

		#endregion

		#region Implementation

		JobSailing Sailing;
		JobSailing Sailing2;
		VoyageOrigin Origin;
		VoyageOrigin Origin2;
		VoyageDestination Destination;
		JobVoyage Voyage;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in GenerateVoyagesAndSailings")]
		SailingsForTestClasses Helper;

		protected override void SetUp()
		{
			base.SetUp();
			GenerateVoyagesAndSailings();
		}

		void GenerateVoyagesAndSailings()
		{
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			Voyage.JV_VoyageFlight = "234";
			Voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Origin2 = Voyage.Origins.AddNew();
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";

			Destination = Voyage.Destinations.AddNew();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			Sailing2 = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "USLAX");

			if (Sailing == null)
			{
				Fail("Sailing not set");
			}

			if (Sailing2 == null)
			{
				Fail("Sailing2 not set");
			}

			Factory.Save();

			Helper = new SailingsForTestClasses(Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in JX_JA_RL_NKPortOfLoadingInfo_ValueChanged")]
		int JX_JA_RL_NKPortOfLoadingInfo_ChangeCount;
		void JX_JA_RL_NKPortOfLoadingInfo_ValueChanged(object sender, EventArgs e)
		{
			JX_JA_RL_NKPortOfLoadingInfo_ChangeCount++;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			JobVoyage voyage1 = factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage1.JV_VoyageFlight = "234";
			voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First().RV_FK;

			VoyageOrigin origin1 = factory.New<VoyageOrigin>();
			origin1.JA_JV = voyage1.PK;
			origin1.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination1 = factory.New<VoyageDestination>();
			destination1.JB_JV = voyage1.PK;
			destination1.JB_RL_NKPortOfDischarge = "USLAX";

			BaseJobSailing sailing1 = factory.New<BaseJobSailing>();
			sailing1.JX_JA = origin1.PK;
			sailing1.JX_JB = destination1.PK;

			return sailing1;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Voyage = Factory.New<JobVoyage>();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;

			Origin = Factory.New<VoyageOrigin>();
			Origin.JA_JV = Voyage.PK;
			Origin.JA_RL_NKPortOfLoading = "AUSYD";

			Destination = Factory.New<VoyageDestination>();
			Destination.JB_JV = Voyage.PK;
			Destination.JB_RL_NKPortOfDischarge = "USLAX";

			Sailing = Factory.New<JobSailing>();
			Sailing.JX_JA = Origin.PK;
			Sailing.JX_JB = Destination.PK;

			Factory.Save();

			return Sailing;
		}

		#endregion
	}
}
