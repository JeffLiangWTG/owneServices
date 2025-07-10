using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNMFSHarvestingDetailAddInfo))]
	public class USNMFSHarvestingDetailAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USNMFSHarvestingDetailAddInfo(NMFSLine.HarvestingDetails.AddNew().B7_AddInfoDataInfo);
		}

		public void TestGetNewValidation()
		{
			var addInfo1 = new USNMFSHarvestingDetailAddInfo(NMFSLine.HarvestingDetails.AddNew().B7_AddInfoDataInfo);
			AssertEquals(typeof(NMFSHarvestingDetailAddInfoValidation), addInfo1.Validation.GetType());

			var addInfo2 = new USNMFSHarvestingDetailAddInfo(InvoiceLine.FishingInformations.AddNew().B7_AddInfoDataInfo);
			AssertEquals(typeof(FishingInformationAddInfoValidation), addInfo2.Validation.GetType());
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get { return nmfsLine ?? (nmfsLine = InvoiceLine.NMFSLines.AddNew()); }
		}

		NMFSLine nmfsLine;

		#endregion
	}
}
