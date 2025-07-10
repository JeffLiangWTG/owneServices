using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusLineTariffDetailValidationTest : Customs.Business.Testing.CusLineTariffDetailValidationTest
	{
		public void TestCheckBZ_Type()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariffDetail1 = invoiceLine.CusLineTariffDetails.AddNew();
			var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail1.BZ_Type = "XXX";
			AssertHasMessageError(tariffDetail1.BZ_TypeInfo, ListValidation.InvalidCodeMessageError);
			tariffDetail1.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part1;
			AssertHasMessageError(tariffDetail1.BZ_TypeInfo, ListValidation.InvalidCodeMessageError);
			tariffDetail1.BZ_Type = "3P1";
			AssertNoMessageErrors(tariffDetail1.BZ_TypeInfo);
			tariffDetail1.BZ_Type = ZString.Empty;
			var youHaveNotEnteredMessage = MandatoryValidation.YouHaveNotEnteredMessage("Part");
			AssertHasMessageError(tariffDetail1.BZ_TypeInfo, youHaveNotEnteredMessage);
			tariffDetail1.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertNoMessageErrors(tariffDetail1.BZ_TypeInfo);
			var duplicateAdditionalDutyScheduleMessage = ValidationConstants.CusLineTariffDetail.DuplicateAdditionalDutySchedule(tariffDetail2.BZ_TypeDesc);
			AssertHasError(tariffDetail1.BZ_TypeInfo, duplicateAdditionalDutyScheduleMessage);
			tariffDetail1.BZ_Type = "12B";
			AssertNoError(tariffDetail1.BZ_TypeInfo, duplicateAdditionalDutyScheduleMessage);
			tariffDetail2.BZ_Type = "2P1";
			tariffDetail1.BZ_Type = "2P2";
			var similarAdditionalDutyTypeMessage = ValidationConstants.CusLineTariffDetail.SimilarAdditionalDutyType("2");
			AssertNoError(tariffDetail1.BZ_TypeInfo, duplicateAdditionalDutyScheduleMessage);
			AssertHasMessageError(tariffDetail1.BZ_TypeInfo, similarAdditionalDutyTypeMessage);
			tariffDetail1.BZ_Type = "3P1";
			AssertNoErrors(tariffDetail1.BZ_TypeInfo);
			tariffDetail2.BZ_Type = "XXX";
			tariffDetail1.BZ_Type = "3P2";
			AssertNoErrors(tariffDetail1.BZ_TypeInfo);
		}

		public void TestCheckBZ_TypeCanUseSchedule6WithoutWarning()
		{
			var tariffType6P5 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P5");
			tariffType6P5.ZZI_Description = "6P5DESC";
			var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			tariffType6P1.ZZI_Description = "6P1DESC";
			Factory.Save();
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "691010101", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P5.PK, "691010100", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._68, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 1", ZAJobMessageTypeList.Codes.Export, string.Empty);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._52, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 2", ZAJobMessageTypeList.Codes.Export, string.Empty);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "H",
				UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._00, string.Empty,
				"TEST PROCEDURE 3", ZAJobMessageTypeList.Codes.Import, string.Empty);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				AssertBZ_TypeCanUseSchedule6WithoutWarning("6P5&Export&_68", false, declaration, "6P5", "691010100", ZAJobMessageTypeList.Codes.Export, UniversalReferenceConstants.ProcedureCodes._68);
				AssertBZ_TypeCanUseSchedule6WithoutWarning("6P5&Import&_68", true, declaration, "6P5", "691010100", ZAJobMessageTypeList.Codes.Import, UniversalReferenceConstants.ProcedureCodes._68);
				AssertBZ_TypeCanUseSchedule6WithoutWarning("6P1&Export&_52", false, declaration, "6P1", "691010101", ZAJobMessageTypeList.Codes.Export, UniversalReferenceConstants.ProcedureCodes._52);
				AssertBZ_TypeCanUseSchedule6WithoutWarning("6P1&Import&_40", true, declaration, "6P1", "691010101", ZAJobMessageTypeList.Codes.Import, UniversalReferenceConstants.ProcedureCodes._40);
			});
		}
		void AssertBZ_TypeCanUseSchedule6WithoutWarning(string message, bool hasWarning, JobDeclaration declaration, string tariffType, string tariff, string messageType, string procedure)
		{
			declaration.JE_MessageType = messageType;
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			entryInstruction.CEI_Style = procedure;
			var invoiceLine = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = procedure + UniversalReferenceConstants.ProcedureCodes._00;
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Tariff = tariff;
			tariffDetail.BZ_Type = tariffType;
			if (hasWarning)
			{
				AssertHasMessageError("BZ_Type:" + message, tariffDetail.BZ_TypeInfo, ValidationConstants.CusLineTariffDetail.ScheduleIsNotValidForProcedure(tariffType + "DESC", procedure, UniversalReferenceConstants.ProcedureCodes._00));
			}
			else
			{
				AssertNoMessageErrors("BZ_Type:" + message, tariffDetail.BZ_TypeInfo);
			}
		}

		public void TestCheckBZ_Type_Excl13D_ForUsed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.N;
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			var errorMsg = tariffDetail.Validation.Schedule1Part3DForNewError;
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part3D;
			AssertNoMessageError(tariffDetail.BZ_TypeInfo, errorMsg);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.U;
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertNoMessageError(tariffDetail.BZ_TypeInfo, errorMsg);
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part3D;
			AssertHasMessageError(tariffDetail.BZ_TypeInfo, errorMsg);
			invoiceLine.JI_NewUsed = GoodsTypeList.Codes.S;
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertNoMessageError(tariffDetail.BZ_TypeInfo, errorMsg);
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part3D;
			AssertHasMessageError(tariffDetail.BZ_TypeInfo, errorMsg);
		}

		[TestDate(1990, 6, 1)]
		public void TestCheckFormulaSpecificValue()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var tariffType6P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P2");
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "", "6", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "00", "6", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6010101010", startDate, endDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6030101010", startDate, endDate);
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P2.PK, "6010101010", startDate, endDate);
			var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var relationship2 = helper.CreateTariffRelationship(tariff3.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var relationship3 = helper.CreateTariffRelationship(tariff4.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			var rate1 = helper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Qestion For Testing""\}");
			var rate2 = helper.CreateRate(tariff3, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,0):""Qestion For Testing 2""\}");
			var rate3 = helper.CreateRate(tariff4, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{""Qestion For Testing 3""\}");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDate(1980, 01, 01), new ZDate(2070, 06, 06));
			var testApplicability2 = helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability3 = helper.CreateCusApplicability(rate2, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability4 = helper.CreateCusApplicability(rate3, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "5#";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
			var tariffDetail = invoiceLine.CusLineTariffDetails[0];
			tariffDetail.BZ_Tariff = "6010101010";
			AssertEquals("PreCondition", "Qestion For Testing", tariffDetail.FormulaSpecificQuestion);
			AssertHasWarning(tariffDetail.FormulaSpecificValueInfo, QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(3));
			tariffDetail.BZ_Tariff = "6030101010";
			AssertEquals("PreCondition", "Qestion For Testing 2", tariffDetail.FormulaSpecificQuestion);
			AssertHasWarning(tariffDetail.FormulaSpecificValueInfo, QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(0));
			tariffDetail.BZ_Type = "6P2";
			AssertEquals("PreCondition", "Qestion For Testing 3", tariffDetail.FormulaSpecificQuestion);
			AssertHasWarning(tariffDetail.FormulaSpecificValueInfo, QuestionForFormulaSpecificValue.ADefaultFormulaSpecificValueIsRequired(2));
			tariffDetail.FormulaSpecificValue = "1.23";
			AssertNoWarnings(tariffDetail.FormulaSpecificValueInfo);
		}

		public void TestCheckBZ_Tariff()
		{
			TestCaseHelper.ClearTable("RefDatabase_RefCusRate");
			TestCaseHelper.ClearTable(AutoRefCusRateCode.Schema.TableName);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var tariffType12C = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12C");
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "12B", rateType_ZA_DTY.PK);
			var rateType_ZA_ADD = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping, "Anti Dumping");
			var rateCode_ZA_ADD_D = helper.LoadOrCreateNewCusRateCode(Factory, "12C", rateType_ZA_ADD.PK);
			var rateType_ZA_REF = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Refund, "Refd");
			var rateCode_ZA_REF_D = helper.LoadOrCreateNewCusRateCode(Factory, "12A", rateType_ZA_REF.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			var procedure_Concession4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "4", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var procedure_Concession5 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "3#", "4$", "5", "Concession 5", ZAJobMessageTypeList.Codes.Import);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), preferencePk: preference.PK);
			var tariff1Rate2 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate3 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate4 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020304060", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020304070", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff3Rate = helper.CreateRate(tariff3, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 5, 1), "1TR");
			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1020304080", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff5Rate1 = helper.CreateRate(tariff5, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), "1TR");
			var tariff6 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "3020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff6Rate1 = helper.CreateRate(tariff6, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff6Rate2 = helper.CreateRate(tariff6, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff6Rate3 = helper.CreateRate(tariff6, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff6Rate4 = helper.CreateRate(tariff6, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff7 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "4010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff8 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P1.PK, "5010101011", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff9 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12C.PK, "6010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff9Rate1 = helper.CreateRate(tariff9, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1), "1TR");
			var tariff10 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12C.PK, "7010101011", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff10Rate1 = helper.CreateRate(tariff10, rateCode_ZA_ADD_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff11 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "8010101011", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "9010101011", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff13 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "1110101011", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var relationship1 = helper.CreateTariffRelationship(tariff1.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			var relationship2 = helper.CreateTariffRelationship(tariff2.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			var relationship3 = helper.CreateTariffRelationship(tariff3.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			var relationship4 = helper.CreateTariffRelationship(tariff6.PK, tariff7.ZZ1_ZZI_TariffType, "401010");
			var relationship5 = helper.CreateTariffRelationship(tariff7.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			var relationship6 = helper.CreateTariffRelationship(tariff8.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			var relationship7 = helper.CreateTariffRelationship(tariff10.PK, tariff5.ZZ1_ZZI_TariffType, "601010");
			var relationship8 = helper.CreateTariffRelationship(tariff11.PK, tariff5.ZZ1_ZZI_TariffType, "");
			var relationship9 = helper.CreateTariffRelationship(tariff12.PK, tariff5.ZZ1_ZZI_TariffType, "201011");
			var relationship10 = helper.CreateTariffRelationship(tariff13.PK, tariff5.ZZ1_ZZI_TariffType, "201010");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "1TR", new ZDateTime(2010, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.Australia, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(tariff1Rate1, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			var applicability2 = helper.CreateCusApplicability(tariff1Rate2, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			var applicability5 = helper.CreateCusApplicability(tariff5Rate1, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			var applicability6 = helper.CreateCusApplicability(tariff10Rate1, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			tariff5Rate1.ZZ2_ZZS_Preference = preference.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "2$";
			invoiceLine.JI_Tariff = tariff5.ZZ1_TariffCode;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "12B";
			tariffDetail.BZ_Tariff = ZString.Empty;
			AssertHasMessageErrorContaining(tariffDetail.BZ_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			tariffDetail.BZ_Tariff = "1020304090";
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			var tariffMissingMessageError = ValidationConstants.CusLineTariffDetail.ScheduleTariffDoesNotExists(tariffDetail.BZ_TypeDesc, "1020304090");
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, tariffMissingMessageError);
			tariffDetail.BZ_Tariff = "1020304060";
			var noValidRate = "There is no applicable Duty rate for the Tariff";
			AssertHasMessageErrorContaining(tariffDetail.BZ_TariffInfo, noValidRate);
			tariffDetail.BZ_Tariff = "1020304070";
			AssertHasMessageErrorContaining(tariffDetail.BZ_TariffInfo, noValidRate);
			tariffDetail.BZ_Tariff = "1020304080";
			tariffMissingMessageError = ValidationConstants.CusLineTariffDetail.ScheduleTariffDoesNotExists(tariffDetail.BZ_TypeDesc, "1020304080");
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, noValidRate);
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, tariffMissingMessageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			tariffDetail.BZ_Tariff = "1020304050";
			AssertNoMessageError(tariffDetail.BZ_TariffInfo, tariffMissingMessageError);
			string messageError = "There is no applicable Duty rate for the Tariff '1020304050' and Country/Region Of Origin 'NZ' as at 30-May-15 00:00:00.";
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, messageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrors(tariffDetail.BZ_TariffInfo);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_PrimaryPreference = ZString.Empty;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, messageError);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.Standard;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, "There is more than one applicable Duty rate with rate code 12B for the Tariff '1020304050' and Country/Region Of Origin 'AU' as at 30-May-15 00:00:00.");
			tariffDetail.BZ_Tariff = tariff6.ZZ1_TariffCode;
			var tariffNotValidForScheduleMessage = "based on the current Schedule '1P1' settings.";
			AssertHasMessageErrorContaining(tariffDetail.BZ_TariffInfo, tariffNotValidForScheduleMessage);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, tariffNotValidForScheduleMessage);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			tariffDetail.BZ_Type = "3P1";
			tariffDetail.BZ_Tariff = tariff11.ZZ1_TariffCode;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, tariffNotValidForScheduleMessage);
			tariffDetail.BZ_Tariff = tariff12.ZZ1_TariffCode;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageErrorContaining(tariffDetail.BZ_TariffInfo, tariffNotValidForScheduleMessage);
			tariffDetail.BZ_Tariff = tariff13.ZZ1_TariffCode;
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, tariffNotValidForScheduleMessage);
			var tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail2.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail2.BZ_Tariff = "999";
			tariffDetail2.Validation.ValidateBZ_Tariff();
			AssertHasMessageErrorContaining(tariffDetail2.BZ_TariffInfo, "Tariff '999' is not a valid 'Schedule 1 Part 2 A' Tariff Code.");
			CombineAssertions("Validation On Rate count- No valiation for refund rate type", () =>
			{
				tariffDetail2.BZ_Type = "5P1";
				tariffDetail2.BZ_Tariff = "5010101011";
				tariffDetail2.Validation.ValidateBZ_Tariff();
				AssertHasMessageErrorContaining(tariffDetail2.BZ_TariffInfo, "There is no applicable  rate for the Tariff");
				entryInstruction.CEI_Style = "3#";
				invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "4$";
				tariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
				tariffDetail2.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
				tariffDetail2.BZ_Tariff = "4010101010";
				tariffDetail2.Validation.ValidateBZ_Tariff();
				AssertNoMessageErrors(tariffDetail2.BZ_TariffInfo);
			});
			invoiceLine.JI_Tariff = tariff9.ZZ1_TariffCode;
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = "12C";
			tariffDetail.BZ_Tariff = tariff10.ZZ1_TariffCode;
			AssertNoMessageErrorContaining(tariffDetail.BZ_TariffInfo, noValidRate);
		}

		[TestDate(2015, 10, 10)]
		public void TestCheckBZ_TariffWhenUOMsExceed()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var rateType_ZA_DTY = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_DTY.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "2010101010", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1020304050", new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			helper.CreateTariffRelationship(tariff1.PK, tariff0.ZZ1_ZZI_TariffType, "201010");
			Factory.Save();
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			var tariff1Rate2 = helper.CreateRate(tariff1, rateCode_ZA_DTY_D.PK, new ZDateTime(2015, 1, 1), new ZDateTime(2015, 12, 1));
			helper.CreateTariffUOM(tariff0, "CU1", "KM");
			helper.CreateTariffUOM(tariff0, "CU2", "KG");
			helper.CreateTariffUOM(tariff0, "RU1", "MM");
			helper.CreateTariffUOM(tariff1, "CU1", "GJ");
			helper.CreateTariffUOM(tariff1, "CU2", "LI");
			helper.CreateTariffUOM(tariff1, "RU1", "CM");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "1TR", new ZDateTime(2010, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(tariff1Rate1, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "1#";
			entryInstruction.CEI_DateForDuty = new ZDateTime(2015, 5, 30);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.NewZealand;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "2$";
			invoiceLine.JI_Tariff = tariff0.ZZ1_TariffCode;
			AssertEquals("Already 2 Additional UOMs", 2, invoiceLine.DistinctAdditionalUOMsFromAllValidTariffs.Count());
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			tariffDetail.BZ_Tariff = tariff1.ZZ1_TariffCode;
			AssertEquals("should have 3 UOMs", 3, tariffDetail.UniversalTariff.UnitsOfMeasure.Count);
			AssertEquals("BZ_UQ1 = GJ", "GJ", tariffDetail.BZ_UQ1);
			AssertEquals("Now 5 Additional UOMs", 5, invoiceLine.DistinctAdditionalUOMsFromAllValidTariffs.Count());
			tariffDetail.Validation.ValidateBZ_Tariff();
			AssertHasMessageError(tariffDetail.BZ_TariffInfo, ValidationConstants.CusLineTariffDetail.UOMsExceed);
		}

		public void TestCheckBZ_Tariff_ValidationTariffWhenMultipleAvailble()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var dtyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY");
			var addRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "ADD");
			var dtyTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var addTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var rateCode_ZA_ADD_D = helper.LoadOrCreateNewCusRateCode(Factory, "2P1", addRateType.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", "IMP");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999111", startDate, endDate, "DESC 1P1 for CN", 0, "");
			var cnAddTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate.AddDays(-1), endDate, "DESC 2P1 CN", 0, "", "99999111");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "01", cnAddTariff);
			var cnAddTariffRate = helper.CreateRate(cnAddTariff, rateCode_ZA_ADD_D.PK, startDate, endDate, "1", preference.PK);
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999112", startDate, endDate, "DESC 1P1 for IN", 0, "");
			var inAddTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate, endDate, "DESC 2P1 IN", 1, "", "99999112");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "02", inAddTariff);
			helper.CreateRate(inAddTariff, rateCode_ZA_ADD_D.PK, startDate, endDate, "1");
			Factory.Save();
			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(2010, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.China, new ZDate(2010, 01, 01), new ZDate(2079, 06, 06));
			var applicability1 = helper.CreateCusApplicability(cnAddTariffRate, testTradeGroup1, new ZDateTime(2011, 01, 01), new ZDateTime(2070, 06, 06));
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInst = declaration.CustomsEntryInstructions.AddNew();
			entryInst.CEI_Style = "XX";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInst.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "YY";
			invoiceLine.JI_CountryOfOrigin = "CN";
			invoiceLine.JI_PrimaryPreference = "100";
			invoiceLine.JI_Tariff = "99999111";
			CombineAssertions(() =>
			{
				AssertEquals("Pre: Defaulting 2P1", 0, invoiceLine.CusLineTariffDetails.Count);
				var lineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Tariff = "99999211";
				AssertEquals("9999921101", lineTariffDetail.UniversalTariff.GetTariffCodeWithCheckDigit());
				lineTariffDetail.Validation.ValidateBZ_Tariff();
				AssertNoNotifications(lineTariffDetail.BZ_TariffInfo);
			});
		}

		protected override void SetUp()
		{
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper helper;
	}
}
