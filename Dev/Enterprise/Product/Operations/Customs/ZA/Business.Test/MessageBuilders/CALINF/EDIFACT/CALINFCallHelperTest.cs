using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFCallHelperTest : TestCaseWithFactory
	{
		public void TestPrepareDepartureData_Sea()
		{
			var jobVoyage = PrepareJobVoyage();
			var responsibleParty = CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope;
			var helper = new CALINFCallHelper("139", responsibleParty);
			var callInfoList = helper.PrepareDepartureData(jobVoyage.Origins);
			AssertEquals(2, callInfoList.Count);
			var callInfo = callInfoList[0];
			AssertEquals("DEHAM", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 20, 13, 30, 0), callInfo.CallDateTime);
			callInfo = callInfoList[1];
			AssertEquals("GBLON", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 22, 14, 40, 0), callInfo.CallDateTime);
		}

		public void TestPrepareDepartureData_Air()
		{
			var jobVoyage = PrepareJobVoyage();
			var responsibleParty = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			var helper = new CALINFCallHelper("145", responsibleParty, mustConvertToIataLocationCode: true, jobVoyage.Factory);
			var callInfoList = helper.PrepareDepartureData(jobVoyage.Origins);
			AssertEquals(2, callInfoList.Count);
			var callInfo = callInfoList[0];
			AssertEquals("HAM", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 20, 13, 30, 0), callInfo.CallDateTime);
			callInfo = callInfoList[1];
			AssertEquals("LON", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 22, 14, 40, 0), callInfo.CallDateTime);
		}

		public void TestPrepareDestinationData_Sea()
		{
			var jobVoyage = PrepareJobVoyage();
			var responsibleParty = CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope;
			var helper = new CALINFCallHelper("139", responsibleParty);
			var callInfoList = helper.PrepareDestinationData(jobVoyage.Destinations);
			AssertEquals(2, callInfoList.Count);
			var callInfo = callInfoList[0];
			AssertEquals("ZAJHB", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 25, 15, 35, 0), callInfo.CallDateTime);
			callInfo = callInfoList[1];
			AssertEquals("ZADUR", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 27, 17, 47, 0), callInfo.CallDateTime);
		}

		public void TestPrepareDestinationData_Air()
		{
			var jobVoyage = PrepareJobVoyage();
			var responsibleParty = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			var helper = new CALINFCallHelper("145", responsibleParty, mustConvertToIataLocationCode: true, jobVoyage.Factory);
			var callInfoList = helper.PrepareDestinationData(jobVoyage.Destinations);
			AssertEquals(2, callInfoList.Count);
			var callInfo = callInfoList[0];
			AssertEquals("JHB", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 25, 15, 35, 0), callInfo.CallDateTime);
			callInfo = callInfoList[1];
			AssertEquals("DUR", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 27, 17, 47, 0), callInfo.CallDateTime);
		}

		JobVoyage PrepareJobVoyage()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			var origin = jobVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "DEHAM";
			origin.JA_A_DEP = new ZDateTime(2020, 05, 20, 13, 30, 0);
			origin.JA_E_DEP = new ZDateTime(2020, 05, 21, 13, 50, 0);
			origin = jobVoyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "GBLON";
			origin.JA_A_DEP = ZDateTime.Empty;
			origin.JA_E_DEP = new ZDateTime(2020, 05, 22, 14, 40, 0);
			var destination = jobVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "ZAJHB";
			destination.JB_A_ARV = new ZDateTime(2020, 06, 25, 15, 35, 0);
			destination.JB_E_ARV = new ZDateTime(2020, 06, 26, 16, 46, 0);
			destination = jobVoyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "ZADUR";
			destination.JB_A_ARV = ZDateTime.Empty;
			destination.JB_E_ARV = new ZDateTime(2020, 06, 27, 17, 47, 0);
			return jobVoyage;
		}
	}
}
