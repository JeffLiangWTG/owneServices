using System;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(USDeclarationModuleColumnProvider))]
	sealed class USDeclarationModuleColumnProviderTest : BaseDeclarationModuleColumnProviderTest
	{
		protected override GridColumnProvider GetNewTestProvider()
		{
			return new USDeclarationModuleColumnProvider();
		}

		public override void TestTranslatability()
		{
			Assert(true);
		}

		[HttpContextEnabledTest]
		public override void TestUniqueColumns()
		{
			using (WebDataRegistry.Instance.UseWebAccountsModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var testHelper = new TestHelper(Factory);
				testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
				Assert("Precondition", !((TrackingSiteUser)WebEnv.AppInstance.SiteUser).CanViewAccounts);

				SetupNewProvider();
				SetupColumns();
				base.TestUniqueColumns();
			}
		}

		[HttpContextEnabledTest]
		public void TestCanViewAccounts()
		{
			using (WebDataRegistry.Instance.UseWebAccountsModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testHelper = new TestHelper(Factory);
				var orgRight = testHelper.TestOrg.SecurityRights.AddNew();
				orgRight.OX_Granted = true;
				orgRight.OX_SecurityItemName = WebSecurityRightsList.WebInvoicingAndStatements.Code;
				var userRight = testHelper.TestContact.SecurityRightsForBindingOnly.AddNew();
				userRight.OZ_OX = orgRight.PK;
				userRight.OZ_Granted = true;
				testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
				Assert("Precondition", ((TrackingSiteUser)WebEnv.AppInstance.SiteUser).CanViewAccounts);

				SetupNewProvider();
				SetupColumns();
				base.TestUniqueColumns();
			}
		}

		protected override void SetupCountrySpecificColumns()
		{
			AddColumn(new ZDateTimeColumn("Entry Submitted", "Declaration.EntrySubmittedDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntrySubmittedDate });
			AddColumn(new ZTextEditColumn("Entry Status", "Declaration.EntrySummaryStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatus });
			AddColumn(new ZTextEditColumn("Entry Status Desc.", "Declaration.EntrySummaryStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryStatusDescription });
			AddColumn(new ZTextEditColumn("Cargo Status", "Declaration.CargoReleaseStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatus });
			AddColumn(new ZTextEditColumn("Cargo Status Description", "Declaration.CargoReleaseStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CargoStatusDescription });
			AddColumn(new ZDateTimeColumn("Release Date", "Declaration.JE_EntryAuthorisationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReleaseDate });
			AddColumn(new ZTextEditColumn("Release Status Description", "Declaration.ReleaseStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReleaseStatusDesc });
			AddColumn(new ZTextEditColumn("Entry Port", "Declaration.US_SchDEntry") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryPort });
			AddColumn(new ZTextEditColumn("ENS Status Description", "Declaration.EntrySummaryStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ENSStatusDescription });
			AddColumn(new ZTextEditColumn("Paperless", "Declaration.US_PaperlessEntry") { ColumnKey = WebTracker.Grids.TrackingDeclarations.Paperless });
			AddColumn(new ZTextEditColumn("Statement Number", "Declaration.StatementNo") { ColumnKey = WebTracker.Grids.TrackingDeclarations.StatementNumber });
			AddColumn(new ZTextEditColumn("Filer Code", "Declaration.EntryFilerCode") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FilerCode });
			AddColumn(new ZTextEditColumn("Payment Type", "Declaration.US_PaymentType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentType });
			AddColumn(new ZDateTimeColumn("Payment Due Date", "Declaration.US_PaymentDueDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentDueDate });
			AddColumn(new ZDateTimeColumn("Statement Paid Date", "Declaration.StatementPaidDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.StatementPaidDate });
			AddColumn(new ZTextEditColumn("Payment Status", "Declaration.PaymentStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PaymentStatus });
			AddColumn(new ZTextEditColumn("Entry Type", "Declaration.US_EntryType") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EntryType });
			AddColumn(new ZTextEditColumn("Periodic Statement Month", "Declaration.US_PeriodicStatementMM") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PeriodicStatementMonth });
			AddColumn(new ZCalcEditColumn("Total Entered Value", "Declaration." + AutoJobDeclaration.Schema.US_TotalEnteredValue) { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalEnteredValue, BindToDecimals = null, Decimals = 0 });
			AddColumn(new ZCalcEditColumn("Total Duty & Fees", "Declaration.TotalPayable") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalPayable });

			if (WebEnv.AppInstance?.SiteUser is TrackingSiteUser siteUser && siteUser.CanViewAccounts)
			{
				AddColumn(new ZCalcEditColumn("Total Invoiced", "Declaration.TotalInvoicedAmount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalInvoiced });
				AddColumn(new ZCalcEditColumn("Total Outstanding", "Declaration.TotalOutstandingAmount") { ColumnKey = WebTracker.Grids.TrackingDeclarations.TotalOutstanding });
			}

			AddColumn(new ZTextEditColumn("OGA FDA Status", "Declaration.FDAStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FDAStatus });
			AddColumn(new ZTextEditColumn("OGA FDA Status Description", "Declaration.FDAStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.FDAStatusDescription });
			AddColumn(new ZDateTimeColumn("Last Audit Date", "Declaration.AuditDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.AuditDate });
			AddColumn(new ZTextEditColumn("EI Status Desc", "Declaration.ElectronicInvoiceStatusDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.EIStatusDesc });
			AddColumn(new ZTextEditColumn("IT Number", "Declaration.JE_PrimaryITNumber") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ITNumber });
			AddColumn(new ZDateTimeColumn("Liquidation Date", "Declaration.LiquidationDate") { ColumnKey = WebTracker.Grids.TrackingDeclarations.LiquidationDate });
			AddColumn(new ZTextEditColumn("Recon Issue", "Declaration.US_OtherReconIndicator") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReconIssue });
			AddColumn(new ZTextEditColumn("Recon Issue Desc", "Declaration.OtherReconIndicatorDescription") { ColumnKey = WebTracker.Grids.TrackingDeclarations.ReconIssueDesc });
			AddColumn(new ZTextEditColumn("Ultimate Consignee Name", "Declaration.UltimateConsigneeName") { ColumnKey = WebTracker.Grids.TrackingDeclarations.UltimateConsigneeName });
			AddColumn(new ZTextEditColumn("PGA Status", "Declaration.PGAStatus") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PGAStatus });
			AddColumn(new ZTextEditColumn("PGA Status Description", "Declaration.PGAStatusDesc") { ColumnKey = WebTracker.Grids.TrackingDeclarations.PGAStatusDescription });
			AddColumn(new ZTextEditColumn("Carrier SCAC", "Declaration.US_UI_NKCarrierSCAC") { ColumnKey = WebTracker.Grids.TrackingDeclarations.CarrierSCAC });
		}
	}
}
