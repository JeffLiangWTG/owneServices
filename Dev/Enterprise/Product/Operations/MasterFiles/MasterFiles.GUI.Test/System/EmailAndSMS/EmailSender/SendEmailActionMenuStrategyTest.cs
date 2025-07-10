using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SendEmailActionMenuStrategyTest : TestCaseWithFactory
	{
		public void TestAddSendEmailActionMenuIfApplicable()
		{
			bool isController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				GlbStaff.CurrentUser.GS_IsController = true;
				var org = Factory.New<OrgHeader>();
				AssertEquals("Precondition", true, org is ISendEmailSource);
				AssertAddedMenuItemAndAbility(org, "Should add menu item if form BusinessEntity implements ISendEmailSource", ControllerIDs.Organisation);

				var staff = Factory.New<GlbStaff>();
				AssertEquals("Precondition", false, staff is ISendEmailSource);
				using (var form = new ZForm(staff))
				{
					form.Show();
					var menuItem = GetSendMenuItem(form);
					AssertNull("Should not add menu item if form BusinessEntity does not implement ISendEmailSource", menuItem);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = isController;
			}
		}

		void AssertAddedMenuItemAndAbility(BusinessObject businessEntity, string addedMenuAssertionMessage, ControllerID id)
		{
			using (var form = new ZForm(businessEntity))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.ControllerID = id;
				form.Show();

				MenuItem menuItem = GetSendMenuItem(form);
				AssertNotNull(addedMenuAssertionMessage, menuItem);
				AssertEquals("Menu Item should be disabled if form is readonly", false, menuItem.Enabled);
			}

			using (var form = new ZForm(businessEntity))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.ControllerID = id;
				form.Show();

				MenuItem menuItem = GetSendMenuItem(form);
				AssertNotNull(addedMenuAssertionMessage, menuItem);
				AssertEquals("Menu Item should be enabled if form is in edit mode", true, menuItem.Enabled);
			}

			using (var form = new ZForm(businessEntity))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.ControllerID = id;
				form.Show();

				MenuItem menuItem = GetSendMenuItem(form);
				AssertNotNull(addedMenuAssertionMessage, menuItem);
				AssertEquals("Menu Item should be disabled if form is in delete mode", false, menuItem.Enabled);
			}
		}

		MenuItem GetSendMenuItem(ZForm form)
		{
			return ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == EmailSender.GetSendEmailMenuItemTextForTest());
		}
	}
}
