using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RelatedPartiesUserControlTest : TestCaseWithFactory
	{
		public void TestRelatedPartiesGridContextMenu_Popup()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			relatedParty.PR_CustomsStatus = CSARelatedPartyStatusList.Codes.Added;
			relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			using (var testForm = new ZOrganisationsForm(testHeader))
			{
				using (var control = new RelatedPartiesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					var grid = control.Controls.Find("RelatedPartiesGrid", true).FirstOrDefault() as ZGrid;
					grid.DataSource = testHeader.AllRelatedPartiesView;
					grid.Select(0);
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					AssertNotNull("Set Related Party as already added", grid.ContextMenu.MenuItems.FindByText("Set Related Party as already added"));
					AssertNotNull("Refresh the Related Party", grid.ContextMenu.MenuItems.FindByText("Refresh the Related Party"));
					grid.Select(1);
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					AssertNull("Set Related Party as already added", grid.ContextMenu.MenuItems.FindByText("Set Related Party as already added"));
					AssertNull("Refresh the Related Party", grid.ContextMenu.MenuItems.FindByText("Refresh the Related Party"));
					grid.Select(2);
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					AssertNull("Set Related Party as already added", grid.ContextMenu.MenuItems.FindByText("Set Related Party as already added"));
					AssertNull("Refresh the Related Party", grid.ContextMenu.MenuItems.FindByText("Refresh the Related Party"));
				}
			}
		}

		[RequiresSTA]
		public void TestSetRelatedPartyAddedClick()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			using (var testForm = new ZOrganisationsForm(testHeader))
			{
				using (var control = new RelatedPartiesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					var grid = control.Controls.Find("RelatedPartiesGrid", true).FirstOrDefault() as ZGrid;
					grid.DataSource = testHeader.AllRelatedPartiesView;
					grid.Select(0);
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					var menuItem = grid.ContextMenu.MenuItems.FindByText("Set Related Party as already added");
					menuItem.PerformClick();
					AssertEquals("Confirmation box", $"You should only manually change the CSA Status to Added if the related party was registered with Customs externally to {Core.Constants.ProductName}. Do you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestRefreshRelatedPartyClick()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CSAApprovedUltimateConsignee;
			relatedParty = testHeader.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			using (var testForm = new ZOrganisationsForm(testHeader))
			{
				using (var control = new RelatedPartiesUserControl())
				{
					testForm.Controls.Add(control);
					testForm.Show();
					var grid = control.Controls.Find("RelatedPartiesGrid", true).FirstOrDefault() as ZGrid;
					grid.DataSource = testHeader.AllRelatedPartiesView;
					grid.Select(0);
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					var menuItem = grid.ContextMenu.MenuItems.FindByText("Refresh the Related Party");
					menuItem.PerformClick();
					AssertEquals("Confirmation box", "This will result in the details at Customs being refreshed by first sending a delete message and if that is accepted a subsequent add message will be sent. Do you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					grid.SelectAllElements();
					control.RelatedPartiesGridContextMenu_Popup(grid, null);
					menuItem = grid.ContextMenu.MenuItems.FindByText("Refresh the Related Party");
					AssertNull("No menu item", menuItem);
				}
			}
		}
	}
}
