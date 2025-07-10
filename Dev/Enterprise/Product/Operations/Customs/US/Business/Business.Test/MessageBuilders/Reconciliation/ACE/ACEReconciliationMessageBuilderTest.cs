using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEReconciliationMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBolck50WithSPI()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.NotApplicable;
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "SV973056390";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_NAFTAReconIndicator = true;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Calc_Invoice = "SV973056390";
			invoiceLine1.JI_LineNo = 1;
			invoiceLine1.US_R_OrigEntryLineNo = "1";
			invoiceLine1.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine1.US_R_OrigTariff = "3920.43.1000";
			invoiceLine1.JI_Tariff = "3920.43.1000";
			invoiceLine1.US_R_OrigCV = 5988m;
			invoiceLine1.JI_LinePrice = 5600m;
			invoiceLine1.US_R_OrigFirstQty = 750m;
			invoiceLine1.JI_CustomsQuantity = 750m;
			invoiceLine1.JI_CustomsUnitQty = "M2";
			invoiceLine1.US_R_OrigFirstUQ = "M2";
			invoiceLine1.US_SPI = "S";
			invoiceLine1.US_R_OrigSPI = "S";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Calc_Invoice = "SV973056390";
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.US_R_OrigEntryLineNo = "2";
			invoiceLine2.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine2.US_R_OrigTariff = "3920.99.2000";
			invoiceLine2.JI_Tariff = "3920.99.2000";
			invoiceLine2.US_R_OrigCV = 5000m;
			invoiceLine2.JI_LinePrice = 5100m;
			invoiceLine2.US_R_OrigFirstQty = 750m;
			invoiceLine2.JI_CustomsQuantity = 750m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.US_R_OrigFirstUQ = "KG";
			invoiceLine2.US_SPI = "S";
			invoiceLine2.US_R_OrigSPI = "S";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Calc_Invoice = "SV973056390";
			invoiceLine3.JI_LineNo = 3;
			invoiceLine3.US_R_OrigEntryLineNo = "1";
			invoiceLine3.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine3.US_R_OrigTariff = "3920.43.1000";
			invoiceLine3.JI_Tariff = "3920.43.1000";
			invoiceLine3.US_R_OrigCV = 5000m;
			invoiceLine3.JI_LinePrice = 5100m;
			invoiceLine3.US_R_OrigFirstQty = 750m;
			invoiceLine3.JI_CustomsQuantity = 750m;
			invoiceLine3.JI_CustomsUnitQty = "M2";
			invoiceLine3.US_R_OrigFirstUQ = "M2";
			invoiceLine3.US_SPI = "N/A";
			invoiceLine3.US_R_OrigSPI = "N/A";

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Calc_Invoice = "SV973056390";
			invoiceLine4.JI_LineNo = 4;
			invoiceLine4.US_R_OrigEntryLineNo = "2";
			invoiceLine4.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine4.US_R_OrigTariff = "3920.99.2000";
			invoiceLine4.JI_Tariff = "3920.99.2000";
			invoiceLine4.US_R_OrigCV = 5988m;
			invoiceLine4.JI_LinePrice = 6000m;
			invoiceLine4.US_R_OrigFirstQty = 750m;
			invoiceLine4.JI_CustomsQuantity = 750m;
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.US_R_OrigFirstUQ = "KG";
			invoiceLine4.US_SPI = "N/A";
			invoiceLine4.US_R_OrigSPI = "N/A";

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();

			var b50s = message.MessageBlock.MessageBlocks.OfType<AREC50>().ToList();
			AssertEquals(4, b50s.Count);

			var b50 = b50s[0];
			AssertEquals("3920431000", b50.PrimaryHTSNumber);
			AssertEquals("XC", b50.CountryOfOriginCode);
			AssertEquals("S", b50.TradeAgreementSpecialProgramClaimCode);

			b50 = b50s[1];
			AssertEquals("3920431000", b50.PrimaryHTSNumber);
			AssertEquals("XC", b50.CountryOfOriginCode);
			AssertEquals(ZString.Empty, b50.TradeAgreementSpecialProgramClaimCode);

			b50 = b50s[2];
			AssertEquals("3920992000", b50.PrimaryHTSNumber);
			AssertEquals("XC", b50.CountryOfOriginCode);
			AssertEquals("S", b50.TradeAgreementSpecialProgramClaimCode);

			b50 = b50s[3];
			AssertEquals("3920992000", b50.PrimaryHTSNumber);
			AssertEquals("XC", b50.CountryOfOriginCode);
			AssertEquals(ZString.Empty, b50.TradeAgreementSpecialProgramClaimCode);
		}

		public void TestBlock56AndBlock57AndBlock58()
		{
			Mock<IReconciliation> mock = GetTheMockForIsNAFTARecon();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			CombineAssertions(() =>
			{
				var b56 = messageBlock.MessageBlocks.OfType<AREC56>().FirstOrDefault();
				AssertEquals("The original customs value should be 1230+210", "0000001440", b56.OriginalPrimaryMerchandiseValue);
				AssertEquals("The original duty should be 12.3+4.2", "00000001650", b56.OriginalPrimaryHTSDuty);

				var b57s = messageBlock.MessageBlocks.OfType<AREC57>().ToArray();
				AssertEquals("The count of b57 should be 2", 2, b57s.Length);
				AssertEquals("The Amount of 99038801 should be 110+160", "0000000270", b57s[0].AdditionalOriginalMerchandiseValue1);
				AssertEquals("The Amount of 99038801 should be 10.8+16.1", "00000002690", b57s[0].AdditionalOriginalHTSDuty1);
				AssertEquals("The Amount of 99038802 should be 210+300", "0000000510", b57s[0].AdditionalOriginalMerchandiseValue2);
				AssertEquals("The Amount of 99038802 should be 20.8+21.8", "00000004260", b57s[0].AdditionalOriginalHTSDuty2);
				AssertEquals("The Amount of 99038803 should be 310+340", "0000000650", b57s[1].AdditionalOriginalMerchandiseValue1);
				AssertEquals("The Amount of 99038803 should be 30.8+39.8", "00000007060", b57s[1].AdditionalOriginalHTSDuty1);
				AssertEquals("The Amount should empty", ZString.Empty, b57s[1].AdditionalOriginalMerchandiseValue2);
				AssertEquals("The Amount should empty", ZString.Empty, b57s[1].AdditionalOriginalHTSDuty2);

				var b58s = messageBlock.MessageBlocks.OfType<AREC58>().ToArray();
				AssertEquals("The count of b58 should be 1", 1, b58s.Length);
				AssertEquals("Class code should be 001", "001", b58s[0].AccountingClassCodeLine1);
				AssertEquals("The Amount of 001 should be 10", "00000001000", b58s[0].OriginalLineRevenueAmount1);
				AssertEquals("Class code should be 499", "499", b58s[0].AccountingClassCodeLine2);
				AssertEquals("The Amount of 499 should be 70", "00000007000", b58s[0].OriginalLineRevenueAmount2);
				AssertEquals("Class code should be 044", "044", b58s[0].AccountingClassCodeLine3);
				AssertEquals("The Amount of 044 should be 20+100", "00000012000", b58s[0].OriginalLineRevenueAmount3);
				AssertEquals("Class code should be 501", "501", b58s[0].AccountingClassCodeLine4);
				AssertEquals("The Amount of 501 should be 66", "00000006600", b58s[0].OriginalLineRevenueAmount4);
				AssertEquals("Class code should be empty", ZString.Empty, b58s[0].AccountingClassCodeLine5);
				AssertEquals("Amount should be empty", ZString.Empty, b58s[0].OriginalLineRevenueAmount5);
			});
		}

		public void TestAddMessage()
		{
			Mock<IReconciliation> mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b10 = messageBlock.MessageBlocks.OfType<AREC10>().FirstOrDefault();
			AssertEquals("XJ5", b10.EntryFilerCode);
			AssertEquals("EN", b10.EntryNumber);
			AssertEquals("2304", b10.ReconciliationProcessingPort);
			AssertEquals("BRN", b10.BrokerReferenceNumber);
			AssertEquals("IID", b10.ImporterOfRecordNumber);
			AssertEquals("SCD", b10.SuretyCompanyCode);
			AssertEquals("CE1", b10.ReconciliationTypeCode);
			AssertEquals("4811", b10.DesignatedNotifyParty4811Number);
			AssertEquals("Y", b10.PriorDisclosureIndicator);

			var b11 = message.MessageBlock.MessageBlocks.OfType<AREC11>().FirstOrDefault();
			AssertEquals("BROKERNAME", b11.ContactName);
			AssertEquals("99999999", b11.ContactPhoneNumber);
			AssertEquals("EMAIL", b11.ContactEmailAddress);

			var b15 = message.MessageBlock.MessageBlocks.OfType<AREC15>();
			AssertEquals(0, b15.Count());

			var bD1 = message.MessageBlock.MessageBlocks.OfType<ARECD1>();
			var bD2 = message.MessageBlock.MessageBlocks.OfType<ARECD2>();
			var bD3 = message.MessageBlock.MessageBlocks.OfType<ARECD3>();
			var bC1 = message.MessageBlock.MessageBlocks.OfType<ARECC1>();
			var bC2 = message.MessageBlock.MessageBlocks.OfType<ARECC2>();
			var bC3 = message.MessageBlock.MessageBlocks.OfType<ARECC3>();

			AssertEquals(0, bD1.Count());
			AssertEquals(0, bD2.Count());
			AssertEquals(0, bD3.Count());
			AssertEquals(0, bC1.Count());
			AssertEquals(0, bC2.Count());
			AssertEquals(0, bC3.Count());

			var bP1s = message.MessageBlock.MessageBlocks.OfType<ARECP1>();
			AssertEquals(1, bP1s.Count());
			var bP1 = bP1s.FirstOrDefault();
			AssertEquals("PID", bP1.ProtestPetitionIdentifier);
			var bQ1s = message.MessageBlock.MessageBlocks.OfType<ARECQ1>();
			AssertEquals(0, bQ1s.Count());

			var b20 = message.MessageBlock.MessageBlocks.OfType<AREC20>().FirstOrDefault();
			AssertEquals("IEF", b20.AssociatedEntryFilerCode);
			AssertEquals("CN", b20.AssociatedEntryNumber);

			var b21s = message.MessageBlock.MessageBlocks.OfType<AREC21>();
			AssertEquals(1, b21s.Count());

			var b50s = message.MessageBlock.MessageBlocks.OfType<AREC50>();
			AssertEquals(2, b50s.Count());
			var b51s = message.MessageBlock.MessageBlocks.OfType<AREC51>();
			AssertEquals(0, b51s.Count());
			var b52s = message.MessageBlock.MessageBlocks.OfType<AREC52>();
			AssertEquals(2, b52s.Count());
			var b53s = message.MessageBlock.MessageBlocks.OfType<AREC53>();
			AssertEquals(2, b53s.Count());
			var b54s = message.MessageBlock.MessageBlocks.OfType<AREC54>();
			AssertEquals(0, b54s.Count());
			var b55s = message.MessageBlock.MessageBlocks.OfType<AREC55>();
			AssertEquals(0, b55s.Count());

			var b50 = b50s.ToArray()[0];
			AssertEquals("1234567890", b50.PrimaryHTSNumber);
			AssertEquals("AU", b50.CountryOfOriginCode);
			AssertEquals("SP", b50.TradeAgreementSpecialProgramClaimCode);
			AssertEquals(ZDate.Empty, b50.BeginEffectiveDate);
			AssertEquals("REASON", b50.ReconciliationReasonText);

			b50 = b50s.ToArray()[1];
			AssertEquals("1234567890", b50.PrimaryHTSNumber);
			AssertEquals("AU", b50.CountryOfOriginCode);
			AssertEquals("SP", b50.TradeAgreementSpecialProgramClaimCode);
			AssertEquals(ZDate.Empty, b50.BeginEffectiveDate);
			AssertEquals("REASON", b50.ReconciliationReasonText);

			var b52 = b52s.FirstOrDefault(x => x.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber == "EN1");
			AssertEquals("EN1", b52.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber);
			AssertEquals("EF1", b52.UnderlyingEntrySummaryLineItemIdentifier1FilerCode);
			AssertEquals("001", b52.UnderlyingEntrySummaryLineItemIdentifier1LineNumber);

			b52 = b52s.FirstOrDefault(x => x.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber == "EN2");
			AssertEquals("EN2", b52.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber);
			AssertEquals("EF2", b52.UnderlyingEntrySummaryLineItemIdentifier1FilerCode);
			AssertEquals("001", b52.UnderlyingEntrySummaryLineItemIdentifier1LineNumber);

			var b531 = b53s.ToArray()[0];
			AssertEquals("1234567899", b531.ReconciledPrimaryHTSNumber);
			AssertEquals("0000000020", b531.ReconciledPrimaryMerchandiseValue);
			AssertEquals("00000001000", b531.ReconciledPrimaryDuty);
			AssertEquals("", b531.ReconciledTradeAgreementSpecialProgramClaimCode);

			var b532 = b53s.ToArray()[1];
			AssertEquals("1234567811", b532.ReconciledPrimaryHTSNumber);
			AssertEquals("0000000030", b532.ReconciledPrimaryMerchandiseValue);
			AssertEquals("00000004000", b532.ReconciledPrimaryDuty);
			AssertEquals("", b532.ReconciledTradeAgreementSpecialProgramClaimCode);
		}

		public void Test53BlocksWithSPI()
		{
			Mock<IReconciliation> mock = GetTheMockForSPITest();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b53s = message.MessageBlock.MessageBlocks.OfType<AREC53>();
			AssertEquals(2, b53s.Count());

			var b531 = b53s.ToArray()[0];
			AssertEquals("", b531.ReconciledPrimaryHTSNumber);
			AssertEquals("0000000030", b531.ReconciledPrimaryMerchandiseValue);
			AssertEquals("00000004000", b531.ReconciledPrimaryDuty);
			AssertEquals("NS", b531.ReconciledTradeAgreementSpecialProgramClaimCode);

			var b532 = b53s.ToArray()[1];
			AssertEquals("", b532.ReconciledPrimaryHTSNumber);
			AssertEquals("0000000020", b532.ReconciledPrimaryMerchandiseValue);
			AssertEquals("00000001000", b532.ReconciledPrimaryDuty);
			AssertEquals("", b532.ReconciledTradeAgreementSpecialProgramClaimCode);
		}

		public void TestDeleteMessage()
		{
			Mock<IReconciliation> mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("D", reconciliation, true, false).Generate();
			var b11 = message.MessageBlock.MessageBlocks.OfType<AREC11>();
			var b15 = message.MessageBlock.MessageBlocks.OfType<AREC15>();
			var bQ1 = message.MessageBlock.MessageBlocks.OfType<ARECQ1>();
			var b20 = message.MessageBlock.MessageBlocks.OfType<AREC20>();
			var b21 = message.MessageBlock.MessageBlocks.OfType<AREC21>();
			var b50 = message.MessageBlock.MessageBlocks.OfType<AREC50>();
			var b51 = message.MessageBlock.MessageBlocks.OfType<AREC51>();
			var b52 = message.MessageBlock.MessageBlocks.OfType<AREC52>();
			var b53 = message.MessageBlock.MessageBlocks.OfType<AREC53>();
			var b54 = message.MessageBlock.MessageBlocks.OfType<AREC54>();
			var b55 = message.MessageBlock.MessageBlocks.OfType<AREC55>();
			var b90 = message.MessageBlock.MessageBlocks.OfType<AREC90>();
			var b91 = message.MessageBlock.MessageBlocks.OfType<AREC91>();
			var b92 = message.MessageBlock.MessageBlocks.OfType<AREC92>();
			AssertEquals(0, b11.Count());
			AssertEquals(0, b15.Count());
			AssertEquals(0, bQ1.Count());
			AssertEquals(0, b20.Count());
			AssertEquals(0, b21.Count());
			AssertEquals(0, b50.Count());
			AssertEquals(0, b51.Count());
			AssertEquals(0, b52.Count());
			AssertEquals(0, b53.Count());
			AssertEquals(0, b54.Count());
			AssertEquals(0, b55.Count());
			AssertEquals(0, b90.Count());
			AssertEquals(0, b91.Count());
			AssertEquals(0, b92.Count());
		}

		public void TestAREC52ForSplitLines()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.FTA;
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "SV971200685";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Calc_Invoice = "SV971200685";
			invoiceLine3.JI_Tariff = "9101.90.1000";
			invoiceLine3.JI_CustomsQuantity = 50m;
			invoiceLine3.US_Duty = 40m;
			invoiceLine3.US_R_OrigDuty = 20m;
			invoiceLine3.US_SPI = "N/A";
			invoiceLine3.US_R_OrigSPI = "N/A";

			invoiceLine3.JI_LinePrice = 3000m;
			invoiceLine3.US_R_OrigEntryLineNo = "2";

			SetReconOriginalValues(invoiceLine3);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Calc_Invoice = "SV971200685";
			invoiceLine.JI_Tariff = "9101.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.US_Duty = 40m;
			invoiceLine.US_R_OrigDuty = 20m;
			invoiceLine.US_SPI = "N/A";
			invoiceLine.US_R_OrigSPI = "N/A";

			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_R_OrigEntryLineNo = "1";

			SetReconOriginalValues(invoiceLine);

			var invoiceLineSplit = invoice.JobComInvoiceLines.AddNew();
			invoiceLineSplit.JI_Calc_Invoice = "SV971200685";
			invoiceLineSplit.JI_Tariff = "9101.90.1000";
			invoiceLineSplit.JI_CustomsQuantity = 50m;
			invoiceLineSplit.US_Duty = 40m;
			invoiceLineSplit.US_R_OrigDuty = 20m;
			invoiceLineSplit.US_SPI = "CA";
			invoiceLineSplit.US_R_OrigSPI = "N/A";

			invoiceLineSplit.JI_LinePrice = 3000m;
			invoiceLineSplit.US_R_OrigEntryLineNo = "1";

			SetReconOriginalValues(invoiceLineSplit);

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);

			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b52s = message.MessageBlock.MessageBlocks.OfType<AREC52>();
			AssertEquals(1, b52s.Count());
			var b52 = b52s.FirstOrDefault();
			AssertEquals("71200685", b52.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber);
			AssertEquals("SV9", b52.UnderlyingEntrySummaryLineItemIdentifier1FilerCode);
			AssertEquals("001", b52.UnderlyingEntrySummaryLineItemIdentifier1LineNumber);

			AssertEquals("71200685", b52.UnderlyingEntrySummaryLineItemIdentifier2EntrySummaryNumber);
			AssertEquals("SV9", b52.UnderlyingEntrySummaryLineItemIdentifier2FilerCode);
			AssertEquals("002", b52.UnderlyingEntrySummaryLineItemIdentifier2LineNumber);

			AssertEquals("Shouldn't generate the 3rd entry summary number", "", b52.UnderlyingEntrySummaryLineItemIdentifier3EntrySummaryNumber);
			AssertEquals("Shouldn't generate the 3rd fiter code", "", b52.UnderlyingEntrySummaryLineItemIdentifier3FilerCode);
			AssertEquals("Shouldn't generate the 3rd line number", "", b52.UnderlyingEntrySummaryLineItemIdentifier3LineNumber);
		}

		public void TestWatchRepairCase()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.US_IssueCode = ReconIssueCodeList.Codes._9802Recon;
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5222";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9101.90.1000";
			invoiceLine1.JI_CustomsQuantity = 50m;
			invoiceLine1.US_Duty = 40m;
			invoiceLine1.US_R_OrigDuty = 20m;
			invoiceLine1.US_SPI = "P1";
			invoiceLine1.US_R_OrigSPI = "S1";

			invoiceLine1.JI_LinePrice = 3000m;
			invoiceLine1.US_SupTariff = "9813.00.8015";
			invoiceLine1.US_SupDuty = 40m;
			invoiceLine1.US_R_OrigSupDuty = 20m;
			invoiceLine1.US_R_OrigEntryLineNo = "1";

			SetReconOriginalValues(invoiceLine1);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.US_Duty = 40m;
			invoiceLine2.US_R_OrigDuty = 20m;
			invoiceLine2.US_SPI = "P1";
			invoiceLine2.US_R_OrigSPI = "S1";

			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.US_SupTariff = "9813.00.8015";
			invoiceLine2.US_SupDuty = 40m;
			invoiceLine2.US_R_OrigSupDuty = 20m;
			invoiceLine2.US_R_OrigEntryLineNo = "1";
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			SetReconOriginalValues(invoiceLine2);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9301.90.1000";
			invoiceLine3.JI_CustomsQuantity = 50m;
			invoiceLine3.US_Duty = 40m;
			invoiceLine3.US_R_OrigDuty = 20m;
			invoiceLine3.US_SPI = "P1";
			invoiceLine3.US_R_OrigSPI = "S1";
			invoiceLine3.JI_LinePrice = 3000m;
			invoiceLine3.US_R_OrigEntryLineNo = "1";
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			SetReconOriginalValues(invoiceLine3);

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9101.90.1000";
			invoiceLine4.JI_CustomsQuantity = 50m;
			invoiceLine4.US_Duty = 40m;
			invoiceLine4.US_R_OrigDuty = 20m;
			invoiceLine4.US_SPI = "P2";
			invoiceLine4.US_R_OrigSPI = "S1";

			invoiceLine4.JI_LinePrice = 3000m;
			invoiceLine4.US_SupTariff = "9813.00.8015";
			invoiceLine4.US_SupDuty = 40m;
			invoiceLine4.US_R_OrigSupDuty = 20m;
			invoiceLine4.US_R_OrigEntryLineNo = "2";

			SetReconOriginalValues(invoiceLine4);

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "9201.90.1000";
			invoiceLine5.JI_CustomsQuantity = 50m;
			invoiceLine5.US_Duty = 40m;
			invoiceLine5.US_R_OrigDuty = 20m;
			invoiceLine5.US_SPI = "P2";
			invoiceLine5.US_R_OrigSPI = "S1";

			invoiceLine5.JI_LinePrice = 3000m;
			invoiceLine5.US_SupTariff = "9813.00.8015";
			invoiceLine5.US_SupDuty = 40m;
			invoiceLine5.US_R_OrigSupDuty = 20m;
			invoiceLine5.US_R_OrigEntryLineNo = "2";
			invoiceLine5.JI_ParentID = invoiceLine4.PK;
			SetReconOriginalValues(invoiceLine5);

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "9301.90.1000";
			invoiceLine6.JI_CustomsQuantity = 50m;
			invoiceLine6.US_Duty = 40m;
			invoiceLine6.US_R_OrigDuty = 20m;
			invoiceLine6.US_SPI = "P2";
			invoiceLine6.US_R_OrigSPI = "S1";
			invoiceLine6.JI_LinePrice = 3000m;
			invoiceLine6.US_R_OrigEntryLineNo = "2";
			invoiceLine6.JI_ParentID = invoiceLine4.PK;
			SetReconOriginalValues(invoiceLine6);

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);

			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b50s = message.MessageBlock.MessageBlocks.OfType<AREC50>();
			AssertEquals(1, b50s.Count());
			var b50 = b50s.FirstOrDefault();
			AssertEquals("9813008015", b50.PrimaryHTSNumber);

			var b51s = message.MessageBlock.MessageBlocks.OfType<AREC51>().ToArray();
			AssertEquals(2, b51s.Length);
			var b511 = b51s[0];
			AssertEquals("9101901000", b511.AdditionalIdentifyingHTSNumber1);
			AssertEquals("9813008015", b511.AdditionalIdentifyingHTSNumber2);
			AssertEquals("9201901000", b511.AdditionalIdentifyingHTSNumber3);
			AssertEquals("9813008015", b511.AdditionalIdentifyingHTSNumber4);
			var b512 = b51s[1];
			AssertEquals("9301901000", b512.AdditionalIdentifyingHTSNumber1);

			var b52s = message.MessageBlock.MessageBlocks.OfType<AREC52>();
			AssertEquals(1, b52s.Count());
			var b52 = b52s.FirstOrDefault();
			AssertEquals("222", b52.UnderlyingEntrySummaryLineItemIdentifier1EntrySummaryNumber);
			AssertEquals("XJ5", b52.UnderlyingEntrySummaryLineItemIdentifier1FilerCode);
			AssertEquals("001", b52.UnderlyingEntrySummaryLineItemIdentifier1LineNumber);

			AssertEquals("222", b52.UnderlyingEntrySummaryLineItemIdentifier2EntrySummaryNumber);
			AssertEquals("XJ5", b52.UnderlyingEntrySummaryLineItemIdentifier2FilerCode);
			AssertEquals("002", b52.UnderlyingEntrySummaryLineItemIdentifier2LineNumber);

			var b53s = message.MessageBlock.MessageBlocks.OfType<AREC53>().ToArray();
			AssertEquals(2, b53s.Length);
			var b531 = b53s[0];
			var b532 = b53s[1];
			AssertEquals("", b531.ReconciledPrimaryHTSNumber);
			AssertEquals("", b531.ReconciledTradeAgreementSpecialProgramClaimCode);

			AssertEquals("", b532.ReconciledPrimaryHTSNumber);
			AssertEquals("", b532.ReconciledTradeAgreementSpecialProgramClaimCode);

			var b54s = message.MessageBlock.MessageBlocks.OfType<AREC54>().ToArray();
			AssertEquals(6, b54s.Length);

			AssertEquals("", b54s[0].AdditionalReconciledHTSNumber1);
			AssertEquals("", b54s[0].ReconciledHTSNumber2);
			AssertEquals("", b54s[1].AdditionalReconciledHTSNumber1);
			AssertEquals("", b54s[1].ReconciledHTSNumber2);
			AssertEquals("", b54s[2].AdditionalReconciledHTSNumber1);

			AssertEquals("", b54s[3].AdditionalReconciledHTSNumber1);
			AssertEquals("", b54s[3].ReconciledHTSNumber2);
			AssertEquals("", b54s[4].AdditionalReconciledHTSNumber1);
			AssertEquals("", b54s[4].ReconciledHTSNumber2);
			AssertEquals("", b54s[5].AdditionalReconciledHTSNumber1);
		}

		public void TestTotalFeeBlocksWithNoChangeAggregate()
		{
			Mock<IReconciliation> mock = GetTheMock();
			mock.Setup(m => m.IsNoChangeAggregate).Returns(true);

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b91s = message.MessageBlock.MessageBlocks.OfType<AREC91>();
			var b92s = message.MessageBlock.MessageBlocks.OfType<AREC92>();
			AssertEquals(0, b91s.Count());
			AssertEquals(0, b92s.Count());
		}

		public void TestTotalFeeBlocksWithAggregate()
		{
			Mock<IReconciliation> mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b91s = message.MessageBlock.MessageBlocks.OfType<AREC91>();
			var b92s = message.MessageBlock.MessageBlocks.OfType<AREC92>();
			AssertEquals(1, b91s.Count());
			var b91 = b91s.FirstOrDefault();
			AssertEquals("001", b91.AccountingClassCode1);
			AssertEquals("00000002000", b91.ReconciledRevenueAmountTotal1);
			AssertEquals("002", b91.AccountingClassCode2);
			AssertEquals("00000001000", b91.ReconciledRevenueAmountTotal2);

			AssertEquals(1, b92s.Count());
			var b92 = b92s.FirstOrDefault();
			AssertEquals("001", b92.AccountingClassCode1);
			AssertEquals("00000001000", b92.PayableRevenueAmountTotal1);
			AssertEquals("002", b92.AccountingClassCode2);
			AssertEquals("00000000000", b92.PayableRevenueAmountTotal2);

			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(true);
			reconciliation = mock.Object;
			message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			text = message.EM_FormattedMessageText;
			messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			b91s = message.MessageBlock.MessageBlocks.OfType<AREC91>();
			b92s = message.MessageBlock.MessageBlocks.OfType<AREC92>();
			AssertEquals(1, b91s.Count());
			b91 = b91s.FirstOrDefault();
			AssertEquals("001", b91.AccountingClassCode1);
			AssertEquals("00000002000", b91.ReconciledRevenueAmountTotal1);
			AssertEquals("002", b91.AccountingClassCode2);
			AssertEquals("00000001000", b91.ReconciledRevenueAmountTotal2);
			AssertEquals("044", b91.AccountingClassCode3);
			AssertEquals("00000004000", b91.ReconciledRevenueAmountTotal3);

			AssertEquals(1, b92s.Count());
			b92 = b92s.FirstOrDefault();
			AssertEquals("001", b92.AccountingClassCode1);
			AssertEquals("00000001000", b92.PayableRevenueAmountTotal1);
			AssertEquals("002", b92.AccountingClassCode2);
			AssertEquals("00000000000", b92.PayableRevenueAmountTotal2);
			AssertEquals("044", b92.AccountingClassCode3);
			AssertEquals("00000004000", b92.PayableRevenueAmountTotal3);
		}

		public void TestBlock90AndBlock92()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			reconDec.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			reconDec.US_PreliminaryStatementPrintDate = new ZDate(2018, 3, 7);
			reconDec.US_ClientBranchDesignation = "AA";

			var originalEntry = reconDec.OriginalEntries.AddNew();
			originalEntry.CH_OrigEntryReference = "XJ5222";
			originalEntry.US_ImportDate = new ZDateTime(2007, 12, 31);
			originalEntry.US_R_DutyRateDate = new ZDateTime(2008, 1, 5);
			originalEntry.US_R_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			originalEntry.US_SchDEntry = "1234";
			originalEntry.US_R_CalcOrigDuty = true;

			var invoice = originalEntry.Invoice;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_InvoiceNumber = "HAOA780";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "KR";
			invoice.US_UC_NKCountryOfOrigin = "KR";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9101.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.US_Duty = 40m;
			invoiceLine.US_R_OrigDuty = 20m;
			invoiceLine.US_SPI = "P1";
			invoiceLine.US_R_OrigSPI = "S1";

			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SupTariff = "9813.00.8015";
			invoiceLine.US_SupDuty = 40m;
			invoiceLine.US_R_OrigSupDuty = 20m;
			invoiceLine.US_R_OrigEntryLineNo = "1";

			SetReconOriginalValues(invoiceLine);

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);
			var message = new ACEReconciliationMessageBuilder("R", reconciliation, true, false).Generate();
			var messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b90s = message.MessageBlock.MessageBlocks.OfType<AREC90>();
			AssertEquals(1, b90s.Count());
			var b90 = b90s.FirstOrDefault();
			AssertEquals("PaymentTypeCode", PaymentTypeList.Codes.IndividualBasis, b90.PaymentTypeCode);
			AssertEquals("PreliminaryStatementPrintDate", new ZDate(2018, 3, 7), b90.PreliminaryStatementPrintDate);
			AssertEquals("StatementClientBranchIdentifier", "AA", b90.StatementClientBranchIdentifier);

			var b92s = message.MessageBlock.MessageBlocks.OfType<AREC92>().ToArray();
			AssertEquals(1, b92s.Length);
			var b92 = b92s.FirstOrDefault();
			AssertEquals("AccountingClassCode1", "001", b92.AccountingClassCode1);
			AssertEquals("00000000000", b92.PayableRevenueAmountTotal1);

			var reconciliationPaid = new ReconDeclarationIReconciliation(reconDec);
			var messagePaid = new ACEReconciliationMessageBuilder("R", reconciliationPaid, true, true).Generate();
			var messageBlockPaid = new ABIInputBlockControlGenerator(reconciliationPaid.EntryFilerCode, reconciliationPaid.ProcessingDistrictPort, ZString.Empty);
			messageBlockPaid.Deserialise(BlockPadder.Pad(messagePaid.EM_MessageText));

			var b90sPaid = messagePaid.MessageBlock.MessageBlocks.OfType<AREC90>();
			AssertEquals(1, b90sPaid.Count());
			var b90Paid = b90sPaid.FirstOrDefault();
			AssertEquals("PaymentTypeCode", ZString.Empty, b90Paid.PaymentTypeCode);
			AssertEquals("PreliminaryStatementPrintDate", ZDate.Empty, b90Paid.PreliminaryStatementPrintDate);
			AssertEquals("StatementClientBranchIdentifier", ZString.Empty, b90Paid.StatementClientBranchIdentifier);

			var b92sPaid = messageBlockPaid.MessageBlocks.OfType<AREC92>().ToArray();
			AssertEquals(0, b92sPaid.Length);
		}

		public void TestTotalFeeBlocksWithRefundFee()
		{
			Mock<IReconciliation> mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b91s = message.MessageBlock.MessageBlocks.OfType<AREC91>().ToArray();
			var b92s = message.MessageBlock.MessageBlocks.OfType<AREC92>().ToArray();
			AssertEquals(1, b91s.Length);
			AssertEquals(1, b92s.Length);
			AssertEquals("001", b91s[0].AccountingClassCode1);
			AssertEquals("00000002000", b91s[0].ReconciledRevenueAmountTotal1);
			AssertEquals("002", b91s[0].AccountingClassCode2);
			AssertEquals("00000001000", b91s[0].ReconciledRevenueAmountTotal2);

			AssertEquals("001", b92s[0].AccountingClassCode1);
			AssertEquals("00000001000", b92s[0].PayableRevenueAmountTotal1);
			AssertEquals("002", b92s[0].AccountingClassCode2);
			AssertEquals("00000000000", b92s[0].PayableRevenueAmountTotal2);
		}

		public void TestRemoteLocationFiling()
		{
			var bBlock = GetAABIInputB();
			CombineAssertions(() =>
			{
				AssertEquals("1234", bBlock.RemotePreparerDistrictPortCode);
				AssertEquals("XJ5", bBlock.RemotePreparerFilerCode);
				AssertEquals("1", bBlock.RemotePreparerOfficeCode);
				AssertEquals("1", bBlock.RemotelyFiledIndicator);
			});
		}

		public void TestFTA()
		{
			Mock<IReconciliation> mock = GetTheMock();
			mock.Setup(m => m.IssueCode).Returns(ReconIssueCodeList.Codes.FTA);
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b21s = message.MessageBlock.MessageBlocks.OfType<AREC21>();
			AssertEquals(1, b21s.Count());
			var b53s = message.MessageBlock.MessageBlocks.OfType<AREC53>().ToArray();
			AssertEquals(2, b53s.Length);
			var b531 = b53s[0];
			var b532 = b53s[1];
			AssertEquals("", b531.ReconciledPrimaryHTSNumber);
			AssertEquals("", b532.ReconciledPrimaryHTSNumber);
			AssertEquals("RS", b531.ReconciledTradeAgreementSpecialProgramClaimCode);
			AssertEquals("NS", b532.ReconciledTradeAgreementSpecialProgramClaimCode);
		}

		[TestDate(2018, 10, 24)]
		public void TestCottonFeeMandatory()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			reconDec.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			reconDec.US_PreliminaryStatementPrintDate = new ZDate(2018, 10, 24);
			reconDec.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;

			var originalEntry1 = reconDec.OriginalEntries.AddNew();
			originalEntry1.CH_OrigEntryReference = "SV974562137";
			originalEntry1.US_ImportDate = new ZDateTime(2017, 07, 01);
			originalEntry1.US_R_DutyRateDate = new ZDateTime(2017, 07, 01);
			originalEntry1.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntry1.US_SchDEntry = "1101";
			var invoiceLine1 = originalEntry1.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "5609004000";
			invoiceLine1.JI_CustomsQuantity = 50m;
			invoiceLine1.US_Duty = 40m;
			invoiceLine1.US_R_OrigDuty = 20m;
			invoiceLine1.JI_LinePrice = 3000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "CH";
			invoiceLine1.US_CottonFeeExempt = "Y";
			SetReconOriginalValues(invoiceLine1);

			var originalEntry2 = reconDec.OriginalEntries.AddNew();
			originalEntry2.CH_OrigEntryReference = "SV974556188";
			originalEntry2.US_ImportDate = new ZDateTime(2017, 07, 01);
			originalEntry2.US_R_DutyRateDate = new ZDateTime(2017, 07, 01);
			originalEntry2.US_R_IsHMFApplicable = YesNoDefaultList.Codes.No;
			originalEntry2.US_SchDEntry = "1101";
			var invoiceLine2 = originalEntry2.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5609004000";
			invoiceLine2.JI_CustomsQuantity = 60m;
			invoiceLine2.US_Duty = 450m;
			invoiceLine2.US_R_OrigDuty = 25m;
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "CH";
			invoiceLine2.US_CottonFeeExempt = "Y";
			SetReconOriginalValues(invoiceLine2);

			var reconciliation = new ReconDeclarationIReconciliation(reconDec);
			var message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			AssertContains(
@"B         RE                                           1   <<MSGNO PLACEHOLDER>>
10A     <E#PLCH>                 X                             CE1US            
11                    0732682903                                                
20SV9  74562137                                                                 
2100100000000000 49900000000000                                                 
20SV9  74556188                                                                 
2100100000000000 49900000000000                                                 
505609004000CH                                                                  
52SV9  74556188  001 SV9  74562137  001                                         
53           0000004000 00000049000                                             
5500100000049000 49900000000000                                                 
901 102418                                                                      
9100100000000000 49900000000000                                                 
9200100000000000 49900000000000                                                 
Y         RE", message.EM_FormattedMessageText);

			originalEntry1.US_R_CottonFeeMandatory = true;
			reconciliation = new ReconDeclarationIReconciliation(reconDec);
			message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			AssertContains(
@"B         RE                                           1   <<MSGNO PLACEHOLDER>>
10A     <E#PLCH>                 X                             CE1US            
11                    0732682903                                                
20SV9  74562137                                                                 
2100100000000000 49900000000000 05600000000000                                  
20SV9  74556188                                                                 
2100100000000000 49900000000000                                                 
505609004000CH                                                                  
52SV9  74556188  001                                                            
53           0000001000 00000045000                                             
5500100000045000 49900000000000                                                 
505609004000CH                                                                  
52SV9  74562137  001                                                            
53           0000003000 00000004000                                             
5500100000004000 49900000000000 05600000000000                                  
901 102418                                                                      
9100100000000000 49900000000000 05600000000000                                  
9200100000000000 49900000000000 05600000000000                                  
Y         RE", message.EM_FormattedMessageText);
		}

		public void TestInterestFee()
		{
			Mock<IReconciliation> mock = GetTheMock();
			var importEntries = new List<IReconciliationImportEntry>();
			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.EntryFilerCode).Returns("IEF");
			importEntryMock.Setup(m => m.EntryNumber).Returns("CN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);
			importEntryMock.Setup(m => m.ProtestID).Returns("PID");
			importEntryMock.Setup(m => m.PendingActionIDType).Returns("");
			importEntryMock.Setup(m => m.PendingActionID).Returns("");

			var entryFeeMock1 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock1.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock1.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock1.Setup(m => m.EstimatedReconciliationFee).Returns(20m);

			var entryFeeMock2 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock2.Setup(m => m.FeeClass).Returns("044");
			entryFeeMock2.Setup(m => m.OriginalFee).Returns(0m);
			entryFeeMock2.Setup(m => m.EstimatedReconciliationFee).Returns(100m);
			var entryFees = new List<IReconciliationImportEntryFee>();
			entryFees.Add(entryFeeMock1.Object);
			entryFees.Add(entryFeeMock2.Object);

			importEntryMock.Setup(m => m.Fees).Returns(entryFees);
			importEntries.Add(importEntryMock.Object);
			mock.Setup(m => m.ImportEntries).Returns(importEntries);
			mock.Setup(m => m.InterestPaymentAmount).Returns(200m);

			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			ZString text = message.EM_FormattedMessageText;
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b21 = message.MessageBlock.MessageBlocks.OfType<AREC21>().FirstOrDefault();
			AssertEquals("001", b21.AccountingClassCodeEntrySummary1);
			AssertEquals("044", b21.AccountingClassCodeEntrySummary2);
			AssertEquals("00000010000", b21.EstimatedReconciledRevenueAmountEntrySummary2);

			var b91 = message.MessageBlock.MessageBlocks.OfType<AREC91>().FirstOrDefault();
			AssertEquals("001", b91.AccountingClassCode1);
			AssertEquals("044", b91.AccountingClassCode2);
			AssertEquals("00000010000", b91.ReconciledRevenueAmountTotal2);

			Mock<IReconciliation> mock2 = GetTheMock();
			var importEntries2 = new List<IReconciliationImportEntry>();
			var importEntryMock2 = new Mock<IReconciliationImportEntry>();
			importEntryMock2.Setup(m => m.EntryFilerCode).Returns("IEF");
			importEntryMock2.Setup(m => m.EntryNumber).Returns("CN");
			importEntryMock2.Setup(m => m.Port).Returns("3901");
			importEntryMock2.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock2.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock2.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock2.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock2.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);
			importEntryMock2.Setup(m => m.ProtestID).Returns("PID");
			importEntryMock2.Setup(m => m.PendingActionIDType).Returns("");
			importEntryMock2.Setup(m => m.PendingActionID).Returns("");

			var entryFeeMock21 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock21.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock21.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock21.Setup(m => m.EstimatedReconciliationFee).Returns(20m);
			var entryFees2 = new List<IReconciliationImportEntryFee>();
			entryFees2.Add(entryFeeMock21.Object);

			importEntryMock2.Setup(m => m.Fees).Returns(entryFees2);
			importEntries2.Add(importEntryMock2.Object);
			mock2.Setup(m => m.ImportEntries).Returns(importEntries2);
			mock2.Setup(m => m.InterestPaymentAmount).Returns(200m);

			mock2.Setup(m => m.AggregateReconciliationIndicator).Returns(true);

			reconciliation = mock2.Object;
			message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			text = message.EM_FormattedMessageText;
			messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));

			var b21s = message.MessageBlock.MessageBlocks.OfType<AREC21>();
			AssertEquals(0, b21s.Count());

			b91 = message.MessageBlock.MessageBlocks.OfType<AREC91>().FirstOrDefault();
			AssertEquals("001", b91.AccountingClassCode1);
			AssertEquals("00000002000", b91.ReconciledRevenueAmountTotal1);
			AssertEquals("044", b91.AccountingClassCode2);
			AssertEquals("00000020000", b91.ReconciledRevenueAmountTotal2);
		}

		AABIInputB GetAABIInputB()
		{
			var mock = GetTheMock();
			IReconciliation reconciliation = mock.Object;
			MQEDIMessage message = new ACEReconciliationMessageBuilder("A", reconciliation, true, false).Generate();
			BlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(reconciliation.EntryFilerCode, reconciliation.ProcessingDistrictPort, ZString.Empty);
			messageBlock.Deserialise(BlockPadder.Pad(message.EM_MessageText));
			return (AABIInputB)message.MessageBlock.B;
		}

		void SetReconOriginalValues(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigCV = invoiceLine.JI_CustomsValue >= invoiceLine.US_98GoodsValue ? (ZDecimal)(invoiceLine.JI_CustomsValue - invoiceLine.US_98GoodsValue) : invoiceLine.JI_CustomsValue;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_R_OrigSecondQty = invoiceLine.JI_CustomsSecondQuantity;
			invoiceLine.US_R_OrigCottonFeeExempt = invoiceLine.US_CottonFeeExempt;

			invoiceLine.US_R_OrigSupTariff = invoiceLine.US_SupTariff;
			invoiceLine.US_R_Orig98Value = invoiceLine.US_98GoodsValue;
			invoiceLine.US_R_OrigSupQty1 = invoiceLine.US_SupQty1;
			invoiceLine.US_R_OrigSupQty2 = invoiceLine.US_SupQty2;
			invoiceLine.US_R_OrigSupQty3 = invoiceLine.US_SupQty3;
		}

		Mock<IReconciliation> GetTheMock()
		{
			var mock = new Mock<IReconciliation>();
			// R10
			mock.Setup(m => m.CompanyPK).Returns(GlbCompany.CurrentCompany.PK.ToGuid());
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("VL");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(false);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			mock.Setup(m => m.DesignatedNotifyParty4811Number).Returns("4811");
			mock.Setup(m => m.PriorDisclosureIndicator).Returns(true);
			mock.Setup(m => m.ContactName).Returns("BrokerName");
			mock.Setup(m => m.ContactPhone).Returns("99999999");
			mock.Setup(m => m.ContactEmail).Returns("Email");
			mock.Setup(m => m.QualifyingGoodFreeTradeDec).Returns(false);
			mock.Setup(m => m.SummaryDocProvidedStatement).Returns(false);
			mock.Setup(m => m.NAFTA303ClaimStatement).Returns(false);
			mock.Setup(m => m.ProtestOrPetitionFiledStatement).Returns(false);
			mock.Setup(m => m.DocumentRecipientID).Returns("Recipient111");
			mock.Setup(m => m.DocumentProvidedDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.DocumentRecipientAddress).Returns((JobDocAddress)null);
			mock.Setup(m => m.ClaimentID).Returns("Claiment1111");
			mock.Setup(m => m.ClaimDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClaimIdentifier).Returns("ID1111");
			mock.Setup(m => m.ClaimentAddress).Returns((JobDocAddress)null);

			var importEntries = new List<IReconciliationImportEntry>();
			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.EntryFilerCode).Returns("IEF");
			importEntryMock.Setup(m => m.EntryNumber).Returns("CN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);
			importEntryMock.Setup(m => m.ProtestID).Returns("PID");
			importEntryMock.Setup(m => m.PendingActionIDType).Returns("");
			importEntryMock.Setup(m => m.PendingActionID).Returns("");

			var entryFeeMock1 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock1.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock1.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock1.Setup(m => m.EstimatedReconciliationFee).Returns(20m);

			var entryFeeMock2 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock2.Setup(m => m.FeeClass).Returns("002");
			entryFeeMock2.Setup(m => m.OriginalFee).Returns(20m);
			entryFeeMock2.Setup(m => m.EstimatedReconciliationFee).Returns(10m);
			var entryFees = new List<IReconciliationImportEntryFee>();
			entryFees.Add(entryFeeMock1.Object);
			entryFees.Add(entryFeeMock2.Object);

			importEntryMock.Setup(m => m.Fees).Returns(entryFees);
			importEntries.Add(importEntryMock.Object);

			mock.Setup(m => m.ImportEntries).Returns(importEntries);
			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			var originalEntryLineMock1 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock1.Setup(m => m.EntryNumber).Returns("EN1");
			originalEntryLineMock1.Setup(m => m.EntryFilerCode).Returns("EF1");
			originalEntryLineMock1.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines1 = new List<IReconOriginalEntryLine>();
			originalEntryLines1.Add(originalEntryLineMock1.Object);

			var originalEntryLineMock2 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock2.Setup(m => m.EntryNumber).Returns("EN2");
			originalEntryLineMock2.Setup(m => m.EntryFilerCode).Returns("EF2");
			originalEntryLineMock2.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines2 = new List<IReconOriginalEntryLine>();
			originalEntryLines2.Add(originalEntryLineMock2.Object);

			var mergedEntryLines = new List<IReconEntryLineGroup>();
			var mergedEntryLineMock1 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock1.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock1.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock1.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock1.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock1.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock1.Setup(m => m.ReconHTS).Returns("1234567899");
			mergedEntryLineMock1.Setup(m => m.ReconCustomsValue).Returns(20m);
			mergedEntryLineMock1.Setup(m => m.ReconDuty).Returns(10m);
			mergedEntryLineMock1.Setup(m => m.ReconSPI).Returns("RS");
			mergedEntryLineMock1.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock1.Setup(m => m.AllAdditionalHTSs).Returns("");
			mergedEntryLineMock1.Setup(m => m.SecondaryLines).Returns(System.Array.Empty<IReconSecondaryLine>());
			mergedEntryLineMock1.Setup(m => m.Fees).Returns(System.Array.Empty<IReconciliationImportEntryFee>());
			mergedEntryLineMock1.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines1);
			mergedEntryLineMock1.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock1.Setup(m => m.IsNAFTARecon).Returns(false);
			mergedEntryLineMock1.Setup(m => m.CalculateYear).Returns("2022");

			var mergedEntryLineMock2 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock2.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock2.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock2.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock2.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock2.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock2.Setup(m => m.ReconHTS).Returns("1234567811");
			mergedEntryLineMock2.Setup(m => m.ReconCustomsValue).Returns(30m);
			mergedEntryLineMock2.Setup(m => m.ReconDuty).Returns(40m);
			mergedEntryLineMock2.Setup(m => m.ReconSPI).Returns("NS");
			mergedEntryLineMock2.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock2.Setup(m => m.AllAdditionalHTSs).Returns("");
			mergedEntryLineMock2.Setup(m => m.SecondaryLines).Returns(System.Array.Empty<IReconSecondaryLine>());
			mergedEntryLineMock2.Setup(m => m.Fees).Returns(System.Array.Empty<IReconciliationImportEntryFee>());
			mergedEntryLineMock2.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines2);
			mergedEntryLineMock2.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock2.Setup(m => m.IsNAFTARecon).Returns(false);
			mergedEntryLineMock2.Setup(m => m.CalculateYear).Returns("2023");

			mergedEntryLines.Add(mergedEntryLineMock1.Object);
			mergedEntryLines.Add(mergedEntryLineMock2.Object);
			mock.Setup(m => m.EntryLineGroups).Returns(mergedEntryLines);
			return mock;
		}

		Mock<IReconciliation> GetTheMockForSPITest()
		{
			var mock = new Mock<IReconciliation>();
			// R10
			mock.Setup(m => m.CompanyPK).Returns(GlbCompany.CurrentCompany.PK.ToGuid());
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("NF");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(false);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			mock.Setup(m => m.DesignatedNotifyParty4811Number).Returns("4811");
			mock.Setup(m => m.PriorDisclosureIndicator).Returns(true);
			mock.Setup(m => m.ContactName).Returns("BrokerName");
			mock.Setup(m => m.ContactPhone).Returns("99999999");
			mock.Setup(m => m.ContactEmail).Returns("Email");
			mock.Setup(m => m.QualifyingGoodFreeTradeDec).Returns(false);
			mock.Setup(m => m.SummaryDocProvidedStatement).Returns(false);
			mock.Setup(m => m.NAFTA303ClaimStatement).Returns(false);
			mock.Setup(m => m.ProtestOrPetitionFiledStatement).Returns(false);
			mock.Setup(m => m.DocumentRecipientID).Returns("Recipient111");
			mock.Setup(m => m.DocumentProvidedDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.DocumentRecipientAddress).Returns((JobDocAddress)null);
			mock.Setup(m => m.ClaimentID).Returns("Claiment1111");
			mock.Setup(m => m.ClaimDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClaimIdentifier).Returns("ID1111");
			mock.Setup(m => m.ClaimentAddress).Returns((JobDocAddress)null);

			var importEntries = new List<IReconciliationImportEntry>();
			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.EntryFilerCode).Returns("IEF");
			importEntryMock.Setup(m => m.EntryNumber).Returns("CN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);
			importEntryMock.Setup(m => m.ProtestID).Returns("PID");
			importEntryMock.Setup(m => m.PendingActionIDType).Returns("");
			importEntryMock.Setup(m => m.PendingActionID).Returns("");

			var entryFeeMock1 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock1.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock1.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock1.Setup(m => m.EstimatedReconciliationFee).Returns(20m);

			var entryFeeMock2 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock2.Setup(m => m.FeeClass).Returns("002");
			entryFeeMock2.Setup(m => m.OriginalFee).Returns(20m);
			entryFeeMock2.Setup(m => m.EstimatedReconciliationFee).Returns(10m);
			var entryFees = new List<IReconciliationImportEntryFee>();
			entryFees.Add(entryFeeMock1.Object);
			entryFees.Add(entryFeeMock2.Object);

			importEntryMock.Setup(m => m.Fees).Returns(entryFees);
			importEntries.Add(importEntryMock.Object);

			mock.Setup(m => m.ImportEntries).Returns(importEntries);
			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			var originalEntryLineMock1 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock1.Setup(m => m.EntryNumber).Returns("EN1");
			originalEntryLineMock1.Setup(m => m.EntryFilerCode).Returns("EF1");
			originalEntryLineMock1.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines1 = new List<IReconOriginalEntryLine>();
			originalEntryLines1.Add(originalEntryLineMock1.Object);

			var originalEntryLineMock2 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock2.Setup(m => m.EntryNumber).Returns("EN2");
			originalEntryLineMock2.Setup(m => m.EntryFilerCode).Returns("EF2");
			originalEntryLineMock2.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines2 = new List<IReconOriginalEntryLine>();
			originalEntryLines2.Add(originalEntryLineMock2.Object);

			var mergedEntryLines = new List<IReconEntryLineGroup>();
			var mergedEntryLineMock1 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock1.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock1.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock1.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock1.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock1.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock1.Setup(m => m.ReconHTS).Returns("1234567899");
			mergedEntryLineMock1.Setup(m => m.ReconCustomsValue).Returns(20m);
			mergedEntryLineMock1.Setup(m => m.ReconDuty).Returns(10m);
			mergedEntryLineMock1.Setup(m => m.ReconSPI).Returns("");
			mergedEntryLineMock1.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock1.Setup(m => m.AllAdditionalHTSs).Returns("");
			mergedEntryLineMock1.Setup(m => m.SecondaryLines).Returns(System.Array.Empty<IReconSecondaryLine>());
			mergedEntryLineMock1.Setup(m => m.Fees).Returns(System.Array.Empty<IReconciliationImportEntryFee>());
			mergedEntryLineMock1.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines1);
			mergedEntryLineMock1.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock1.Setup(m => m.IsNAFTARecon).Returns(false);

			var mergedEntryLineMock2 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock2.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock2.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock2.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock2.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock2.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock2.Setup(m => m.ReconHTS).Returns("1234567811");
			mergedEntryLineMock2.Setup(m => m.ReconCustomsValue).Returns(30m);
			mergedEntryLineMock2.Setup(m => m.ReconDuty).Returns(40m);
			mergedEntryLineMock2.Setup(m => m.ReconSPI).Returns("NS");
			mergedEntryLineMock2.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock2.Setup(m => m.AllAdditionalHTSs).Returns("");
			mergedEntryLineMock2.Setup(m => m.SecondaryLines).Returns(System.Array.Empty<IReconSecondaryLine>());
			mergedEntryLineMock2.Setup(m => m.Fees).Returns(System.Array.Empty<IReconciliationImportEntryFee>());
			mergedEntryLineMock2.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines2);
			mergedEntryLineMock2.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock2.Setup(m => m.IsNAFTARecon).Returns(false);

			mergedEntryLines.Add(mergedEntryLineMock1.Object);
			mergedEntryLines.Add(mergedEntryLineMock2.Object);
			mock.Setup(m => m.EntryLineGroups).Returns(mergedEntryLines);
			return mock;
		}

		Mock<IReconciliation> GetTheMockForIsNAFTARecon()
		{
			var mock = new Mock<IReconciliation>();
			// R10
			mock.Setup(m => m.CompanyPK).Returns(GlbCompany.CurrentCompany.PK.ToGuid());
			mock.Setup(m => m.EntryFilerCode).Returns("XJ5");
			mock.Setup(m => m.Factory).Returns(Factory);
			mock.Setup(m => m.EntryNumber).Returns("EN");
			mock.Setup(m => m.ProcessingDistrictPort).Returns("2304");
			mock.Setup(m => m.PreparerDistrictPort).Returns("1234");
			mock.Setup(m => m.OfficeCode).Returns("1");
			mock.Setup(m => m.ImporterID).Returns("IID");
			mock.Setup(m => m.SuretyCode).Returns("SCD");
			mock.Setup(m => m.EstimatedReconciliationEntrySummaryDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.IssueCode).Returns("VL");
			mock.Setup(m => m.AggregateReconciliationIndicator).Returns(false);
			mock.Setup(m => m.IncreaseRefundIndicator).Returns("2");
			mock.Setup(m => m.IsWaiveRefund).Returns(false);
			mock.Setup(m => m.EarliestImportDate).Returns(new ZDate(2000, 1, 1));
			mock.Setup(m => m.EarliestEntrySummaryDate).Returns(new ZDate(2000, 1, 2));
			mock.Setup(m => m.AgentBrokerReferenceID).Returns("ABR");
			mock.Setup(m => m.BrokerReferenceNumber).Returns("BRN");

			// R15 + R16
			mock.Setup(m => m.ImportEntrySource).Returns(2);
			mock.Setup(m => m.TextComment).Returns(new ZString('A', 75) + new ZString('B', 76));

			mock.Setup(m => m.PaymentTypeIndicator).Returns(PaymentTypeList.Codes.IndividualBasis);
			mock.Setup(m => m.PreliminaryStatementPrintDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClientBranchDesignation).Returns("");

			mock.Setup(m => m.DutyPaymentAmount).Returns(10m);
			mock.Setup(m => m.TaxPaymentAmount).Returns(20m);
			mock.Setup(m => m.FeePaymentAmount).Returns(30m);
			mock.Setup(m => m.InterestPaymentAmount).Returns(40m);
			mock.Setup(m => m.TeamNumber).Returns(new ZString("R1R"));
			mock.Setup(m => m.AggregateRefundedFees).Returns(System.Array.Empty<ZString>());

			mock.Setup(m => m.DesignatedNotifyParty4811Number).Returns("4811");
			mock.Setup(m => m.PriorDisclosureIndicator).Returns(true);
			mock.Setup(m => m.ContactName).Returns("BrokerName");
			mock.Setup(m => m.ContactPhone).Returns("99999999");
			mock.Setup(m => m.ContactEmail).Returns("Email");
			mock.Setup(m => m.QualifyingGoodFreeTradeDec).Returns(false);
			mock.Setup(m => m.SummaryDocProvidedStatement).Returns(false);
			mock.Setup(m => m.NAFTA303ClaimStatement).Returns(false);
			mock.Setup(m => m.ProtestOrPetitionFiledStatement).Returns(false);
			mock.Setup(m => m.DocumentRecipientID).Returns("Recipient111");
			mock.Setup(m => m.DocumentProvidedDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.DocumentRecipientAddress).Returns((JobDocAddress)null);
			mock.Setup(m => m.ClaimentID).Returns("Claiment1111");
			mock.Setup(m => m.ClaimDate).Returns(ZDate.BrettsBirthday);
			mock.Setup(m => m.ClaimIdentifier).Returns("ID1111");
			mock.Setup(m => m.ClaimentAddress).Returns((JobDocAddress)null);

			var importEntries = new List<IReconciliationImportEntry>();
			var importEntryMock = new Mock<IReconciliationImportEntry>();
			importEntryMock.Setup(m => m.EntryFilerCode).Returns("IEF");
			importEntryMock.Setup(m => m.EntryNumber).Returns("CN");
			importEntryMock.Setup(m => m.Port).Returns("3901");
			importEntryMock.Setup(m => m.OriginalDuty).Returns(100m);
			importEntryMock.Setup(m => m.EstimatedReconciliationDuty).Returns(120m);
			importEntryMock.Setup(m => m.OriginalTax).Returns(200m);
			importEntryMock.Setup(m => m.EstimatedReconciliationTax).Returns(220m);
			importEntryMock.Setup(m => m.EstimatedReconciliationInterest).Returns(30m);
			importEntryMock.Setup(m => m.ProtestID).Returns("PID");
			importEntryMock.Setup(m => m.PendingActionIDType).Returns("");
			importEntryMock.Setup(m => m.PendingActionID).Returns("");

			var entryFeeMock1 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock1.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock1.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock1.Setup(m => m.EstimatedReconciliationFee).Returns(20m);

			var entryFeeMock2 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock2.Setup(m => m.FeeClass).Returns("002");
			entryFeeMock2.Setup(m => m.OriginalFee).Returns(20m);
			entryFeeMock2.Setup(m => m.EstimatedReconciliationFee).Returns(10m);
			var entryFees = new List<IReconciliationImportEntryFee>();
			entryFees.Add(entryFeeMock1.Object);
			entryFees.Add(entryFeeMock2.Object);

			importEntryMock.Setup(m => m.Fees).Returns(entryFees);
			importEntries.Add(importEntryMock.Object);

			mock.Setup(m => m.ImportEntries).Returns(importEntries);
			mock.Setup(m => m.IsNoChangeAggregate).Returns(false);

			var originalEntryLineMock1 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock1.Setup(m => m.EntryNumber).Returns("EN1");
			originalEntryLineMock1.Setup(m => m.EntryFilerCode).Returns("EF1");
			originalEntryLineMock1.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines1 = new List<IReconOriginalEntryLine>();
			originalEntryLines1.Add(originalEntryLineMock1.Object);

			var originalEntryLineMock2 = new Mock<IReconOriginalEntryLine>();
			originalEntryLineMock2.Setup(m => m.EntryNumber).Returns("EN2");
			originalEntryLineMock2.Setup(m => m.EntryFilerCode).Returns("EF2");
			originalEntryLineMock2.Setup(m => m.EntryLineNumber).Returns("1");
			List<IReconOriginalEntryLine> originalEntryLines2 = new List<IReconOriginalEntryLine>();
			originalEntryLines2.Add(originalEntryLineMock2.Object);

			var mergedEntryLines = new List<IReconEntryLineGroup>();
			var mergedEntryLineMock1 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock1.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock1.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock1.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock1.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock1.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock1.Setup(m => m.ReconHTS).Returns("1234567899");
			mergedEntryLineMock1.Setup(m => m.ReconCustomsValue).Returns(20m);
			mergedEntryLineMock1.Setup(m => m.ReconDuty).Returns(10m);
			mergedEntryLineMock1.Setup(m => m.ReconSPI).Returns("RS");
			mergedEntryLineMock1.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock1.Setup(m => m.AllAdditionalHTSs).Returns("99038801:99038802:99038803");
			mergedEntryLineMock1.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines1);
			mergedEntryLineMock1.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock1.Setup(m => m.IsNAFTARecon).Returns(true);
			mergedEntryLineMock1.Setup(m => m.OriginalCustomsValue).Returns(1230m);
			mergedEntryLineMock1.Setup(m => m.OriginalDuty).Returns(12.3m);

			var reconSecondaryLines1 = new List<IReconSecondaryLine>();
			var secondaryLine11 = new Mock<IReconSecondaryLine>();
			secondaryLine11.Setup(m => m.OriginalHTS).Returns("99038801");
			secondaryLine11.Setup(m => m.ReconHTS).Returns("99038801");
			secondaryLine11.Setup(m => m.ReconCustomsValue).Returns(100m);
			secondaryLine11.Setup(m => m.ReconDuty).Returns(9.9m);
			secondaryLine11.Setup(m => m.OriginalCustomsValue).Returns(110m);
			secondaryLine11.Setup(m => m.OriginalDuty).Returns(10.8m);
			var secondaryLine12 = new Mock<IReconSecondaryLine>();
			secondaryLine12.Setup(m => m.OriginalHTS).Returns("99038802");
			secondaryLine12.Setup(m => m.ReconHTS).Returns("99038802");
			secondaryLine12.Setup(m => m.ReconCustomsValue).Returns(200m);
			secondaryLine12.Setup(m => m.ReconDuty).Returns(19.9m);
			secondaryLine12.Setup(m => m.OriginalCustomsValue).Returns(210m);
			secondaryLine12.Setup(m => m.OriginalDuty).Returns(20.8m);
			var secondaryLine13 = new Mock<IReconSecondaryLine>();
			secondaryLine13.Setup(m => m.OriginalHTS).Returns("99038803");
			secondaryLine13.Setup(m => m.ReconHTS).Returns("99038803");
			secondaryLine13.Setup(m => m.ReconCustomsValue).Returns(300m);
			secondaryLine13.Setup(m => m.ReconDuty).Returns(29.9m);
			secondaryLine13.Setup(m => m.OriginalCustomsValue).Returns(310m);
			secondaryLine13.Setup(m => m.OriginalDuty).Returns(30.8m);
			reconSecondaryLines1.Add(secondaryLine11.Object);
			reconSecondaryLines1.Add(secondaryLine12.Object);
			reconSecondaryLines1.Add(secondaryLine13.Object);
			mergedEntryLineMock1.Setup(m => m.SecondaryLines).Returns(reconSecondaryLines1);

			var entryFeeMock11 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock11.Setup(m => m.FeeClass).Returns("001");
			entryFeeMock11.Setup(m => m.OriginalFee).Returns(10m);
			entryFeeMock11.Setup(m => m.EstimatedReconciliationFee).Returns(20m);
			var entryFeeMock12 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock12.Setup(m => m.FeeClass).Returns("044");
			entryFeeMock12.Setup(m => m.OriginalFee).Returns(20m);
			entryFeeMock12.Setup(m => m.EstimatedReconciliationFee).Returns(100m);
			var entryFees1 = new List<IReconciliationImportEntryFee>();
			entryFees1.Add(entryFeeMock11.Object);
			entryFees1.Add(entryFeeMock12.Object);
			mergedEntryLineMock1.Setup(m => m.Fees).Returns(entryFees1);

			var mergedEntryLineMock2 = new Mock<IReconEntryLineGroup>();
			mergedEntryLineMock2.Setup(m => m.OriginalCoutryOfOrigin).Returns("AU");
			mergedEntryLineMock2.Setup(m => m.OriginalSPI).Returns("SP");
			mergedEntryLineMock2.Setup(m => m.OriginalHTSEffectiveDate).Returns(ZDate.BrettsBirthday);
			mergedEntryLineMock2.Setup(m => m.ReconReason).Returns("reason");
			mergedEntryLineMock2.Setup(m => m.OriginalHTS).Returns("1234567890");
			mergedEntryLineMock2.Setup(m => m.ReconHTS).Returns("1234567811");
			mergedEntryLineMock2.Setup(m => m.ReconCustomsValue).Returns(30m);
			mergedEntryLineMock2.Setup(m => m.ReconDuty).Returns(40m);
			mergedEntryLineMock2.Setup(m => m.ReconSPI).Returns("NS");
			mergedEntryLineMock2.Setup(m => m.HTSChangedDueToValueIndicator).Returns(true);
			mergedEntryLineMock2.Setup(m => m.AllAdditionalHTSs).Returns("99038801:99038802:99038803");
			mergedEntryLineMock2.Setup(m => m.OriginalEntryLines).Returns(originalEntryLines2);
			mergedEntryLineMock2.Setup(m => m.IsCottonFeeMandatory).Returns(false);
			mergedEntryLineMock2.Setup(m => m.IsNAFTARecon).Returns(true);
			mergedEntryLineMock2.Setup(m => m.OriginalCustomsValue).Returns(210m);
			mergedEntryLineMock2.Setup(m => m.OriginalDuty).Returns(4.2m);

			var reconSecondaryLines2 = new List<IReconSecondaryLine>();
			var secondaryLine21 = new Mock<IReconSecondaryLine>();
			secondaryLine21.Setup(m => m.OriginalHTS).Returns("99038801");
			secondaryLine21.Setup(m => m.ReconHTS).Returns("99038801");
			secondaryLine21.Setup(m => m.ReconCustomsValue).Returns(150);
			secondaryLine21.Setup(m => m.ReconDuty).Returns(15.1m);
			secondaryLine21.Setup(m => m.OriginalCustomsValue).Returns(160m);
			secondaryLine21.Setup(m => m.OriginalDuty).Returns(16.1m);
			var secondaryLine22 = new Mock<IReconSecondaryLine>();
			secondaryLine22.Setup(m => m.OriginalHTS).Returns("99038802");
			secondaryLine22.Setup(m => m.ReconHTS).Returns("99038802");
			secondaryLine22.Setup(m => m.ReconCustomsValue).Returns(285m);
			secondaryLine22.Setup(m => m.ReconDuty).Returns(19.9m);
			secondaryLine22.Setup(m => m.OriginalCustomsValue).Returns(300m);
			secondaryLine22.Setup(m => m.OriginalDuty).Returns(21.8m);
			var secondaryLine23 = new Mock<IReconSecondaryLine>();
			secondaryLine23.Setup(m => m.OriginalHTS).Returns("99038803");
			secondaryLine23.Setup(m => m.ReconHTS).Returns("99038803");
			secondaryLine23.Setup(m => m.ReconCustomsValue).Returns(310m);
			secondaryLine23.Setup(m => m.ReconDuty).Returns(29.9m);
			secondaryLine23.Setup(m => m.OriginalCustomsValue).Returns(340m);
			secondaryLine23.Setup(m => m.OriginalDuty).Returns(39.8m);
			reconSecondaryLines2.Add(secondaryLine21.Object);
			reconSecondaryLines2.Add(secondaryLine22.Object);
			reconSecondaryLines2.Add(secondaryLine23.Object);
			mergedEntryLineMock2.Setup(m => m.SecondaryLines).Returns(reconSecondaryLines2);

			var entryFeeMock21 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock21.Setup(m => m.FeeClass).Returns("044");
			entryFeeMock21.Setup(m => m.OriginalFee).Returns(100m);
			entryFeeMock21.Setup(m => m.EstimatedReconciliationFee).Returns(20m);
			var entryFeeMock22 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock22.Setup(m => m.FeeClass).Returns("499");
			entryFeeMock22.Setup(m => m.OriginalFee).Returns(70m);
			entryFeeMock22.Setup(m => m.EstimatedReconciliationFee).Returns(100m);
			var entryFeeMock23 = new Mock<IReconciliationImportEntryFee>();
			entryFeeMock23.Setup(m => m.FeeClass).Returns("501");
			entryFeeMock23.Setup(m => m.OriginalFee).Returns(66m);
			entryFeeMock23.Setup(m => m.EstimatedReconciliationFee).Returns(100m);
			var entryFees2 = new List<IReconciliationImportEntryFee>();
			entryFees2.Add(entryFeeMock21.Object);
			entryFees2.Add(entryFeeMock22.Object);
			entryFees2.Add(entryFeeMock23.Object);
			mergedEntryLineMock2.Setup(m => m.Fees).Returns(entryFees2);

			mergedEntryLines.Add(mergedEntryLineMock1.Object);
			mergedEntryLines.Add(mergedEntryLineMock2.Object);
			mock.Setup(m => m.EntryLineGroups).Returns(mergedEntryLines);
			return mock;
		}
	}
}
