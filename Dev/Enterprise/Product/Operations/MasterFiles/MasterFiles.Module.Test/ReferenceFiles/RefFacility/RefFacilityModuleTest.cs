using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefFacilityModule))]
	sealed class RefFacilityModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestDeleteButtonClick()
		{
			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			refFacility1.RFT_IsSystem = true;

			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_IsSystem = false;

			Factory.Save();

			using (var form = new ZForm())
			using (var module = new RefFacilityModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menuItems = module.GetNewStandardMenuItemsForTest();
				var deleteMenuItem = menuItems[3];
				AssertEquals("Precondition: ", "&Delete", deleteMenuItem.Text);

				module.PerformSearch_ForTest();
				AssertEquals("Precondition: ", 2, module.GridCollection.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectSingleElement(refFacility1);
				deleteMenuItem.PerformClick();
				AssertEquals("System created reference files cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectAllElements();
				deleteMenuItem.PerformClick();
				AssertEquals("System created reference files cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectSingleElement(refFacility2);
				deleteMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var openForms = ZApplication.GetOpenForms();
				foreach (var openForm in openForms.Where(x => x.GetType() == typeof(RefFacilityForm)))
				{
					openForm.Close();
				}
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new RefFacilityModule())
			{
				AssertEquals("SecurityCheckpoint should be RefFacility:", Env.Security.RefFacility, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = new RefFacilityModule())
			{
				AssertEquals("LicenseCheckPoint should be Core Licence", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new RefFacilityModuleForTest())
			using (var filterControl = module.GetNewFilterControlForTest())
			{
				AssertEquals("Type of filter control should be RefFacilityFilterControl", true, filterControl is RefFacilityFilterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new RefFacilityModuleForTest())
			{
				var shippingLineCollection = module.GetNewGridCollectionForTest();
				AssertEquals("Type of grid collection should be RefFacilityCollection", true, shippingLineCollection is RefFacilityCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new RefFacilityModuleForTest())
			{
				var filterBusinessObject = module.GetNewFilterBusinessObjectForTest();
				AssertEquals("Type of filter bizo should be RefFacilityFilterBusinessObject", true, filterBusinessObject is RefFacilityFilterBusinessObject);
			}
		}

		#region Implementation 

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.RefFacility;

		#endregion
	}
}
