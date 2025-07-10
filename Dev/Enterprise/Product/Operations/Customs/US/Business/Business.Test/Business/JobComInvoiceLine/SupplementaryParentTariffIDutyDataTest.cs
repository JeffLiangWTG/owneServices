using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class SupplementaryParentTariffIDutyDataTest : TestCaseWithFactory
	{
		[TestDate(2019, 05, 10)]
		public void TestAAUTariffOnReconJobWithXVVSets()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateAdValorem = 0.2780m;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "~9342838";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_InvoiceAmount = 3406m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";// AAU
			invoiceLine.JI_Tariff = "10000000";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_98GoodsValue = 0m;
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.US_SetInd = "X";

			var secondLine = invoiceLine.AddSecondaryInvoiceLine();
			secondLine.US_SetInd = "V";
			secondLine.JI_ParentID = invoiceLine.PK;
			secondLine.US_SupTariff = "9802008068";
			secondLine.JI_Tariff = "10000000";
			secondLine.JI_LinePrice = 7409.52m;
			secondLine.US_98GoodsValue = 50000.00m;

			var thirdLine = invoiceLine.AddSecondaryInvoiceLine();
			thirdLine.US_SetInd = "V";
			thirdLine.JI_ParentID = invoiceLine.PK;
			thirdLine.US_SupTariff = "9802008068";
			thirdLine.JI_Tariff = "6505000100";
			thirdLine.JI_LinePrice = 1348.08m;
			thirdLine.US_98GoodsValue = 25000.00m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(2434.72m, invoiceLine.US_Duty);
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			new ReconImportEntryRetriever(reconDec).ImportLines();

			AssertEquals(3, reconDec.InvoiceLines.Count);
			var xline = reconDec.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.US_SecondarySPI == "X");
			AssertNotNull(xline);

			AssertEquals("10000000", xline.JI_Tariff);
			AssertEquals("9802008068", xline.US_SupTariff);

			AssertEquals(75000.00m, xline.US_98GoodsValue);
			AssertEquals(8758.00m, xline.JI_LinePrice);

			reconDec.CalculateDutyFeesForAllEntries();
			AssertEquals(2434.72m, xline.US_Duty);
		}

		[TestDate(2019, 05, 10)]
		public void TestAAUTariffOnReconJobWithOutXVVSets()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateAdValorem = 0.2780m;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "20000000";
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff2.UE_Column1RateAdValorem = 0.0640m;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "~9342838";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_InvoiceAmount = 3406m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9802008068";// AAU
			invoiceLine.JI_Tariff = "10000000";
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_98GoodsValue = 0m;
			invoiceLine.JI_CustomsQuantity = 100m;

			var secondLine = invoiceLine.AddSecondaryInvoiceLine();
			secondLine.JI_ParentID = invoiceLine.PK;
			secondLine.US_SupTariff = "9802008068";
			secondLine.JI_Tariff = "10000000";
			secondLine.JI_LinePrice = 7409.52m;
			secondLine.US_98GoodsValue = 50000.00m;

			var thirdLine = invoiceLine.AddSecondaryInvoiceLine();
			thirdLine.JI_ParentID = invoiceLine.PK;
			thirdLine.US_SupTariff = "9802008068";
			thirdLine.JI_Tariff = "20000000";
			thirdLine.JI_LinePrice = 1348.08m;
			thirdLine.US_98GoodsValue = 25000.00m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(0m, invoiceLine.US_Duty);
			AssertEquals(1668.83m, secondLine.US_Duty);
			AssertEquals(176.32m, thirdLine.US_Duty);
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			new ReconImportEntryRetriever(reconDec).ImportLines();

			AssertEquals(3, reconDec.InvoiceLines.Count);

			reconDec.CalculateDutyFeesForAllEntries();
			var lines = reconDec.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ToArray();
			AssertEquals(0m, lines[0].US_Duty);
			AssertEquals(1668.83m, lines[1].US_Duty);
			AssertEquals(176.32m, lines[2].US_Duty);
		}

		public void TestSecondaryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8457.20.0010";
			invoiceLine.US_SupTariff = "9802.00.8068";
			invoiceLine.US_98GoodsValue = 319m;
			invoiceLine.JI_LinePrice = 1356m;

			IFeeCalculationDataProvider supTariffDutyData = invoiceLine.SupplementaryParentTariffIDutyData;

			var secondaryLines = supTariffDutyData.SecondaryLines;
			AssertEquals("invoiceline with sup tariff is a secondary line of its own sup line as far as IDutyData is concerned", 1, secondaryLines.Count());

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802.00.8068";
			invoiceLine2.JI_Tariff = "9102.11.1010";
			AssertEquals("PreCondition:SecondaryTariffLines are added", 3, invoiceLine2.SecondaryTariffLines.Count());

			supTariffDutyData = invoiceLine2.SupplementaryParentTariffIDutyData;
			secondaryLines = supTariffDutyData.SecondaryLines;
			AssertEquals("invoiceline with sup tariff is a secondary line of its own sup line as far as IDutyData is concerned", 7, secondaryLines.Count());
		}

		public void TestSecondaryRateTypeWithSupLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204212000";
			AssertNotNull(invoiceLine.ImportTariff);
			Assert("ComputationCode: C and users have to enter a value in this field", !invoiceLine.US_TaxRateTInfo.ReadOnly);
			invoiceLine.JI_LinePrice = 14613m;
			invoiceLine.JI_CustomsQuantity = 10.00000m;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_TaxRateT = RateTypeList.Codes.Secondary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8.72m, invoiceLine.CusEntryLine.Fees.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines));

			invoiceLine.US_SupTariff = "99010050";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(8.72m, invoiceLine.CusEntryLine.Fees.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.Wines));
		}

		public void TestTIBWatchAndCustomsValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			var invoiceLine4 = invoiceLine1.AddSecondaryInvoiceLine();

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine1.US_SupTariff = "98130020";
				invoiceLine1.JI_Tariff = "9101118010";
				invoiceLine1.JI_LinePrice = 2158m;
				invoiceLine1.JI_CustomsQuantity = 3m;

				invoiceLine2.JI_Tariff = "9101118020";
				invoiceLine2.US_SupTariff = "";
				invoiceLine2.JI_LinePrice = 10062m;
				invoiceLine2.JI_CustomsQuantity = 9m;

				invoiceLine3.JI_Tariff = "9101118030";
				invoiceLine3.US_SupTariff = "";
				invoiceLine3.JI_LinePrice = 292m;
				invoiceLine3.JI_CustomsQuantity = 3m;

				invoiceLine4.JI_Tariff = "9101118040";
				invoiceLine4.US_SupTariff = "";
				invoiceLine4.JI_LinePrice = 16m;
				invoiceLine4.JI_CustomsQuantity = 3m;
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var supLine = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals("No CustomsValue to calculate line level duty correctly for BondAmount", 0m, supLine.CL_CustomsValue);
			AssertEquals("Customsvalue should be recorded against CusEntryLine with a rate", 2158m, invoiceLine1.CusEntryLine.CL_CustomsValue);
			AssertEquals("Customsvalue should be recorded against CusEntryLine with a rate", 10062m, invoiceLine2.CusEntryLine.CL_CustomsValue);
			AssertEquals("Customsvalue should be recorded against CusEntryLine with a rate", 292m, invoiceLine3.CusEntryLine.CL_CustomsValue);
			AssertEquals("Customsvalue should be recorded against CusEntryLine with a rate", 16m, invoiceLine4.CusEntryLine.CL_CustomsValue);
		}

		public void TestCustomsValueForEntryLineFor9801()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98GoodsValue = 3m;
			invoiceLine1.JI_Tariff = "6104442010";
			invoiceLine1.JI_LinePrice = 14613m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9801001097";
			invoiceLine2.US_98GoodsValue = 2916m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals(3, declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines.Count);

			var entryLine = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals(3m, entryLine.CL_CustomsValue);

			entryLine = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(14613m, entryLine.CL_CustomsValue);

			entryLine = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2916m, entryLine.CL_CustomsValue);
		}

		public void TestCustomsValueForEntryLineFor9801_2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "9802008068";
			invoiceLine1.US_98ValueInvCurr = 3m;
			invoiceLine1.JI_Tariff = "6104442010";
			invoiceLine1.JI_LinePrice = 14613m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9801001097";
			invoiceLine2.US_98ValueInvCurr = 2916m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals(3, declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines.Count);

			var entryLine = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true);
			AssertEquals(3m, entryLine.CL_CustomsValue);

			entryLine = invoiceLine1.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(14613m, entryLine.CL_CustomsValue);

			entryLine = invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, false);
			AssertEquals(2916m, entryLine.CL_CustomsValue);
		}

		public void TestMergeWith98_99Details()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5208112040";
			invoiceLine.JI_LinePrice = 500m;

			invoiceLine.US_SupTariff = "9802006000";
			invoiceLine.US_98GoodsValue = 10000m;

			IDutyData supDutyData = ((IDutyData)invoiceLine).ParentTariffLine;

			AssertEquals("9802006000", supDutyData.ImportTariff.UE_Tariff);
			AssertEquals(ZString.Empty, supDutyData.UQ1);
			AssertEquals(ZString.Empty, supDutyData.UQ2);
			AssertEquals(ZString.Empty, supDutyData.UQ3);
			AssertEquals(10000m, supDutyData.CustomsValue);
		}

		[TestDate(2008, 12, 1)]
		public void TestDutyDataCustomsValueForAdditionalDuties()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra
			invoiceLine.US_SupQty1 = 5000.51m;

			invoiceLine.JI_Tariff = "2909.19.1800"; // 5.5%
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 10000m;

			IDutyData supDutyData = ((IDutyData)invoiceLine).ParentTariffLine;
			AssertEquals("99010052", supDutyData.ImportTariff.UE_Tariff);
			AssertEquals(10000m, supDutyData.CustomsValue);
			AssertEquals("L", supDutyData.UQ1);
			AssertEquals(5001m, supDutyData.Quantity1);

			IDutyData classificationDutyData = invoiceLine;
			AssertEquals("2909191800", classificationDutyData.ImportTariff.UE_Tariff);
			// value should be declared at the parent
			AssertEquals(0m, classificationDutyData.CustomsValue);
			AssertEquals("KG", classificationDutyData.UQ1);
			AssertEquals(1000m, classificationDutyData.Quantity1);
		}

		[TestDate(2020, 03, 04)]
		public void TestImportRegularSetsIntoRecon()
		{
			#region Setup Tariffs

			var tariff8215100000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8215100000")).LastOrDefault();
			if (tariff8215100000 == null)
			{
				tariff8215100000 = Factory.New<USCTariff>();
				tariff8215100000.UE_Tariff = "8215100000";
				tariff8215100000.UE_DutyComputationCode = "9";
				tariff8215100000.UE_Column1RateAdValorem = 1m;
			}
			tariff8215100000.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff8215100000.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff8215916000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8215916000")).LastOrDefault();
			if (tariff8215916000 == null)
			{
				tariff8215916000 = Factory.New<USCTariff>();
				tariff8215916000.UE_Tariff = "8215916000";
				tariff8215916000.UE_DutyComputationCode = "7";
				tariff8215916000.UE_Column1RateAdValorem = 0.042m;
			}
			tariff8215916000.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff8215916000.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038815 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038815")).LastOrDefault();
			if (tariff99038815 == null)
			{
				tariff99038815 = Factory.New<USCTariff>();
				tariff99038815.UE_Tariff = "99038815";
				tariff99038815.UE_DutyComputationCode = "7";
				tariff99038815.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038815.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038815.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~9342838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "8215100000";
			invoiceLineOne.US_SupTariff = "99038815";
			invoiceLineOne.JI_LinePrice = 0m;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "8215916000";
			invoiceLineTwo.US_SupTariff = ZString.Empty;
			invoiceLineTwo.JI_LinePrice = 10000m;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";

			var invoiceLineThree = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineThree.JI_Tariff = "8215100000";
			invoiceLineThree.US_SupTariff = ZString.Empty;
			invoiceLineThree.JI_LinePrice = 0m;
			invoiceLineThree.US_UC_NKCountryOfOrigin = "HK";
			invoiceLineThree.US_UC_NKCountryOfExport = "HK";

			var invoiceLineFour = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineFour.JI_ParentID = invoiceLineThree.PK;
			invoiceLineFour.JI_Tariff = "8215916000";
			invoiceLineFour.US_SupTariff = ZString.Empty;
			invoiceLineFour.JI_LinePrice = 10000m;
			invoiceLineFour.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineFour.US_UC_NKCountryOfExport = "CN";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			CombineAssertions("Duties on import declaration", () =>
			{
				AssertEquals("For 1st invoice line, normal duty = 10000 * 0.42", 420m, invoiceLineOne.US_Duty);
				AssertEquals("For 1st invoice line, prov duty = 10000 * 0.15", 1500m, invoiceLineOne.US_SupDuty);
				AssertEquals("For 2nd invoice line, normal duty should be zero", 0m, invoiceLineTwo.US_Duty);
				AssertEquals("For 2nd invoice line, prov duty should be zero", 0m, invoiceLineTwo.US_SupDuty);
				AssertEquals("For 3rd invoice line, normal duty = 10000 * 0.42", 420m, invoiceLineThree.US_Duty);
				AssertEquals("For 3rd invoice line, prov duty should be zero", 0m, invoiceLineThree.US_SupDuty);
				AssertEquals("For 4th invoice line, normal duty should be zero", 0m, invoiceLineFour.US_Duty);
				AssertEquals("For 4th invoice line, prov duty should be zero", 0m, invoiceLineFour.US_SupDuty);
			});
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			reconEntry.US_R_CalcOrigDuty = true;
			new ReconImportEntryRetriever(reconDec).ImportLines();
			AssertEquals(4, reconDec.InvoiceLines.Count);
			CombineAssertions("customs values after import declaration into recon", () =>
			{
				AssertEquals("Orig normal customs value on 1st recon line", 0m, reconDec.InvoiceLines[0].US_R_OrigCV);
				AssertEquals("Recon normal customs value on 1st recon line", 0m, reconDec.InvoiceLines[0].JI_LinePrice);
				AssertEquals("Orig 98 customs value on 1st recon line", 0m, reconDec.InvoiceLines[0].US_R_Orig98Value);
				AssertEquals("Recon 98 customs value on 1st recon line", 0m, reconDec.InvoiceLines[0].US_98GoodsValue);
				AssertEquals("Orig normal customs value on 2nd recon line", 10000m, reconDec.InvoiceLines[1].US_R_OrigCV);
				AssertEquals("Recon normal customs value on 2nd recon line", 10000m, reconDec.InvoiceLines[1].JI_LinePrice);
				AssertEquals("Orig 98 customs value on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_R_Orig98Value);
				AssertEquals("Recon 98 customs value on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_98GoodsValue);
				AssertEquals("Orig normal customs value on 3rd recon line", 0m, reconDec.InvoiceLines[2].US_R_OrigCV);
				AssertEquals("Recon normal customs value on 3rd recon line", 0m, reconDec.InvoiceLines[2].JI_LinePrice);
				AssertEquals("Orig 98 customs value on 3rd recon line", 0m, reconDec.InvoiceLines[2].US_R_Orig98Value);
				AssertEquals("Recon 98 customs value on 3rd recon line", 0m, reconDec.InvoiceLines[2].US_98GoodsValue);
				AssertEquals("Orig normal customs value on 4th recon line", 10000m, reconDec.InvoiceLines[3].US_R_OrigCV);
				AssertEquals("Recon normal customs value on 4th recon line", 10000m, reconDec.InvoiceLines[3].JI_LinePrice);
				AssertEquals("Orig 98 customs value on 4th recon line", 0m, reconDec.InvoiceLines[3].US_R_Orig98Value);
				AssertEquals("Recon 98 customs value on 4th recon line", 0m, reconDec.InvoiceLines[3].US_98GoodsValue);
			});

			reconDec.CalculateDutyFeesForAllEntries();
			CombineAssertions("Duties on recon invoice lines", () =>
			{
				AssertEquals("Normal origin duty on 1st recon line", 420m, reconDec.InvoiceLines[0].US_R_OrigDuty);
				AssertEquals("Normal recon duty on 1st recon line", 420m, reconDec.InvoiceLines[0].US_Duty);
				AssertEquals("Sup origin duty on 1st recon line", 1500m, reconDec.InvoiceLines[0].US_R_OrigSupDuty);
				AssertEquals("Sup recon duty on 1st recon line", 1500m, reconDec.InvoiceLines[0].US_SupDuty);
				AssertEquals("Normal origin duty on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_R_OrigDuty);
				AssertEquals("Normal recon duty on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_Duty);
				AssertEquals("Sup origin duty on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_R_OrigSupDuty);
				AssertEquals("Sup recon duty on 2nd recon line", 0m, reconDec.InvoiceLines[1].US_SupDuty);
				AssertEquals("Normal origin duty on 3rd recon line", 420m, reconDec.InvoiceLines[2].US_R_OrigDuty);
				AssertEquals("Normal recon duty on 3rd recon line", 420m, reconDec.InvoiceLines[2].US_Duty);
				AssertEquals("Sup origin duty on 3rd recon line", 0m, reconDec.InvoiceLines[2].US_R_OrigSupDuty);
				AssertEquals("Sup recon duty on 3rd recon line", 0m, reconDec.InvoiceLines[2].US_SupDuty);
				AssertEquals("Normal origin duty on 4th recon line", 0m, reconDec.InvoiceLines[3].US_R_OrigDuty);
				AssertEquals("Normal recon duty on 4th recon line", 0m, reconDec.InvoiceLines[3].US_Duty);
				AssertEquals("Sup origin duty on 4th recon line", 0m, reconDec.InvoiceLines[3].US_R_OrigSupDuty);
				AssertEquals("Sup recon duty on 4th recon line", 0m, reconDec.InvoiceLines[3].US_SupDuty);
			});
		}

		public void TestQuantityImplementation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "9901.00.52"; // 5.99 cents/litre extra

			IDutyData supDutyData = ((IDutyData)invoiceLine).ParentTariffLine;

			invoiceLine.US_SupUQ1 = "KG";
			invoiceLine.US_SupUQ2 = "LT";
			invoiceLine.US_SupUQ3 = "NO";
			invoiceLine.US_SupQty1 = 2.523m;
			invoiceLine.US_SupQty2 = 4.625m;
			invoiceLine.US_SupQty3 = 8.867m;

			AssertEquals("Quantity1 rounded", 3m, supDutyData.Quantity1);
			AssertEquals("UQ1", "KG", supDutyData.UQ1);
			AssertEquals("Quantity2", 5m, supDutyData.Quantity2);
			AssertEquals("UQ2", "LT", supDutyData.UQ2);
			AssertEquals("Quantity3", 9m, supDutyData.Quantity3);
			AssertEquals("UQ3", "NO", supDutyData.UQ3);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			tariff.UE_Column1RateSpecific = 1.44m;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
			invoiceLine.US_SupUQ1 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupUQ2 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupUQ3 = ABIUnitOfMeasureList.Codes.ProofLiter;
			invoiceLine.US_SupQty1 = 2.523m;
			invoiceLine.US_SupQty2 = 4.625m;
			invoiceLine.US_SupQty3 = 8.867m;

			AssertEquals("Quantity1 rounded to two decimals", 2.52m, supDutyData.Quantity1);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
			tariff.UE_Column2RateSpecific = 1.44m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";

			AssertEquals("Quantity2 rounded to two decimals", 4.63m, supDutyData.Quantity2);

			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.CompoundSpecificAdValorem;
			dutyRate.UD_TaxFeeAdvalorem = 1.4m;
			dutyRate.UD_TaxFeeSpecificRate = 1.6m;

			AssertEquals("Quantity3 rounded to two decimals", 8.87m, supDutyData.Quantity3);
		}

		[TestDate(2020, 03, 03)]
		public void TestCustomsValueForEmbroideryTariffWhenImportIntoRecon()
		{
			#region Setup Tariffs

			var helper = new UniversalReferenceTestDataHelper(Factory);

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			Factory.Save();

			var zzTariff = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "5810929080", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateTariffAttribute("RULE", "EMB", zzTariff);

			var tariff5810929080 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5810929080")).LastOrDefault();
			if (tariff5810929080 == null)
			{
				tariff5810929080 = Factory.New<USCTariff>();
				tariff5810929080.UE_Tariff = "5810929080";
				tariff5810929080.UE_DutyComputationCode = "7";
				tariff5810929080.UE_Column1RateAdValorem = 0.074m;
			}
			tariff5810929080.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5810929080.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff5407532060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "5407532060")).LastOrDefault();
			if (tariff5407532060 == null)
			{
				tariff5407532060 = Factory.New<USCTariff>();
				tariff5407532060.UE_Tariff = "5407532060";
				tariff5407532060.UE_DutyComputationCode = "7";
				tariff5407532060.UE_Column1RateAdValorem = 0.12m;
			}
			tariff5407532060.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff5407532060.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~9342838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = "5810929080";
			invoiceLineOne.JI_LinePrice = 5000;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";
			invoiceLineOne.JI_CustomsQuantity = 220m;
			invoiceLineOne.JI_CustomsUnitQty = "KG";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = "5407532060";
			invoiceLineTwo.JI_LinePrice = 0;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			invoiceLineTwo.JI_CustomsQuantity = 579m;
			invoiceLineTwo.JI_CustomsUnitQty = "M2";
			invoiceLineTwo.JI_CustomsSecondQuantity = 220m;
			invoiceLineTwo.JI_CustomsSecondUnitQty = "KG";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			new ReconImportEntryRetriever(reconDec).ImportLines();
			AssertEquals(2, reconDec.InvoiceLines.Count);
			AssertEquals(5000m, reconDec.InvoiceLines[0].JI_LinePrice);
			AssertEquals(5000m, reconDec.InvoiceLines[0].US_R_OrigCV);
			AssertEquals(0m, reconDec.InvoiceLines[1].JI_LinePrice);
			AssertEquals(0m, reconDec.InvoiceLines[1].US_R_OrigCV);
		}

		[TestDate(2020, 05, 28)]
		public void TestDutyCalculationForCombinedLinesOnRecon()
		{
			#region Setup Tariffs

			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
				tariff99038801.UE_DutyComputationCode = "7";
				tariff99038801.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038801.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff99038804 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038804")).LastOrDefault();
			if (tariff99038804 == null)
			{
				tariff99038804 = Factory.New<USCTariff>();
				tariff99038804.UE_Tariff = "99038804";
				tariff99038804.UE_DutyComputationCode = "7";
				tariff99038804.UE_Column1RateAdValorem = 0.15m;
			}
			tariff99038804.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff99038804.UE_DateTo = new ZDateTime(2021, 01, 01);

			var tariff8517620020 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8517620020")).LastOrDefault();
			if (tariff8517620020 == null)
			{
				tariff8517620020 = Factory.New<USCTariff>();
				tariff8517620020.UE_Tariff = "8517620020";
				tariff8517620020.UE_DutyComputationCode = "7";
				tariff8517620020.UE_Column1RateAdValorem = 0.05m;
			}
			tariff8517620020.UE_DateFrom = new ZDateTime(2020, 01, 01);
			tariff8517620020.UE_DateTo = new ZDateTime(2021, 01, 01);

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~9342838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = ZString.Empty;
			invoiceLineOne.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLineOne.JI_LinePrice = 0m;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_ParentID = invoiceLineOne.PK;
			invoiceLineTwo.JI_Tariff = tariff8517620020.UE_Tariff;
			invoiceLineTwo.US_SupTariff = tariff99038804.UE_Tariff;
			invoiceLineTwo.JI_LinePrice = 10000m;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			CombineAssertions("Duty on import declaration", () =>
			{
				AssertEquals("Duty on first invoice line", 0m, invoiceLineOne.US_Duty);
				AssertEquals("Sup duty on first invoice line", 2500m, invoiceLineOne.US_SupDuty);
				AssertEquals("Duty on second invoice line", 500m, invoiceLineTwo.US_Duty);
				AssertEquals("Sup duty on second invoice line", 1500m, invoiceLineTwo.US_SupDuty);
			});

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~9342838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;
			reconEntry.US_R_CalcOrigDuty = true;
			new ReconImportEntryRetriever(reconDec).ImportLines();
			AssertEquals(2, reconDec.InvoiceLines.Count);
			CombineAssertions("Customs value on recon declaration", () =>
			{
				AssertEquals("Original customs value on first invoice line", 0m, reconDec.InvoiceLines[0].JI_LinePrice);
				AssertEquals("Recon customs value on first invoice line", 0m, reconDec.InvoiceLines[0].US_R_OrigCV);
				AssertEquals("Original customs value on second invoice line", 10000m, reconDec.InvoiceLines[1].JI_LinePrice);
				AssertEquals("Recon customs value on second invoice line", 10000m, reconDec.InvoiceLines[1].US_R_OrigCV);
			});

			reconDec.InvoiceLines[1].JI_LinePrice = 11000m;
			reconDec.CalculateDutyFeesForAllEntries();
			CombineAssertions("Duty on recon declaration", () =>
			{
				AssertEquals("Original Duty on first invoice line", 0m, reconDec.InvoiceLines[0].US_R_OrigDuty);
				AssertEquals("Recon Duty on first invoice line", 0m, reconDec.InvoiceLines[0].US_Duty);
				AssertEquals("Original Sup Duty on first invoice line", 2500m, reconDec.InvoiceLines[0].US_R_OrigSupDuty);
				AssertEquals("Recon Sup Duty on first invoice line", 2750m, reconDec.InvoiceLines[0].US_SupDuty);
				AssertEquals("Original Duty on second invoice line", 500m, reconDec.InvoiceLines[1].US_R_OrigDuty);
				AssertEquals("Recon Duty on second invoice line", 550m, reconDec.InvoiceLines[1].US_Duty);
				AssertEquals("Original Sup Duty on second invoice line", 1500m, reconDec.InvoiceLines[1].US_R_OrigSupDuty);
				AssertEquals("Recon Sup Duty on second invoice line", 1650m, reconDec.InvoiceLines[1].US_SupDuty);
			});
		}

		[TestDate(2020, 05, 28)]
		public void TestCustomsValueRetrievedFromLinesToRecon()
		{
			#region Setup Tariffs

			var tariff55544333 = Factory.New<USCTariff>();
			tariff55544333.UE_Tariff = "55544333";
			tariff55544333.UE_DutyComputationCode = "7";
			tariff55544333.UE_Column1RateAdValorem = 0.25m;
			tariff55544333.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff55544333.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;

			var tariff8888626220 = Factory.New<USCTariff>();
			tariff8888626220.UE_Tariff = "8888626220";
			tariff8888626220.UE_DutyComputationCode = "7";
			tariff8888626220.UE_Column1RateAdValorem = 0.05m;
			tariff8888626220.UE_DateFrom = ZDateTime.MinSmallDateTimeValue;
			tariff8888626220.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;

			Factory.Save();

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "~1112838";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_Tariff = tariff8888626220.UE_Tariff;
			invoiceLineOne.US_SupTariff = tariff55544333.UE_Tariff;
			invoiceLineOne.JI_LinePrice = 333m;
			invoiceLineOne.JI_CustomsQuantity = 50m;
			invoiceLineOne.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineOne.US_UC_NKCountryOfExport = "CN";

			var invoiceLineTwo = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineTwo.JI_Tariff = tariff8888626220.UE_Tariff;
			invoiceLineTwo.US_SupTariff = tariff55544333.UE_Tariff;
			invoiceLineTwo.JI_LinePrice = 555m;
			invoiceLineTwo.JI_CustomsQuantity = 50m;
			invoiceLineTwo.US_UC_NKCountryOfOrigin = "CN";
			invoiceLineTwo.US_UC_NKCountryOfExport = "CN";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconEntry = reconDec.OriginalEntries.AddNew();
			reconEntry.CH_OrigEntryReference = "XJ5~1112838";
			reconEntry.US_R_ReleaseDate = ZDateTime.Today;
			reconEntry.US_R_DutyRateDate = ZDateTime.Today;

			new ReconImportEntryRetriever(reconDec).ImportLines();
			AssertEquals(1, reconDec.InvoiceLines.Count);
			AssertEquals("Original customs value on first invoice line", 888m, reconDec.InvoiceLines[0].JI_LinePrice);
			AssertEquals("Original customs value on first invoice line", 100m, reconDec.InvoiceLines[0].JI_CustomsQuantity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
