using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class AddInfoCUSDECValidationTest : AddInfoJobDeclarationValidationTest
	{
		public void TestOutwardVessel()
		{
			//wrong transport mode
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			//empty vessel
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
			//list validation
			AddInfoJobDeclaration.SG_OutwardVesselName = "ADM";
			Validation.ValidateSG_OutwardVesselName();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVesselNameInfo.HasMessageErrors());
		}

		public void TestOutwardVoyageNo()
		{
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "123";
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "";
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals("For TN4.1 Road transport mode requires Vehicle licence", true, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "";
			AddInfoJobDeclaration.SG_OutwardFolio = "";
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals("For air transport, flight no or chartered aircraft rego required", true, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardFolio = "TNJ-498";
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestOutwardFlightNo()
		{
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(true, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardVoyageFlightNo = "QF123";
			Validation.ValidateSG_OutwardVoyageFlightNo();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardVoyageFlightNoInfo.HasMessageErrors());
		}

		public void TestInwardVesselBerth()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKInwardVesselBerth = "KZ";
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKInwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Validation.ValidateSG_US_NKInwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKInwardVesselBerthInfo.HasMessageErrors());
		}

		public void TestOutwardVesselBerth()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			AddInfoJobDeclaration.Lookups.SGCPlacesList.Load();
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKOutwardVesselBerth = "KZ";
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKOutwardVesselBerth = "ABC";
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Validation.ValidateSG_US_NKOutwardVesselBerth();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKOutwardVesselBerthInfo.HasMessageErrors());
		}

		public void TestNextPortOfCall()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.Lookups.SGLocoList.Load();
			Validation.ValidateSG_RL_NKNextPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKNextPortOfCallInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Validation.ValidateSG_RL_NKNextPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKNextPortOfCallInfo.HasMessageErrors());
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_RL_NKNextPortOfCall = "ZZXZZ";
			Validation.ValidateSG_RL_NKNextPortOfCall();
			AssertEquals(true, Declaration.SG_RL_NKNextPortOfCallInfo.HasMessageErrors());
			Declaration.SG_RL_NKNextPortOfCall = "MYKUL";
			Validation.ValidateSG_RL_NKNextPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKNextPortOfCallInfo.HasMessageErrors());
		}

		public void TestFinalPortOfCall()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.Lookups.SGLocoList.Load();
			Validation.ValidateSG_RL_NKFinalPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKFinalPortOfCallInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Validation.ValidateSG_RL_NKFinalPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKFinalPortOfCallInfo.HasMessageErrors());
			InvoiceLine.JI_Tariff = "24011010";
			Validation.ValidateSG_RL_NKFinalPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKFinalPortOfCallInfo.HasMessageErrors());
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			Declaration.SG_RL_NKFinalPortOfCall = "ZZXZZ";
			Validation.ValidateSG_RL_NKFinalPortOfCall();
			AssertEquals(true, Declaration.SG_RL_NKFinalPortOfCallInfo.HasMessageErrors());
			Declaration.SG_RL_NKFinalPortOfCall = "MYKUL";
			Validation.ValidateSG_RL_NKFinalPortOfCall();
			AssertEquals(false, Declaration.SG_RL_NKFinalPortOfCallInfo.HasMessageErrors());
		}

		public void TestPlaceOfRelease()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Validation.ValidateSG_US_NKPlaceOfCargoRelease();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKPlaceOfCargoReleaseInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKPlaceOfCargoRelease = "KZ";
			Validation.ValidateSG_US_NKPlaceOfCargoRelease();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKPlaceOfCargoReleaseInfo.HasMessageErrors());
		}

		public void TestConsigneeValidationIsTriggered()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "LW1", SGCPlaces.Constants.PremiseType.LicensedWarehouse, "LW1");
			Factory.Save();
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.SG_US_NKPlaceOfCargoRelease = "LW1";
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = false;
			AssertEquals(true, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.KeppelFTZ;
			AssertEquals(false, Declaration.JE_OH_ConsigneeInfo.HasMessageErrors());
		}

		public void TestPlaceOfReceipt()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Validation.ValidateSG_US_NKPlaceOfReceipt();
			AssertEquals(true, AddInfoJobDeclaration.SG_US_NKPlaceOfReceiptInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKPlaceOfReceipt = "KZ";
			Validation.ValidateSG_US_NKPlaceOfReceipt();
			AssertEquals(false, AddInfoJobDeclaration.SG_US_NKPlaceOfReceiptInfo.HasMessageErrors());
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_IsConsignee = true;
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "12345678901J");
			importer.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "TEST-UEN");
			AddInfoJobDeclaration.Declaration.JE_OH_Importer = importer.PK;
			AddInfoJobDeclaration.Declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			AddInfoJobDeclaration.Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.MajorExporterScheme;
			Validation.ValidateSG_US_NKPlaceOfReceipt();
			AssertEquals(false, AddInfoJobDeclaration.Declaration.JE_OH_ImporterInfo.HasErrors());
		}

		public void TestCheckSG_PreviousPermitNo()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			var messageError = "A Previous Permit Number is required when Place of Receipt indicates a Short Payment";
			Declaration.AddInfoValidation.ValidateSG_PreviousPermitNo();
			AssertNoMessageErrors(Declaration.SG_PreviousPermitNoInfo);
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.PremiseType.BondedWarehouse;
			AssertNoMessageErrors(Declaration.SG_PreviousPermitNoInfo);
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates;
			AssertHasMessageError(Declaration.SG_PreviousPermitNoInfo, messageError);
			Declaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates;
			AssertHasMessageError(Declaration.SG_PreviousPermitNoInfo, messageError);
			Declaration.SG_PreviousPermitNo = "TEST";
			AssertNoMessageErrors(Declaration.SG_PreviousPermitNoInfo);
		}

		public void TestNoOfCrew()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Validation.ValidateSG_NoOfCrew();
			AssertEquals(false, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_NoOfCrew = -1;
			AssertEquals(true, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasErrors());
			AddInfoJobDeclaration.SG_NoOfCrew = 600;
			AssertEquals(false, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasErrors());
			AssertEquals(true, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasWarnings());
			AddInfoJobDeclaration.SG_NoOfCrew = 0;
			AssertEquals(false, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasWarnings());
			AssertEquals(false, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_IsSeaStore = true;
			Validation.ValidateSG_NoOfCrew();
			AssertEquals(true, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_NoOfCrew = 10;
			AssertEquals(false, AddInfoJobDeclaration.SG_NoOfCrewInfo.HasMessageErrors());
		}

		public void TestVoyageDuration()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Validation.ValidateSG_VoyageDuration();
			AssertEquals(false, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_VoyageDuration = -1;
			AssertEquals(true, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasErrors());
			AddInfoJobDeclaration.SG_VoyageDuration = 600;
			AssertEquals(false, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasErrors());
			AssertEquals(true, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasWarnings());
			AddInfoJobDeclaration.SG_VoyageDuration = 0;
			AssertEquals(false, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasWarnings());
			AssertEquals(false, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_IsSeaStore = true;
			Validation.ValidateSG_VoyageDuration();
			AssertEquals(true, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_VoyageDuration = 10;
			AssertEquals(false, AddInfoJobDeclaration.SG_VoyageDurationInfo.HasMessageErrors());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreatePortCusCodeList(helper, "MYKUL", "KUALA LUMPUR");
			Factory.Save();
		}

		public static void CreatePortCusCodeList(Universal.Testing.UniversalReferenceTestDataHelper helper, string port, string description)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, port, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
