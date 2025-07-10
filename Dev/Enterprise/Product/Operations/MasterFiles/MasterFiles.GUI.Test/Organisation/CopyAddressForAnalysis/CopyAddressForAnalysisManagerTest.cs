using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CopyAddressForAnalysisManagerTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrgMenuOnNullParentFormArgument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_Code = "ABC";

			new CopyAddressForAnalysisManager().CreateOrgMenu(null, org);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateOrgMenuOnNullOrgHeaderArgument()
		{
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new CopyAddressForAnalysisManager().CreateOrgMenu(dummyForm, null);
			}
		}

		public void TestCreateOrgMenu()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";

			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new CopyAddressForAnalysisManager().CreateOrgMenu(dummyForm, org);
				dummyForm.Show();
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertNotNull("Check menu item exists", actionsMenuItem.MenuItems.FindByText("Copy Address For Analysis"));
			}
		}
	}
}
