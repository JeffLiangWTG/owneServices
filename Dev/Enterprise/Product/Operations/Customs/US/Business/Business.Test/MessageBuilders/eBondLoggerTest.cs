using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class eBondLoggerTest : TestCaseWithFactory
	{
		public void TestRegistryItemIseBondAutoSendACEMessage()
		{
			USCustomsDataRegistry.Instance.IseBondAutoSendACEMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			Assert(!eBondLogger.ShouldAddAutoSendLog(declaration));

			USCustomsDataRegistry.Instance.IseBondAutoSendACEMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(eBondLogger.ShouldAddAutoSendLog(declaration));
		}

		public void TestCanAutoSendMessage()
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.ENB;
			Assert(eBondLogger.CanAutoSendMessage(declaration));

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.ENB;
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode2 = BondDispositionCodeList.Codes.ENB;
			Assert(eBondLogger.CanAutoSendMessage(declaration));

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.ENB;
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode2 = BondDispositionCodeList.Codes.CTS;
			Assert(!eBondLogger.CanAutoSendMessage(declaration));
		}

		public void TestShouldNoticeUserWillAutoSendMessage()
		{
			USCustomsDataRegistry.Instance.IseBondAutoSendACEMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CTS;
			Assert(eBondLogger.ShouldAddAutoSendLog(declaration));

			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.ENB;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CTS;
			Assert(eBondLogger.ShouldAddAutoSendLog(declaration));

			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.ENB;
			Assert(!eBondLogger.ShouldAddAutoSendLog(declaration));

			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			Assert(!eBondLogger.ShouldAddAutoSendLog(declaration));
		}

		public void TestUpdateOrAddAutoSendEvent()
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var simpEntryHeader = declaration.ActiveEntryHeaders.SimplifiedEntry;
			eBondLogger.AddAutoSendEvent(simpEntryHeader.Logs, ImportMessageSendingMessageType.Original, false);

			var athLog = eBondLogger.GetAutoSendEvent(simpEntryHeader.Logs);
			AssertNotNull(athLog);
			Assert(athLog.SL_Reference.Contains("ORG"));
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			declaration.US_BondDispositionCode = BondDispositionCodeList.Codes.CVB;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
		}
	}
}
