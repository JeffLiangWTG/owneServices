using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D16A.Elements;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.CALINF.Testing
{
	sealed class CALINFTransportAirTest : TestCaseWithFactory
	{
		public void TestDataMapping()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "125";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MiscServ.OM_RM_Airline = airline.PK;
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "BA123";
			jobVoyage.JV_OH_Line = orgHeader.PK;
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
			var transport = (ICALINFTransportInformation)new CALINFTransportAir(jobVoyage);
			AssertEquals("BA123", transport.ConveyanceNumber);
			AssertEquals("4", transport.TransportMode);
			AssertEquals("125", transport.CarrierCode);
			AssertEquals(ZString.Empty, transport.CarrierName);
			AssertEquals(CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation, transport.CarrierCodeListResponsibleAgencyCode);
			AssertEquals(ZString.Empty, transport.MeansOfTransportId);
			AssertEquals(ZString.Empty, transport.MeansOfTransportName);
			AssertEquals(ZString.Empty, transport.MeansOfTransportNationality);
			AssertEquals("146", transport.MeansOfTransportCodeListIdentificationCode);
			AssertEquals("BA123", transport.PrincipalCarrierConveyanceNumber);
			var responsibleParty = CodeListResponsibleAgencyCodeList.IataInternationalAirTransportAssociation;
			AssertEquals(2, transport.DepartureDetails.Count);
			var callInfo = transport.DepartureDetails[0];
			AssertEquals("HAM", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 20, 13, 30, 0), callInfo.CallDateTime);
			callInfo = transport.DepartureDetails[1];
			AssertEquals("LON", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 05, 22, 14, 40, 0), callInfo.CallDateTime);
			AssertEquals(2, transport.CallDetails.Count);
			callInfo = transport.CallDetails[0];
			AssertEquals("JHB", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 25, 15, 35, 0), callInfo.CallDateTime);
			callInfo = transport.CallDetails[1];
			AssertEquals("DUR", callInfo.CallLocation);
			AssertEquals("145", callInfo.LocationCodeListIdentificationCode);
			AssertEquals(responsibleParty, callInfo.LocationCodeListResponsibleAgencyCode);
			AssertEquals(new ZDateTime(2020, 06, 27, 17, 47, 0), callInfo.CallDateTime);
		}
	}
}
