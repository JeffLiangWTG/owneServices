using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsCarrierLabelsBatchPrintingSubscriberTest : WhsTestCaseWithFactory
	{
		public void TestGetCarrierLabelRequestBatches()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 28m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, "P-000001", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3, packages.Length);

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG1";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG2";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG3";
			Factory.Save();

			var orderToPackageItemNumber = new WhsOrderToPackageItemNumbers(order);
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(palletPackage, 1));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(casePackage, 2));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(splitCasePackage, 3));

			var labelSeparatorNumbers = new[] { 1, 2, 3 };

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var result = subscriber.SubscribePackages(pick, new[] { orderToPackageItemNumber }, labelSeparatorNumbers);
			AssertEquals("Subscription is successful.", true, result.Success);

			var batchedRequests = subscriber.GetCarrierLabelsBatchRequests.Cast<UniversalShipment>();
			AssertNotNull(batchedRequests);

			AssertEquals("There are 3 requests returned.", 3, batchedRequests.Count());
			AssertBatchedRequest(batchedRequests.ElementAt(0), order.WD_DocketID, 1, "PKG1");
			AssertBatchedRequest(batchedRequests.ElementAt(1), order.WD_DocketID, 2, "PKG2");
			AssertBatchedRequest(batchedRequests.ElementAt(2), order.WD_DocketID, 3, "PKG3");
		}

		public void TestGetCarrierLabelRequestBatches_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 23m);
			var order2 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick = helper.CreatePickNew(order1, order2);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order1.PackageJob, "P-000001", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = pick.OuterPackages;
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3, packages.Count);
			Factory.Save();

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG1";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG2";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG3";
			Factory.Save();

			var orderToPackageItemNumber1 = new WhsOrderToPackageItemNumbers(order1);
			orderToPackageItemNumber1.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(palletPackage, 1));
			orderToPackageItemNumber1.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(splitCasePackage, 3));

			var orderToPackageItemNumber2 = new WhsOrderToPackageItemNumbers(order2);
			orderToPackageItemNumber2.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(casePackage, 2));

			var labelSeparatorNumbers = new[] { 1, 2, 3 };

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var result = subscriber.SubscribePackages(pick, new[] { orderToPackageItemNumber1, orderToPackageItemNumber2 }, labelSeparatorNumbers);
			AssertEquals("Subscription is successful.", true, result.Success);

			var batchedRequests = subscriber.GetCarrierLabelsBatchRequests.Cast<UniversalShipment>();
			AssertNotNull(batchedRequests);

			AssertEquals("There are 3 requests returned.", 3, batchedRequests.Count());
			AssertBatchedRequest(batchedRequests.ElementAt(0), order1.WD_DocketID, 1, "PKG1");
			AssertBatchedRequest(batchedRequests.ElementAt(1), order2.WD_DocketID, 2, "PKG2");
			AssertBatchedRequest(batchedRequests.ElementAt(2), order1.WD_DocketID, 3, "PKG3");
		}

		void AssertBatchedRequest(UniversalShipment shipmentRequest, string expectedDataSourceKey, ZShort expectedItemNumber, string expectedPackLineReferenceNumber)
		{
			var dataSourceCollection = shipmentRequest.DataContext.DataSourceCollection;
			AssertEquals("DataSourceCollection count.", 1, dataSourceCollection.Count());

			var dataSource = dataSourceCollection.Single();
			AssertEquals("DataContextType", nameof(DataContextType.WarehouseOrder), dataSource.Type);
			AssertEquals("DataSourceKey", expectedDataSourceKey, dataSource.Key);

			var subShipment = shipmentRequest.SubShipmentCollection.Single();
			var packLine = subShipment.PackingLineCollection.Single();
			AssertEquals("PackLine Item No.", expectedItemNumber, packLine.ItemNo);
			AssertEquals("PackLine Reference No.", expectedPackLineReferenceNumber, packLine.ReferenceNumber);
		}

		public void TestGetCarrierLabelRequestBatches_NoCustomSegmentNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 28m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, "P-000001", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3, packages.Length);

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG1";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG2";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG3";
			Factory.Save();

			var orderToPackageItemNumber = new WhsOrderToPackageItemNumbers(order);
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(palletPackage, 1));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(casePackage, 2));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(splitCasePackage, 3));

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var result = subscriber.SubscribePackages(pick, new[] { orderToPackageItemNumber }, Array.Empty<int>());
			AssertEquals("Subscription is successful.", true, result.Success);

			var batchedRequests = subscriber.GetCarrierLabelsBatchRequests.Cast<UniversalShipment>();
			AssertNotNull(batchedRequests);

			AssertEquals("There's only 1 request returned.", 1, batchedRequests.Count());
			var shipmentRequest = batchedRequests.Single();
			var dataSourceCollection = shipmentRequest.DataContext.DataSourceCollection;
			AssertEquals("DataSourceCollection count.", 1, dataSourceCollection.Count());

			var dataSource = dataSourceCollection.Single();
			AssertEquals("DataContextType", nameof(DataContextType.WarehouseOrder), dataSource.Type);
			AssertEquals("DataSourceKey", order.WD_DocketID, dataSource.Key);

			var subShipment = shipmentRequest.SubShipmentCollection.Single();
			AssertEquals("There are 3 packing lines.", 3, subShipment.PackingLineCollection.Count);
			var packLine1 = subShipment.PackingLineCollection.Single(packLine => packLine.ItemNo == 1);
			AssertEquals("PackLine Reference No.", "PKG1", packLine1.ReferenceNumber);

			var packLine2 = subShipment.PackingLineCollection.Single(packLine => packLine.ItemNo == 2);
			AssertEquals("PackLine Reference No.", "PKG2", packLine2.ReferenceNumber);

			var packLine3 = subShipment.PackingLineCollection.Single(packLine => packLine.ItemNo == 3);
			AssertEquals("PackLine Reference No.", "PKG3", packLine3.ReferenceNumber);
		}

		public void TestGetCarrierLabelRequestBatches_AllPackagesIncluded()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pickLines = pick.GetAllPickLines();

			var nonEmptyPackage = packageJob.Packages.AddNew();
			nonEmptyPackage.KP_PackageID = "PKG1";
			var pickLine = pickLines.Single();
			packingHelper.CreatePackageDivot(nonEmptyPackage, pickLine);

			var emptyPackage = packageJob.Packages.AddNew();
			emptyPackage.KP_PackageID = "PKG2";
			Factory.Save();

			AssertEquals("Precondition", 2, pick.OuterPackages.Count);

			var orderToPackageItemNumber = new WhsOrderToPackageItemNumbers(order);
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(nonEmptyPackage, 1));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(emptyPackage, 2));

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var result = subscriber.SubscribePackages(pick, new[] { orderToPackageItemNumber }, Array.Empty<int>());
			AssertEquals("Subscription is successful.", true, result.Success);

			var batchedRequests = subscriber.GetCarrierLabelsBatchRequests.Cast<UniversalShipment>();
			AssertNotNull(batchedRequests);

			AssertEquals("There's only 1 request returned.", 1, batchedRequests.Count());
			var shipmentRequest = batchedRequests.Single();
			var dataSourceCollection = shipmentRequest.DataContext.DataSourceCollection;
			AssertEquals("DataSourceCollection count.", 1, dataSourceCollection.Count());

			var dataSource = dataSourceCollection.Single();
			AssertEquals("DataContextType", nameof(DataContextType.WarehouseOrder), dataSource.Type);
			AssertEquals("DataSourceKey", order.WD_DocketID, dataSource.Key);

			var subShipment = shipmentRequest.SubShipmentCollection.Single();
			AssertEquals("There are 2 packing lines.", 2, subShipment.PackingLineCollection.Count);
			var packLine1 = subShipment.PackingLineCollection.Single(packLine => packLine.ItemNo == 1);
			AssertEquals("PackLine Reference No.", "PKG1", packLine1.ReferenceNumber);

			var packLine2 = subShipment.PackingLineCollection.Single(packLine => packLine.ItemNo == 2);
			AssertEquals("PackLine Reference No.", "PKG2", packLine2.ReferenceNumber);
		}

		public void TestGetCarrierLabelRequestBatches_ErrorInSubscribing()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 28m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, "P-000001", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);

			var packages = order.PackageJob.GetAllPackagesOnJob();
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3, packages.Length);

			var palletPackage = packages.Single(package => package.KP_F3_NKPackType == "PLT");
			palletPackage.KP_PackageID = "PKG1";
			var casePackage = packages.Single(package => package.KP_F3_NKPackType == "CAS");
			casePackage.KP_PackageID = "PKG2";
			var splitCasePackage = packages.Single(package => package.KP_F3_NKPackType == "UNT");
			splitCasePackage.KP_PackageID = "PKG3";
			Factory.Save();

			var orderToPackageItemNumber = new WhsOrderToPackageItemNumbers(order);
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(palletPackage, 1));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(casePackage, 2));
			orderToPackageItemNumber.PackageItemNumbers.Add(new KeyValuePair<PkgPackage, int>(splitCasePackage, 4));

			var subscriber = ObjectFactory.Get<IWhsCarrierLabelsBatchPrintingSubscriber>(nameof(IWhsCarrierLabelsBatchPrintingSubscriber));
			var result = subscriber.SubscribePackages(pick, new[] { orderToPackageItemNumber }, Array.Empty<int>());

			AssertEquals("Subscription is not successful.", false, result.Success);
			AssertEquals("Subscription is not successful.", "ItemNo 3 missing.", result.Message);
		}
	}
}
