using System.Linq;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.GUI.Testing
{
	[TestedType(typeof(PackageDetailsUserControlDialogForm))]
	class PackageDetailsUserControlDialogFormTest : ZFormBasherTest
	{
		#region TestPackageDetailsUserControlIsShown

		public void TestPackageDetailsUserControlIsShown()
		{
			var package = CreatePackageForTesting();
			using (var packageDetailsUserControlDialogForm = new PackageDetailsUserControlDialogForm(package))
			{
				packageDetailsUserControlDialogForm.Show();
				var packageDetailsUserControl = (PackageDetailUserControl)packageDetailsUserControlDialogForm.Controls.Find("PackageDetailUserControl", true).Single();
				AssertEquals(true, packageDetailsUserControl.Visible);

				var packQtyCalcEdit = (ZCalcEdit)packageDetailsUserControl.Controls.Find("PackQtyCalcEdit", true).Single();
				var packageIDTextBox = (ZTextBox)packageDetailsUserControl.Controls.Find("PackageIDTextBox", true).Single();
				var transportRefTextBox = (ZTextBox)packageDetailsUserControl.Controls.Find("TransportRefTextBox", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals(true, packQtyCalcEdit.ReadOnly);
					AssertEquals(true, packageIDTextBox.ReadOnly);
					AssertEquals(true, transportRefTextBox.ReadOnly);
				});
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			var package = CreatePackageForTesting();
			using (var form = new PackageDetailsUserControlDialogForm(package))
			{
				AssertEquals("Package Details - PKG2", form.FormCaption);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var package = CreatePackageForTesting();
			return new PackageDetailsUserControlDialogForm(package);
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override bool AllowHasChangesOnFormOpen => true;

		PkgPackage CreatePackageForTesting()
		{
			var parent = Factory.New<DummyBizOWithPackLines>();
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			return packageState.Package;
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
