using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	class DeduplicationInitializer
	{
		public void CreateOrgMenu(ZForm parentForm, OrgHeader organization)
		{
			Argument.NotNull(parentForm, "Parent Form");
			Argument.NotNull(organization, "Organization");

			dedupRunnerMenu = new ZMenuItem(ResString.GetMultilingualString("DeDuplicationInitializer|CheckDuplicate", "Find &Duplicates"), OnDedupRunnerMenuClick)
			{
				Shortcut = Shortcut.CtrlG
			};

			IFileMenuItemsProvider menuProvider = parentForm;

			if (menuProvider.ActionsMenuItem.MenuItems.Count > 0 &&
				menuProvider.ActionsMenuItem.MenuItems[menuProvider.ActionsMenuItem.MenuItems.Count - 1].Text != "-")
			{
				ZFormMenuStrategy.AddActionsMenuItem(menuProvider, "-", null);
			}

			ZFormMenuStrategy.AddActionsMenuItem(parentForm, dedupRunnerMenu);

			if (menuProvider.ActionsMenuItem.MenuItems.IndexOf(dedupRunnerMenu) <= menuProvider.ActionsMenuItem.MenuItems.Count - 1)
			{
				ZFormMenuStrategy.AddActionsMenuItem(menuProvider, "-", null);
			}

			if (parentForm is BaseOrganisationsForm form)
			{
				orgHeader = organization;
				orgForm = form;
				OrganisationsTabControl_SelectedIndexChanged(this, null);
				orgForm.OrganisationsTabControl.SelectedIndexChanged += OrganisationsTabControl_SelectedIndexChanged;
			}
		}

		OrgHeader orgHeader;
		BaseOrganisationsForm orgForm;
		ZMenuItem dedupRunnerMenu;

		void OnDedupRunnerMenuClick(object sender, EventArgs args)
		{
			if (orgForm != null)
			{
				if (orgForm.OrganisationsTabControl.SelectedTab == orgForm.DetailsTabPage || orgForm.OrganisationsTabControl.SelectedTab == orgForm.AddressesTabPage)
				{
					orgHeader.FindDuplicates();
				}
				else if (orgForm.OrganisationsTabControl.SelectedTab == orgForm.ContactsTabPage)
				{
					var person = orgForm.ContactsControl.SelectedContact?.Person;
					if (person != null)
					{
						person.FindDuplicates();
					}
				}
			}
		}

		void OrganisationsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			var selectTab = orgForm.OrganisationsTabControl.SelectedTab;
			if (selectTab == orgForm.DetailsTabPage || selectTab == orgForm.AddressesTabPage || selectTab == orgForm.ContactsTabPage)
			{
				if (selectTab == orgForm.ContactsTabPage)
				{
					var person = orgForm.ContactsControl.SelectedContact?.Person;
					dedupRunnerMenu.Enabled = person != null && Environment.Env.Security.PersonIntelligenceDuplicateDetection.IsAllowed && person.IsDeduplicationAllowed && orgHeader.OH_IsActive;
				}
				else
				{
					dedupRunnerMenu.Enabled = Environment.Env.Security.OrgDuplicateDetection.IsAllowed && orgHeader.IsDeduplicationAllowed && orgHeader.OH_IsActive;
				}
			}
			else
			{
				dedupRunnerMenu.Enabled = false;
			}
		}
	}
}
