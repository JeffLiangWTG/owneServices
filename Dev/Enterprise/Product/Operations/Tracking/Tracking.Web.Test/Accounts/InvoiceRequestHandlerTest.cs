using System;
using System.IO;
using System.Web;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web
{
	[TestedType(typeof(InvoiceRequestHandler))]
	sealed class InvoiceRequestHandlerTest : DataRequestHandlerTestCase<InvoiceRequestHelper>
	{
		#region Setup

		bool useCreditNote;
		bool useMockInvoiceHandler;

		public Invoice[] Invoices
		{
			get
			{
				if (invoices == null)
				{
					var testOrg = Factory.New<OrgHeader>();
					testOrg.OH_Code = "_TESTORG";

					invoices = new Invoice[] { Factory.New<ARInvoice>(), Factory.New<ARInvoice>() };
					invoices[0].AH_OH = testOrg.PK;
					invoices[0].AH_ConsolidatedInvoiceRef = "S00001001";
					invoices[1].AH_OH = testOrg.PK;

					Factory.Save();
				}

				return invoices;
			}
		}
		Invoice[] invoices;

		public CreditNote CreditNote
		{
			get
			{
				if (creditNote == null)
				{
					var testOrg = Factory.New<OrgHeader>();
					testOrg.OH_Code = "_TESTORG";

					creditNote = Factory.New<ARCreditNote>();
					creditNote.AH_OH = testOrg.PK;

					Factory.Save();
				}

				return creditNote;
			}
		}
		CreditNote creditNote;

		protected override DataRequestHandler<InvoiceRequestHelper> GetNewRequestHandler()
		{
			InvoiceRequestHandler testRequestHandler = useMockInvoiceHandler ? new MockInvoiceRequestHandler() : new InvoiceRequestHandler();

			if (useCreditNote)
			{
				testRequestHandler.QueryString.Add(DataRequestHelper.DataKey, CreditNote.PK.ToString());
			}
			else
			{
				testRequestHandler.QueryString.Add(DataRequestHelper.DataKey, Invoices[0].PK.ToString() + "," + Invoices[1].PK.ToString());
			}

			return testRequestHandler;
		}

		#endregion

		#region Test Handler supports both Invoices and Credit Notes

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}

		public void TestCannotRePrint()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				InvoicingBase invoice = RequestHandler.BusinessObjects[0] as InvoicingBase;
				invoice.AH_InvoicePrinted = true;
				Factory.Save();
				Assert("Can not Print", !invoice.CheckCanPrintPostedInvoicingBase().Result);

				MemoryStream ms = new MemoryStream();
				var testResponseFilter = new ZArchitecture.Web.Business.Testing.TestResponseFilter(HttpContext.Current.Response.Filter, ms);
				HttpContext.Current.Response.Filter = testResponseFilter;

				var expectedExceptionMessage = "Will automatically throw ThreadAbortException when call HttpContext.Current.Response.End()";
				AssertExceptionThrown(typeof(Exception), expectedExceptionMessage, () => RequestHandler.ProcessRequest(HttpContext.Current));

				ms.Position = 0;
				StreamReader reader = new StreamReader(ms);
				string responseData = reader.ReadToEnd();
				reader.Close();

				AssertContains("The response should be the error message.", invoice.CheckCanPrintPostedInvoicingBase().ReasonForNotBeingAbleToPrint.Replace("\r\n", " "), responseData);
			}
		}

		public void TestRequestHandlerWorksWithInvoices()
		{
			useCreditNote = false;

			AssertNotNull("The request handler must be able to return BusinessObjects", RequestHandler.BusinessObjects);
			AssertEquals("THere should be two BusinessObjects", 2, RequestHandler.BusinessObjects.Length);
			Assert("The first BusinessObject should be an invoice", RequestHandler.BusinessObjects[0] is Invoice);
			Assert("The second BusinessObject should be an invoice", RequestHandler.BusinessObjects[1] is Invoice);
		}

		public void TestHandlerWorksWithCreditNotes()
		{
			useCreditNote = true;

			Assert("The test data should be a credit note", RequestHandler.BusinessObjects[0] is CreditNote);
			AssertNotNull("The request handler must be able to return a credit note", RequestHandler.BusinessObjects[0]);
		}

		public void TestFileName()
		{
			var expectedFilePrefix = "Invoice-";
			var expectedFileExtension = ".pdf";
			var fileName = RequestHandler.FileName;
			AssertStartsWith("starts with Invoice-", expectedFilePrefix, fileName);
			AssertEndsWith("ends with PDF file extension", expectedFileExtension, fileName);

			var transactionNumbers = fileName.Replace(expectedFilePrefix, string.Empty).Replace(expectedFileExtension, string.Empty).Split(',');
			AssertContainsExactElementsInAnyOrder(new string[] { Invoices[0].AH_ConsolidatedInvoiceRef, Invoices[1].AH_TransactionNum }, transactionNumbers);
		}

		#endregion

		public void TestGetBinaryDataAddsRequestedViaWebLog()
		{
			useCreditNote = false;

			AssertNotNull("The request handler must be able to return BusinessObjects", RequestHandler.BusinessObjects);
			AssertEquals("THere should be two BusinessObjects", 2, RequestHandler.BusinessObjects.Length);

			InvoicingBase invoice1 = RequestHandler.BusinessObjects[0] as InvoicingBase;
			AssertNotNull("The first BusinessObject should be an InvoicingBase", invoice1);
			AssertEquals("LastRequestedViaWeb should be empty", ZDateTime.Empty, invoice1.LastRequestedViaWeb);

			InvoicingBase invoice2 = RequestHandler.BusinessObjects[1] as InvoicingBase;
			AssertNotNull("The second BusinessObject should be an InvoicingBase", invoice2);
			AssertEquals("LastRequestedViaWeb should be empty", ZDateTime.Empty, invoice2.LastRequestedViaWeb);

			AssertNotNull("Should return some data", RequestHandler.GetBinaryData());
			ZDateTime date1 = invoice1.LastRequestedViaWeb;
			AssertEquals("Should not be empty", false, date1.IsEmpty);
			Assert("LastRequestedViaWeb date should be set", date1 <= ZDateTime.Now);

			ZDateTime date2 = invoice2.LastRequestedViaWeb;
			AssertEquals("Should not be empty", false, date2.IsEmpty);
			Assert("LastRequestedViaWeb date should be set", date2 <= ZDateTime.Now);
		}

		public void TestGetBinaryDataDoesNotAddRequestedViaWebLogWhenDataIsEmpty()
		{
			useMockInvoiceHandler = true;

			AssertNotNull("The request handler must be able to return BusinessObjects", RequestHandler.BusinessObjects);
			AssertEquals("THere should be two BusinessObjects", 2, RequestHandler.BusinessObjects.Length);

			InvoicingBase invoice1 = RequestHandler.BusinessObjects[0] as InvoicingBase;
			AssertNotNull("The first BusinessObject should be an InvoicingBase", invoice1);
			AssertEquals("LastRequestedViaWeb should be empty", ZDateTime.Empty, invoice1.LastRequestedViaWeb);

			InvoicingBase invoice2 = RequestHandler.BusinessObjects[1] as InvoicingBase;
			AssertNotNull("The second BusinessObject should be an InvoicingBase", invoice2);
			AssertEquals("LastRequestedViaWeb should be empty", ZDateTime.Empty, invoice2.LastRequestedViaWeb);

			AssertEquals(Array.Empty<byte>(), RequestHandler.GetBinaryData());
			ZDateTime date1 = invoice1.LastRequestedViaWeb;
			AssertEquals("Should be empty", true, date1.IsEmpty);

			ZDateTime date2 = invoice2.LastRequestedViaWeb;
			AssertEquals("Should be empty", true, date2.IsEmpty);
		}

		#region implementation

		class MockInvoiceRequestHandler : InvoiceRequestHandler
		{
			protected override DocumentPack GetDocumentPack(InvoicePrintTask invoicePrintTask)
			{
				return null;
			}
		}

		#endregion
	}
}
