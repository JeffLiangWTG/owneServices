using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectParent))]
	sealed class JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new JobDeclarationMessageSendingObjectParent(declaration1);
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);
			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var testWrapper2 = new JobDeclarationMessageSendingObjectParent(declaration2);
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}

		public void TestHasDutiableSendingObject()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invHeader.JZ_IncoTerm = "FOB";
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_LinePrice = 200m;
			invLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine);
			var testWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
			AssertEquals(1, testWrapper.SendingObjectsCollection.Count);
			var sendingObject1 = testWrapper.SendingObjectsCollection[0];
			AssertEquals("Sending Object Amount Due is zero", 0m, sendingObject1.AmountDueAfter);
			AssertEquals("Not deferrable when Amount Due is zero", false, sendingObject1.IsDutiable);
			AssertEquals("Not deferrable when Amount Due is zero", false, testWrapper.HasDutiableSendingObject);
			entryLine.CL_CustomsValue = 210m;
			entryLine.Fees.AddOrUpdate("1P1", 60m);
			entryLine.Fees.AddOrUpdate("12B", 80m);
			entryLine.Fees.AddOrUpdate("VAT", 90m);
			AssertEquals(230.0m, sendingObject1.AmountDueAfter);
			AssertEquals("Deferrable when AmountDue is above zero", true, sendingObject1.IsDutiable);
			AssertEquals("Deferrable when a sending object is deferrable", true, testWrapper.HasDutiableSendingObject);
		}

		public void TestUpdateFromDeferredSubmission()
		{
			var messageDeferralSettings = new AutomaticDeferredSelection()
			{ AllowAutomaticDeferredSelection = true, DaysBeforeETA = 3 };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "12345678ABC2018";
				var testWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
				var sendingObject1 = testWrapper.SendingObjectsCollection[0];
				var helper = new DeferredSubmissionHelper(declaration);
				var mockSubmission = new Mock<DeferredSubmission>(helper) { CallBase = true };
				mockSubmission.Setup(m => m.AgentChanged).Returns(false);
				mockSubmission.Setup(m => m.CanDeferMessages).Returns(false);
				mockSubmission.Setup(m => m.SubmissionDate).Returns(ZDateTime.Today.AddDays(1));
				var submission = mockSubmission.Object;
				var header = sendingObject1.Header;
				header.CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
				header.CH_BGMReference = "22222222ABC1024";
				AssertEquals(sendingObject1.LocalReferenceNumber, "12345678ABC2018");
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("LRN unchanged", "12345678ABC2018", sendingObject1.LocalReferenceNumber);
				AssertEquals("Not Set when CanDeferMessages is false", ZDateTime.Empty, sendingObject1.SubmissionDate);
				mockSubmission.Setup(m => m.CanDeferMessages).Returns(true);
				mockSubmission.Setup(m => m.AgentChanged).Returns(false);
				mockSubmission.Setup(m => m.SubmissionDate).Returns(ZDateTime.Today);
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("LRN unchanged", "12345678ABC2018", sendingObject1.LocalReferenceNumber);
				AssertEquals("Not Set when Date is Today", ZDateTime.Empty, sendingObject1.SubmissionDate);
				mockSubmission.Setup(m => m.AgentChanged).Returns(true);
				mockSubmission.Setup(m => m.SubmissionDate).Returns(ZDateTime.Today);
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("LRN changed when agent changed", "22222222ABC1024", sendingObject1.LocalReferenceNumber);
				AssertEquals("Not Set when Date is Today", ZDateTime.Empty, sendingObject1.SubmissionDate);
				mockSubmission.Setup(m => m.SubmissionDate).Returns(ZDateTime.Today.AddDays(1));
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("Set when Can Defer and after today", ZDateTime.Today.AddDays(1), sendingObject1.SubmissionDate);
				header.CH_PaymentMethod = PaymentMethodCodeList.Codes.Free;
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("Not Set when Free and after today", ZDateTime.Empty, sendingObject1.SubmissionDate);
				header.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				testWrapper.UpdateFromDeferredSubmission(submission);
				AssertEquals("Set when VAT and after today", ZDateTime.Today.AddDays(1), sendingObject1.SubmissionDate);

				mockSubmission.VerifyAll();
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new JobDeclarationMessageSendingObjectParent(declaration);
		}
	}
}
