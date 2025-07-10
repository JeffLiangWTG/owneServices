using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class InvoicingServiceTest : TestCaseWithFactory
	{
		public void TestPrint_NoContact()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			var result = service.Print(Guid.NewGuid(), invoice.PK.ToGuid());

			AssertNull(result);
		}

		public void TestPrint_ContactNotRelatedToInvoice()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			var result = service.Print(contact.PK.ToGuid(), invoice.PK.ToGuid());

			AssertNull(result);
		}

		public void TestPrint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = org.PK;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_ConsolidatedInvoiceRef = "S00001001";
			Factory.Save();

			var result = service.Print(contact.PK.ToGuid(), invoice.PK.ToGuid());

			CombineAssertions(() =>
			{
				AssertEquals("Invoice-S00001001.pdf", result.FileName);
				Assert(result.FileContents.Length > 0);
			});
		}

		public void TestPrint_CannotRePrint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = org.PK;
			invoice.AH_InvoicePrinted = true;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var result = service.Print(contact.PK.ToGuid(), invoice.PK.ToGuid());

				AssertEquals(result.ErrorMessage, invoice.CheckCanPrintPostedInvoicingBase().ReasonForNotBeingAbleToPrint);
			}
		}

		public void TestPrint_RelatedParty()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;
			var contact = org.Contacts.AddNew();

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.New<OrgRelatedParty>();
			relatedParty.PR_OH_Parent = otherOrg.PK;
			relatedParty.PR_OH_RelatedParty = org.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = otherOrg.PK;
			Factory.Save();

			using (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = service.Print(contact.PK.ToGuid(), invoice.PK.ToGuid());

				AssertNotNull(result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			service = new InvoicingService();
		}
		InvoicingService service;
	}
}
