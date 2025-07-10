using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USProductsAssemblyDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestAssembledAbroadOfUSProductsForChapter98ChildLine()
		{
			var testHelper = new Chapter98HelperTest();
			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = TariffRuleList.Codes.AssembledAbroadOfUSProducts;
			tariffRule1.U1_Tariff = testHelper.Test9802005060Tariff.UE_Tariff;
			tariffRule1.U1_DateFrom = ZDateTime.Today;
			Factory.Save();

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff;//99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff;//9802005060 Assembled Abroad Of US Products
			Assert("Assembled Abroad Of US Products", testHelper.Test9802005060Tariff.Applies(TariffRuleList.Codes.AssembledAbroadOfUSProducts, ZDate.Today));

			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.ChildLine.US_98GoodsValue = 1000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(700m, testHelper.ParentLine.US_Duty);
			AssertEquals(100m, testHelper.ParentLine.US_SupDuty);
			AssertEquals("For 98 child line has not duty", 0m, testHelper.ChildLine.US_Duty);
			AssertEquals("For 98 child line has not Prov/Prog duty", 0m, testHelper.ChildLine.US_SupDuty);

			testHelper.ParentLine.JI_LinePrice = 2000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1400m, testHelper.ParentLine.US_Duty);
		}
		/// <summary>
		/// Example 1 from Admin message 0741
		/// </summary>
		[TestDate(2009, 6, 1)]
		public void TestWatchesAssembedAbroadFromUSComponents()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 9426m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.US_SupTariff = "9802008068";
				line1.US_98GoodsValue = 1852m;
				line1.JI_Tariff = "9102111010";
				line1.JI_LinePrice = 3406m;
				line1.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.US_SupTariff = "9802008068";
				line3.US_98GoodsValue = 1010m;
				line3.JI_Tariff = "9102111020";
				line3.JI_LinePrice = 1609m;
				line3.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line5 = line1.AddSecondaryInvoiceLine();
				line5.US_SupTariff = "9802008068";
				line5.US_98GoodsValue = 0m;
				line5.JI_Tariff = "9102111030";
				line5.JI_LinePrice = 1345m;
				line5.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line7 = line1.AddSecondaryInvoiceLine();
				line7.US_SupTariff = "9802008068";
				line7.US_98GoodsValue = 204m;
				line7.JI_Tariff = "9102111040";
				line7.JI_LinePrice = 0m;
				line7.JI_CustomsQuantity = 1000m;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("Entries generated", 1, declaration.CustomsEntryHeaders.Count);

				AssertEquals("Duty for line2", 296.88m, declaration.CustomsEntryHeaders[0].MergedLines[1].DutyAmount);
				AssertEquals("Duty for line4", 106.03m, declaration.CustomsEntryHeaders[0].MergedLines[3].DutyAmount);
				AssertEquals("Duty for line6", 127.05m, declaration.CustomsEntryHeaders[0].MergedLines[5].DutyAmount);
				AssertEquals("Duty for line8", 7.29m, declaration.CustomsEntryHeaders[0].MergedLines[7].DutyAmount);

				AssertEquals("Total Duty Calculated", 537.25m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);

				AssertEquals("MPF Calculated .21%", 7.15m, line1.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 3.38m, line3.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 2.82m, line5.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 0m, line7.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

				AssertEquals("MPF Calculated .21%", 7.15m, line1.CusEntryLine.MPFAmount);
				AssertEquals("MPF Calculated .21%", 3.38m, line3.CusEntryLine.MPFAmount);
				AssertEquals("MPF Calculated .21%", 2.82m, line5.CusEntryLine.MPFAmount);

				AssertEquals("total MPF", 25m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
			}
		}

		/// <summary>
		/// Example 1 from Admin message 0741
		/// </summary>
		[TestDate(2009, 6, 1)]
		public void TestWatchesAssembedAbroadFromUSComponentsWithSup()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 9426m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

				JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
				line1.US_SupTariff = "9802008068";
				line1.US_98GoodsValue = 1852m;
				line1.JI_Tariff = "9102111010";
				line1.JI_LinePrice = 3406m;
				line1.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line2 = line1.AddSecondaryInvoiceLine();
				line2.US_SupTariff = "9802008068";
				line2.US_98GoodsValue = 1010m;
				line2.JI_Tariff = "9102111020";
				line2.JI_LinePrice = 1609m;
				line2.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line3 = line1.AddSecondaryInvoiceLine();
				line3.US_SupTariff = "9802008068";
				line3.US_98GoodsValue = 0m;
				line3.JI_Tariff = "9102111030";
				line3.JI_LinePrice = 1345m;
				line3.JI_CustomsQuantity = 1000m;

				JobComInvoiceLine line4 = line1.AddSecondaryInvoiceLine();
				line4.US_SupTariff = "9802008068";
				line4.US_98GoodsValue = 204m;
				line4.JI_Tariff = "9102111040";
				line4.JI_LinePrice = 0m;
				line4.JI_CustomsQuantity = 1000m;

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				AssertEquals("Entries generated", 1, declaration.CustomsEntryHeaders.Count);

				AssertEquals("Duty for line2", 296.88m, declaration.CustomsEntryHeaders[0].MergedLines[1].DutyAmount);
				AssertEquals("Duty for line4", 106.03m, declaration.CustomsEntryHeaders[0].MergedLines[3].DutyAmount);
				AssertEquals("Duty for line6", 127.05m, declaration.CustomsEntryHeaders[0].MergedLines[5].DutyAmount);
				AssertEquals("Duty for line8", 7.29m, declaration.CustomsEntryHeaders[0].MergedLines[7].DutyAmount);

				AssertEquals("Total Duty Calculated", 537.25m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);

				AssertEquals("MPF Calculated .21%", 7.15m, line1.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 3.38m, line2.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 2.82m, line3.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing).Round(2));
				AssertEquals("MPF Calculated .21%", 0m, line4.FeeCusCodes.GetValue(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

				AssertEquals("MPF Calculated .21%", 7.15m, line1.CusEntryLine.MPFAmount);
				AssertEquals("MPF Calculated .21%", 3.38m, line2.CusEntryLine.MPFAmount);
				AssertEquals("MPF Calculated .21%", 2.82m, line3.CusEntryLine.MPFAmount);

				AssertEquals("total MPF", 25m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);
			}
		}

		public void TestAssemblyOfUSProductsWith9802009000()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoice.US_UC_NKCountryOfExport = "MX";
			invoice.US_UC_NKCountryOfOrigin = "MX";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802.00.90 00";
			invoiceLine1.US_98GoodsValue = 8000m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Tariff = "6110.90.90 10";
			invoiceLine1.JI_CustomsQuantity = 250m;
			invoiceLine1.JI_CustomsSecondQuantity = 12m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Entries generated", 1, declaration.CustomsEntryHeaders.Count);

			AssertEquals("Duty calculated", 0m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		[TestDate(2008, 1, 1)]
		public void TestRoundingAppliesToTheFinalDutyResult()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802.00.80 60";
			invoiceLine1.US_98GoodsValue = 2544.64m;
			invoiceLine1.JI_LinePrice = 2465.12m;
			invoiceLine1.JI_Tariff = "6203.43.4030";
			invoiceLine1.JI_CustomsQuantity = 83m;
			invoiceLine1.JI_CustomsSecondQuantity = 437m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Entries generated", 1, declaration.CustomsEntryHeaders.Count);

			AssertEquals("Duty calculated", 687.74m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
		}

		public void TestCombinedAssemblyTariffLines()
		{
			#region Setup tariffs for test

			var tariff9802008068 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9802008068")).LastOrDefault();
			if (tariff9802008068 == null)
			{
				tariff9802008068 = Factory.New<USCTariff>();
				tariff9802008068.UE_Tariff = "9802008068";
			}
			tariff9802008068.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff9802008068.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
				tariff99038801.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff99038801.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038801.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff8544300000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8544300000")).LastOrDefault();
			if (tariff8544300000 == null)
			{
				tariff8544300000 = Factory.New<USCTariff>();
				tariff8544300000.UE_Tariff = "8544300000";
				tariff8544300000.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff8544300000.UE_Column1RateAdValorem = 0.05m;
			}
			tariff8544300000.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff8544300000.UE_DateTo = new ZDateTime(2099, 1, 1);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_SupTariff = tariff99038801.UE_Tariff;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff9802008068.UE_Tariff;
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.US_98GoodsValue = 5000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Prov duty on first invoice line is 2500", 2500m, invoiceLine1.US_SupDuty);
			AssertEquals("Duty on first invoice line is 0", 0m, invoiceLine1.US_Duty);
			AssertEquals("Prov duty on second invoice line is 0", 0m, invoiceLine2.US_SupDuty);
			AssertEquals("Duty on second invoice line is 500", 500m, invoiceLine2.US_Duty);

			invoice.JobComInvoiceLines.RemoveAndDeleteAll();
			invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_SupTariff = tariff9802008068.UE_Tariff;
			invoiceLine1.US_98GoodsValue = 5000m;
			invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine2.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Prov duty on first invoice line is 0", 0m, invoiceLine1.US_SupDuty);
			AssertEquals("Duty on first invoice line is 0", 0m, invoiceLine1.US_Duty);
			AssertEquals("Prov duty on second invoice line is 2500", 2500m, invoiceLine2.US_SupDuty);
			AssertEquals("Duty on second invoice line is 500", 500m, invoiceLine2.US_Duty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
