using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	public sealed class ACEFDAAddressesUserControlTest : TestCaseWithFactory
	{
		public void TestDocAddressControl()
		{
			var fda = Factory.New<ACEFDA>();
			var docAddress = fda.DocAddresses.AddNew();

			using (var form = new ZChildForm(fda))
			{
				var fdaDocAddressesControl = new ACEFDAAddressesUserControl();
				fdaDocAddressesControl.SetBindingMember("DocAddresses");
				fdaDocAddressesControl.Dock = System.Windows.Forms.DockStyle.Fill;
				form.Controls.Add(fdaDocAddressesControl);
				form.Show();
				var addressGrid = (ZGrid)form.Controls.Find("DocAddressGrid", true).Single();
				var addressControl = (ZDocAddressControl)form.Controls.Find("DocAddressControl", true).Single();
				addressGrid.CurrentRowIndex = 0;

				docAddress.E2_AddressType = DocAddressTypes.Codes.GoodsDeliveredTo;
				AssertEquals("Delivery To Party", addressControl.Text);
				AssertType<ConsigneeCollection>(fda.DocAddressOrganizations);

				docAddress.E2_AddressType = DocAddressTypes.Codes.Manufacturer;
				AssertEquals("Manufacturer", addressControl.Text);
				AssertType<ConsignorCollection>(fda.DocAddressOrganizations);

				docAddress.E2_AddressType = DocAddressTypes.Codes.GoodsOwner;
				AssertEquals("Owner", addressControl.Text);
				AssertType<OrganisationsFindBoxCollection>(fda.DocAddressOrganizations);
			}
		}
	}
}
