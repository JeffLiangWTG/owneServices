using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ControllingMessageHeaderLinkInvoiceLineCollection))]
	sealed class ControllingMessageHeaderLinkInvoiceLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ControllingMessageHeaderLinkInvoiceLineCollection>
	{
		public void TestAllowSort()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals(true, ((IBindingList)testCollection).SupportsSorting);
		}

		public void TestDeleteControllingMessageHeaderLinkInvoiceLineByInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line1 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var line2 = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var testCollection = messageHeader.ControllingMessageHeaderLinkInvoiceLines;
			AssertEquals(2, testCollection.Count);

			testCollection.DeleteControllingMessageHeaderLinkInvoiceLineByInvoiceLine(line2);
			AssertEquals(1, testCollection.Count);
			AssertEquals(line1.PK, testCollection.Cast<ControllingMessageHeaderLinkInvoiceLine>().Single().InvoicelinePK);
		}

		public void TestSuspendValidationOnLoading()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = jobDeclartion.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			var cmHeader = controllingMessageHeaders.AddNew();
			cmHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cmHeader.TW1_CertificateType = "15";
			for (int i = 0; i <= 20; i++)
			{
				var line = jobDeclartion.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
				line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			}
			Factory.Save();

			var declarationReloaded = new BusinessObjectFactory().Load<JobDeclaration>(jobDeclartion.PK);
			foreach (ControllingMessageHeaderLinkInvoiceLine linkReloaded in declarationReloaded.CusEntryInstruction.ControllingMessageHeaders[0].ControllingMessageHeaderLinkInvoiceLines)
			{
				AssertNoNotifications(linkReloaded.LinkInfo);
			}
		}

		protected override ControllingMessageHeaderLinkInvoiceLineCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var invoiceHeader = declaration.Invoices.AddNew();
			var line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			return messageHeader.ControllingMessageHeaderLinkInvoiceLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collectionForTesting = GetCollectionToTest();
			return collectionForTesting.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
		}
	}
}
