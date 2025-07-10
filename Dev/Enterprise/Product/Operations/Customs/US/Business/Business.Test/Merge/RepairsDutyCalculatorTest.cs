using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class RepairsDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestRepairForChapter98ChildLine()
		{
			var testHelper = new Chapter98HelperTest();
			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = TariffRuleList.Codes.RepairTariffs;
			tariffRule1.U1_Tariff = testHelper.Test9802005060Tariff.UE_Tariff;
			tariffRule1.U1_DateFrom = ZDateTime.Today;
			Factory.Save();

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060 repair traiff
			Assert("repair traiff", testHelper.Test9802005060Tariff.Applies(TariffRuleList.Codes.RepairTariffs, ZDate.Today));

			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);
			AssertEquals("For 98 child line has not duty", 0m, testHelper.ChildLine.US_Duty);
			AssertEquals("For 98 child line has not Prov/Prog duty", 0m, testHelper.ChildLine.US_SupDuty);

			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 2000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);

			testHelper.ParentLine.JI_LinePrice = 2000m;
			testHelper.ChildLine.US_98GoodsValue = 2000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1400m, testHelper.ParentLine.US_Duty);
			AssertEquals(200m, testHelper.ParentLine.US_SupDuty);
		}

		[TestDate(2025, 04, 24)]
		public void TestRepairForChapter98LineWithAdditionalTariffs()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802004040", "X", 0, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, ZString.Empty);

			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296010", "1", 0m, "NO");
			tariff.UE_Column1RateSpecific = 1.75m;

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296020", "7", 0.048m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296030", "7", 0.022m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030125 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030125", new ZDateTime(2025, 04, 05), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF ANY COUNTRY", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030125);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.JI_FormattedTariff = "9102.29.6010";
			parentLine.SupTariffFormatted = "9903.01.25";
			parentLine.SupFormattedAdditionalTariff1 = "9802.00.4040";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_CustomsQuantity = 100m;
			parentLine.US_98GoodsValue = 3000m;

			var childLine1 = parentLine.ChildLines.ElementAt(0);
			childLine1.JI_FormattedTariff = "9102.29.6020";
			childLine1.JI_LinePrice = 2000m;
			var childLine2 = parentLine.ChildLines.ElementAt(1);
			childLine2.JI_FormattedTariff = "9102.29.6030";
			childLine2.JI_LinePrice = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CombineAssertions("Duty for each tariff", () =>
			{
				AssertEquals("Duty for tariff 9802004040 should be zero", 0m, parentLine.US_SupAdditionalTariff1Duty);
				AssertEquals("Duty for tariff 99030125 = ($5000+$2000+$1000) * 0.1", 800m, parentLine.US_SupDuty);
				AssertEquals("Duty for tariff 9102296010 = (1.75$/NO * 100NO + ($2000+$3000) * 0.048 + ($1000+$3000) * 0.022) / ($5000 + $3000 + $2000 + $1000) * $5000", 228.6m, parentLine.US_Duty);
				AssertEquals("Duty for tariff 9102296020 = (1.75$/NO * 100NO + ($2000+$3000) * 0.048 + ($1000+$3000) * 0.022) / ($5000 + $3000 + $2000 + $1000) * $2000", 91.44m, childLine1.US_Duty);
				AssertEquals("Duty for tariff 9102296030 = (1.75$/NO * 100NO + ($2000+$3000) * 0.048 + ($1000+$3000) * 0.022) / ($5000 + $3000 + $2000 + $1000) * $1000", 45.72m, childLine2.US_Duty);
			});
		}

		public void TestWatchRepairEntryForCACalculationExample4InAdminMessage_0741()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 6360m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfOrigin = "XO";

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.US_SupTariff = "9802004040";
				line1.US_98GoodsValue = 1852m;
				line1.US_SPI = SpecialProgramList.Codes.CA;
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.US_SupTariff = "9802004040";
				line3.US_98GoodsValue = 1010m;
				line3.JI_Tariff = "9102111020";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1609m;

				JobComInvoiceLine line5 = line1.AddSecondaryInvoiceLine();
				line5.US_SupTariff = "9802004040";
				line5.JI_Tariff = "9102111030";
				line5.JI_CustomsQuantity = 1000m;
				line5.JI_LinePrice = 1345m;

				JobComInvoiceLine line7 = line1.AddSecondaryInvoiceLine();
				line7.US_SupTariff = "9802004040";
				line7.JI_Tariff = "9102111040";
				line7.JI_CustomsQuantity = 1000m;
				line7.JI_LinePrice = 204m;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("", line1.DutyFormula);
				AssertEquals("", line3.DutyFormula);
				AssertEquals("", line5.DutyFormula);
				AssertEquals("", line7.DutyFormula);
			}
			AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

			AssertEquals("Duty calculated", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		public void TestWatchRepairEntryCalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.US_SupTariff = "9802004040";
				line1.JI_Tariff = "9102111010";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_LinePrice = 3406m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.US_SupTariff = "9802004040";
				line3.US_98GoodsValue = 500m;
				line3.JI_Tariff = "9102111020";
				line3.JI_CustomsQuantity = 1000m;
				line3.JI_LinePrice = 1609m;

				JobComInvoiceLine line5 = line1.AddSecondaryInvoiceLine();
				line5.US_SupTariff = "9802004040";
				line5.JI_Tariff = "9102111030";
				line5.JI_CustomsQuantity = 1000m;
				line5.JI_LinePrice = 1345m;

				JobComInvoiceLine line7 = line1.AddSecondaryInvoiceLine();
				line7.US_SupTariff = "9802004040";
				line7.JI_Tariff = "9102111040";
				line7.JI_CustomsQuantity = 1000m;
				line7.JI_LinePrice = 204m;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("There should be one entry", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("There should be eight entry lines", 8, declaration.CustomsEntryHeaders[0].MergedLines.Count);

				AssertEquals("Duty calculated for movement", 369.14m, line1.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for case", 174.38m, line3.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for bracelet", 145.77m, line5.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty calculated for battery", 22.11m, line7.CusEntryLine.Fees.GetAmount(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount));
				AssertEquals("Duty percent", 10.838m, line1.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line3.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line5.CusEntryLine.CL_DutyPercent);
				AssertEquals("Duty percent", 10.838m, line7.CusEntryLine.CL_DutyPercent);

				AssertEquals("Duty percent", "10.838%", line1.CusEntryLine.CL_DutyPercentAsString);
				AssertEquals("Duty percent", "10.838%", line3.CusEntryLine.CL_DutyPercentAsString);
				AssertEquals("Duty percent", "10.838%", line5.CusEntryLine.CL_DutyPercentAsString);
				AssertEquals("Duty percent", "10.838%", line7.CusEntryLine.CL_DutyPercentAsString);
			}
		}

		public void TestWithSingleAdValoremRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 220m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var line = invoice.JobComInvoiceLines.AddNew();
			line.US_SupTariff = "9802004040";
			line.JI_Tariff = "7315900000";
			line.JI_CustomsQuantity = 10m;
			line.JI_LinePrice = 220m;
			line.US_98GoodsValue = 11968m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("total duty", 6.38m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalDutyAmount);
			AssertEquals("Duty percent", "2.9%", line.CusEntryLine.CL_DutyPercentAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
		}
	}
}
