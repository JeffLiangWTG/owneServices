using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class AddInfoJobDeclarationValidation_INPTest : AddInfoCUSDECValidationTest
	{
		public void TestStartDate()
		{
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals(false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKN;
			AddInfoJobDeclaration.Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.FourPointOne;
			Validation.ValidateSG_RemovalStartDate();
			AssertEquals("Start date is mandatory for Blanket permit.", true, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RemovalStartDate = ZDateTime.Today;
			AssertEquals("Start date for Blanket permit entered.", false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCI;
			AddInfoJobDeclaration.SG_RemovalStartDate = ZDateTime.Empty;
			AssertEquals("Temporary consignment TCI does not require Start Date.", false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			AddInfoJobDeclaration.SG_RemovalStartDate = ZDateTime.Empty;
			AssertEquals("All other Temporary consignments requiring Start Date.", true, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_RemovalStartDate = ZDateTime.Today;
			AssertEquals("Start date for Blanket permit entered.", false, AddInfoJobDeclaration.SG_RemovalStartDateInfo.HasMessageErrors());
		}

		public void TestEndDateForTempImportPeriod()
		{
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(false, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCE;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(true, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCI;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(false, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCO;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(true, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCR;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(true, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.TCS;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(true, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_EndDateTempImport = ZDateTime.Today;
			Validation.ValidateSG_EndDateTempImport();
			AssertEquals(false, AddInfoJobDeclaration.SG_EndDateTempImportInfo.HasMessageErrors());
		}

		public void TestCheckSG_OutwardTransportMode()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			SGPlacesRefCusCodeListTestDataHelper.CreateFACSGPlace(Factory, "BWCY1", SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard, "BWCY1");
			Factory.Save();
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SHO;
			Declaration.SG_OutwardTransportMode = ZString.Empty;
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertNoMessageErrors(Declaration.SG_OutwardTransportModeInfo);
			var messageError = "You have not entered an Outward Transport Mode.";
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertHasMessageError(Declaration.SG_OutwardTransportModeInfo, messageError);
			Declaration.SG_US_NKPlaceOfStorage = "BWCY1";
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertNoMessageError(Declaration.SG_OutwardTransportModeInfo, messageError);
			Declaration.SG_US_NKPlaceOfStorage = ZString.Empty;
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertHasMessageError(Declaration.SG_OutwardTransportModeInfo, messageError);
			Declaration.SG_US_NKPlaceOfStorage = SGCPlaces.Constants.FreeTradeZones.KeppelFTZ;
			Declaration.AddInfoValidation.ValidateSG_OutwardTransportMode();
			AssertNoMessageError(Declaration.SG_OutwardTransportModeInfo, messageError);
		}

		public void TestClaimantCode()
		{
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GTR;
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(true, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ClaimantCode = "T2203948R";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals(false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKT;
			AddInfoJobDeclaration.SG_ClaimantCode = "";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals("Claimant can be entered or left blank for BKT", false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			AddInfoJobDeclaration.SG_ClaimantCode = "P0029438";
			Validation.ValidateSG_ClaimantCode();
			AssertEquals("Claimant can be entered or left blank for BKT", false, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.SFZ;
			Validation.ValidateSG_ClaimantCode();
			AssertEquals("For all other dec types, entry of this field should show a message error.", true, AddInfoJobDeclaration.SG_ClaimantCodeInfo.HasMessageErrors());
		}

		public void TestCheckSG_RN_NKFinalDestination()
		{
			Declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.REX;
			Declaration.SG_RN_NKFinalDestination = "";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(true, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.SG_IsSeaStore = true;
			Declaration.SG_RN_NKFinalDestination = "";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.SG_RN_NKFinalDestination = Core.Constants.CountryCodes.Japan;
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
			Declaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.DRT;
			Declaration.SG_RN_NKFinalDestination = "";
			Validation.ValidateSG_RN_NKFinalDestination();
			AssertEquals(false, Declaration.SG_RN_NKFinalDestinationInfo.HasMessageErrors());
		}

		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.INP;
			}
		}
	}
}
