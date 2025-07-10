using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class FormalEntrySingleMessageManagerTest : SingleMessageManagerTestCase
	{
		public virtual void TestChargeBalanceValidation()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);

				CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
				CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
				invoice.JZ_InvoiceNumber = "123";
				BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				invoice.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);

				string detailedMessage;
				AssertEquals("PreCondition: Charges are not balanced", false, testDec.Invoices.AreChargesBalancedForInvoices(out detailedMessage));
				AssertEquals(string.Format("(OFT at Invoice 123) Inv. Amount:100.00 {0}, Total Line Amount:0.00 {0}", testDec.LocalCurrencyCode), detailedMessage);

				FormalEntrySingleMessageManager testManager = GetTestManager(entryHeader);
				MessageSendingNotificationCollection result = testManager.GetNotificationsForSendingAnOriginal();

				string error = result.ErrorNotificationsAsString();
				Assert("Should have an error for unbalanced apportionment for Original", error.Contains("Current apportionment is not balanced"));

				result = testManager.GetNotificationsForSendingAReplacement();
				error = result.ErrorNotificationsAsString();
				Assert("Should have an error for unbalanced apportionment for replacement", error.Contains("Current apportionment is not balanced"));

				result = testManager.GetNotificationsForSendingAWithdrawal();
				error = result.ErrorNotificationsAsString();
				Assert("Should have an error for unbalanced apportionment for withdrawal", error.Contains("Current apportionment is not balanced"));

				var apportionedCharge = invoiceLine.ApportionedCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 100m, testDec.LocalCurrencyCode);
				apportionedCharge.J7_IsNotIncludedInInvoice = true;
				apportionedCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Value;
				AssertEquals("PreCondition: Charges are balanced now", true, testDec.Invoices.AreChargesBalancedForInvoices(out detailedMessage));

				result = testManager.GetNotificationsForSendingAnOriginal();
				error = result.ErrorNotificationsAsString();
				Assert("Should not have an error for unbalanced apportionment", !error.Contains("Current apportionment is not balanced"));
			}
		}

		protected override void AddError(BusinessObject bizObj, string error)
		{
			((CusEntryHeader)bizObj).Declaration.AddRowError(error);
		}

		protected override void AddMessageError(BusinessObject bizObj, string message)
		{
			((CusEntryHeader)bizObj).Declaration.AddRowMessageError(message);
		}

		protected abstract FormalEntrySingleMessageManager GetTestManager(CusEntryHeader entryHeader);
	}
}
