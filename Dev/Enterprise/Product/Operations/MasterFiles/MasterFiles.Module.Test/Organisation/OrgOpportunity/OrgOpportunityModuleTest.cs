using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgOpportunityModule))]
	sealed class OrgOpportunityModuleTest : ZModuleBasherTest
	{
		public void TestActionsMenu_DivideAllLegacyEstimateValuesByTwelve()
		{
			using (var module = new OrgOpportunityModule())
			{
				var initializeFormActionMenu = module.FormActionMenu;
				var divideLegacyValuesActionMenuItem = module.ActionsMenuItem.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "Divide all Legacy Values by 12 (One-off)");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				divideLegacyValuesActionMenuItem.PerformClick();

				divideLegacyValuesActionMenuItem = module.ActionsMenuItem.MenuItems.OfType<ZMenuItem>().FirstOrDefault(x => x.Caption == "Divide all Legacy Values by 12 (One-off)");
				AssertNull("Should hide this action menu once it has been run", divideLegacyValuesActionMenuItem);
			}
		}

		[RequiresSTA]
		public void TestActionsMenu_BulkUpdateExchangeRateDate()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_Status = "XXX";
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_Status = "XXX";
			Factory.Save();

			using (var module = new OrgOpportunityModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.SetFormsModalTo(form);

				var initializeFormActionMenu = module.FormActionMenu;
				var bulkUpdateMenuItem = module.ActionsMenuItem.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "Bulk update Exchange Rate Date");

				IFilterModuleInternalsForTesting moduleInternals = module;
				var statusFilter = (ModuleTextFilter)module.FilterBusinessObject["Status"];
				statusFilter.IsActive = true;
				statusFilter.Property = "YYY";
				moduleInternals.PerformSearch();
				AssertEquals("Precondition", 0, module.GridCollection.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				bulkUpdateMenuItem.PerformClick();
				AssertEquals("No opportunities have been selected. Please filter for the opportunities that you wish to update exchange rate date for.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);

				statusFilter.Property = "XXX";
				moduleInternals.PerformSearch();
				AssertEquals("Precondition", 2, module.GridCollection.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				bulkUpdateMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(BulkUpdateOpportunityDateForExchangeRateForm), lastShownForm);
					var bulkUpdateForm = (BulkUpdateOpportunityDateForExchangeRateForm)lastShownForm;
					AssertEquals(2, bulkUpdateForm.BusinessEntity.Count);
				}

				module.DisplayGrid.Select(1);
				bulkUpdateMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(BulkUpdateOpportunityDateForExchangeRateForm), lastShownForm);
					var bulkUpdateForm = (BulkUpdateOpportunityDateForExchangeRateForm)lastShownForm;
					AssertEquals(1, bulkUpdateForm.BusinessEntity.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestAdditionalMenu_CopyOpportunity()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();
			var contact3 = org3.Contacts.AddNew();

			var sourceOpp = Factory.NewWithValidTestData<OrgOpportunity>();
			sourceOpp.P8_OpportunityDescription = "Source Opportunity to copy from";
			sourceOpp.P8_PackageType = "PRD";
			sourceOpp.P8_OpportunityType = "NEW";
			sourceOpp.P8_DiscountAmount = 12;
			sourceOpp.P8_RentalMultiplier = 100;
			sourceOpp.P8_OH = org1.PK;
			sourceOpp.P8_OC = contact1.PK;
			sourceOpp.P8_GS_NKPrimarySalesPerson = "AAA";
			sourceOpp.P8_OA_AssignedOffice = org2.MainAddress.PK;
			sourceOpp.P8_OC_AssignedOfficeContact = contact2.PK;
			sourceOpp.P8_Source = "WEB";
			sourceOpp.P8_SourceDetails = "From web enquiry";
			sourceOpp.P8_OH_ReferringOrganisation = org3.PK;
			sourceOpp.P8_OC_ReferringContact = contact3.PK;

			Factory.Save();

			using (var module = new OrgOpportunityModule())
			using (var form = new ZForm())
			{
				var formCached = OpenedFormCache.GetInstance();
				try
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					module.SetFormsModalTo(form);

					var toolBarButtons = module.ToolBarButtons;
					var copyMenuItem = (ZToolBarButton)toolBarButtons.FirstOrDefault(x => x.Text == "Copy");

					UnitTestUserNotification.Instance.AddOKAnswer();
					copyMenuItem.PerformClick();
					AssertEquals("Please select one opportunity to copy.", UnitTestUserNotification.Instance.LastMessage.Text);

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition", 1, module.GridCollection.Count);

					module.DisplayGrid.Select(0);
					copyMenuItem.PerformClick();

					AssertEquals(1, formCached.Count);
					AssertEquals(typeof(OpportunityForm), formCached.FormCache.Values.First().GetType());

					var newOppForm = formCached.FormCache.Values.First() as OpportunityForm;
					var newOpp = newOppForm.BusinessEntity;
					CombineAssertions(() =>
					{
						AssertEquals("P8_OpportunityDescription", "Source Opportunity to copy from", newOpp.P8_OpportunityDescription);
						AssertEquals("P8_PackageType", "PRD", newOpp.P8_PackageType);
						AssertEquals("P8_OpportunityType", "NEW", newOpp.P8_OpportunityType);
						AssertEquals("P8_DiscountAmount", 12m, newOpp.P8_DiscountAmount);
						AssertEquals("P8_RentalMultiplier", 100m, newOpp.P8_RentalMultiplier);
						AssertEquals("P8_OH", org1.PK, newOpp.P8_OH);
						AssertEquals("P8_OC", contact1.PK, newOpp.P8_OC);
						AssertEquals("P8_GS_NKPrimarySalesPerson", "AAA", newOpp.P8_GS_NKPrimarySalesPerson);
						AssertEquals("P8_OA_AssignedOffice", org2.MainAddress.PK, newOpp.P8_OA_AssignedOffice);
						AssertEquals("P8_OC_AssignedOfficeContact", contact2.PK, newOpp.P8_OC_AssignedOfficeContact);
						AssertEquals("P8_Source", "WEB", newOpp.P8_Source);
						AssertEquals("P8_SourceDetails", "From web enquiry", newOpp.P8_SourceDetails);
						AssertEquals("P8_OH_ReferringOrganisation", org3.PK, newOpp.P8_OH_ReferringOrganisation);
						AssertEquals("P8_OC_ReferringContact", contact3.PK, newOpp.P8_OC_ReferringContact);
						AssertEquals("SourceOpportunityOrgPK", org1.PK, newOpp.SourceOpportunityOrgPK);
					});
				}
				finally
				{
					formCached.CloseAllCachedForms();
				}
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (OrgOpportunityModule module = new OrgOpportunityModule())
			{
				AssertEquals(ModuleIDs.Opportunity, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Opportunity;
		}
	}
}
