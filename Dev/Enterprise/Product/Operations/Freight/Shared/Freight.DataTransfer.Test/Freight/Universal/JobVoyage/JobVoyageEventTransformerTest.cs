using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.DataTransfer.Freight.Universal.Testing
{
	sealed class JobVoyageEventTransformerTest : TestCaseWithFactory
	{
		public void TestVoyageEventTransform_DepartureToStatusUpdated()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;

			var origin1 = voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUBNE";
			origin1.JA_A_DEP = ZDateTime.Now;

			var origin2 = voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_A_DEP = ZDateTime.Now;

			var dest = voyage.Destinations.AddNew();
			dest.JB_RL_NKPortOfDischarge = "NZAKL";

			var eventValue = new EventValue(Events.Departure, true, reference: "Dummy Description|FAC=CTO|LOC=AUSYD");

			var destEventValue = JobVoyageEventTransformer.Transform(eventValue, voyage);
			AssertEquals("STU event", Events.StatusUpdatedCode, destEventValue.Code);
			AssertEquals("STU description", Events.StatusUpdated.Description, destEventValue.Description);
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETD rejected")));
			Assert("RES parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("RES", "ETD received after ATD")));
			Assert("LOC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("LOC", "AUSYD")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(destEventValue.Reference));
		}

		public void TestVoyageEventTransform_ArrivalToStatusUpdated()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "LIAM HANRAHAN";
			vessel.RV_LloydsNumber = "LIAMTH";

			var voyage = UniversalTestHelper.CreateSeaVoyage(Factory, vessel.RV_Name, "1100");
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			voyage.JV_FlightDate = ZDateTime.Today;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var dest1 = voyage.Destinations.AddNew();
			dest1.JB_RL_NKPortOfDischarge = "HKHKG";
			dest1.JB_A_ARV = ZDateTime.Now;

			var dest2 = voyage.Destinations.AddNew();
			dest2.JB_RL_NKPortOfDischarge = "NZAKL";
			dest2.JB_A_ARV = ZDateTime.Now;

			var eventValue = new EventValue(Events.Arrival, true, reference: "Dummy Description|FAC=CTO|LOC=NZAKL");

			var destEventValue = JobVoyageEventTransformer.Transform(eventValue, voyage);
			AssertEquals("STU event", Events.StatusUpdatedCode, destEventValue.Code);
			AssertEquals("STU description", Events.StatusUpdated.Description, destEventValue.Description);
			Assert("TYP parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("TYP", "Change of ETA rejected")));
			Assert("RES parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("RES", "ETA received after ATA")));
			Assert("LOC parameter", destEventValue.Parameters.Contains(new KeyValuePair<string, string>("LOC", "NZAKL")));
			AssertEquals("FreeText", "Dummy Description", StmALog.GetFreeTextFromReference(destEventValue.Reference));
		}
	}
}
