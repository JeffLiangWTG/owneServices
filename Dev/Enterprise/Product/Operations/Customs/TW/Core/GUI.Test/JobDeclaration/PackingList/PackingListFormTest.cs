using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusPackageJob = Enterprise.Customs.TW.Business.CusPackageJob;
using CusPackingList = Enterprise.Customs.TW.Business.CusPackingList;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(PackingListForm))]
	sealed class PackingListFormTest : ZFormBasherTest
	{
		public void TestPackingListDetailsUserControl()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				AssertType<TWPackingListDetailsUserControl>(form.FindSingle<ZDynamicControlCreationUserControl>("PackingListDetailsDynamicUserControl").HostedControl);
			}
		}

		public void TestActionsMenuItem()
		{
			using (var form = new TestPackingListForm(packingList))
			{
				form.Show();
				AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("Calculate Pack Qty From Pack #"));
			}
		}

		public void TestCalculatePackQtyFromPack()
		{
			using (var form = new TestPackingListForm(packingList))
			{
				form.Show();
				var calculatePackQtyFromSummaryMenu = form.ActionsMenuItemForTest.MenuItems.FindByText("Calculate Pack Qty From Pack #");
				var packageJob = (CusPackageJob)packingList.PackageJob;

				calculatePackQtyFromSummaryMenu.PerformClick();
				Assert("calculatePackQtyFromSummaryMenu is checked", calculatePackQtyFromSummaryMenu.Checked);
				Assert("Is calculatePackQtyFromPack", packageJob.IsCalculatePackQtyFromPack);
				calculatePackQtyFromSummaryMenu.PerformClick();
				Assert("calculatePackQtyFromSummaryMenu is not checked", !calculatePackQtyFromSummaryMenu.Checked);
				Assert("Is not calculatePackQtyFromPack", !packageJob.IsCalculatePackQtyFromPack);
			}
		}

		public void TestDefaultCalculatePackQtyFromPackMenuItemChecked()
		{
			using (TWCustomsDataRegistry.Instance.AlwaysCalculatePackQtyFromPackNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, false))
			{
				using (var form = new TestPackingListForm(packingList))
				{
					form.Show();
					var calculatePackQtyFromSummaryMenu = form.ActionsMenuItemForTest.MenuItems.FindByText("Calculate Pack Qty From Pack #");
					var packageJob = (CusPackageJob)packingList.PackageJob;
					Assert("calculatePackQtyFromSummaryMenu default is not checked", !calculatePackQtyFromSummaryMenu.Checked);
					Assert("default Is not calculatePackQtyFromPack", !packageJob.IsCalculatePackQtyFromPack);
				}
			}

			using (TWCustomsDataRegistry.Instance.AlwaysCalculatePackQtyFromPackNumber.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				using (var form = new TestPackingListForm(packingList))
				{
					form.Show();
					var calculatePackQtyFromSummaryMenu = form.ActionsMenuItemForTest.MenuItems.FindByText("Calculate Pack Qty From Pack #");
					var packageJob = (CusPackageJob)packingList.PackageJob;
					Assert("calculatePackQtyFromSummaryMenu default is checked", calculatePackQtyFromSummaryMenu.Checked);
					Assert("default Is calculatePackQtyFromPack", packageJob.IsCalculatePackQtyFromPack);
				}
			}
		}

		[TestDate(2022, 04, 13)]
		public void TestPackableItemRelataionsAfterDoSplit()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var packingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			var package = packingList.PackageJob.Packages.AddNew();
			var packableItemRelataions = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			using (var form = new TestPackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid").Select(0);
				var zToolStrip = form.FindSingle<ZToolStrip>(x => x.Name == "zToolStrip");
				var splitStripButton = zToolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "Split_StripButton");
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var splitPartsForm = f as PackableItemSplitPartsForm;
					var okButton = splitPartsForm.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
					var packableItemsSplitter = (PackableItemsSplitter)splitPartsForm.BusinessEntity;
					var part1 = packableItemsSplitter.PackableItemParts.AddNew();
					part1.GoodsDescription = "Part 1";
					part1.PackableQuantity = 101;
					part1.PackableUQ = "ROL";

					var part2 = packableItemsSplitter.PackableItemParts.AddNew();
					part2.GoodsDescription = "Part 2";
					part2.PackableQuantity = 143;
					part2.PackableUQ = "ROL";
				});
				splitStripButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, packableItemRelataions.Count());
					Assert(packableItemRelataions.All(c => !c.PackableItem.IsDeleted));
				});
			}
		}

		protected override Form GetFormToBashCore() => new PackingListForm(Factory.NewWithValidTestData<CusPackingList>());

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			packingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
		}

		CusPackingList packingList;
	}

	class TestPackingListForm : PackingListForm
	{
		public TestPackingListForm(CusPackingList cusPackingList)
			: base(cusPackingList)
		{
		}

		public MenuItem ActionsMenuItemForTest => base.ActionsMenuItem;
	}
}
