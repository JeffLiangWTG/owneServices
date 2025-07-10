using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RelatableActivityActionMenuStrategyTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAddRelatableActivityMenuItemsIfApplicable()
		{
			var addMenuStrategy = new RelatableActivityActionMenuStrategy();

			var opportunity = Factory.New<OrgOpportunity>();
			AssertEquals("Precondition", true, opportunity is IRelatableActivity);

			using (var form = new ZForm(opportunity))
			{
				addMenuStrategy.AddRelatableActivityMenuItemsIfApplicable(form);

				var menuItemTexts = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Text);
				AssertCollectionContains("Should add menu items if form BusinessEntity implements IRelatableActivity", "View Related Communications", menuItemTexts);
				AssertCollectionContains("Should add menu items if form BusinessEntity implements ISalesRelationActivity", "View Sales Relations", menuItemTexts);
			}

			var staff = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, staff is IRelatableActivity);

			using (var form = new ZForm(staff))
			{
				addMenuStrategy.AddRelatableActivityMenuItemsIfApplicable(form);

				var menuItemTexts = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Text);
				AssertCollectionNotContains("Should not add menu item if form BusinessEntity does not implement IRelatableActivity", "View Related Communications", menuItemTexts);
				AssertCollectionNotContains("Should not add menu item if form BusinessEntity does not implement ISalesRelationActivity", "View Sales Relations", menuItemTexts);
			}
		}

		[RequiresSTA]
		public void TestDoesNotAddViewSalesRelationsMenuItemForCommunication()
		{
			var addMenuStrategy = new RelatableActivityActionMenuStrategy();

			var communication = Factory.New<OrgSalesCall>();
			AssertEquals("Precondition", true, communication is ISalesRelationActivity);
			using (var form = new ZForm(communication))
			{
				addMenuStrategy.AddRelatableActivityMenuItemsIfApplicable(form);

				var menuItemTexts = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Text);
				AssertCollectionContains("Should add menu items if form BusinessEntity implements IRelatableActivity", "View Related Communications", menuItemTexts);
				AssertCollectionNotContains("Should not add menu items if form BusinessEntity is OrgSalesCall", "View Sales Relations", menuItemTexts);
			}
		}

		[RequiresSTA]
		public void TestMenuItemsHaveCorrectNames()
		{
			var addMenuStrategy = new RelatableActivityActionMenuStrategy();

			var opportunity = Factory.New<OrgOpportunity>();
			AssertEquals("Precondition", true, opportunity is IRelatableActivity);

			using (var form = new ZForm(opportunity))
			{
				addMenuStrategy.AddRelatableActivityMenuItemsIfApplicable(form);

				var viewRelatedCommunicationsMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(m => m.Text == "View Related Communications");
				var viewSalesRelationsMenuItem = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(m => m.Text == "View Sales Relations");

				AssertNotNull("Precondition: View Related Communications menu item should have been added", viewRelatedCommunicationsMenuItem);
				AssertNotNull("Precondition: View Sales Relations menu item should have been added", viewSalesRelationsMenuItem);

				AssertEquals("View Related Communications menu item should have correct Name.", "ViewRelatedCommunications", viewRelatedCommunicationsMenuItem.Name);
				AssertEquals("View Sales Relations menu item should have correct Name.", "ViewSalesRelations", viewSalesRelationsMenuItem.Name);
			}
		}
	}
}
