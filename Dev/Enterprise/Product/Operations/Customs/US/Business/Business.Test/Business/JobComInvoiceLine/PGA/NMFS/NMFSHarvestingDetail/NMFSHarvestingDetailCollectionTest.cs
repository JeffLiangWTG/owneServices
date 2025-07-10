using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSHarvestingDetailCollection))]
	public class NMFSHarvestingDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowAddNew()
		{
			var collection = NMFSLine.HarvestingDetails;
			AssertEquals(typeof(NMFSHarvestingDetailCollection), collection.GetType());

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals(true, collection.AllowNew);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals(false, collection.AllowNew);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals(true, collection.AllowNew);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals(true, collection.AllowNew);
		}

		public void TestSetDefaults()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;

			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			AssertEquals(ZString.Empty, harvestingDetail.US_ContactPartyType);

			NMFSLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			AssertEquals(EntityRoleCodeList.Codes.AquacultureFacility, harvestingDetail.US_ContactPartyType);

			using (InvoiceLine.NMFSLines.SuspendAdditionallyForImport())
			{
				harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				AssertEquals(ZString.Empty, harvestingDetail.US_ContactPartyType);
			}

			harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			AssertEquals(EntityRoleCodeList.Codes.AquacultureFacility, harvestingDetail.US_ContactPartyType);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NMFSHarvestingDetailCollection(NMFSLine);
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
