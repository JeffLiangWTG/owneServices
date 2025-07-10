using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class FDALotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_TemperatureQualifier()
		{
			Lot.US_TemperatureQualifier = "!";
			AssertHasMessageError(Lot.US_TemperatureQualifierInfo, ListValidation.InvalidCodeMessageError);

			Lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.DryIce;
			AssertNoMessageError(Lot.US_TemperatureQualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DegreeType()
		{
			Lot.US_DegreeType = "!";
			AssertHasMessageError(Lot.US_DegreeTypeInfo, ListValidation.InvalidCodeMessageError);

			Lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.DryIce;
			Lot.US_DegreeType = DegreeTypeList.Codes.Celsius;
			AssertNoMessageError(Lot.US_DegreeTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Lot.US_DegreeTypeInfo, FDALotValidation.TempDetailsRequired);

			Lot.US_DegreeType = ZString.Empty;
			AssertHasMessageError(Lot.US_DegreeTypeInfo, FDALotValidation.TempDetailsRequired);
		}

		public void TestCheckUS_LocationOfTemp()
		{
			Lot.US_LocationOfTemp = "!";
			AssertHasMessageError(Lot.US_LocationOfTempInfo, ListValidation.InvalidCodeMessageError);

			Lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.DryIce;
			Lot.US_LocationOfTemp = PGAStorageTypeList.Codes.Conveyance;
			AssertNoMessageError(Lot.US_LocationOfTempInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(Lot.US_LocationOfTempInfo, FDALotValidation.TempDetailsRequired);

			Lot.US_LocationOfTemp = ZString.Empty;
			AssertHasMessageError(Lot.US_LocationOfTempInfo, FDALotValidation.TempDetailsRequired);
		}

		public void TestCheckUS_Temperature()
		{
			Lot.US_TemperatureQualifier = TemperatureQualifierList.Codes.DryIce;
			Lot.US_Temperature = 12.5m;
			AssertNoWarningContaining(Lot.US_TemperatureInfo, ZString.Format(FDALotValidation.ZeroTemperatureWillBeSent, ""));

			Lot.US_Temperature = ZDecimal.Zero;
			AssertHasWarningContaining(Lot.US_TemperatureInfo, ZString.Format(FDALotValidation.ZeroTemperatureWillBeSent, ""));
		}

		public void TestCheckUS_LotNumber()
		{
			var fda = Lot.FDA;
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			Lot.AddInfoValidation.ValidateUS_LotNumber();
			AssertNoMessageErrorContaining(Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var newFactory = new BusinessObjectFactory();
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(newFactory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			newFactory.Save();

			var productCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "24DCS18", Core.Constants.CountryCodes.UnitedStates, listType, ZDateTime.Now);
			productCode.ZZD_Code = "04AAI00";
			fda.US_ProductCode = productCode.ZZD_Code;

			Lot.AddInfoValidation.ValidateUS_LotNumber();
			AssertHasMessageErrorContaining(Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			Lot.US_LotNumber = "123456";
			AssertNoMessageErrorContaining(Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			productCode.ZZD_Code = "04AAK00";
			fda.US_ProductCode = productCode.ZZD_Code;

			Lot.US_LotNumber = ZString.Empty;
			AssertNoMessageErrorContaining(Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			productCode.ZZD_Code = "40CAK00";
			fda.US_ProductCode = productCode.ZZD_Code;

			Lot.AddInfoValidation.ValidateUS_LotNumber();
			AssertHasMessageError("Infant Formula", Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			Lot.US_LotNumber = "100506";
			AssertNoMessageErrorContaining(Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);

			productCode.ZZD_Code = "40YAK00";
			fda.US_ProductCode = productCode.ZZD_Code;

			Lot.US_LotNumber = ZString.Empty;
			AssertNoMessageError("Infant Formula", Lot.US_LotNumberInfo, FDALotValidation.LotNumberMandatoryForFood);
		}

		public void TestLotNumberIsMandatory()
		{
			var fda = Lot.FDA;
			Lot.US_LotNumber = ZString.Empty;
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			Lot.AddInfoValidation.ValidateUS_LotNumber();
			AssertHasMessageError(lot.US_LotNumberInfo, "For the Section 804 Importation Program, Lot Number is mandatory.");

			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_INV;
			Lot.AddInfoValidation.ValidateUS_LotNumber();
			AssertNoMessageError(lot.US_LotNumberInfo, "For the Section 804 Importation Program, Lot Number is mandatory.");
		}

		Lot Lot
		{
			get
			{
				if (lot == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var fda = invoiceLine.ACE_FDALines.AddNew();
					lot = fda.Lots.AddNew();
				}
				return lot;
			}
		}
		Lot lot;
	}
}
