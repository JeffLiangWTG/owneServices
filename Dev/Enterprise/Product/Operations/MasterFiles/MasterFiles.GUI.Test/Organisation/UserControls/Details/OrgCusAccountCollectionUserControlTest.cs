using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgCusAccountCollectionUserControlTest : TestCaseWithFactory
	{
		public void TestControlsArePresent()
		{
			AssertEquals("OrgCusAccountCollectionsGrid Count", 1, control.Find(x => x.Name == "OrgCusAccountCollectionsGrid").Count());
		}

		[RequiresSTA]
		public void TestColumnsForCountrySpecific()
		{
			var orgCusAccountCollectionsGrid = control.FindSingle<ZGrid>("OrgCusAccountCollectionsGrid");

			AssertContainsExactElementsInAnyOrder("orgCusAccountCollectionsGrid.Columns bound to null", new[]
			{
				"Code",
				"Account",
				"Account Type",
				"Issuer",
				"Decrypted Password",
				"Reporting Period",
			}, orgCusAccountCollectionsGrid.Columns.Select(x => x.ColumnStyle.HeaderText));

			control.SetDataBinding(org, "DefermentAccountNumberCollection");

			AssertContainsExactElementsInAnyOrder("orgCusAccountCollectionsGrid.Columns bound to DefermentAccountNumberCollection", new[]
			{
				"Account",
				"Account Number",
				"Account Type",
				"Prefix",
				"BIN",
				"Reporting Period",
			}, orgCusAccountCollectionsGrid.Columns.Select(x => x.ColumnStyle.HeaderText));
		}

		protected override void SetUp()
		{
			base.SetUp();

			org = OrgHeader.New(Factory);
			form = new ZForm(org);
			control = new OrgCusAccountCollectionUserControl();
			form.Controls.Add(control);
			form.Show();
		}

		OrgHeader org;
		ZForm form;
		OrgCusAccountCollectionUserControl control;

		protected override void TearDown()
		{
			control?.Dispose();
			form?.Dispose();

			base.TearDown();
		}
	}
}
