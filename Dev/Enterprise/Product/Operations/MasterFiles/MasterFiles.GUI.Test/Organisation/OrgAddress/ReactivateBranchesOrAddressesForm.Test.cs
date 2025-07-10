using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	[TestedType(typeof(ReactivateBranchesOrAddressesForm))]
	public class ReactivateBranchesOrAddressesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ReactivateBranchesOrAddressesForm(new ReactivateBranchOrAddressModel(new OrgAddressDependentCollection(Factory)));
		}

		public void TestFormTextAndColumnStyle()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_IsActive = true;
			var model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Addresses To Reactivate", form.HeaderLabel_Exposed.Text);
					AssertEquals("Reactivate Addresses", form.FormCaption);
					AssertEquals("Deselect All", form.SelectUnselectAllCheckBox_Exposed.Text);
					Assert(form.ReactivateGrid_Exposed is ReactivateAddressesGrid);
				});

				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = false;
				AssertEquals("Selected All", form.SelectUnselectAllCheckBox_Exposed.Text);
			}

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_IsActive = false;
			model = new ReactivateBranchOrAddressModel(company.Branches);
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Branches To Reactivate", form.HeaderLabel_Exposed.Text);
					AssertEquals("Reactivate Branches", form.FormCaption);
					AssertEquals("Selected All", form.SelectUnselectAllCheckBox_Exposed.Text);
					Assert(form.ReactivateGrid_Exposed is ReactivateBranchesGrid);
				});

				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
				AssertEquals("Deselect All", form.SelectUnselectAllCheckBox_Exposed.Text);
			}
		}

		[RequiresSTA]
		public void TestApplyStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_IsActive = false;
			var model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
				AssertEquals("Haven't Change", false, address.OA_IsActive);

				form.ActivateButton_Exposed.PerformClick();
				AssertEquals("Status Applied", true, address.OA_IsActive);
			}

			address.OA_IsActive = false;
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
				AssertEquals("Haven't Change", false, address.OA_IsActive);

				form.CancelButton_Exposed.PerformClick();
				AssertEquals("Haven't Change", false, address.OA_IsActive);
			}
		}

		public void TestActivateButtonEnabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;
			address.OA_IsActive = true;
			var model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				AssertEquals(true, form.ActivateButton_Exposed.Enabled);
			}

			address.OA_IsActive = false;
			model = new ReactivateBranchOrAddressModel(orgHeader.Addresses);
			using (var form = new ReactivateBranchesOrAddressesFormForTest(model))
			{
				form.Show();
				AssertEquals(false, form.ActivateButton_Exposed.Enabled);

				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = true;
				AssertEquals(true, form.ActivateButton_Exposed.Enabled);

				model.ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().First().Selected = false;
				AssertEquals(false, form.ActivateButton_Exposed.Enabled);
			}
		}

		#region Implementation

		public class ReactivateBranchesOrAddressesFormForTest : ReactivateBranchesOrAddressesForm
		{
			public ReactivateBranchesOrAddressesFormForTest(ReactivateBranchOrAddressModel model) : base(model) { }

			public ZCheckBox SelectUnselectAllCheckBox_Exposed => SelectUnselectAllCheckBox;
			public ZButton ActivateButton_Exposed => ActivateButton;
			public ZArchitecture.ZLabel HeaderLabel_Exposed => HeaderLabel;
			public ZUserControl ReactivateGrid_Exposed => ReactivateGrid;
			public ZButton CancelButton_Exposed => CancelButton;
		}

		#endregion
	}
}
