using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidation_OUTTest : AddInfoCUSDECValidationTest
	{
		public void TestEntryYear()
		{
			Validation.ValidateSG_EntryYear();
			AssertEquals(false, AddInfoJobDeclaration.SG_EntryYearInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AddInfoJobDeclaration.SG_Cert1Type = "9";
			Validation.ValidateSG_EntryYear();
			AssertEquals("Optional for TX cert types 9 & 18", false, AddInfoJobDeclaration.SG_EntryYearInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			AddInfoJobDeclaration.SG_Cert1Type = "9";
			AddInfoJobDeclaration.SG_EntryYear = 2007;
			Validation.ValidateSG_EntryYear();
			AssertEquals("Optional for TX cert types 9 & 18", false, AddInfoJobDeclaration.SG_EntryYearInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			AddInfoJobDeclaration.SG_Cert1Type = "1";
			AddInfoJobDeclaration.SG_EntryYear = 2007;
			Validation.ValidateSG_EntryYear();
			AssertEquals("Not applicable all others", true, AddInfoJobDeclaration.SG_EntryYearInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			AddInfoJobDeclaration.SG_EntryYear = 2007;
			Validation.ValidateSG_EntryYear();
			AssertEquals("Not applicable all others", true, AddInfoJobDeclaration.SG_EntryYearInfo.HasMessageErrors());
		}

		public void TestCheckSG_OutwardTransportMode()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			Declaration.SG_US_NKPlaceOfStorage = "KZ";
			Declaration.SG_OutwardTransportMode = ZString.Empty;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			Declaration.SG_US_NKPlaceOfStorage = ZString.Empty;
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertHasMessageError(Declaration.SG_OutwardTransportModeInfo, "You have not entered an Outward Transport Mode.");
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			Declaration.SG_OutwardTransportMode = "ABC";
			AssertHasMessageError(Declaration.SG_OutwardTransportModeInfo, "The code you have selected is not in the list.");
			Declaration.SG_US_NKPlaceOfCargoRelease = "KZ";
			Declaration.SG_US_NKPlaceOfReceipt = "KZ";
			Declaration.SG_US_NKPlaceOfStorage = "KZ";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.SG_OutwardTransportMode = ZString.Empty;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			Declaration.SG_US_NKPlaceOfReceipt = "JZ";
			Declaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
		}

		public void TestOutwardMasterBill()
		{
			Declaration.JE_MessageSubType = "";
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AddInfoJobDeclaration.SG_OutwardMAWB = "";
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals("Outward Masterbill required unless meant for storage", true, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardMAWB = "Out-Mawb";
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals("Outward Masterbill required", false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardMAWB = "";
			AddInfoJobDeclaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.KeppelFTZ;
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals("Outward Masterbill NOT required if goods meant for storage", false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardMAWB = "Out-Mawb";
			AddInfoJobDeclaration.SG_US_NKPlaceOfStorage = "";
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = "";
			AddInfoJobDeclaration.SG_US_NKPlaceOfCargoRelease = "KZ";
			AddInfoJobDeclaration.SG_US_NKPlaceOfReceipt = "KZ";
			AddInfoJobDeclaration.SG_US_NKPlaceOfStorage = "KZ";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_US_NKPlaceOfReceipt = "JZ";
			AddInfoJobDeclaration.SG_OutwardTransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AddInfoJobDeclaration.Validation.ValidateSG_OutwardMAWB();
			AssertEquals(false, AddInfoJobDeclaration.SG_OutwardMAWBInfo.HasMessageErrors());
		}

		public void TestCountryOfFinalDestination()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(true, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Slovakia;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			AddInfoJobDeclaration.SG_RN_NKFinalDestination = "";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, AddInfoJobDeclaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
		}

		public void TestSeaStore()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.SG_IsSeaStore = true;
			Declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Validation.ValidateSG_IsSeaStore();
			AssertEquals(true, Declaration.SG_IsSeaStoreInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.APS;
			Validation.ValidateSG_IsSeaStore();
			AssertEquals(false, AddInfoJobDeclaration.SG_IsSeaStoreInfo.HasMessageErrors());
		}

		protected override string MessageType => MessageTypeCodeList.Codes.OUT;
	}
}
