using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting.Testing
{
	[TestedType(typeof(ACEEntryHeaderENS7501Line))]
	sealed class ACEEntryHeaderENS7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestADDSpecificRate()
		{
			var caseRecord = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, "A570904091");
			if (caseRecord == null)
			{
				caseRecord = Factory.New<USCACCase>();
				caseRecord.U5_CaseNumber = "A570904091";
				caseRecord.U5_CaseStatus = "AC";
				caseRecord.U5_CaseStatusDate = new ZDateTime(2007, 4, 27);
			}

			caseRecord.CaseTariffs.DeleteAll();
			var caseTariff = caseRecord.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "3802100000";

			caseRecord.CaseRates.DeleteAll();
			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_Unit = "KG";
			caseRate.U6_SpecificRate = 0.44m;
			caseRate.U6_EffectiveDate = new ZDateTime(2012, 11, 09);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "ABC";
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A570904091";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDQty = 89m;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printBO = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("44c/KG", printBO.ADDRate);
		}

		public void TestSPIAndOrSecondarySPI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DOTMayBeApplicable;
			invoiceLine.JI_InvoiceQuantity = 100;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.US_UC_NKCountryOfExport = "FR";
			invoiceLine.US_UC_NKCountryOfExport = "DE";
			invoiceLine.US_SPI = "R";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.H;
			invoiceLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine = declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines[0];
			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("SPI and Secondary SPI for ACE", "R.H,X", entryPrintLine.SPIAndOrSecondarySPI);

			invoiceLine.US_SPI = ZString.Empty;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("Secondary SPI", "M,X", entryPrintLine.SPIAndOrSecondarySPI);

			invoiceLine.US_SetInd = ZString.Empty;
			entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("Secondary SPI", "M", entryPrintLine.SPIAndOrSecondarySPI);
		}

		public void TestLicenseNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7210490091";
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "u9arfb072");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Misc License Number", "01-U9ARFB072", printLine.LicenseNumber);

			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._14, "001AAA");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Misc License Number", "01-U9ARFB072\r\n14-001AAA", printLine.LicenseNumber);

			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._13, "");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Misc License Number", "01-U9ARFB072\r\n14-001AAA", printLine.LicenseNumber);
		}

		public void TestLicenseNumberForXVVSets()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7210490091";
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			var invoiceLine3 = invoiceLine2.AddSecondaryInvoiceLine();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "u9arfb072");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var printLine = new ACEEntryHeaderENS7501Line(invoiceLine2.CusEntryLine, false, false);
			AssertEquals("Misc License Number", "01-U9ARFB072", printLine.LicenseNumber);
		}

		[TestDate(2013, 08, 13)]
		public void TestSecondaryLineCompoundDutyRateForWI00046587()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_SchDLoading = "58201";
			declaration.US_SchDArrival = "3901";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV081013";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 20000m;
			invoice1.JZ_RN_NKDefaultOrigin = "IR";
			invoice1.US_UC_NKCountryOfExport = "HK";
			invoice1.US_TransactionsRelated = "N";
			invoice1.JZ_IncoTerm = "FOB";

			var freightCharge = invoice1.Charges.AddNew();
			freightCharge.J7_Amount = 1000m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "6201110010";
			line1.US_SupTariff = "9802008068";
			line1.JI_InvoiceQuantity = 42m;
			line1.JI_InvoiceUQ = "DOZ";
			line1.JI_CustomsQuantity = 42m;
			line1.JI_CustomsUnitQty = "DOZ";
			line1.JI_CustomsSecondQuantity = 1000m;
			line1.JI_CustomsSecondUnitQty = "KG";
			line1.JI_LinePrice = 15000m;
			line1.US_98GoodsValue = 0m;
			line1.US_98ValueInvCurr = 5000m;
			line1.JI_Weight = 1000m;
			line1.JI_WeightUQ = "KG";
			line1.US_DestinationState = "IL";
			AssertEquals("PreCondition", 20000m, line1.JI_CustomsValue);

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);

			AssertEquals("Line 1 Duty to print on 7501", 0m, printLine.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", printLine.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Compound Duty to print on 7501 should be total duty", 2752.50m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Compound Duty Rate to print on 7501", "18.35%", printLine.SecondaryLine1DutyPercentAsString);
		}

		public void TestVisaCertificateNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7210490091";
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.US_VisaNo = "1CH001045";
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "u9arfb072");
			var cottonCert = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "CTN01789");
			var cottonCert2 = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._23, "AMSCERT45");
			var sugCert = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._16, "CA_SG9890");
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._18, "CBT178010");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entry.MergedLines[0];
			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("VisaCertificateNumber", "V 1CH001045", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.US_VisaNo = ZString.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("VisaCertificateNumber", "C CTN01789, AMSCERT45", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.LicenceAndPermits.RemoveAndDelete(cottonCert);
			invoiceLine1.LicenceAndPermits.RemoveAndDelete(cottonCert2);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("VisaCertificateNumber", "C CA_SG9890", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.LicenceAndPermits.RemoveAndDelete(sugCert);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals("VisaCertificateNumber", "C CBT178010", entryPrintLine.VisaCertificateNumber);
		}

		public void TestPrintDutyAmountAndPercentageOnCombinedLine()
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

			var tariff9802005060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9802005060")).LastOrDefault();
			if (tariff9802005060 == null)
			{
				tariff9802005060 = Factory.New<USCTariff>();
				tariff9802005060.UE_Tariff = "9802005060";
			}
			tariff9802005060.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff9802005060.UE_DateTo = new ZDateTime(2099, 1, 1);

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
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV20081901";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.China;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_SupTariff = tariff9802008068.UE_Tariff;
			invoiceLine1.US_98GoodsValue = 5000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine2.JI_LinePrice = 10000m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine3.US_SupTariff = tariff9802005060.UE_Tariff;
			invoiceLine3.US_98GoodsValue = 5000m;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			invoiceLine4.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine4.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine4.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(8, entry.MergedLines.Count);
			var printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("Line 1 Duty to print on 7501", 0m, printLine.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", printLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty to print on 7501", 2500m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "25%", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 500m, printLine.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "5%", printLine.SecondaryLine2DutyPercentAsString);

			printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[4], false, false);
			AssertEquals("Line 2 Duty to print on 7501", 0m, printLine.DutyAmount);
			AssertEquals("Line 2 Duty Rate to print on 7501", "Free", printLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty to print on 7501", 2500m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "25%", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 500m, printLine.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "5%", printLine.SecondaryLine2DutyPercentAsString);
		}

		public void TestPrintCustomsValueAndLicenseNumberOnClassificationTariffWhenSTNRuleApplies()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var testHelper = new Chapter98HelperTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "9102.11.3010";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_SupTariff = testHelper.Test99038815Tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 1750m;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._28, "A00004");
			AssertEquals(3, invoiceLine.ChildLines.Count());

			var childLineOne = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3020");
			childLineOne.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._28, "A00001");
			AssertNotNull(childLineOne);
			childLineOne.JI_LinePrice = 150m;
			var childLineTwo = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3030");
			childLineTwo.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._28, "A00002");
			AssertNotNull(childLineTwo);
			childLineTwo.JI_LinePrice = 250m;
			var childLineThree = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3040");
			childLineThree.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._28, "A00003");
			AssertNotNull(childLineThree);
			childLineThree.JI_LinePrice = 350m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(5, entry.MergedLines.Count);

			var printLine = new ACEEntryHeaderENS7501Line(entry.MergedLines[0], false, false);
			AssertEquals("9903.88.15", printLine.FormattedTariff);
			AssertEquals(0m, printLine.TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("15%", printLine.DutyPercentAsString);
			AssertEquals(375m, printLine.DutyAmount);

			AssertEquals("9102.11.3010", printLine.SecondaryLine1FormattedTariff);
			AssertEquals(1750m, printLine.SecondaryLine1TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("44c/NO", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals(440m, printLine.SecondaryLine1DutyAmount);
			AssertEquals(ZString.Empty, printLine.SecondaryLine1LicenseNumber);

			AssertEquals("9102.11.3020", printLine.SecondaryLine2FormattedTariff);
			AssertEquals(150m, printLine.SecondaryLine2TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("6%", printLine.SecondaryLine2DutyPercentAsString);
			AssertEquals(9m, printLine.SecondaryLine2DutyAmount);
			AssertEquals("A00001", printLine.SecondaryLine2LicenseNumber);

			AssertEquals("9102.11.3030", printLine.SecondaryLine3FormattedTariff);
			AssertEquals(250m, printLine.SecondaryLine3TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("2.8%", printLine.SecondaryLine3DutyPercentAsString);
			AssertEquals(7m, printLine.SecondaryLine3DutyAmount);
			AssertEquals("A00002", printLine.SecondaryLine3LicenseNumber);

			AssertEquals("9102.11.3040", printLine.SecondaryLine4FormattedTariff);
			AssertEquals(350m, printLine.SecondaryLine4TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("5.3%", printLine.SecondaryLine4DutyPercentAsString);
			AssertEquals(18.55m, printLine.SecondaryLine4DutyAmount);
			AssertEquals("A00003", printLine.SecondaryLine4LicenseNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => new ACEEntryHeaderENS7501Line(Factory.New<CusEntryLine>(), false, false);
	}
}
