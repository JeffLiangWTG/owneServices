using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCHeaderCollection))]
	public class USHFCHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 1000m;
			var hfcHeader1 = invoiceLine.USHFCHeaders.AddNew();
			AssertEquals("Set Default Value", EntityRoleCodeList.Codes.Importer, hfcHeader1.US_CertifyingIndividual);
			AssertEquals("The net weight value of the first HFC header should be defaulted to the value of the invoice line's Customs Qty", 1000m, hfcHeader1.US_NetWeight);

			hfcHeader1.US_NetWeight = 700m;
			hfcHeader1.US_CertifyingIndividual = EntityRoleCodeList.Codes.Consignee;
			hfcHeader1.US_HFCImageSent = true;
			var hfcHeader2 = invoiceLine.USHFCHeaders.AddNew();
			AssertEquals("The net weight value of the first HFC header should be defaulted to the value of the invoice line's Customs Qty", 300m, hfcHeader2.US_NetWeight);
			AssertEquals("The value of US_HFCImageSent is reset", false, hfcHeader2.US_HFCImageSent);
		}

		public void TestAllowNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.USHFCHeaders;
			AssertEquals(true, collection.AllowNew);
			collection.AllowAddNewPGALines = false;
			AssertEquals(false, collection.AllowNew);
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetTrackingID();
			collection = invoiceLine.USHFCHeaders;
			AssertEquals(false, collection.AllowNew);
			collection.AllowAddNewPGALines = true;
			AssertEquals(true, collection.AllowNew);
		}

		public void TestIPGADataCorrectionCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var collection = invoiceLine.USHFCHeaders;
			var hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			var iPGADataCorrectionCollection = (IPGADataCorrectionCollection)collection;
			AssertCollectionContains(hfcHeader, iPGADataCorrectionCollection.CorrectionItems);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceLine = Declaration.InvoiceLines.AddNew();
			return new USHFCHeaderCollection(invoiceLine);
		}

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				fDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
