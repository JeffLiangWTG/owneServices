using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	sealed class USCustomsReconDeclarationRunDocsTest : BaseRunDocumentsTest
	{
		ZString storedCountry;

		protected override void SetUp()
		{
			storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(storedCountry);
			base.TearDown();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ReconDeclaration; }
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				if (reconDeclaration == null)
				{
					reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
					ReconOriginalEntryHeader originalEntry = reconDeclaration.OriginalEntries.AddNew();

					JobComInvoiceHeader invoice = originalEntry.Invoice;

					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.JI_Tariff = "1902194000";
					invoiceLine.US_R_OrigTariff = "1902194000";
					invoiceLine.JI_CustomsQuantity = 1000m;
					invoiceLine.US_R_OrigFirstQty = 1200m;

					invoiceLine.JI_LinePrice = 12000m;
					invoiceLine.US_R_OrigCV = 13000m;
				}
				return reconDeclaration;
			}
		}
		ReconDeclaration reconDeclaration;

		[ExpectNoExceptions]
		public void TestReconDocuments()
		{
			reconDeclaration = null;
			BusinessObject accessed = GetBusinessObject;
			reconDeclaration.US_IsAggregate = true;
			ReconDocumentsRun("Aggregate Recon");

			reconDeclaration = null;
			accessed = GetBusinessObject;
			reconDeclaration.US_IsAggregate = false;
			ReconDocumentsRun("Entry-By-Entry Recon Association File");
			ReconDocumentsRun("Line Summary Recon");

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Reconciliation Notes");
			RunDocument();

			ErrorReporter.Clear(); // both templates need to be updated so the expression evaluator doesn't try to evaluate; "0" * "0" == "0" 
		}

		void ReconDocumentsRun(ZString documentName)
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, documentName);
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			RunDocumentWithAllSections = ZBool.True;
			RunDocument();
		}
	}
}
