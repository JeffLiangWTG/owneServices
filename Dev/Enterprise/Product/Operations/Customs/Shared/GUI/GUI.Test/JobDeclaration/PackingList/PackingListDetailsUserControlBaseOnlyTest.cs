using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(PackingListDetailsUserControl))]
	sealed class PackingListDetailsUserControlBaseOnlyTest : TestCaseWithFactory
	{
		public void TestPackagesGridVisibility()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var grid = control.PackagesGrid;
				var dunnageWeightColumn = grid.GetColumnStyle(CusPackage.Schema.KP_DunnageWeight);
				AssertNotNull("PackagesGrid should contain the conlumn 'KP_DunnageWeight'.", dunnageWeightColumn);
				Assert("KP_DunnageWeight column should be visible by default.", !dunnageWeightColumn.IsVisible);
			}
		}

		public void TestPackagesGridDefalutColumns()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var grid = control.PackagesGrid;
				for (int i = 0; i < ExpectedDefaultColumnsForGrid.Count; i++)
				{
					var column = grid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedDefaultColumnsForGrid[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestPackableItemsGrid()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var grid = control.PackableItemsGrid;
				var originalGoodsDescriptionColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.OriginalGoodsDescription);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'OriginalGoodsDescription'.", originalGoodsDescriptionColumn);
				Assert("OriginalGoodsDescription column should be hidden by default.", !originalGoodsDescriptionColumn.IsVisible);

				var relatedPacksColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.RelatedPacks);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'RelatedPacks'.", relatedPacksColumn);
				Assert("RelatedPacks column should be hidden by default.", !relatedPacksColumn.IsVisible);

				var sequenceColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.Sequence);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'CUI_Sequence'.", sequenceColumn);
				Assert("RelatedPacks column should be visible.", sequenceColumn.IsVisible);

				var netWeightColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.NetWeight);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'NetWeight'.", netWeightColumn);
				Assert("RelatedPacks column should be visible.", netWeightColumn.IsVisible);

				var netWeightUQColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.NetWeightUQ);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'NetWeightUQ'.", netWeightUQColumn);
				Assert("RelatedPacks column should be visible.", netWeightUQColumn.IsVisible);

				var packableUQColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.PackableUQ);
				AssertEquals(75, packableUQColumn.Width);

				var groupingColumn = grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.Grouping);
				AssertNotNull("PackableItemsGrid should contain the conlumn 'Grouping'.", groupingColumn);
			}
		}

		public void TestDeleteButtonClick()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var packableItemsGrid = form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var zToolStrip = form.FindSingle<ZToolStrip>(x => x.Name == "zToolStrip");
				var deleteStripButton = zToolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "Delete_StripButton");
				AssertEquals(1, packingList.PackageJob.Packages.Count);
				AssertEquals(2, package.PackableItemRelataions.Count);

				packableItemsGrid.Select(1);
				deleteStripButton.PerformClick();
				AssertEquals("The selected pack item record cannot be deleted because it is the last item of the corresponding invoice line. An invoice line must have at least one pack item record.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, package.PackableItemRelataions.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				var packageCusPackableItemRelation = package.PackableItemRelataions[1];
				var newPackableItem = (CusPackableItem)packageCusPackableItemRelation.PackableItem.Clone();
				package.PackableItemRelataions.Add(new CusPackageCusPackableItemRelation(package, newPackableItem));
				AssertEquals(3, package.PackableItemRelataions.Count);

				packableItemsGrid.UnSelectAll();
				packableItemsGrid.Select(1);
				packableItemsGrid.Select(2);
				deleteStripButton.PerformClick();
				AssertEquals("The selected pack item record cannot be deleted because it is the last item of the corresponding invoice line. An invoice line must have at least one pack item record.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, package.PackableItemRelataions.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				packableItemsGrid.UnSelectAll();
				packableItemsGrid.Select(0);
				packableItemsGrid.Select(2);
				deleteStripButton.PerformClick();
				AssertEquals("The selected pack item record cannot be deleted because it is the last item of the corresponding invoice line. An invoice line must have at least one pack item record.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, package.PackableItemRelataions.Count);

				UnitTestUserNotification.Instance.ClearMessages();
				packableItemsGrid.UnSelectAll();
				packableItemsGrid.Select(2);
				deleteStripButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, package.PackableItemRelataions.Count);
			}
		}

		public void TestCheckAndSplitPackableItems()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Line 1";
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line 2";
			invoiceLine2.JI_CustomsQuantity = 200m;
			invoiceLine2.JI_InvoiceUQ = "CTN";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Line 3";
			invoiceLine3.JI_CustomsQuantity = 300m;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packingList.CUL_Remarks = "Remarks 1";
			packingList.CUL_PackingListNumber = "PACK#1";
			packingList.CUL_PackingListDate = ZDate.Today;
			var packages1 = packingList.PackageJob.Packages.AddNew();
			var packages2 = packingList.PackageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10m;
			packagesR1.PackableUQ = "PCE";
			packagesR1.NetWeight = 100m;
			packagesR1.NetWeightUQ = "LB";
			packagesR1.InvoiceLineNetWeight = 200m;
			packagesR1.NetWeightUQ = "LB";
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10m;
			packagesR2.PackableUQ = "PCE";
			packagesR2.NetWeight = 200m;
			packagesR2.NetWeightUQ = "LB";
			packagesR2.InvoiceLineNetWeight = 400m;
			packagesR2.NetWeightUQ = "LB";
			var packagesR3 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(2);
			packagesR3.PackableQuantity = 10m;
			packagesR3.PackableUQ = "PCE";
			packagesR3.NetWeight = 300m;
			packagesR3.NetWeightUQ = "LB";
			packagesR3.InvoiceLineNetWeight = 600m;
			packagesR3.NetWeightUQ = "LB";
			Factory.Save();

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var packableItemsGrid = form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var zToolStrip = form.FindSingle<ZToolStrip>(x => x.Name == "zToolStrip");
				var splitStripButton = zToolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "Split_StripButton");
				packableItemsGrid.Select(1);
				packableItemsGrid.Select(2);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				splitStripButton.PerformClick();
				AssertEquals("Error Up to one item can be split at a time.", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				packableItemsGrid.UnSelectAll();
				splitStripButton.PerformClick();
				AssertEquals("Error No item is selected. Please select an item to split.", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				packableItemsGrid.Select(2);
				var selectedPackableItem = packableItemsGrid.GetSelectedRows().Cast<CusPackageCusPackableItemRelation>().Single().PackableItem;
				packages1.CustomsPackItem(selectedPackableItem, 10m);
				splitStripButton.PerformClick();
				AssertEquals("Error A packed item cannot be split. Please unpack the item first.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSplitPackableItems()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Line 1";
			invoiceLine1.JI_CustomsQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_NetWeight = 200;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line 2";
			invoiceLine2.JI_CustomsQuantity = 200;
			invoiceLine2.JI_InvoiceUQ = "CTN";
			invoiceLine1.JI_NetWeight = 400;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Line 3";
			invoiceLine3.JI_CustomsQuantity = 300;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_NetWeight = 600;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			Factory.Save();

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packingList.CUL_Remarks = "Remarks 1";
			packingList.CUL_PackingListNumber = "PACK#1";
			packingList.CUL_PackingListDate = ZDate.Today;
			var packages1 = packingList.PackageJob.Packages.AddNew();
			var packages2 = packingList.PackageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10;
			packagesR1.PackableUQ = "PCE";
			packagesR1.InvoiceLineNetWeight = 20;
			packagesR1.PackableUQ = Core.Constants.Weight.Kilograms;
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10;
			packagesR2.PackableUQ = "PCE";
			packagesR2.InvoiceLineNetWeight = 20;
			packagesR2.PackableUQ = Core.Constants.Weight.Kilograms;
			var packagesR3 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(2);
			packagesR3.PackableQuantity = 10;
			packagesR3.PackableUQ = "PCE";
			packagesR3.InvoiceLineNetWeight = 20;
			packagesR3.PackableUQ = Core.Constants.Weight.Kilograms;
			Factory.Save();

			using (var packingListForm = new PackingListForm(packingList))
			{
				packingListForm.Show();
				var packableItemsGrid = packingListForm.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var zToolStrip = packingListForm.FindSingle<ZToolStrip>(x => x.Name == "zToolStrip");
				var splitStripButton = zToolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "Split_StripButton");
				AssertEquals(3, packableItemsGrid.List.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var form = (ZForm)f;
					var packableItemsSplitter = (PackableItemsSplitter)form.BusinessEntity;
					var part1 = packableItemsSplitter.PackableItemParts.AddNew();
					part1.GoodsDescription = "Part 1";
					part1.PackableQuantity = 101;
					part1.PackableUQ = "CTN";
					var part2 = packableItemsSplitter.PackableItemParts.AddNew();
					part2.GoodsDescription = "Part 2";
					part2.PackableQuantity = 143;
					part2.PackableUQ = "CTN";
					var part3 = packableItemsSplitter.PackableItemParts.AddNew();
					part3.GoodsDescription = "Part 3";
					part3.PackableQuantity = 98;
					part3.PackableUQ = "PCE";
				});
				packableItemsGrid.Select(1);
				splitStripButton.PerformClick();
				AssertEquals(5, packableItemsGrid.List.Count);
				var row1 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[0];
				var row2 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[1];
				var row3 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[2];
				var row4 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[3];
				var row5 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[4];
				AssertEquals((ZShort)1, row1.Sequence);
				AssertEquals("Line 1", row1.GoodsDescription);
				AssertEquals((ZShort)2, row2.Sequence);
				AssertEquals("Part 1", row2.GoodsDescription);
				AssertEquals((ZShort)3, row3.Sequence);
				AssertEquals("Part 2", row3.GoodsDescription);
				AssertEquals((ZShort)4, row4.Sequence);
				AssertEquals("Part 3", row4.GoodsDescription);
				AssertEquals((ZShort)5, row5.Sequence);
				AssertEquals("Line 3", row5.GoodsDescription);
			}
		}

		public void TestPackageDescriptionLongTextControlBindingMember()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var packageDescriptionLongTextControl = control.FindSingle<LongTextControl>(x => x.Name == "PackageDescriptionLongTextControl");
				AssertEquals("CUL_PackageDescription", control.BindingSource.GetBindingMember(packageDescriptionLongTextControl));
			}
		}

		public void TestCUL_RemarksLongTextBoxBindingMember()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var remarksLongTextBox = control.FindSingle<LongTextControl>(x => x.Name == "CUL_RemarksLongTextBox");
				AssertEquals("CUL_Remarks", control.BindingSource.GetBindingMember(remarksLongTextBox));
			}
		}

		public void TesCUL_DescriptionLongTextBoxBindingMember()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var descriptionLongTextBox = control.FindSingle<LongTextControl>(x => x.Name == "CUL_DescriptionLongTextBox");
				AssertEquals("CUL_Description", control.BindingSource.GetBindingMember(descriptionLongTextBox));
			}
		}

		public void TestRemarksCharacterCasingSetting()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var remarksLongTextBox = control.FindSingle<LongTextControl>(x => x.Name == "CUL_RemarksLongTextBox");
				AssertEquals(CharacterCasing.Normal, remarksLongTextBox.CharacterCasing);
			}
		}

		public void TestTotalPackedQtyTextWhenPackedQtyChange()
		{
			var factory = new BusinessObjectFactory();
			var loadPackingList = factory.Load(typeof(CusPackingList), packingList.PK) as CusPackingList;
			var loadPackage = loadPackingList.PackageJob.Packages.Cast<CusPackage>().First();
			var loadPackageCusPackableItemRelation = loadPackage.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			var loadPackableItem = loadPackageCusPackableItemRelation.PackableItem;

			using (var form = new PackingListForm(loadPackingList))
			{
				form.Show();
				var totalPackedQtyInfo = packingList.TotalPackedQtyInfo;
				var packableItemsGrid = form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var zToolStrip = form.FindSingle<ZToolStrip>(x => x.Name == "zToolStrip");
				var deleteStripButton = zToolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "Delete_StripButton");
				var totalPackedQtyTextBox = form.FindSingle<ZTextBox>(x => x.Name == "TotalPackedQtyTextBox");
				AssertEquals("5.00 BBK", totalPackedQtyTextBox.Text);

				loadPackageCusPackableItemRelation.PackedQty = 6m;
				AssertEquals("6.00 BBK", totalPackedQtyTextBox.Text);

				loadPackageCusPackableItemRelation.PackableUQ = "BAG";
				AssertEquals("6.00 BAG", totalPackedQtyTextBox.Text);

				var newPackableItem = (CusPackableItem)loadPackableItem.Clone();
				var newPackageCusPackableItemRelation = new CusPackageCusPackableItemRelation(loadPackage, newPackableItem);
				loadPackage.PackableItemRelataions.Add(newPackageCusPackableItemRelation);
				newPackableItem.CUI_ClusterKey = loadPackableItem.CUI_ClusterKey;
				newPackableItem.CUI_PackableQty = 10m;
				newPackableItem.CUI_PackableUQ = "BBK";
				loadPackage.CustomsPackItem(newPackableItem, 3m);
				AssertEquals("6.00 BAG, 3.00 BBK", totalPackedQtyTextBox.Text);

				loadPackage.CustomsPackItem(newPackableItem, 1m);
				AssertEquals("6.00 BAG, 4.00 BBK", totalPackedQtyTextBox.Text);

				loadPackage.CustomsUnpackItem(newPackableItem, 2m);
				AssertEquals("6.00 BAG, 2.00 BBK", totalPackedQtyTextBox.Text);

				packableItemsGrid.Select(2);
				deleteStripButton.PerformClick();
				AssertEquals("6.00 BAG", totalPackedQtyTextBox.Text);
			}
		}

		public void TestTotalNWCalcDropEditWhenNetWeightChanged()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				package.KP_WeightUQ = "KG";
				var totalPackedQtyInfo = packingList.TotalPackedQtyInfo;
				var packableItemsGrid = form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var totalNWCalcDropEdit = form.FindSingle<ZCalcDropEdit>(x => x.Name == "TotalNWCalcDropEdit");
				var totalNWAmountCalcEdit = totalNWCalcDropEdit.FindSingle<ZCalcEdit>(x => x.Name == "AmountCalcEdit");
				AssertEquals("0.000", totalNWAmountCalcEdit.Text);
				package.NetWeight = 5m;

				AssertEquals("5.000", totalNWAmountCalcEdit.Text);

				package.KP_WeightUQ = "G";
				AssertEquals("0.005", totalNWAmountCalcEdit.Text);
			}
		}

		public void TestTotalGWCalcDropEditWhenGrossWeightChanged()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				package.KP_WeightUQ = "KG";
				var totalPackedQtyInfo = packingList.TotalPackedQtyInfo;
				var packableItemsGrid = form.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var totalGWCalcDropEdit = form.FindSingle<ZCalcDropEdit>(x => x.Name == "TotalGWCalcDropEdit");
				var totalGWAmountCalcEdit = totalGWCalcDropEdit.FindSingle<ZCalcEdit>(x => x.Name == "AmountCalcEdit");
				AssertEquals("0.000", totalGWAmountCalcEdit.Text);
				package.KP_Weight = 5m;

				AssertEquals("5.000", totalGWAmountCalcEdit.Text);

				package.KP_WeightUQ = "G";
				AssertEquals("0.005", totalGWAmountCalcEdit.Text);
			}
		}

		public void TestQuickPackMenuItem()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var packableItemsGrid = control.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				Assert("QuickPackMenuItem must be visible", packableItemsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "quickPackMenuItem").Visible);
			}
		}

		public void TestResetInvoiceLineValuesMenuItemClick()
		{
			using (var form = new PackingListForm(packingList))
			{
				var warning = @"This will remove the changes you have made to this packing list item.
The packing list item values, including Goods Description, Packable Quantity, and Packable Quantity Unit, will be reset using the current invoice line values.
Do you want to proceed ?";

				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var packableItemsGrid = control.FindSingle<ZGrid>(x => x.Name == "PackableItemsGrid");
				var resetInvoiceLineValuesMenuItem = packableItemsGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Name == "resetInvoiceLineValuesMenuItem");
				Assert("ResetInvoiceLineValuesMenuItem must be visible", resetInvoiceLineValuesMenuItem.Visible);

				resetInvoiceLineValuesMenuItem.PerformClick();
				AssertEquals("Please select at least one pack item.", UnitTestUserNotification.Instance.LastMessage.Text);
				packableItemsGrid.Select(1);
				var packableItemRelataion1 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[0];
				var packableItemRelataion2 = (CusPackageCusPackableItemRelation)packableItemsGrid.List[1];
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				resetInvoiceLineValuesMenuItem.PerformClick();
				AssertEquals(warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GoodsDescription1", packableItemRelataion1.GoodsDescription);
				AssertEquals(10m, packableItemRelataion1.PackableQuantity);
				AssertEquals("BBK", packableItemRelataion1.PackableUQ);

				AssertEquals("GoodsDescription2", packableItemRelataion2.GoodsDescription);
				AssertEquals(9m, packableItemRelataion2.PackableQuantity);
				AssertEquals("ADT", packableItemRelataion2.PackableUQ);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				resetInvoiceLineValuesMenuItem.PerformClick();
				AssertEquals(warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("GoodsDescription1", packableItemRelataion1.GoodsDescription);
				AssertEquals(10m, packableItemRelataion1.PackableQuantity);
				AssertEquals("BBK", packableItemRelataion1.PackableUQ);

				AssertEquals("test Description2", packableItemRelataion2.GoodsDescription);
				AssertEquals(4m, packableItemRelataion2.PackableQuantity);
				AssertEquals("CAK", packableItemRelataion2.PackableUQ);
			}
		}

		public void TestPackableItemsGridColumnsDecimals()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var grid = control.PackableItemsGrid;
				var packableQuantityColumn = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.PackableQuantity);
				AssertEquals(3, packableQuantityColumn.Decimals);
				var notPackedQtyColumn = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.NotPackedQty);
				AssertEquals(3, notPackedQtyColumn.Decimals);
				var packedQtyColumn = (ZCalcEditColumnStyleInfo)grid.GetColumnStyle(CusPackageCusPackableItemRelation.Schema.PackedQty);
				AssertEquals(3, packedQtyColumn.Decimals);
			}
		}

		public void TestKP_MarksAndNumbersControlEnableAcceptEnterMultipleLines()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var grid = control.PackagesGrid;
				AssertEquals(typeof(ZMultiLineTextBoxColumnStyle), grid.GetColumnStyle(PkgPackage.Schema.KP_MarksAndNumbers).ColumnStyleType);
			}
		}

		public void TestPackagesGridCustomLabels()
		{
			var supplier1 = Factory.New<OrgHeader>();
			var supplier2 = Factory.New<OrgHeader>();

			declaration.JE_OH_Supplier = supplier1.PK;
			using (var form = new PackingListForm(packingList))
			{
				var customLabels1 = supplier1.CustomFormLabels.AddNew();
				customLabels1.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomAttribute1;
				customLabels1.OT_Caption = "TEST attr. 1";
				form.Show();

				var control = (PackingListDetailsUserControl)form.PackingListDetailsDynamicUserControl.HostedControl;
				var packagesGrid = control.PackagesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("TEST attr. 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomAttribute1));
					AssertEquals("Custom Attribute 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomAttribute2));
					AssertEquals("Custom Flag 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomFlag1));
					AssertEquals("Custom Flag 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomFlag2));
					AssertEquals("Custom Date 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDate1));
					AssertEquals("Custom Date 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDate2));
					AssertEquals("Custom Number 1 ", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDecimal1));
					AssertEquals("Custom Number 2 ", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDecimal2));
				});

				var customLabels2 = supplier2.CustomFormLabels.AddNew();
				customLabels2.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomAttribute2;
				customLabels2.OT_Caption = "TEST attr. 2";

				var customLabels3 = supplier2.CustomFormLabels.AddNew();
				customLabels3.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomFlag1;
				customLabels3.OT_Caption = "TEST flag 1";

				var customLabels4 = supplier2.CustomFormLabels.AddNew();
				customLabels4.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomFlag2;
				customLabels4.OT_Caption = "TEST flag 2";

				var customLabels5 = supplier2.CustomFormLabels.AddNew();
				customLabels5.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDate1;
				customLabels5.OT_Caption = "TEST date 1";

				var customLabels6 = supplier2.CustomFormLabels.AddNew();
				customLabels6.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDate2;
				customLabels6.OT_Caption = "TEST date 2";

				var customLabels7 = supplier2.CustomFormLabels.AddNew();
				customLabels7.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDecimal1;
				customLabels7.OT_Caption = "TEST number 1";

				var customLabels8 = supplier2.CustomFormLabels.AddNew();
				customLabels8.OT_FieldName = Core.Constants.CustomLabels.CusPackage.CustomDecimal2;
				customLabels8.OT_Caption = "TEST number 2";

				declaration.JE_OH_Supplier = supplier2.PK;
				CombineAssertions(() =>
				{
					AssertEquals("Custom Attribute 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomAttribute1));
					AssertEquals("TEST attr. 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomAttribute2));
					AssertEquals("TEST flag 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomFlag1));
					AssertEquals("TEST flag 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomFlag2));
					AssertEquals("TEST date 1", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDate1));
					AssertEquals("TEST date 2", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDate2));
					AssertEquals("TEST number 1 ", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDecimal1));
					AssertEquals("TEST number 2 ", packagesGrid.GetColumnCaption(CusPackage.Schema.CustomDecimal2));
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Test1234";
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "test Description1";
			invoiceLine1.JI_InvoiceQuantity = 5m;
			invoiceLine1.JI_InvoiceUQ = "ACR";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Test4321";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "test Description2";
			invoiceLine2.JI_InvoiceQuantity = 4m;
			invoiceLine2.JI_InvoiceUQ = "CAK";

			packingList = declaration.LoadOrCreateCusPackingList(Factory);
			package = packingList.PackageJob.Packages.AddNew();

			var packageCusPackableItemRelation1 = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			var packableItem1 = packageCusPackableItemRelation1.PackableItem;
			packableItem1.CUI_GoodsDescription = "GoodsDescription1";
			packableItem1.CUI_PackableQty = 10m;
			packableItem1.CUI_PackableUQ = "BBK";
			package.CustomsPackItem(packableItem1, 5m);

			var packageCusPackableItemRelation2 = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			var packableItem2 = packageCusPackableItemRelation2.PackableItem;
			packableItem2.CUI_GoodsDescription = "GoodsDescription2";
			packableItem2.CUI_PackableQty = 9m;
			packableItem2.CUI_PackableUQ = "ADT";
			Factory.Save();
		}

		CusPackingList packingList;
		CusPackage package;
		BaseJobDeclaration declaration;

		List<string> ExpectedDefaultColumnsForGrid
		{
			get
			{
				var columns = new List<string>();
				columns.Add(CusPackage.Schema.KP_Sequence);
				columns.Add(CusPackage.Schema.KP_MarksAndNumbers);
				columns.Add(CusPackage.Schema.KP_GoodsDescription);
				columns.Add(CusPackage.Schema.KP_PackageQty);
				columns.Add(CusPackage.Schema.KP_F3_NKPackType);
				columns.Add(CusPackage.Schema.UnitNetWeight);
				columns.Add(CusPackage.Schema.NetWeight);
				columns.Add(CusPackage.Schema.KP_TareWeight);
				columns.Add(CusPackage.Schema.UnitGrossWeight);
				columns.Add(CusPackage.Schema.KP_Weight);
				columns.Add(CusPackage.Schema.KP_WeightUQ);
				columns.Add(CusPackage.Schema.KP_Length);
				columns.Add(CusPackage.Schema.KP_Width);
				columns.Add(CusPackage.Schema.KP_Height);
				columns.Add(CusPackage.Schema.KP_DimensionUQ);
				columns.Add(CusPackage.Schema.KP_Volume);
				columns.Add(CusPackage.Schema.KP_VolumeUQ);
				return columns;
			}
		}
	}
}
