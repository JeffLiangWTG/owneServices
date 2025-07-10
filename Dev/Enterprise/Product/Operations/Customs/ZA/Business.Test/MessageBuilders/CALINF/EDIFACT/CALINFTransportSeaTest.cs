using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFTransportSeaTest : TestCaseWithFactory
	{
		public void TestDataMapping()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_CarrierCode = "MSC";
			vessel.RV_RadioCallSign = "RCS01";
			vessel.RV_Code = "BlueOcean";
			vessel.RV_RN_NKCountryOfReg = "GB";
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_VoyageFlight = "VOY-123";
			AssertEquals("Prerequisite: Vessel must be specified", "BlueOcean", jobVoyage.Vessel.RV_Code);
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
			var transport = (ICALINFTransportInformation)new CALINFTransportSea(jobVoyage);
			AssertEquals("VOY-123", transport.ConveyanceNumber);
			AssertEquals("1", transport.TransportMode);
			AssertEquals("MSC", transport.CarrierCode);
			AssertEquals(ZString.Empty, transport.CarrierName);
			AssertEquals(CodeListResponsibleAgencyCodeList.BicBureauInternationalDesContaineurs, transport.CarrierCodeListResponsibleAgencyCode);
			AssertEquals("RCS01", transport.MeansOfTransportId);
			AssertEquals("BlueOcean", transport.MeansOfTransportName);
			AssertEquals("GB", transport.MeansOfTransportNationality);
			AssertEquals("103", transport.MeansOfTransportCodeListIdentificationCode);
			AssertEquals("VOY-123", transport.PrincipalCarrierConveyanceNumber);
			var responsibleParty = CodeListResponsibleAgencyCodeList.UnEceUnitedNationsEconomicCommissionForEurope;
			AssertEquals(2, transport.DepartureDetails.Count);
			var callInfo = transport.DepartureDetails[0];
			AssertEquals("DEHAM", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 20, 13, 30, 0), callInfo.CallDateTime);
			callInfo = transport.DepartureDetails[1];
			AssertEquals("GBLON", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 22, 14, 40, 0), callInfo.CallDateTime);
			AssertEquals(2, transport.CallDetails.Count);
			callInfo = transport.CallDetails[0];
			AssertEquals("ZAJHB", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 25, 15, 35, 0), callInfo.CallDateTime);
			callInfo = transport.CallDetails[1];
			AssertEquals("ZADUR", callInfo.CallLocation);
			AssertEquals("139", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 27, 17, 47, 0), callInfo.CallDateTime);
		}
	}
}
