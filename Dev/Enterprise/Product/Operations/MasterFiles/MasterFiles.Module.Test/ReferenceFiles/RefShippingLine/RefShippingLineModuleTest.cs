using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
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
	[TestedType(typeof(RefShippingLineModule))]
	sealed class RefShippingLineModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestDeleteButtonClick()
		{
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_IsSystem = true;

			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_IsSystem = false;

			Factory.Save();

			using (var form = new ZForm())
			using (var module = new RefShippingLineModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var menuItems = module.GetNewStandardMenuItemsForTest();
				var deleteMenuItem = menuItems[3];
				AssertEquals("Precondition: ", "&Delete", deleteMenuItem.Text);

				module.PerformSearch_ForTest();
				AssertEquals("Precondition: ", 2, module.GridCollection.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectSingleElement(shippingLine1);
				deleteMenuItem.PerformClick();
				AssertEquals("System created reference files cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectAllElements();
				deleteMenuItem.PerformClick();
				AssertEquals("System created reference files cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				module.DisplayGrid.SelectSingleElement(shippingLine2);
				deleteMenuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				Application.OpenForms.OfType<RefShippingLineForm>().FirstOrDefault()?.Close();
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new RefShippingLineModule())
			{
				AssertEquals("SecurityCheckpoint should be RefShippingLine:", Env.Security.RefShippingLine, module.SecurityCheckpoint);
			}
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = new RefShippingLineModule())
			{
				AssertEquals("LicenseCheckPoint should be Core Licence", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new RefShippingLineModuleForTest())
			using (var filterControl = module.GetNewFilterControlForTest())
			{
				AssertEquals("Type of filter control should be RefShippingLineFilterControl", true, filterControl is RefShippingLineFilterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new RefShippingLineModuleForTest())
			{
				var shippingLineCollection = module.GetNewGridCollectionForTest();
				AssertEquals("Type of grid collection should be RefShippingLineCollection", true, shippingLineCollection is RefShippingLineCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new RefShippingLineModuleForTest())
			{
				var filterBusinessObject = module.GetNewFilterBusinessObjectForTest();
				AssertEquals("Type of filter bizo should be RefShippingLineFilterBusinessObject", true, filterBusinessObject is RefShippingLineFilterBusinessObject);
			}
		}

		#region Implementation 

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.RefShippingLine;

		public class RefShippingLineModuleForTest : RefShippingLineModule
		{
			public MenuItem[] GetNewStandardMenuItemsForTest() => base.GetNewStandardMenuItems();

			public IFilterControl GetNewFilterControlForTest() => GetNewFilterControl();

			public IBusinessObjectCollection GetNewGridCollectionForTest() => GetNewGridCollection();

			public FilterBusinessObject GetNewFilterBusinessObjectForTest() => GetNewFilterBusinessObject();
		}

		#endregion
	}
}
