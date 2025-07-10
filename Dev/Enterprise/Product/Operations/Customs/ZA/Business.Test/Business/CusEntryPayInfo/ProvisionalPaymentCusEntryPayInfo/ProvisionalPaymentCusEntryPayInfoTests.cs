using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ProvisionalPaymentCusEntryPayInfo))]
	sealed class ProvisionalPaymentCusEntryPayInfoTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSettingC9_RemAdvReceived()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 1;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			AssertEquals(1, entryHeader.MergedLines.Count);
			var payInfo1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			var payInfo2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 1m, "1", "REF");
			var entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var entryLinePP1 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP1.CY_Code = "PEN";
			entryLinePP1.CY_Value = 1m;
			var entryLinePP2 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP2.CY_Code = "PPA";
			entryLinePP2.CY_Value = 2m;
			var entryLinePP3 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP3.CY_Code = "PEN";
			entryLinePP3.CY_Value = 3m;
			payInfo1.C9_RemAdvReceived = true;
			AssertEquals(1, entryLine.ProvisionalPayments.Count);
			AssertEquals("PPA", entryLine.ProvisionalPayments[0].CY_Code);
		}

		[ExpectNoExceptions]
		public void TestSettingC9_RemAdvReceivedDoesNotCauseExceptionIfThereAreNoEntryLines()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 1;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.MergedLines.RemoveAndDeleteAll();
			AssertEquals(0, entryHeader.MergedLines.Count);
			var payInfo1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			payInfo1.C9_RemAdvReceived = true;
		}

		[TestDate(2021, 3, 29)]
		public void TestSettingC9_ReceiptDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvoice1 = declaration.Invoices.AddNew();
			testInvoice1.JZ_InvoiceNumber = "INV3";
			testInvoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine11 = testInvoice1.InvoiceLines.AddNew();
			invoiceLine11.JI_CEI = testInst.PK;
			invoiceLine11.JI_Tariff = "1";
			invoiceLine11.JI_PreviousEntryLineNumber = 1;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			entryHeader.CH_BGMReference = "TST123456";
			AssertEquals(1, entryHeader.MergedLines.Count);
			entryHeader.PenaltyAmountBefore = 10m;
			entryHeader.ProvisionalPaymentAmountBefore = 20m;
			var payInfo1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Empty, "C", "PEN", 1m, "1", "REF", true);
			var payInfo2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Empty, "C", "PPA", 2m, "1", "REF", true);
			var payInfo3 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Empty, "C", "PPT", 3m, "1", "REF", true);
			var entryLine = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var entryLinePP1 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP1.CY_Code = "PEN";
			entryLinePP1.CY_Value = 1m;
			var entryLinePP2 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP2.CY_Code = "PPA";
			entryLinePP2.CY_Value = 2m;
			var entryLinePP3 = entryLine.ProvisionalPayments.AddNew();
			entryLinePP3.CY_Code = "PPT";
			entryLinePP3.CY_Value = 3m;
			payInfo1.C9_ReceiptDate = ZDate.Today;
			AssertEquals(9m, entryHeader.PenaltyAmountBefore);
			AssertEquals(20m, entryHeader.ProvisionalPaymentAmountBefore);
			var stmLogs1 = entryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusUpdated.ToString()).OrderBy(x => x.SL_EventTime);
			AssertEquals(1, stmLogs1.Count());
			var stmLog = stmLogs1.FirstOrDefault();
			AssertEquals("Entry TST123456 updated VOC-before data for PEN, reduced by 1 to 9 because liquidation date set to 29-Mar-21 00:00:00", stmLog.SL_Reference);
			payInfo2.C9_ReceiptDate = ZDate.Today;
			AssertEquals(9m, entryHeader.PenaltyAmountBefore);
			AssertEquals(18m, entryHeader.ProvisionalPaymentAmountBefore);
			var stmLogs2 = entryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusUpdated.ToString()).OrderBy(x => x.SL_EventTime);
			AssertEquals(2, stmLogs2.Count());
			AssertEquals("Entry TST123456 updated VOC-before data for PPA, reduced by 2 to 18 because liquidation date set to 29-Mar-21 00:00:00", stmLogs2.LastOrDefault().SL_Reference);
			payInfo3.C9_ReceiptDate = ZDate.Today;
			AssertEquals(9m, entryHeader.PenaltyAmountBefore);
			AssertEquals(15m, entryHeader.ProvisionalPaymentAmountBefore);
			var stmLogs3 = entryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.StatusUpdated.ToString()).OrderBy(x => x.SL_EventTime);
			AssertEquals(3, stmLogs3.Count());
			AssertEquals("Entry TST123456 updated VOC-before data for PPT, reduced by 3 to 15 because liquidation date set to 29-Mar-21 00:00:00", stmLogs3.LastOrDefault().SL_Reference);
		}

		public void TestC9_ReceiptDate()
		{
			var tester = Factory.New<ProvisionalPaymentCusEntryPayInfo>();
			AssertEquals("Liquidation Date", tester.C9_ReceiptDateInfo.Description);
		}

		public void TestC9_PaymentStatusDescription()
		{
			var uniHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			uniHelper.CreateCusCodeType("CSTA", "Status");
			uniHelper.CreateCusCodeList("ZA", "CSTA", "YY", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var tester = Factory.New<ProvisionalPaymentCusEntryPayInfo>();
			tester.C9_PaymentStatus = ZString.Empty;
			AssertEquals(ZString.Empty, tester.C9_PaymentStatusDescription);
			tester.C9_PaymentStatus = "YY";
			AssertEquals("YY DESC", tester.C9_PaymentStatusDescription);
			tester.C9_PaymentStatus = "XX";
			AssertEquals(ZString.Empty, tester.C9_PaymentStatusDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ProvisionalPaymentCusEntryPayInfo>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<CusEntryHeader>();
			var result = factory.New<ProvisionalPaymentCusEntryPayInfo>();
			result.C9_CH = header.PK;
			return result;
		}
	}
}
