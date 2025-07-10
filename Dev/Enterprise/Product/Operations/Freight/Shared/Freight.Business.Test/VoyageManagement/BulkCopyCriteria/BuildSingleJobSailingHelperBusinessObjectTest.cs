using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BuildSingleJobSailingHelperBusinessObjectTest : TestCaseWithFactory
	{
		[TestDate(2006, 12, 12, 12, 0, 0)]
		public void TestBulkCopySchedulesDaily()
		{
			BuildSingleJobSailingHelper helper = new BuildSingleJobSailingHelper(Sailing);
			BaseJobSailing newSailing = helper.CopySchedule(TimeSpan.Zero);
			AssertNull("No Schedule is created as Schedule already existed", newSailing);

			newSailing = helper.CopySchedule(new TimeSpan(1, 0, 0, 0));
			AssertNotNull("Schedule is created", newSailing);
			AssertCopyAllRelevantInfo(newSailing);
			AssertAllScheduleDatesAreCorrect(newSailing, new TimeSpan(1, 0, 0, 0));

			helper.FilteredOrigin = Sailing.PortOfLoading.RL_Code;
			helper.FilteredDestination = Sailing.PortOfDischarge.RL_Code;
			newSailing = helper.CopySchedule(new TimeSpan(2, 0, 0, 0));
			AssertNotNull("Schedule is created", newSailing);
			AssertCopyAllRelevantInfo(newSailing);
			AssertAllScheduleDatesAreCorrect(newSailing, new TimeSpan(2, 0, 0, 0));
		}

		#region Implementation

		void AssertCopyAllRelevantInfo(BaseJobSailing newSailing)
		{
			AssertEquals("Transport Mode", Sailing.Voyage.JV_AirSeaRoad, newSailing.Voyage.JV_AirSeaRoad);
			AssertEquals("Carrier", Sailing.Voyage.JV_OH_Line, newSailing.Voyage.JV_OH_Line);
			AssertEquals("Discharge Port", Sailing.JX_JB_RL_NKPortOfDischarge, newSailing.JX_JB_RL_NKPortOfDischarge);
			AssertEquals("Load Port", Sailing.JX_JA_RL_NKPortOfLoading, newSailing.JX_JA_RL_NKPortOfLoading);
			AssertEquals("Flight No", Sailing.JX_JV_VoyageFlight, newSailing.JX_JV_VoyageFlight);
		}

		void AssertAllScheduleDatesAreCorrect(BaseJobSailing newSailing, TimeSpan interval)
		{
			AssertEquals("JX_DepotReceivalCommences", Sailing.JX_DepotReceivalCommences.Add(interval), newSailing.JX_DepotReceivalCommences);
			AssertEquals("JX_DepotCutOff", Sailing.JX_DepotCutOff.Add(interval), newSailing.JX_DepotCutOff);
			AssertEquals("JX_JB_CTOCutOff", Sailing.JX_JA_CTOCutOff.Add(interval), newSailing.JX_JA_CTOCutOff);
			AssertEquals("JX_JA_DocumentaryCutoff", Sailing.JX_JA_DocumentaryCutoff.Add(interval), newSailing.JX_JA_DocumentaryCutoff);
			AssertEquals("JX_DepotStorageDate", Sailing.JX_DepotStorageDate.Add(interval), newSailing.JX_DepotStorageDate);
			AssertEquals("JX_JA_E_DEP", Sailing.JX_JA_E_DEP.Add(interval), newSailing.JX_JA_E_DEP);
			AssertEquals("JX_JB_E_ARV", Sailing.JX_JB_E_ARV.Add(interval), newSailing.JX_JB_E_ARV);
			Assert("JB_A_ARV should not be specified.", newSailing.JX_JB_A_ARV.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Sailing = CreateSailing();
		}

		BaseJobSailing CreateSailing()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "CX123";
			voyage.JV_OH_Line = CreateCarrier().PK;

			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 12, 12, 12, 0, 0);
			origin.JA_CutOff = ZDateTime.Today;
			origin.JA_DocumentaryCutoff = new ZDateTime(2006, 12, 13, 13, 45, 0);

			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = new ZDateTime(2006, 12, 13, 10, 59, 0);
			destination.JB_A_ARV = ZDateTime.Now;

			voyage.GenerateSailings();
			BaseJobSailing sailing = voyage.Sailings[0];
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			sailing.JX_DepotReceivalCommences = new ZDateTime(2006, 10, 12, 12, 0, 0);
			sailing.JX_DepotCutOff = sailing.JX_DepotReceivalCommences;
			sailing.JX_DepotStorageDate = sailing.JX_JA_DocumentaryCutoff;
			Factory.Save();

			return sailing;
		}

		OrgHeader CreateCarrier()
		{
			Carrier = Factory.New<OrgHeader>();
			Carrier.OH_Code = "Carrier";
			Carrier.OH_IsShippingProvider = true;
			Carrier.OH_IsAirLine = true;

			return Carrier;
		}

		OrgHeader Carrier;
		BaseJobSailing Sailing;

		#endregion
	}
}
