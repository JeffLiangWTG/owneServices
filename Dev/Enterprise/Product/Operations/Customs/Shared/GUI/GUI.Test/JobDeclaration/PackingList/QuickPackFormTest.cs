using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(QuickPackForm))]
	sealed class QuickPackFormTest : ZFormBasherTest
	{
		public void TestQuickPackGridNumber()
		{
			var packableItem1 = CusPackingList.PackableItems.First();
			var packableItem2 = CusPackingList.PackableItems.ElementAt(1);
			package1.CustomsPackItem(packableItem2, 1);
			AssertEquals(1m, packableItem1.NotPackedQty);
			AssertEquals(1m, packableItem2.NotPackedQty);
			AssertQuickPackGridNumber(2);

			package2.CustomsPackItem(packableItem2, 1);
			AssertEquals(1m, packableItem1.NotPackedQty);
			AssertEquals(0m, packableItem2.NotPackedQty);
			AssertQuickPackGridNumber(1);

			package2.CustomsPackItem(packableItem1, 1);
			AssertEquals(0m, packableItem1.NotPackedQty);
			AssertEquals(0m, packableItem2.NotPackedQty);
			AssertQuickPackGridNumber(0);
		}

		public void AssertQuickPackGridNumber(int expectedNumber)
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var quickPackGrid = form.FindSingleOrDefault<ZGrid>(c => c.Name == "QuickPackItemsGrid");
				AssertEquals(expectedNumber, quickPackGrid.List.Count);
			}
		}

		public void TestQuickPackButtonClick()
		{
			var packableItem1 = CusPackingList.PackableItems.First();
			var packableItem2 = CusPackingList.PackableItems.ElementAt(1);
			Factory.Save();
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var quickPackGrid = form.FindSingleOrDefault<ZGrid>(c => c.Name == "QuickPackItemsGrid");
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				AssertEquals(2, quickPackGrid.List.Count);

				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Information Please select at least one pack.", UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var quickPackitems = ((QuickPack)quickPackGrid.DataSource).QuickPackItems.Cast<QuickPackItem>();
				var quickPackitem1 = quickPackitems.FirstOrDefault(c => c.GoodsDesc == "ITEM 1");
				quickPackitem1.Pack = "Pack #1";
				quickPackitem1.PackedQty = 3m;

				var quickPackitem2 = quickPackitems.FirstOrDefault(c => c.GoodsDesc == "ITEM 2");
				quickPackitem2.Pack = "Pack #3";
				quickPackitem2.PackedQty = 2m;

				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());

				quickPackitem1.PackedQty = 1m;
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestQuickFormSize()
		{
			for (int i = 0; i < 28; i++)
			{
				var packableItem = CusPackingList.PackableItems.AddNew();
				packableItem.CUI_PackableQty = 2m;
			}

			using (var form = GetFormToBashCore())
			{
				form.Show();
				var size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(869, 600, true);
				AssertEquals(size.Width, form.ClientSize.Width);
				AssertEquals(size.Height, form.ClientSize.Height);
			}
		}

		public void TestQuickPackGridVerticalScrollBarVisibleWhenDataRowCountEqualsThirty()
		{
			CusPackingList.PackableItems.DeleteAll();
			for (int i = 0; i < 30; i++)
			{
				var packableItem = CusPackingList.PackableItems.AddNew();
				packableItem.CUI_PackableQty = 2m;
			}

			using (var form = GetFormToBashCore())
			{
				form.Show();
				var quickPackGrid = form.FindSingleOrDefault<ZGrid>(c => c.Name == "QuickPackItemsGrid");
				AssertEquals(30, quickPackGrid.List.Count);
				Assert(!quickPackGrid.IsVerticalScrollBarVisible);
			}
		}

		public void TestPackSeq()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var columnInfos = form.FindSingleOrDefault<ZGrid>(c => c.Name == "QuickPackItemsGrid").ColumnStyles.Cast<ZGridColumnInfo>();
				Assert(columnInfos.Any(column => column.ColumnName == "PackSeq" && column is ZDropEditColumnStyleInfo));
				Assert(columnInfos.Any(column => column.ColumnName == "Pack" && column is ZTextBoxColumnStyleInfo));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var quickPack = new QuickPack(CusPackingList.PackableItems);
			quickPack.HasChanges = false;
			quickPack.QuickPackItems.Cast<QuickPackItem>().ForEach(c => c.HasChanges = false);
			return new QuickPackForm(quickPack);
		}

		CusPackingList CusPackingList
		{
			get
			{
				if (packingList == null)
				{
					var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
					var invoiceForTest = decl.Invoices.AddNew();
					var line1 = invoiceForTest.InvoiceLines.AddNew();
					line1.JI_Description = "ITEM 1";
					line1.JI_InvoiceQuantity = 1;
					line1.JI_InvoiceUQ = "PCE";
					var line2 = invoiceForTest.InvoiceLines.AddNew();
					line2.JI_Description = "ITEM 2";
					line2.JI_InvoiceQuantity = 2;
					line2.JI_InvoiceUQ = "PCE";

					packingList = decl.LoadOrCreateCusPackingList(Factory);
					packageJob = packingList.PackageJob;
					var packages = packageJob.Packages;
					package1 = packages.AddNew();
					package1.KP_MarksAndNumbers = "Pack #1";
					package2 = packages.AddNew();
					package2.KP_MarksAndNumbers = "Pack #2";
					var cusPackage1Relations = package1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
					var cusPackage2Relations = package2.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
				}
				return packingList;
			}
		}
		CusPackage package1;
		CusPackage package2;
		CusPackingList packingList;
		CusPackageJob packageJob;
	}
}
