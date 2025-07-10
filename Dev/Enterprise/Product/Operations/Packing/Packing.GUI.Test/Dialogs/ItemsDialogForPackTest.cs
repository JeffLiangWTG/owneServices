using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	#region ItemsDialogForPackTest

	[TestedType(typeof(PackOrUnpackItemsDialog))]
	public class ItemsDialogForPackTest : PackingZFormBasherTest
	{
		#region TestFormHeading

		[RequiresSTA]
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Pack...", form.Text);
			}
		}

		#endregion

		#region TestScanOnPackingScreen

		#region TestScanOnPackingScreen_WithoutFocussingOnColumns

		[RequiresSTA]
		public void TestScanOnPackingScreen_WithoutFocussingOnColumns()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>());

			using (var form = new PackOrUnpackItemsDialog(bizO, false, false))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				AssertNoExceptionThrown(() => SendKeys(form, Keys.Control | Keys.L, Keys.A, Keys.Control | Keys.L));
			}
		}

		#endregion

		#region TestScanOnPackingScreen_WithFocussingOnColumns

		[RequiresSTA]
		public void TestScanOnPackingScreen_WithFocussingOnColumns()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>());

			using (var form = new PackOrUnpackItemsDialog(bizO, false, false))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				gridControl.Select(0);
				gridControl.CurrentCell = new DataGridCell(0, 2);
				AssertNoExceptionThrown(() => SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L));
			}
		}

		#endregion

		#endregion

		#region TestPacking

		#region TestPacking_ScanInScanAllMode

		public void TestPacking_ScanInScanAllMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var bizO = new PackItemsViaScanBusinessObject("P1", packageJob, new[] { firstBatchRedShirt, secondBatchRedShirt, thirdBatchWhiteShirt }, null, false);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, false))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };

				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the Item Attribute or select it in the Grid and click Pack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Pack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
			}
		}

		#endregion

		#region TestPacking_ScanInScanQuantityMode

		public void TestPacking_ScanInScanQuantityMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var bizO = new PackItemsViaScanBusinessObject("P1", packageJob, new[] { firstBatchRedShirt, secondBatchRedShirt, thirdBatchWhiteShirt }, null, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Type in the Quantity to Pack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Pack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Type in the Quantity to Pack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Pack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Pack.", messageLabel.Text);

				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedPackQty; };
				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		#endregion

		#region TestPacking_ScanInScanQuantityModeForOneItemToPack

		public void TestPacking_ScanInScanQuantityModeForOneItemToPack()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var bizO = new PackItemsViaScanBusinessObject("P1", packageJob, new[] { firstBatchRedShirt }, null, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Enter or Scan the Quantity to Pack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.Control | Keys.L); // Invalid character scanned
				AssertEquals("Quantity scanned is invalid. Enter or Scan valid Quantity to Pack.", messageLabel.Text);

				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedPackQty; };
				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		#endregion

		#region TestPacking_TUNAttributeQtyMode

		public void TestPacking_ScanTUNAttributeQtyMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var bizO = new PackItemsViaScanBusinessObject("TUNP1", packageJob, new[] { firstBatchRedShirt, secondBatchRedShirt, thirdBatchWhiteShirt }, null, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Scan the Item Attribute or select it in the Grid and click Pack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Pack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Pack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2, bizO.PackageQtyToCreate);
			}
		}

		#endregion

		#region TestPacking_TUNQtyMode

		[RequiresSTA]
		public void TestPacking_TUNQtyMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "", 0);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var bizO = new PackItemsViaScanBusinessObject("TUNP1", packageJob, new[] { firstBatchRedShirt }, null, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Enter or Scan the PLT Quantity to Pack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2, bizO.PackageQtyToCreate);
			}
		}

		#endregion

		#endregion

		#region TestUnpacking

		#region TestUnPacking_ScanInScanAllModeClosesTheForm

		public void TestUnPacking_ScanInScanAllModeClosesTheForm()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);

			var packedItems = new[]
			{
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(firstBatchRedShirt, 4m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(secondBatchRedShirt, 2m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(thirdBatchWhiteShirt, 2m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, false);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, false))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };

				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
			}
		}

		[RequiresSTA]
		public void TestUnPacking_ScanInScanAllModeClosesTheForm_MultipleDivots()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var package1 = packageJob.Packages.AddNew("UNT");
			var package2 = packageJob.Packages.AddNew("UNT");
			var package3 = packageJob.Packages.AddNew("UNT");

			var packedItems = new[]
			{
				package1.Pack_ForTesting(firstBatchRedShirt, 2m),
				package1.Pack_ForTesting(firstBatchRedShirt, 2m),
				package2.Pack_ForTesting(secondBatchRedShirt, 1m),
				package2.Pack_ForTesting(secondBatchRedShirt, 1m),
				package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m),
				package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, false);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, false))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };

				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
			}
		}

		#endregion

		#region TestUnPacking_ScanInScanQuantityMode

		[RequiresSTA]
		public void TestUnPacking_ScanInScanQuantityMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var packedItems = new[]
			{
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(firstBatchRedShirt, 4m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(secondBatchRedShirt, 2m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(thirdBatchWhiteShirt, 2m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Type in the Quantity to Unpack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Type in the Quantity to Unpack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);

				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedRemoveQty; };
				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		public void TestUnPacking_ScanInScanQuantityMode_MultipleDivots()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var package1 = packageJob.Packages.AddNew("UNT");
			var package2 = packageJob.Packages.AddNew("UNT");
			var package3 = packageJob.Packages.AddNew("UNT");

			var packedItems = new[]
			{
				package1.Pack_ForTesting(firstBatchRedShirt, 2m),
				package1.Pack_ForTesting(firstBatchRedShirt, 2m),
				package2.Pack_ForTesting(secondBatchRedShirt, 1m),
				package2.Pack_ForTesting(secondBatchRedShirt, 1m),
				package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m),
				package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Type in the Quantity to Unpack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Type in the Quantity to Unpack. Alternatively, scan attributes until there is a single line, then scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);

				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedRemoveQty; };
				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		#endregion

		#region TestUnPacking_ScanInScanQuantityModeForOneItemToUnPack

		public void TestUnPacking_ScanInScanQuantityModeForOneItemToUnPack()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var packedItems = new[]
			{
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(firstBatchRedShirt, 4m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedRemoveQty; };

				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		[RequiresSTA]
		public void TestUnPacking_ScanInScanQuantityModeForOneItemToUnPack_MultipleDivots()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();
			var firstBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLine(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var package = packageJob.Packages.AddNew("UNT");
			var packedItems = new[]
			{
				package.Pack_ForTesting(firstBatchRedShirt, 2m),
				package.Pack_ForTesting(firstBatchRedShirt, 2m)
			};
			var barcodeMatch = new BarcodeMatch(true);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();
				decimal quantityScanned = 0m;
				form.FormClosing += delegate
				{ quantityScanned = ((PackableItemParentWrapper)gridControl.List[0]).ProposedRemoveQty; };

				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(2.5m, quantityScanned);
			}
		}

		#endregion

		#region TestUnPacking_TUNAttributeQtyMode

		public void TestUnPacking_TUNAttributeQtyMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var packedItems = new[]
			{
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(firstBatchRedShirt, 4m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(secondBatchRedShirt, 2m),
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(thirdBatchWhiteShirt, 2m)
			};
			var barcodeMatch = new BarcodeMatch(true, "PLT", 1m);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);

				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Scan the Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9, Keys.NumPad9,
					Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals($"The number '999999999999.5' you have scanned is too large. The maximum value allowed is '{Int32.MaxValue}'.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad1, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(1, bizO.PackageQtyToCreate);
			}
		}

		public void TestUnPacking_TUNAttributeQtyMode_MultipleDivots()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "Red", 1);
			var secondBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "Red", 2);
			var thirdBatchWhiteShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 2m, "White", 3);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var package1 = packageJob.Packages.AddNew("UNT");
			var package2 = packageJob.Packages.AddNew("UNT");
			var package3 = packageJob.Packages.AddNew("UNT");

			var packedItem1 = package1.Pack_ForTesting(firstBatchRedShirt, 2m);
			var packedItem2 = package1.Pack_ForTesting(firstBatchRedShirt, 2m);
			AssertEquals("PackedItem should be same", packedItem1, packedItem2);

			var packedItem3 = package2.Pack_ForTesting(secondBatchRedShirt, 1m);
			var packedItem4 = package2.Pack_ForTesting(secondBatchRedShirt, 1m);
			AssertEquals("PackedItem should be same", packedItem3, packedItem4);

			var packedItem5 = package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m);
			var packedItem6 = package3.Pack_ForTesting(thirdBatchWhiteShirt, 1m);
			AssertEquals("PackedItem should be same", packedItem5, packedItem6);

			var packedItems = new[]
			{
				packedItem1,
				packedItem3,
				packedItem5
			};
			var barcodeMatch = new BarcodeMatch(true, "PLT", 1m);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);

				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Scan the Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.R, Keys.E, Keys.D, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals("Scan the next Item Attribute or select it in the Grid and click Unpack.", messageLabel.Text);
				AssertEquals(2, gridControl.List.Count);

				SendKeys(form, Keys.Control | Keys.L, Keys.NumPad1, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals(1, gridControl.List.Count);
				AssertEquals("Enter or Scan the Quantity to Unpack.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad1, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(1, bizO.PackageQtyToCreate);
			}
		}

		#endregion

		#region TestUnPacking_TUNQtyMode

		[RequiresSTA]
		public void TestUnPacking_TUNQtyMode()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "Red", 1);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var packedItems = new[]
			{
				packageJob.Packages.AddNew("UNT").Pack_ForTesting(firstBatchRedShirt, 4m)
			};
			var barcodeMatch = new BarcodeMatch(true, "PLT", 1m);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Enter or Scan the PLT Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad2, Keys.NumPad1, Keys.NumPad4, Keys.NumPad7, Keys.NumPad4, Keys.NumPad8, Keys.NumPad3, Keys.NumPad6, Keys.NumPad4, Keys.NumPad8,
					Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(false, isFormClosed);
				AssertEquals($"The number '2147483648.5' you have scanned is too large. The maximum value allowed is '{Int32.MaxValue}'.", messageLabel.Text);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad1, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(1, bizO.PackageQtyToCreate);
			}
		}

		public void TestUnPacking_TUNQtyMode_MultipleDivots()
		{
			var bizOWithPacking = Factory.New<DummyWithPacking>();

			var firstBatchRedShirt = CreatePackLineForTUN(bizOWithPacking, "Shirt", 4m, "Red", 1);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(bizOWithPacking);
			var package = packageJob.Packages.AddNew("UNT");

			var packedItem1 = package.Pack_ForTesting(firstBatchRedShirt, 2m);
			var packedItem2 = package.Pack_ForTesting(firstBatchRedShirt, 2m);
			AssertEquals("PackedItem should be same", packedItem1, packedItem2);

			var packedItems = new[]
			{
				packedItem1
			};

			var barcodeMatch = new BarcodeMatch(true, "PLT", 1m);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(packageJob, packedItemsWithBarcodes, true);

			using (var form = new PackOrUnpackItemsDialog(bizO, true, true))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ShowDialogWithoutDispose(form);
				var isFormClosed = false;
				form.FormClosed += delegate
				{ isFormClosed = true; };
				var messageLabel = (ZLabel)form.Controls.Find("MsgLabel", true).Single();
				var gridControl = (ItemsGrid)form.Controls.Find("Grid", true).Single();

				AssertEquals("Enter or Scan the PLT Quantity to Unpack.", messageLabel.Text);
				AssertEquals(false, isFormClosed);

				SendKeys(form, Keys.Control | Keys.L, Keys.Q, Keys.T, Keys.NumPad1, Keys.OemPeriod, Keys.Z, Keys.Space, Keys.NumPad5, Keys.Control | Keys.L);
				AssertEquals(true, isFormClosed);
				AssertEquals(1, bizO.PackageQtyToCreate);
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, Array.Empty<PkgPackage>());
			return new PackOrUnpackItemsDialog(bizO, isScanPacking: false, isUserEnteringQty: false);
		}

		static IDummyPackableItemParent CreatePackLine(DummyWithPacking bizOWithPacking, string productDescription, decimal quantity, string code, int number)
		{
			var line = bizOWithPacking.Lines.AddNew();
			line.Barcode = "P1";
			line.Description = productDescription;
			line.TotalQty = quantity;
			line.TotalQtyUQ = "UNT";
			line.AutoPackQtyPerPackage = 1;
			line.AutoPackPackageType = "UNT";
			line.ZD1_Code = code;
			line.ZD1_Number = number;

			return line;
		}

		static IDummyPackableItemParent CreatePackLineForTUN(DummyWithPacking bizOWithPacking, string productDescription,
			decimal quantity, string code, int number)
		{
			var line = bizOWithPacking.Lines.AddNew();
			line.BarcodeTUN = "TUNP1";
			line.Description = productDescription;
			line.TotalQty = quantity;
			line.TotalQtyUQ = "UNT";
			line.AutoPackQtyPerPackage = 1;
			line.AutoPackPackageType = "PLT";
			line.BarcodeTUNPackQty = 1;
			line.BarcodeTUNPackType = "PLT";
			line.ZD1_Code = code;
			line.ZD1_Number = number;

			return line;
		}

		void SendKeys(Control control, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.SendKeyDown(control, control.Handle, key);
			}
		}

		#endregion
	}

	#endregion

	#region ItemsDialogForRemoveTest

	[TestedType(typeof(PackOrUnpackItemsDialog))]
	public class ItemsDialogForRemoveTest : PackingZFormBasherTest
	{
		#region TestFormHeading

		[RequiresSTA]
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals("Unpack...", form.Text);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			Data.CreatePackingData();
			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), Array.Empty<PkgPackage>());

			return new PackOrUnpackItemsDialog(bizO, isScanPacking: false, isUserEnteringQty: false);
		}

		#endregion
	}

	#endregion
}
