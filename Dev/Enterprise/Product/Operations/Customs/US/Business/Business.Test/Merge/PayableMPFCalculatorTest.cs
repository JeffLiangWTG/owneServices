using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PayableMPFCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2010, 3, 24)]
		public void TestCalculate_SumCalculatedLessThanTotalMPFReported()
		{
			JobComInvoiceLine invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6201922051";
			invoiceLine1.JI_CustomsQuantity = 129;
			invoiceLine1.JI_LinePrice = 196061.76;

			JobComInvoiceLine invoiceLine2 = Invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6201922051";
			invoiceLine2.JI_CustomsQuantity = 338;
			invoiceLine2.JI_LinePrice = 81303.18;

			JobComInvoiceLine invoiceLine3 = Invoice1.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6201922051";
			invoiceLine3.JI_CustomsQuantity = 17;
			invoiceLine3.JI_LinePrice = 4502.28;

			JobComInvoiceLine invoiceLine4 = Invoice2.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "6201922051";
			invoiceLine4.JI_CustomsQuantity = 118;
			invoiceLine4.JI_LinePrice = 160867.22;

			JobComInvoiceLine invoiceLine5 = Invoice2.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "6201922051";
			invoiceLine5.JI_CustomsQuantity = 15;
			invoiceLine5.JI_LinePrice = 21000.30;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(205.05m, invoiceLine1.US_PayableMPF);
			AssertEquals(85.03m, invoiceLine2.US_PayableMPF);
			AssertEquals(4.71m, invoiceLine3.US_PayableMPF);
			AssertEquals(168.25m, invoiceLine4.US_PayableMPF);
			AssertEquals(21.96m, invoiceLine5.US_PayableMPF);
			AssertEquals(485m, invoiceLine1.US_PayableMPF
				+ invoiceLine2.US_PayableMPF
				+ invoiceLine3.US_PayableMPF
				+ invoiceLine4.US_PayableMPF
				+ invoiceLine5.US_PayableMPF);
		}

		[TestDate(2018, 07, 30)]
		public void TestPayableMPFForInformal()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;

			JobComInvoiceLine invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6201922051";
			invoiceLine1.JI_CustomsQuantity = 129;
			invoiceLine1.JI_LinePrice = 1000;

			JobComInvoiceLine invoiceLine2 = Invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6201922051";
			invoiceLine2.JI_CustomsQuantity = 338;
			invoiceLine2.JI_LinePrice = 2000;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0.68m, invoiceLine1.US_PayableMPF);
			AssertEquals(1.37m, invoiceLine2.US_PayableMPF);

			Declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0.68m, invoiceLine1.US_PayableMPF);
			AssertEquals(1.37m, invoiceLine2.US_PayableMPF);

			invoiceLine2.JI_LinePrice = 3000;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(0.51m, invoiceLine1.US_PayableMPF);
			AssertEquals(1.54m, invoiceLine2.US_PayableMPF);
		}

		[TestDate(2018, 07, 30)]
		public void TestPayableMPF_ShouldConsiderProvTariffs()
		{
			var invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.SupTariffFormatted = "9801922051";
			invoiceLine1.SupFormattedAdditionalTariff2 = "6201922051";
			invoiceLine1.JI_CustomsQuantity = 129;
			invoiceLine1.JI_LinePrice = 1000;

			ErrorReporter.Clear();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertContains("No error reported", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(25.67m, invoiceLine1.US_PayableMPF);
		}

		public void TestPayableMPFApportionedPerEntryLineIfNotMinOrMaxed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "ACE";
			declaration.JE_MessageType = "IMP";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = "NON";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 4962.32m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceNumber = "37707-1";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 590.80m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3944.00m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 13.32m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 34.08m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 81.60m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_LinePrice = 295.92m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_LinePrice = 2.60m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 1149.60m;
			invoice2.JZ_IncoTerm = "FOB";
			invoice2.JZ_RX_NKInvoice_Currency = "USD";
			invoice2.JZ_InvoiceNumber = "37707-11";

			var invoiceLine8 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_LinePrice = 1149.60m;
			invoiceLine8.US_SPI = "MX";

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceAmount = 10820.48m;
			invoice3.JZ_IncoTerm = "FOB";
			invoice3.JZ_RX_NKInvoice_Currency = "USD";
			invoice3.JZ_InvoiceNumber = "37707-2";

			var invoiceLine9 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine9.JI_LinePrice = 10820.46m;
			invoiceLine9.US_SPI = "MX";

			var invoiceLine10 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine10.JI_LinePrice = 0.02m;

			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceAmount = 3005.00m;
			invoice4.JZ_IncoTerm = "FOB";
			invoice4.JZ_RX_NKInvoice_Currency = "USD";
			invoice4.JZ_InvoiceNumber = "37707-3";

			var invoiceLine11 = invoice4.JobComInvoiceLines.AddNew();
			invoiceLine11.JI_LinePrice = 3000.00m;

			var invoiceLine12 = invoice4.JobComInvoiceLines.AddNew();
			invoiceLine12.JI_LinePrice = 5.00m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			invoiceLine.CusEntryLine.CL_LineNumber = 1;
			invoiceLine2.CusEntryLine.CL_LineNumber = 2;
			invoiceLine3.CusEntryLine.CL_LineNumber = 3;
			invoiceLine4.CusEntryLine.CL_LineNumber = 4;
			invoiceLine5.CusEntryLine.CL_LineNumber = 5;
			invoiceLine6.CusEntryLine.CL_LineNumber = 6;
			invoiceLine7.CusEntryLine.CL_LineNumber = 7;
			invoiceLine8.CusEntryLine.CL_LineNumber = 8;
			invoiceLine9.CusEntryLine.CL_LineNumber = 9;
			invoiceLine10.CusEntryLine.CL_LineNumber = 10;
			invoiceLine11.CusEntryLine.CL_LineNumber = 11;
			invoiceLine12.CusEntryLine.CL_LineNumber = 12;

			Factory.Save();

			AssertEquals(2.05m, invoiceLine.US_PayableMPF);
			AssertEquals(13.66m, invoiceLine2.US_PayableMPF);
			AssertEquals(0.05m, invoiceLine3.US_PayableMPF);
			AssertEquals(0.12m, invoiceLine4.US_PayableMPF);
			AssertEquals(0.28m, invoiceLine5.US_PayableMPF);
			AssertEquals(1.03m, invoiceLine6.US_PayableMPF);
			AssertEquals(0.01m, invoiceLine7.US_PayableMPF);
			AssertEquals(10.39m, invoiceLine11.US_PayableMPF);
			AssertEquals(0.02m, invoiceLine12.US_PayableMPF);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalImportEntry = reconDeclaration.OriginalEntries.AddNew();
			originalImportEntry.CH_OrigEntryReference = "XJ5" + declaration.ActiveEntryHeaders.EntrySummaryEntry.EntryNumber;
			new ReconImportEntryRetriever(reconDeclaration).ImportLines();

			reconDeclaration.InvoiceLines.Sort(JobComInvoiceLine.Schema.JI_LineNo, System.ComponentModel.ListSortDirection.Ascending);

			AssertEquals(2.05m, invoiceLine.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(13.66m, invoiceLine2.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0.05m, invoiceLine3.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0.12m, invoiceLine4.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0.28m, invoiceLine5.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(1.03m, invoiceLine6.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0.01m, invoiceLine7.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(10.39m, invoiceLine11.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(0.02m, invoiceLine12.FeeCusCodes.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
		}

		[TestDate(2010, 3, 24)]
		public void TestCalculate_SumCalculatedGreaterThanTotalMPFReported()
		{
			JobComInvoiceLine invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6201922051";
			invoiceLine1.JI_CustomsQuantity = 129;
			invoiceLine1.JI_LinePrice = 196061.76;

			JobComInvoiceLine invoiceLine2 = Invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6201922051";
			invoiceLine2.JI_CustomsQuantity = 338;
			invoiceLine2.JI_LinePrice = 81303.18;

			JobComInvoiceLine invoiceLine3 = Invoice1.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6201922051";
			invoiceLine3.JI_CustomsQuantity = 17;
			invoiceLine3.JI_LinePrice = 4502.28;

			JobComInvoiceLine invoiceLine4 = Invoice1.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "6201922051";
			invoiceLine4.JI_CustomsQuantity = 15;
			invoiceLine4.JI_LinePrice = 21000.50;

			JobComInvoiceLine invoiceLine5 = Invoice1.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "6201922051";
			invoiceLine5.JI_CustomsQuantity = 15;
			invoiceLine5.JI_LinePrice = 21000.51;

			JobComInvoiceLine invoiceLine6 = Invoice1.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "6201922051";
			invoiceLine6.JI_CustomsQuantity = 15;
			invoiceLine6.JI_LinePrice = 21000.52;

			JobComInvoiceLine invoiceLine7 = Invoice1.InvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "6201922051";
			invoiceLine7.JI_CustomsQuantity = 15;
			invoiceLine7.JI_LinePrice = 21000.53;

			JobComInvoiceLine invoiceLine8 = Invoice2.InvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "6201922051";
			invoiceLine8.JI_CustomsQuantity = 118;
			invoiceLine8.JI_LinePrice = 160867.22;

			JobComInvoiceLine invoiceLine9 = Invoice2.InvoiceLines.AddNew();
			invoiceLine9.JI_Tariff = "6201922051";
			invoiceLine9.JI_CustomsQuantity = 15;
			invoiceLine9.JI_LinePrice = 21000.54;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(173.60m, invoiceLine1.US_PayableMPF);
			AssertEquals(71.99m, invoiceLine2.US_PayableMPF);
			AssertEquals(3.99m, invoiceLine3.US_PayableMPF);
			AssertEquals(18.59m, invoiceLine4.US_PayableMPF);
			AssertEquals(18.59m, invoiceLine5.US_PayableMPF);
			AssertEquals(18.60m, invoiceLine6.US_PayableMPF);
			AssertEquals(18.60m, invoiceLine7.US_PayableMPF);
			AssertEquals(142.44m, invoiceLine8.US_PayableMPF);
			AssertEquals(18.60m, invoiceLine9.US_PayableMPF);
			AssertEquals(485m, invoiceLine1.US_PayableMPF
				+ invoiceLine2.US_PayableMPF
				+ invoiceLine3.US_PayableMPF
				+ invoiceLine4.US_PayableMPF
				+ invoiceLine5.US_PayableMPF
				+ invoiceLine6.US_PayableMPF
				+ invoiceLine7.US_PayableMPF
				+ invoiceLine8.US_PayableMPF
				+ invoiceLine9.US_PayableMPF);
		}

		[TestDate(2010, 3, 24)]
		public void TestCalculate_NoDifferenceSumOfProratedAmounts()
		{
			JobComInvoiceLine invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6201922051";
			invoiceLine1.JI_CustomsQuantity = 129;
			invoiceLine1.JI_LinePrice = 1000000;

			JobComInvoiceLine invoiceLine2 = Invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6201922051";
			invoiceLine2.JI_CustomsQuantity = 338;
			invoiceLine2.JI_LinePrice = 1000000;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(242.5m, invoiceLine1.US_PayableMPF);
			AssertEquals(242.5m, invoiceLine2.US_PayableMPF);
			AssertEquals(485m, invoiceLine1.US_PayableMPF + invoiceLine2.US_PayableMPF);
		}

		public void TestCalucate_FIXMonthlyFiling()
		{
			Declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = 650m;

			var invoiceLine1 = Invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100000m;
			invoiceLine1.US_SPI = SpecialProgramList.Codes.MX;

			var invoiceLine2 = Invoice1.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20000m;
			invoiceLine2.US_SPI = SpecialProgramList.Codes.MX;
			AssertNoExceptionThrown(() => Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
		}

		public void TestCalucateForChapter98()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9802005060Tariff.UE_Tariff;
			invoiceLine2.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 1000m;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals(0m, invoiceLine1.US_PayableMPF);
			AssertEquals(0m, invoiceLine2.US_PayableMPF);

			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test9817002000Tariff.UE_Tariff;
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals(0m, invoiceLine1.US_PayableMPF);
			AssertEquals(0m, invoiceLine2.US_PayableMPF);

			invoiceLine2.US_SupTariff = "980200800";
			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals(0m, invoiceLine1.US_PayableMPF);
			Assert("Payable MPF on the normal tariff line", invoiceLine2.US_PayableMPF > 0);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableENS = true;
					declaration.US_EntryFilerCode = "XJ5";
				}

				return declaration;
			}
		}

		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader Invoice1
		{
			get
			{
				if (invoice1 == null)
				{
					invoice1 = Declaration.Invoices.AddNew();
					Invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}

				return invoice1;
			}
		}

		JobComInvoiceHeader invoice2;
		JobComInvoiceHeader Invoice2
		{
			get
			{
				if (invoice2 == null)
				{
					invoice2 = Declaration.Invoices.AddNew();
					invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				}

				return invoice2;
			}
		}
	}
}
