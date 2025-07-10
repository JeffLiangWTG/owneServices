using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class ManifestMessageProviderTest : TestCaseWithFactory
	{
		public void TestSendManifestShouldNotThrowExceptionWhenAddVehicleVisitedCountry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var port = header.VisitedPorts.AddNew();
			port.CY_Order = 1;
			port.CY_Code = "TRMER-001";
			AssertNoExceptionThrown("Exception: Cannot add ticks to an invalid date", () => ((ITRCustomsMessageGenerator)new TRManifestMessageGenerator(new ManifestMessageProvider(header))).GenerateMessage());
		}

		public void TestSendWithCustomsCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusMapType("CNTRY", "OUT", "Country Code Mapping", false);
			helper.CreateCusMap("CNTRY", "DE", "004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.Turkey);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = TRManifestTypes.Codes.DEMIHR;
			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.Germany;
			var sumDec = new ManifestMessageProvider(header);
			AssertEquals("Send with customs country code rather than CW1 or commercial country code.", "004", sumDec.ConveyanceNationality);
		}

		public void TestSummaryDeclarationInformationMembers()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);

				ICarrierCompany carrierCompany = sumDec.CarrierCompany.FirstOrDefault();
				IVehicleVisitedCountry vehicleVisitedCountry = sumDec.VehicleVisitedCountry.FirstOrDefault();
				IOpeningSummaryDeclaration openingSummaryDeclaration = sumDec.OpeningSummaryDeclaration.FirstOrDefault();
				IBillofLading billofLading = sumDec.BillofLadings.FirstOrDefault();

				CombineAssertions("Header level", () =>
				{
					AssertEquals("BusinessRegNo", "test567890", sumDec.BusinessRegNo);
					AssertEquals("ManifestType", "DENİHR", sumDec.ManifestType);
					AssertEquals("Test Message for manifest descriptions", sumDec.Other);
					AssertEquals("Trailer1RegNo", "34 XXX 123", sumDec.Trailer1RegNo);
					AssertEquals("Trailer1RegCountry", "052", sumDec.Trailer1RegCountry);
					AssertEquals("Trailer2RegNo", "06 XXX 456", sumDec.Trailer2RegNo);
					AssertEquals("Trailer2RegCountry", "052", sumDec.Trailer2RegCountry);
					AssertEquals("NumberofBillsInDeclaration", 0, sumDec.NumberofBillsInDeclaration);
					AssertEquals("SafetySecurity", ZString.Empty, sumDec.SafetySecurity);
					AssertEquals("GroupBillofLadingNumber", ZString.Empty, sumDec.GroupBillofLadingNumber);
					AssertEquals("PresentationCustomsOffice", "041600", sumDec.PresentationCustomsOffice);
					AssertEquals("UserID", "1234test12", sumDec.UserID);
					AssertEquals("AgentType", TurkishConstants.AnswerNo, sumDec.AgentType);
					AssertEquals("CustomsDischargePort", "TRMRA-009", sumDec.CustomsDischargePort);
					AssertEquals("CustomsLoadPort", "TRALI-007", sumDec.CustomsLoadPort);
					AssertEquals("PreviousBillNumber", "BOL", sumDec.PreviousBillNumber);
					AssertEquals("Voyage", "VOYAGE", sumDec.Voyage);
					AssertEquals("LloydsNumber", "IMO NO", sumDec.LloydsNumber);
					AssertEquals("RegistrationNumber", ZString.Empty, sumDec.RegistrationNumber);
					AssertEquals("Nature", TurkishConstants.ShipmentTypeExport, sumDec.Nature);
					AssertEquals("TransportType", "10", sumDec.TransportType);
					AssertEquals("Vessel", "VESSEL NO", sumDec.Vessel);
					AssertEquals("CarrierBusinessRegNo", "1234567890", sumDec.CarrierBusinessRegNo);
					AssertEquals("TruckATAScorecardNumber", "TIR NO", sumDec.TruckATAScorecardNumber);
					AssertEquals("ConveyanceNationality", "728", sumDec.ConveyanceNationality);
					AssertEquals("CustomsDischargeCountryCode", "052", sumDec.CustomsDischargeCountryCode);
					AssertEquals("CustomsLoadCountryCode", "052", sumDec.CustomsLoadCountryCode);
					AssertEquals("LoadingUnloadingPlace", ZString.Empty, sumDec.LoadingUnloadingPlace);
					AssertEquals("CustomsOffice", "042200", sumDec.CustomsOffice);
					AssertEquals("DateAtCustomsOffice", ZDateTime.Today, sumDec.DateAtCustomsOffice);
					AssertEquals("XmlRefId", header.PK.ToString(), sumDec.XmlRefId);
				});
			}
		}

		public void TestUpdateIATACodes()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();
				var sumDec = new ManifestMessageProvider(header);

				CombineAssertions("For Transport Type SEA", () =>
				{
					AssertEquals("TRMRA-009", sumDec.CustomsDischargePort);
					AssertEquals("TRALI-007", sumDec.CustomsLoadPort);
				});

				header.AMA_TransportMode = TransportTypeList.Codes.Air;

				header.AMA_RL_NKPortOfLoading = "TRAJI";
				header.AMA_RL_NKPortOfDischarge = "TRAFY";

				CombineAssertions("For Transport Type AIR with Filled Existent Port Code", () =>
				{
					AssertEquals("AJI", sumDec.CustomsLoadPort);
					AssertEquals("AFY", sumDec.CustomsDischargePort);
				});

				header.AMA_RL_NKPortOfLoading = "";
				header.AMA_RL_NKPortOfDischarge = "";

				CombineAssertions("For Transport Type AIR with Empty Port Code", () =>
				{
					AssertEquals("", sumDec.CustomsLoadPort);
					AssertEquals("", sumDec.CustomsDischargePort);
				});

				header.AMA_RL_NKPortOfLoading = "XYZ";
				header.AMA_RL_NKPortOfDischarge = "ABC";

				CombineAssertions("For Transport Type AIR with Filled Inexistent Port Code", () =>
				{
					AssertEquals("", sumDec.CustomsLoadPort);
					AssertEquals("", sumDec.CustomsDischargePort);
				});
			}
		}

		public void TestCustomsLoadAndDischargePorts()
		{
			using (var helper = new ProviderTestHelper(Factory))
			{
				var header = helper.GetProviderHeader();

				var sumDec = new ManifestMessageProvider(header);

				header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;

				AssertEquals("SupportsCustomsPorts", true, ((IVisitedPortParent)header).SupportsCustomsPorts);

				header.AMA_RL_NKPortOfDischarge = "TRALI";
				header.AMA_CustomsDischargePort = "TRQWE";
				AssertEquals("TRQWE", sumDec.CustomsDischargePort);

				header.AMA_RL_NKPortOfDischarge = "USALI";
				header.AMA_CustomsDischargePort = "USQWE";
				AssertEquals("USALI", sumDec.CustomsDischargePort);

				header.AMA_RL_NKPortOfDischarge = "TRALI";
				header.AMA_CustomsDischargePort = "TRQWE";
				header.AMA_TransportMode = TransportTypeList.Codes.Road;
				header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				AssertEquals("TRALI", sumDec.CustomsDischargePort);

				header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.DENITH;

				header.AMA_RL_NKPortOfLoading = "TRAJI";
				header.AMA_CustomsLoadPort = "TRABC";
				AssertEquals("TRABC", sumDec.CustomsLoadPort);

				header.AMA_CustomsLoadPort = "USAJI";
				header.AMA_RL_NKPortOfLoading = "USABC";
				AssertEquals("USABC", sumDec.CustomsLoadPort);

				header.AMA_RL_NKPortOfLoading = "TRAJI";
				header.AMA_CustomsLoadPort = "TRABC";
				header.AMA_TransportMode = TransportTypeList.Codes.Road;
				header.AMA_ManifestType = TRManifestTypes.Codes.ATAIHR;
				AssertEquals("TRAJI", sumDec.CustomsLoadPort);
			}
		}

		public void TestSendWithGroupBillofLadingNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			CombineAssertions("Manifest Message Provider GRUPAJ Test", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVITH;
				header.TIRNumber = "TIR NO";
				var sumDec = new ManifestMessageProvider(header);
				AssertEquals("TruckATAScorecardNumber is not Empty", "TIR NO", sumDec.TruckATAScorecardNumber);
				AssertEquals("GroupBillofLadingNumber is Empty", ZString.Empty, sumDec.GroupBillofLadingNumber);

				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.TIRNumber = "GRUPAJNUMBER";
				sumDec = new ManifestMessageProvider(header);
				AssertEquals("GroupBillofLadingNumber is not Empty", "GRUPAJNUMBER", sumDec.GroupBillofLadingNumber);
				AssertEquals("TruckATAScorecardNumber is Empty", ZString.Empty, sumDec.TruckATAScorecardNumber);
			});
		}

		public void TestAddRefNoSegmentWhenManifestAmended()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			CombineAssertions("Send Amend Message", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.ATAITH;

				header.RegistrationNumber = "22340300IM12345678";
				header.RegistrationDate = new ZDateTime(2022, 1, 1);

				var sumDec = new ManifestMessageProvider(header);
				AssertEquals("Before Manifest Is Amended", ZString.Empty, sumDec.RegistrationNumber);

				header.ChangeToAmmendManifest();

				AssertEquals("After Manifest Is Amended Reg No Field Should Be Empty", ZString.Empty, header.RegistrationNumber);
				AssertEquals("After Manifest Is Amended Reg Date Field Should Be Empty", ZDateTime.Empty, header.RegistrationDate);

				sumDec = new ManifestMessageProvider(header);
				AssertEquals("After Manifest Is Amended", "22340300IM12345678", sumDec.RegistrationNumber);
			});
		}

		public void TestAddRefNo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				header.RegistrationNumber = "22340300IM12345678";
				header.RegistrationDate = new ZDateTime(2022, 1, 1);
				header.ChangeToAmmendManifest();

				var sumDec = new ManifestMessageProvider(header);
				AssertEquals("22340300IM12345678", sumDec.RegistrationNumber);
			});
		}

		public void TestIncludeTransportType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			CombineAssertions("Manifest Message Provider GRUPAJ Test", () =>
			{
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				var sumDec = new ManifestMessageProvider(header);
				AssertEquals("TransportType should not be Empty", "10", sumDec.TransportType);

				header.AMA_ManifestType = TRManifestTypes.Codes.GRUPAJ;
				sumDec = new ManifestMessageProvider(header);
				AssertEquals("TransportType should be Empty", ZString.Empty, sumDec.TransportType);

				header.AMA_TransportMode = TransportTypeList.Codes.Air;
				sumDec = new ManifestMessageProvider(header);
				AssertEquals("TransportType should not be Empty", "40", sumDec.TransportType);
			});
		}
	}
}
