using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.UserControls.Testing;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class ExportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestUnnecessaryControlsHaveBeenHidden()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");

				var sellerAddressControl = control.FindSingle<ZAddressControl>("DeclarantOfficeAddressControl");
				AssertEquals(false, sellerAddressControl.Visible);

				var representativeAddressControl = control.FindSingle<ZAddressControl>("RepresentativeAddressControl");
				AssertEquals(false, representativeAddressControl.Visible);
			}
		}

		public void TestOrganizationControlsOrder()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");

				var sellerAddressControl = control.FindSingle<ZAddressControl>("SellerAddressControl");
				var manufacturerAddressControl = control.FindSingle<ZAddressControl>("ManufacturerAddressControl");
				var fromWarehouseAddressControl = control.FindSingle<ZAddressControl>("FromWarehouseAddressControl");
				var toWarehouseAddressControl = control.FindSingle<ZAddressControl>("ToWarehouseAddressControl");
				AssertGreaterThan(manufacturerAddressControl.Location.Y, sellerAddressControl.Location.Y);
				AssertGreaterThan(toWarehouseAddressControl.Location.Y, manufacturerAddressControl.Location.Y);
				AssertGreaterThan(fromWarehouseAddressControl.Location.Y, toWarehouseAddressControl.Location.Y);
			}
		}
		public void TestWarehouseControls()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				TestHelper.AssertControlExists(control, "FromWarehouseAddressControl", "JE_EntryFromWarehouse", true);
				TestHelper.AssertControlExists(control, "ToWarehouseAddressControl", "JE_EntryToWarehouse", true);

				var fromWarehouseAddressControl = control.FindSingle<ZAddressControl>("FromWarehouseAddressControl");
				var toWarehouseAddressControl = control.FindSingle<ZAddressControl>("ToWarehouseAddressControl");
				var declarantAddressControl = control.FindSingle<ZAddressControl>("DeclarantOfficeAddressControl");
				var representativeAddressControl = control.FindSingle<ZAddressControl>("RepresentativeAddressControl");
				AssertEquals(representativeAddressControl.Location.Y, toWarehouseAddressControl.Location.Y);
				AssertEquals(declarantAddressControl.Location.Y, fromWarehouseAddressControl.Location.Y);
			}
		}

		public void TestInitializeTradersGrid()
		{
			using (var form = new ZForm())
			using (var control = new ExportOrganizationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var containersGrid = control.FindSingle<ZGrid>("TradersGrid");

				CombineAssertions("Test for Existence of The Grid", () =>
				{
					AssertEquals("E2_AddressType", ((ZGridColumnInfo)containersGrid.ColumnStyles[0]).ColumnName);
					AssertEquals("OrganisationPK", ((ZGridColumnInfo)containersGrid.ColumnStyles[1]).ColumnName);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
