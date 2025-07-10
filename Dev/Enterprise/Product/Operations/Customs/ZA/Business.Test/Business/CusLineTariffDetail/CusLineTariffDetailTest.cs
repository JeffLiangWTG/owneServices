using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	sealed class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailAbstractTest
	{
		public void TestBZ_TypeDesc()
		{
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "T1#");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertEquals("BZ_TypeDesc", ZString.Empty, tariffDetail.BZ_TypeDesc);
			tariffDetail.BZ_Type = "T2#";
			AssertEquals("BZ_TypeDesc", "Schedule 'T2#'", tariffDetail.BZ_TypeDesc);
			tariffDetail.BZ_Type = "T1#";
			AssertEquals("BZ_TypeDesc", tariffType1.ZZI_Description, tariffDetail.BZ_TypeDesc);
		}

		public void TestBZ_UQ1()
		{
			//1P1
			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "991001", Universal.Constants.RateTypes.AntiDumping);

			helper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			helper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "NO");
			//12A

			var tariff2 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "REB", "991012", Universal.Constants.RateTypes.Rebate);
			helper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			//Relationship
			helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, tariff1.ZZ1_TariffCode);

			Factory.Save();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			cusLineTariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			cusLineTariffDetail.BZ_Tariff = "991012";

			AssertEquals("BZ_UQ1", "LA", cusLineTariffDetail.BZ_UQ1);
			AssertEquals("BZ_UQ1 - InvoiceLine", cusLineTariffDetail.BZ_UQ1, invoiceLine.JI_CustomsSecondUnitQty);
		}

		[TestDate(1990, 6, 1)]
		public void TestFormulaSpecificData()
		{
			using (ZArchitecture.Environment.EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
				var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
				var tariffType6P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P2");
				var tariffType6P3 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P3");
				var tariffType6P4 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P4");
				var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
				var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
				var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
				Factory.Save();

				var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "2$", "6", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
				var startDate = new ZDateTime(1990, 1, 1);
				var endDate = new ZDateTime(1991, 1, 1);
				var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
				var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6010101010", startDate, endDate);
				var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6030101010", startDate, endDate);
				var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P2.PK, "6010101010", startDate, endDate);
				var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P3.PK, "6030101010", startDate, endDate);
				var tariff6 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P4.PK, "6040101010", startDate, endDate);

				var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
				var relationship2 = helper.CreateTariffRelationship(tariff3.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
				var relationship3 = helper.CreateTariffRelationship(tariff4.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
				var relationship4 = helper.CreateTariffRelationship(tariff5.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
				var relationship5 = helper.CreateTariffRelationship(tariff6.PK, tariff1.ZZ1_ZZI_TariffType, "101010");

				var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
				var rate1 = helper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Question For Testing""\}");
				var rate2 = helper.CreateRate(tariff3, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,0):""Question For Testing""\}");
				var rate3 = helper.CreateRate(tariff4, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{""Question For Testing 2""\}");
				var rate4 = helper.CreateRate(tariff5, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{""Question For Testing 3""\}");
				var rate5 = helper.CreateRate(tariff6, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{""Question For Testing 4""\}");
				var rate6 = helper.CreateRate(tariff6, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{""Question For Testing 5""\}");
				Factory.Save();

				var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
				helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
				helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
				var testApplicability1 = helper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
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
				AssertCusLineTariffDetail(tariffDetail, "6P1", "", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", true, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
				tariffDetail.FormulaSpecificValue = "23As32";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, ZString.Empty, "0", "{0,0}0");

				tariffDetail.BZ_Tariff = "6010101010";
				AssertCusLineTariffDetail(tariffDetail, "6P1", "6010101010", "Question For Testing", "", "{5,3}");
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", false, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
				tariffDetail.FormulaSpecificValue = "23As32";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing", "0,000", "{5,3}0,000");
				tariffDetail.FormulaSpecificValue = "23,3426";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing", "23,343", "{5,3}23,343");
				tariffDetail.FormulaSpecificValue = "123,3426";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing", "123,343", "{5,3}123,343");

				tariffDetail.BZ_Tariff = "6030101010";
				AssertCusLineTariffDetail(tariffDetail, "6P1", "6030101010", "Question For Testing", "", "{5,0}");
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", false, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
				tariffDetail.FormulaSpecificValue = "23,56";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing", "24", "{5,0}24");
				tariffDetail.FormulaSpecificValue = "123456,56";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing", "123457", "{5,0}123457");

				tariffDetail.BZ_Tariff = "6030101011";
				AssertCusLineTariffDetail(tariffDetail, "6P1", "6030101011", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", true, tariffDetail.FormulaSpecificValueInfo.ReadOnly);

				tariffDetail.BZ_Type = "6P2";
				AssertCusLineTariffDetail(tariffDetail, "6P2", "6010101010", "Question For Testing 2", "", "{12,2}");
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", false, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
				tariffDetail.FormulaSpecificValue = "123456,566";
				AssertCusLineTariffDetailFormulaSpecific(tariffDetail, "Question For Testing 2", "123456,57", "{12,2}123456,57");
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var tariffDetailInDiffFactory = newFactory.Load<CusLineTariffDetail>(tariffDetail.PK);
				AssertCusLineTariffDetailFormulaSpecific(tariffDetailInDiffFactory, "Question For Testing 2", "123456,57", "{12,2}123456,57");

				tariffDetail.BZ_Type = "6P3";
				// No valid rate
				AssertCusLineTariffDetail(tariffDetail, "6P3", "6030101010", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", true, tariffDetail.FormulaSpecificValueInfo.ReadOnly);

				tariffDetail.BZ_Type = "6P4";
				// Multi rates
				AssertCusLineTariffDetail(tariffDetail, "6P4", "6040101010", ZString.Empty, ZString.Empty, ZString.Empty);
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", true, tariffDetail.FormulaSpecificValueInfo.ReadOnly);

				var rateCode_ZA_REB_6P4 = helper.LoadOrCreateNewCusRateCode(Factory, "6P4", rateType_ZA_REB.PK);
				var rate7 = helper.CreateRate(tariff6, rateCode_ZA_REB_6P4.PK, startDate, endDate, "{\"REBATE AMOUNT\"}");
				Factory.Save();
				AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", false, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
			}
		}

		[TestDate(1990, 6, 1)]
		public void TestUseRelatedTariffInGetQuestion()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "STANDARD", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			Factory.Save();

			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "2$", "3", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff0 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "21010101010", startDate, endDate);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3010101010", startDate, endDate);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3010101010", startDate.AddMonths(1), endDate.AddMonths(1));

			helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			helper.CreateTariffRelationship(tariff3.PK, tariff1.ZZ1_ZZI_TariffType, "21010101010");

			var tariff0Rate = helper.CreateRate(tariff0, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate, preferencePk: preference.PK);
			var rate1 = helper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Question For Testing""\}");
			var rate2 = helper.CreateRate(tariff3, rateCode_ZA_REB_D.PK, startDate, endDate, @"1P1+12A+12B+13A+13B+13C+13D+13E+15A+15B");
			Factory.Save();

			var testTradeGroup1 = helper.CreateTradeGroup(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "STANDARD", new ZDateTime(1980, 01, 01), new ZDateTime(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.SouthAfrica, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.AddCountry(testTradeGroup1, Enterprise.Core.Constants.CountryCodes.NewZealand, new ZDate(1980, 01, 01), new ZDate(2079, 06, 06));
			helper.CreateCusApplicability(tariff0Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(tariff1Rate, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(rate1, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			helper.CreateCusApplicability(rate2, testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
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
			tariffDetail.BZ_Tariff = "3010101010";
			AssertCusLineTariffDetail(tariffDetail, "3P1", "3010101010", "Question For Testing", "", "{5,3}");
			AssertEquals("tariffDetail.FormulaSpecificValueInfo.ReadOnly", false, tariffDetail.FormulaSpecificValueInfo.ReadOnly);
		}

		[TestDate(1990, 6, 1)]
		public void TestAdditionalScheduleIsDefaulted()
		{
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType4P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P1");
			var tariffType5P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P1");
			var tariffType6P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P1");
			var tariffType3P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P2");
			var tariffType4P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			var tariffType5P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "5P2");
			var tariffType6P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P2");
			Factory.Save();

			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "2#", "2$", "3", "WENDY's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var procedure3 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "3#", "2$", "4", "JOHN's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var procedure4 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "4#", "2$", "5", "JACK's PROCEDURE", ZAJobMessageTypeList.Codes.Import);
			var procedure5 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "5#", "2$", "6", "JOE's PROCEDURE", ZAJobMessageTypeList.Codes.Import);

			var startDate = new ZDateTime(1990, 1, 1);
			var endDate = new ZDateTime(1991, 1, 1);
			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "1010101010", Universal.Constants.RateTypes.AntiDumping);
			tariff1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;

			var tariff2 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "DTY", "1020101010", Universal.Constants.RateTypes.AdValoremExcise);
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			helper.CreateNewOrGetExistingTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "12", tariff2);

			var tariff3 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "2P2", "DTY", "2020101010", Constants.RateTypes.Duty);
			tariff3.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;

			var tariff4 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3020101010", startDate, endDate);
			var tariff5 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P1.PK, "4020101010", startDate, endDate);
			var tariff6 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P1.PK, "5020101010", startDate, endDate);
			var tariff7 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P1.PK, "6020101010", startDate, endDate);
			var tariff8 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P2.PK, "3030101010", startDate, endDate);
			var tariff9 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "4030101010", startDate, endDate);
			var tariff10 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType5P2.PK, "5030101010", startDate, endDate);
			var tariff11 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P2.PK, "6030101010", startDate, endDate);

			var tariffType1 = tariff1.CusTariffType;
			var tariffType2 = tariff2.CusTariffType;
			var tariffType3 = tariff3.CusTariffType;
			var tariffType4 = tariff4.CusTariffType;
			var tariffType5 = tariff5.CusTariffType;
			var tariffType6 = tariff6.CusTariffType;
			var tariffType7 = tariff7.CusTariffType;
			var tariffType8 = tariff8.CusTariffType;
			var tariffType9 = tariff9.CusTariffType;
			var tariffType10 = tariff10.CusTariffType;
			var tariffType11 = tariff11.CusTariffType;

			var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariffType1.PK, "101010");
			var relationship2 = helper.CreateTariffRelationship(tariff3.PK, tariffType1.PK, "101010");
			var relationship3 = helper.CreateTariffRelationship(tariff4.PK, tariffType1.PK, "101010");
			var relationship4 = helper.CreateTariffRelationship(tariff5.PK, tariffType1.PK, "101010");
			var relationship5 = helper.CreateTariffRelationship(tariff6.PK, tariffType1.PK, "101010");
			var relationship6 = helper.CreateTariffRelationship(tariff7.PK, tariffType1.PK, "101010");
			var relationship7 = helper.CreateTariffRelationship(tariff8.PK, tariffType2.PK, "102010");
			var relationship8 = helper.CreateTariffRelationship(tariff9.PK, tariffType2.PK, "102010");
			var relationship9 = helper.CreateTariffRelationship(tariff10.PK, tariffType2.PK, "102010");
			var relationship10 = helper.CreateTariffRelationship(tariff11.PK, tariffType2.PK, "102010");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "1#";
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "2#";
			var entryInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction3.CEI_Style = "3#";
			var entryInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction4.CEI_Style = "4#";
			var entryInstruction5 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction5.CEI_Style = "5#";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_Tariff = "1010101010";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "2020101010");

			invoiceLine.JI_CEI = entryInstruction2.PK;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "2020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], "3P1", "3020101010");

			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010", "12");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "3P2", "3030101010");

			invoiceLine.JI_CEI = entryInstruction3.PK;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "2020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], "4P1", "4020101010");

			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			tariffDetail.BZ_Tariff = "1020101010";
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 1, invoiceLine.CusLineTariffDetails.Count);
			tariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010", "12");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "4P2", "4030101010");

			invoiceLine.JI_CEI = entryInstruction4.PK;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "2020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], "5P1", "5020101010");

			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010", "12");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "5P2", "5030101010");

			invoiceLine.JI_CEI = entryInstruction5.PK;
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 3, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "2P2", "2020101010");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[2], "6P1", "6020101010");

			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			tariffDetail = invoiceLine.CusLineTariffDetails.AddNew(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010");
			AssertEquals("invoiceLine.CusLineTariffDetails.Count", 2, invoiceLine.CusLineTariffDetails.Count);
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[0], UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "1020101010", "12");
			AssertCusLineTariffDetail(invoiceLine.CusLineTariffDetails[1], "6P2", "6030101010");
		}

		public void TestDefaultingTypeFromTariffCode()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType12A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A);
			var tariffType12B = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			var tariffType13A = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "13A");
			var tariffType2P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var tariffType2P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P2");
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType3P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P2");
			Factory.Save();

			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var tariff1P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariffType1PK = tariff1P1.ZZ1_ZZI_TariffType;

			var tariff12A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "1010201010", startDate, endDate);
			var tariff12B = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12B.PK, "1020201010", startDate, endDate);
			var tariff13A = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType13A.PK, "1030201010", startDate, endDate);
			var tariff2P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P1.PK, "2040201010", startDate, endDate);
			var tariff2P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType2P2.PK, "2050201010", startDate, endDate);
			var tariff3P1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "3060201010", startDate, endDate);
			var tariff3P2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P2.PK, "3070201010", startDate, endDate);
			var relationship01 = helper.CreateTariffRelationship(tariff12A.PK, tariffType1PK, "101010");
			var relationship02 = helper.CreateTariffRelationship(tariff12B.PK, tariffType1PK, "101010");
			var relationship03 = helper.CreateTariffRelationship(tariff13A.PK, tariffType1PK, "101010");
			var relationship04 = helper.CreateTariffRelationship(tariff2P1.PK, tariffType1PK, "101010");
			var relationship05 = helper.CreateTariffRelationship(tariff2P2.PK, tariffType1PK, "101010");
			var relationship06 = helper.CreateTariffRelationship(tariff3P1.PK, tariffType1PK, "101010");
			var relationship07 = helper.CreateTariffRelationship(tariff3P2.PK, tariffType1PK, "101010");
			Factory.Save();

			CombineAssertions(() =>
			{
				var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
				invoiceLine.JI_Tariff = "1010101010";
				invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
				var lineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
				lineTariffDetail.BZ_Tariff = "1010201010";
				AssertEquals(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, lineTariffDetail.BZ_Type);
				AssertEquals("1010201010", lineTariffDetail.BZ_Tariff);

				lineTariffDetail.BZ_Tariff = "998700000";
				AssertEquals(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, lineTariffDetail.BZ_Type);
				AssertEquals("998700000", lineTariffDetail.BZ_Tariff);

				lineTariffDetail.BZ_Tariff = "1030201010";
				AssertEquals(UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, lineTariffDetail.BZ_Type);
				AssertEquals("1030201010", lineTariffDetail.BZ_Tariff);

				lineTariffDetail.BZ_Type = "2P1";
				lineTariffDetail.BZ_Tariff = "998700000";
				AssertEquals("2P1", lineTariffDetail.BZ_Type);
				AssertEquals("998700000", lineTariffDetail.BZ_Tariff);

				lineTariffDetail.BZ_Tariff = "3060201010";
				AssertEquals("2P1", lineTariffDetail.BZ_Type);
				AssertEquals("3060201010", lineTariffDetail.BZ_Tariff);

				lineTariffDetail.BZ_Tariff = "2050201010";
				AssertEquals("2P2", lineTariffDetail.BZ_Type);
				AssertEquals("2050201010", lineTariffDetail.BZ_Tariff);
			});
		}

		public void TestSettingBZ_UQWhenTariffChanged()
		{
			var tariff = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "99991101", Universal.Constants.RateTypes.Duty);
			tariff.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "EX1", "99991201", Universal.Constants.RateTypes.AdValoremExcise);
			tariff1.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff2 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "ADD", "99991201", Universal.Constants.RateTypes.AntiDumping);
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff3 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "13A", "EX1", "99991301", Universal.Constants.RateTypes.Levy);
			tariff3.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff4 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "5P1", "ADD", "99991301", Universal.Constants.RateTypes.Penalty);
			tariff4.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			var tariff5 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, "12B", "EXC", "99991202", Universal.Constants.RateTypes.Excise);
			tariff5.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			helper.CreateTariffUOM(tariff1, "CU1", "LI");
			helper.CreateTariffUOM(tariff2, "CU1", "LA");
			helper.CreateTariffUOM(tariff3, "CU1", "KG");
			helper.CreateTariffUOM(tariff4, "CU1", "KN");
			helper.CreateTariffRelationship(tariff1.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff2.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff3.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff4.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			helper.CreateTariffRelationship(tariff5.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			Factory.Save();

			var invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "99991101";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			var tester = invoiceLine.CusLineTariffDetails.AddNew();

			CombineAssertions(() =>
			{
				tester.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
				AssertEquals("", tester.BZ_UQ1);
				tester.BZ_Tariff = "99991201";
				AssertEquals("LI", tester.BZ_UQ1);
				tester.BZ_Type = "13A";
				tester.BZ_Tariff = "99991201";
				AssertEquals("LA", tester.BZ_UQ1);
				tester.BZ_Tariff = "99991301";
				AssertEquals("KG", tester.BZ_UQ1);
				tester.BZ_Type = "5P1";
				AssertEquals("", tester.BZ_UQ1);
				tester.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
				tester.BZ_Tariff = "99991201";
				AssertEquals("LI", tester.BZ_UQ1);
				tester.BZ_Tariff = "99991202";
				AssertEquals("", tester.BZ_UQ1);
				tester.BZ_Type = "12B";
				tester.BZ_Tariff = "99991201";
				AssertEquals("", tester.BZ_UQ1);
				tester.BZ_Tariff = "99991202";
				AssertEquals("", tester.BZ_UQ1);
			});
		}

		public void TestSettingInvoiceLineAdditionalUQ()
		{
			var tariff1 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "991001", Universal.Constants.RateTypes.AntiDumping);
			helper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LI");
			//12A
			var tariff2 = helper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part2A, "REB", "991012", Universal.Constants.RateTypes.Rebate);
			helper.CreateTariffUOM(tariff2, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "KG");
			tariff2.Rates.FirstOrDefault().CusRateType.ZZR_IsPayable = true;
			//Relationship
			helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, tariff1.ZZ1_TariffCode);
			Factory.Save();

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			cusLineTariffDetail.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			cusLineTariffDetail.BZ_Tariff = "991012";
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			cusLineTariffDetail.BZ_UQ1 = "";
			AssertEquals("", invoiceLine.JI_CustomsSecondUnitQty);
		}

		public void TestLoadingRefCusTariffTakesRelatedTariffIntoConsideration()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var dtyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY");
			var addRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "ADD");
			var dtyTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var addTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "2P1");
			var rateCode_ZA_DTY_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", dtyRateType.PK);
			Factory.Save();

			helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "STANDARD", startDate, endDate);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", "IMP");
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", "IMP");
			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999111", startDate, endDate, "DESC 1P1 for CN", 0, "");
			var cnAddTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate.AddDays(-1), endDate, "DESC 2P1 CN", 0, "", "99999111");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "01", cnAddTariff);
			helper.CreateRate(cnAddTariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "1");

			helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, dtyTariffType.PK, "99999112", startDate, endDate, "DESC 1P1 for IN", 0, "");
			var inAddTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, addTariffType.PK, "99999211", startDate, endDate, "DESC 2P1 IN", 1, "", "99999112");
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "02", inAddTariff);
			helper.CreateRate(inAddTariff, rateCode_ZA_DTY_D.PK, startDate, endDate, "1");
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
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			invoiceLine.JI_Tariff = "99999111";

			CombineAssertions(() =>
			{
				AssertEquals("Pre: Defaulting 2P1", 0, invoiceLine.CusLineTariffDetails.Count);
				var lineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
				lineTariffDetail.BZ_Tariff = "99999211";
				AssertEquals(cnAddTariff.PK, lineTariffDetail.UniversalTariff.PK);
			});
		}

		public void TestNewUsed()
		{
			var cusLTD = Factory.New<CusLineTariffDetailTester>();

			AssertEquals(ZString.Empty, cusLTD.NewUsed);
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			cusLTD.BZ_ParentID = invoiceLine.PK;
			cusLTD.BZ_ParentTableCode = "JI";

			AssertEquals("N", cusLTD.NewUsed);
		}

		public void TestITariffDetail_ShouldBeExcluded()
		{
			var cusLTD = Factory.New<CusLineTariffDetailTester>();
			var iTariff = cusLTD as ITariffDetail;

			AssertEquals(false, iTariff.ShouldBeExcluded);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			cusLTD.BZ_ParentID = invoiceLine.PK;
			cusLTD.BZ_ParentTableCode = "JI";

			AssertEquals(false, iTariff.ShouldBeExcluded);
			invoiceLine.JI_NewUsed = "N";
			AssertEquals(false, iTariff.ShouldBeExcluded);
			cusLTD.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part3D;
			AssertEquals(false, iTariff.ShouldBeExcluded);
			invoiceLine.JI_NewUsed = "U";
			AssertEquals(true, iTariff.ShouldBeExcluded);
			cusLTD.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part2A;
			AssertEquals(false, iTariff.ShouldBeExcluded);
			cusLTD.BZ_Type = UniversalReferenceConstants.CusTariffCode.Schedule1Part3D;
			invoiceLine.JI_NewUsed = ZString.Empty;
			AssertEquals(false, iTariff.ShouldBeExcluded);

			var tariffType4P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "991001", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "991002", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffAttribute = helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.PRCC, "true", tariff2);

			var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "991001");
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var permitHelper = new PermitTestDataHelper(Factory);
			var permitHeader = permitHelper.CreatePermitHeader(importer.PK, "TEST", ZDate.Today, ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.VALA, ZString.Empty, 0m, 1000m);
			Factory.Save();

			declaration.JE_OH_Importer = importer.PK;

			cusLTD.BZ_Type = "4P2";
			cusLTD.BZ_Tariff = "991002";
			invoiceLine.JI_Tariff = "991001";
			declaration.Invoices.AddNew().InvoiceLines.Add(invoiceLine);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var rccCertificate = entryInstruction.RCCCertificates.AddNew();
			rccCertificate.CY_Code = "TEST";
			AssertEquals("A VALA Permit exists so should return true", true, iTariff.ShouldBeExcluded);

			entryInstruction.RCCCertificates.RemoveAndDeleteAll();
			AssertEquals("A VALA or PRC Permit does not exist so should return false", false, iTariff.ShouldBeExcluded);

			var permitHeader2 = permitHelper.CreatePermitHeader(importer.PK, "TEST2", ZDate.Today, ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.PRC, ZString.Empty, 0m, 1000m);
			Factory.Save();
			var prcCertificate = entryInstruction.DutyRebateCertificates.AddNew();
			prcCertificate.CY_Code = "TEST2";
			AssertEquals("A PRC Permit exists so should return true", true, iTariff.ShouldBeExcluded);

			permitHeader2.CPH_Type = PermitTypeList.Codes.EXP;
			AssertEquals("A VALA or PRC Permit does not exist so should return false", false, iTariff.ShouldBeExcluded);

			permitHeader2.CPH_Type = PermitTypeList.Codes.PRC;
			cusLTD.BZ_Type = "DIE";
			cusLTD.BZ_Tariff = "DODGY";
			AssertEquals("A VALA or PRC Permit exists but the tariff is dodgy so should return false", false, iTariff.ShouldBeExcluded);
		}

		public void TestBZ_TariffAndCheckDigit()
		{
			var cusLTD = Factory.New<CusLineTariffDetailTester>();

			AssertEquals(ZString.Empty, cusLTD.BZ_Tariff);
			AssertEquals(ZString.Empty, cusLTD.BZ_CheckDigit);
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			cusLTD.BZ_ParentID = invoiceLine.PK;
			cusLTD.BZ_ParentTableCode = "JI";

			cusLTD.BZ_TariffAndCheckDigit = "1234567890AB";

			AssertEquals("BZ_Tariff should be set", "1234567890", cusLTD.BZ_Tariff);
			AssertEquals("BZ_CheckDigit should be set", "AB", cusLTD.BZ_CheckDigit);

			cusLTD.BZ_Tariff = "987654321";
			cusLTD.BZ_CheckDigit = "55";
			AssertEquals("BZ_TariffAndCheckDigit", "987654321 55", cusLTD.BZ_TariffAndCheckDigit);

			cusLTD.BZ_Tariff = "987654321";
			cusLTD.BZ_CheckDigit = "";
			AssertEquals("BZ_TariffAndCheckDigit", "987654321", cusLTD.BZ_TariffAndCheckDigit);

			cusLTD.BZ_Tariff = "";
			cusLTD.BZ_CheckDigit = "55";
			AssertEquals("BZ_TariffAndCheckDigit", "55", cusLTD.BZ_TariffAndCheckDigit);
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff)
		{
			AssertCusLineTariffDetail(tariffDetail, type, tariff, ZString.Empty);
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff, ZString checkDigit)
		{
			AssertEquals("tariffDetail.BZ_Type", type, tariffDetail.BZ_Type);
			AssertEquals("tariffDetail.BZ_Tariff", tariff, tariffDetail.BZ_Tariff);
			AssertEquals("tariffDetail.BZ_CheckDigit", checkDigit, tariffDetail.BZ_CheckDigit);
		}

		void AssertCusLineTariffDetail(CusLineTariffDetail tariffDetail, ZString type, ZString tariff, ZString question, ZString value, ZString storedValue)
		{
			AssertCusLineTariffDetail(tariffDetail, type, tariff);
			AssertCusLineTariffDetailFormulaSpecific(tariffDetail, question, value, storedValue);
		}

		void AssertCusLineTariffDetailFormulaSpecific(CusLineTariffDetail tariffDetail, ZString question, ZString value, ZString storedValue)
		{
			AssertEquals("tariffDetail.FormulaSpecificQuestion", question, tariffDetail.FormulaSpecificQuestion);
			AssertEquals("tariffDetail.FormulaSpecificValue", value, tariffDetail.FormulaSpecificValue);
			AssertEquals("storedValue", storedValue, tariffDetail.GetSystemDefinedValue<ZString>(CusLineTariffDetail.Schema.FormulaSpecificStoredValue));
		}

		protected override void SetUp()
		{
			helper = new ZAUniversalReferenceTestDataHelper(Factory);
			base.SetUp();
		}

		ZAUniversalReferenceTestDataHelper helper;

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType6P4 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "6P4");
			var rateType_ZA_REB = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Rebate);
			var rateCode_ZA_REB_D = helper.LoadOrCreateNewCusRateCode(Factory, "D", rateType_ZA_REB.PK);
			Factory.Save();

			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "1#", "2$", "6", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import);

			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1010101010", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType6P4.PK, "6020101010", startDate, endDate);

			var relationship1 = helper.CreateTariffRelationship(tariff2.PK, tariff1.ZZ1_ZZI_TariffType, "101010");
			var tariff1Rate = helper.CreateRate(tariff1, rateCode_ZA_REB_D.PK, startDate, endDate);
			var rate1 = helper.CreateRate(tariff2, rateCode_ZA_REB_D.PK, startDate, endDate, @"\{DECIMAL(5,3):""Question For Testing""\}");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-2);
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "1#";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.JI_Tariff = "1010101010";

			return invoiceLine.CusLineTariffDetails[0];
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				if (!result.ContainsKey(CusLineTariffDetail.Schema.FormulaSpecificValue))
				{
					result.Add(CusLineTariffDetail.Schema.FormulaSpecificValue, new ZString("100"));
				}
				return result;
			}
		}
	}

	sealed class CusLineTariffDetailTester : CusLineTariffDetail
	{
		public CusLineTariffDetailTester(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ZString NewUsed => base.NewUsed;
	}
}
