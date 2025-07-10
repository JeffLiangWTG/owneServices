using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderAutoPackAllLinesApplicator))]
	public class OrderAutoPackAllLinesApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			// setup orders
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser, Factory.New<IStmPrintQueue>().PK, 1);
			var order_NoPacks = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order_AlreadyPacked = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order_Packed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);

			// setup picks
			var pick_AlreadyPacked = Helper.CreatePickNew(order_AlreadyPacked);
			var pick_WithNoPacks = Helper.CreatePickNew(false, false, false, true, order_NoPacks);
			var pick_WithPacks = Helper.CreatePickNew(order_Packed);
			pick_AlreadyPacked.AutoAllocateItems();
			pick_WithPacks.AutoAllocateItems();

			// add packs to orders
			order_AlreadyPacked.PackageJob.Packages.AddNew();

			Factory.Save();

			var picks = new[] { order_NoPacks, order_AlreadyPacked, order_Packed };
			ApplyApplicator(picks, string.Format(
@"WARNING: Warehouse Order {0} [HL {0}] - cannot be Auto-Packed as it has not been Picked.
WARNING: Warehouse Order {1} [HL {1}] - has already been packed and was not auto-packed.
INFO: Warehouse Order {2} [HL {2}] - was successfully auto-packed.
".Trim(), order_NoPacks.WD_DocketID, order_AlreadyPacked.WD_DocketID, order_Packed.WD_DocketID));

			var printJobs = new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery());
			AssertEquals("There should be a print job for each package.", 2, printJobs.Length);
			AssertEquals("All Packages should have an ID Generated.", true, order_Packed.PackageJob.Packages.Count > 0 && order_Packed.PackageJob.Packages.All(p => !p.KP_PackageID.IsEmpty));
			AssertEquals("All Packages should be closed.", true, order_Packed.PackageJob.Packages.Count > 0 && order_Packed.PackageJob.Packages.All(p => p.IsClosed));

			foreach (var job in printJobs)
			{
				AssertEquals("The job's parent should match the packagejob.", order_Packed.PackageJob.PK, ((BusinessObject)job)[StmPrintJobSchema.Constants.SP_ParentGuid]);
			}
		}

		public void TestAction_WithoutPrinting()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			// setup orders
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser, Factory.New<IStmPrintQueue>().PK, 1);
			var order_NoPacks = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order_AlreadyPacked = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order_Packed = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);

			// setup picks
			var pick_AlreadyPacked = Helper.CreatePickNew(order_AlreadyPacked);
			var pick_WithNoPacks = Helper.CreatePickNew(false, false, false, true, order_NoPacks);
			var pick_WithPacks = Helper.CreatePickNew(order_Packed);
			pick_AlreadyPacked.AutoAllocateItems();
			pick_WithPacks.AutoAllocateItems();

			// add packs to orders
			order_AlreadyPacked.PackageJob.Packages.AddNew();

			Factory.Save();

			var picks = new[] { order_NoPacks, order_AlreadyPacked, order_Packed };

			using (WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ApplyApplicator(picks, string.Format(
	@"WARNING: Warehouse Order {0} [HL {0}] - cannot be Auto-Packed as it has not been Picked.
WARNING: Warehouse Order {1} [HL {1}] - has already been packed and was not auto-packed.
INFO: Warehouse Order {2} [HL {2}] - was successfully auto-packed.
".Trim(), order_NoPacks.WD_DocketID, order_AlreadyPacked.WD_DocketID, order_Packed.WD_DocketID), saveFactoryOnSuccess: true);
				AssertEquals("All Packages should have an ID Generated.", true, order_Packed.PackageJob.Packages.Count > 0 && order_Packed.PackageJob.Packages.All(p => !p.KP_PackageID.IsEmpty));
				AssertEquals("All Packages should be closed.", true, order_Packed.PackageJob.Packages.Count > 0 && order_Packed.PackageJob.Packages.All(p => p.IsClosed));
			}

			var newFactory = new BusinessObjectFactory();
			var printJobs = newFactory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("There should be no print jobs.", 0, printJobs.Length);

			var orderInNewFactory = newFactory.Load<WhsOrder>(order_Packed.PK);
			AssertEquals("Order should be packed.", 2, order_Packed.PackageJob.Packages.Count);
		}

		#endregion

		#region DBHits

		[TestDate(2023, 12, 11)]
		public void TestAction_DBHits_NoPacks()
		{
			var numberOfOrders = 10;

			var orders = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var warehouse = Helper.CreateWarehouse("W" + i);
				Factory.Save();

				var order = Helper.CreateWhsOrder(org, warehouse, "O" + i);
				Helper.CreatePickNew(false, false, false, true, order);
				orders.Add(order);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < numberOfOrders; i++)
			{
				orders[i] = newFactory.Load<WhsOrder>(orders[i].PK);
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, numberOfOrders },
				{ PkgPackageJobSchema.Constants.TableName, 1 }, // was 10
				{ WhsWarehouseSchema.Constants.TableName, 1 }, // was 10

				// without fetch hints these tables are never hit
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				SimulateRun(orders.Select(x => (BusinessObject)x).ToArray(), false);
			}
		}

		[TestDate(2023, 12, 11)]
		public void TestAction_DBHits_AlreadyPacked()
		{
			var numberOfOrders = 10;

			var orders = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var org = Helper.CreateClient("C" + i, "C" + i);
				var warehouse = Helper.CreateWarehouse("W" + i, "A", 1, 1);
				var part = Helper.CreateProduct(org, "P" + i);
				var receive = Helper.CreateWhsReceiveWithInventory(org, warehouse, "R" + i, part, 15m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(org, warehouse, "O" + i, part, 1m);
				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();
				order.PackageJob.Packages.AddNew();
				orders.Add(order);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < numberOfOrders; i++)
			{
				orders[i] = newFactory.Load<WhsOrder>(orders[i].PK);
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, numberOfOrders },
				{ PkgPackageSchema.Constants.TableName, 1 }, // was 19
				{ PkgPackageJobSchema.Constants.TableName, 1 }, // was 10
				{ WhsDocketLineSchema.Constants.TableName, 1 }, // was 10
				{ WhsPickSchema.Constants.TableName, 1 }, // was 10
				{ WhsWarehouseSchema.Constants.TableName, 1 }, // was 10

				// without fetch hints these tables are never hit
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				SimulateRun(orders.Select(x => (BusinessObject)x).ToArray(), false);
			}
		}

		[TestDate(2023, 12, 11)]
		public void TestAction_DBHits_Success()
		{
			Action_DBHits_Success(isPrinting: true);
		}

		[TestDate(2023, 12, 11)]
		public void TestAction_DBHits_Success_NoPrinting()
		{
			Action_DBHits_Success(isPrinting: false);
		}

		void Action_DBHits_Success(bool isPrinting)
		{
			var numberOfOrders = 10;

			var orders = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var org = Helper.CreateClient("C" + i, "C" + i);
				var consignee = Helper.CreateClient("CNE" + i, "CNE" + i);
				var warehouse = Helper.CreateWarehouse("W" + i, "A", 1, 1);
				var part = Helper.CreateProduct(org, "P" + i);
				var receive = Helper.CreateWhsReceiveWithInventory(org, warehouse, "R" + i, part, 15m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(org, warehouse, "O" + i, part, 1m);
				order.ConsigneePK = consignee.PK;
				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();
				orders.Add(order);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < numberOfOrders; i++)
			{
				orders[i] = newFactory.Load<WhsOrder>(orders[i].PK);
			}

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 12 }, // was 29 (Cant lower further as cache gets reset on Factory.save() in PackageIDGenerator.GenerateIDsCore)
				{ GlbBranchSchema.Constants.TableName, 1 }, // was 10
				{ JobDocAddressSchema.Constants.TableName, 1 }, // was 10
				{ OrgAddressSchema.Constants.TableName, isPrinting ? 2 : 1 }, // was 30
				{ OrgAddressCapabilitySchema.Constants.TableName, isPrinting ? 1 : 0 }, // was 10
				{ OrgContactSchema.Constants.TableName, isPrinting ? 11 : 0 }, // (Cant lower further as cache gets reset on Factory.save() in PackageIDGenerator.GenerateIDsCore)
				{ OrgCusCodeSchema.Constants.TableName, 1 }, // was 20
				{ OrgHeaderSchema.Constants.TableName, 1 }, // was 10
				{ OrgMiscServSchema.Constants.TableName, 1 }, // was 20
				{ OrgSupplierPartSchema.Constants.TableName, 1 }, // was 10
				{ PkgPackageSchema.Constants.TableName, isPrinting ? 10 : 1 }, // (Cant lower further as cache gets reset on Factory.save() in PackageIDGenerator.GenerateIDsCore)
				{ PkgPackageItemDivotSchema.Constants.TableName, isPrinting ? 20 : 10 }, // (Cant lower further as cache gets reset on Factory.save() in PackageIDGenerator.GenerateIDsCore)
				{ PkgPackageJobSchema.Constants.TableName, 1 }, // was 19
				{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1 }, // was 10
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, isPrinting ? 20 : 12 }, // Issue in workflow
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, isPrinting ? 50 : 40 }, // (Cant lower further as cache gets reset on Factory.save() in PackageIDGenerator.GenerateIDsCore)
				{ StmDefaultPrinterSchema.Constants.TableName, isPrinting ? 1 : 0 },
				{ StmEventSchema.Constants.TableName, 3 },
				{ StmMenuEDocsSchema.Constants.TableName, isPrinting ? 1 : 0 },
				{ StmMenuItemSchema.Constants.TableName, isPrinting ? 2 : 0 },
				{ StmMenuMenuPivotSchema.Constants.TableName, isPrinting ? 1 : 0 },
				{ StmMenuTemplatePivotSchema.Constants.TableName, isPrinting ? 1 : 0 },
				{ WhsDocketLineSchema.Constants.TableName, 2 }, // was 20
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 }, // was 10
				{ WhsWarehouseSchema.Constants.TableName, 1 }, // was 10
				{ OrgDocumentSchema.Constants.TableName, isPrinting ? 10 : 0 },
			};

			using (WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, !isPrinting))
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				SimulateRun(orders.Select(x => (BusinessObject)x).ToArray(), saveOnSuccess: !isPrinting);
			}
		}

		#endregion

		#region TestAction_ArithmeticOverflowOfPackageDimensions

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingPackageWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = "M";

			data.Part1.OP_Weight = 500m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Grams;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 100m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);

			// setup picks
			var pick_WithPacks = Helper.CreatePickNew(order1);
			pick_WithPacks.AutoAllocateItems();

			var pick_WithError = Helper.CreatePickNew(order2);
			pick_WithError.AutoAllocateItems();

			var pick3 = Helper.CreatePickNew(order3);
			pick3.AutoAllocateItems();

			Factory.Save();

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Length = 1m;
			packtype.F3_Width = 1m;
			packtype.F3_Height = 1m;

			packtype.F3_Weight = 100m;
			packtype.F3_UnitOfWeight = Core.Constants.Weight.Milligrams;
			packtype.F3_UnitOfDimension = Core.Constants.Length.Metres;

			Factory.Save();

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Weight.Milligrams))
			{
				var picks = new[] { order1, order2, order3 };

				ApplyApplicator(picks, string.Format(
	@"INFO: Warehouse Order {0} [HL {0}] - was successfully auto-packed.
ERROR: Warehouse Order {1} [HL {1}] - was not auto-packed.
Order {1} cannot be saved due to the following validation errors:
Error - KP_Weight: The number 50,000,100 is too large, the maximum value allowed for Gross Weight is 999,999.999.
".Trim(), order1.WD_DocketID, order2.WD_DocketID));

				AssertEquals("Should delete Packages on Failure.", 0, order2.PackageJob.Packages.Count);
			}

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingPackageVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = "M";

			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 10m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);

			// setup picks
			var pick_WithPacks = Helper.CreatePickNew(order1);
			pick_WithPacks.AutoAllocateItems();

			var pick_WithError = Helper.CreatePickNew(order2);
			pick_WithError.AutoAllocateItems();

			var pick3 = Helper.CreatePickNew(order3);
			pick3.AutoAllocateItems();

			Factory.Save();

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Length = 1m;
			packtype.F3_Width = 1m;
			packtype.F3_Height = 1m;

			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = Core.Constants.Weight.Kilograms;
			packtype.F3_UnitOfDimension = Core.Constants.Length.Metres;

			order1.WD_AddPalletWeightToOrder = false;
			order2.WD_AddPalletWeightToOrder = true;

			Factory.Save();

			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Volume.CubicCentimeters))
			{
				var picks = new[] { order1, order2, order3 };

				ApplyApplicator(picks, string.Format(
	@"INFO: Warehouse Order {0} [HL {0}] - was successfully auto-packed.
ERROR: Warehouse Order {1} [HL {1}] - was not auto-packed.
Order {1} cannot be saved due to the following validation errors:
Error - KP_Volume: The number 1,000,000 is too large, the maximum value allowed for Volume is 999,999.999.
".Trim(), order1.WD_DocketID, order2.WD_DocketID));

				AssertEquals("Should delete Packages on Failure.", 0, order2.PackageJob.Packages.Count);
			}

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingOrderWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = "M";

			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Grams;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 2m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 50m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);

			// setup picks
			var pick_WithPacks = Helper.CreatePickNew(order1);
			pick_WithPacks.AutoAllocateItems();

			var pick_WithError = Helper.CreatePickNew(order2);
			pick_WithError.AutoAllocateItems();

			var pick3 = Helper.CreatePickNew(order3);
			pick3.AutoAllocateItems();

			Factory.Save();

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Length = 1m;
			packtype.F3_Width = 1m;
			packtype.F3_Height = 1m;

			packtype.F3_Weight = 100m;
			packtype.F3_UnitOfWeight = Core.Constants.Weight.Kilograms;
			packtype.F3_UnitOfDimension = Core.Constants.Length.Metres;

			order1.WD_AddPalletWeightToOrder = true;
			order2.WD_AddPalletWeightToOrder = true;
			order2.WD_TotalWeightUnit = Core.Constants.Weight.Grams;

			Factory.Save();

			var picks = new[] { order1, order2, order3 };

			ApplyApplicator(picks, string.Format(
	@"INFO: Warehouse Order {0} [HL {0}] - was successfully auto-packed.
ERROR: Warehouse Order {1} [HL {1}] - was not auto-packed.
Order {1} cannot be saved due to the following validation errors:
Error - WD_WeightSent: The number 5,100,000 is too large, the maximum value allowed for Weight Sent is 999,999.999.
".Trim(), order1.WD_DocketID, order2.WD_DocketID));

			AssertEquals("Should delete Packages on Failure.", 0, order2.PackageJob.Packages.Count);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingOrderVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = "M";

			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 2m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 50m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);

			// setup picks
			var pick_WithPacks = Helper.CreatePickNew(order1);
			pick_WithPacks.AutoAllocateItems();

			var pick_WithError = Helper.CreatePickNew(order2);
			pick_WithError.AutoAllocateItems();

			var pick3 = Helper.CreatePickNew(order3);
			pick3.AutoAllocateItems();

			Factory.Save();

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Length = 1000m;
			packtype.F3_Width = 10000m;
			packtype.F3_Height = 10000m;

			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = Core.Constants.Weight.Kilograms;
			packtype.F3_UnitOfDimension = Core.Constants.Length.Centimetres;

			order2.UsePackingWeightAndVolume = true;

			Factory.Save();

			var picks = new[] { order1, order2, order3 };

			ApplyApplicator(picks, string.Format(
	@"INFO: Warehouse Order {0} [HL {0}] - was successfully auto-packed.
ERROR: Warehouse Order {1} [HL {1}] - was not auto-packed.
Order {1} cannot be saved due to the following validation errors:
Error - WD_CubicSent: The number 5,000,000 is too large, the maximum value allowed for Volume Sent is 999,999.999.
".Trim(), order1.WD_DocketID, order2.WD_DocketID));

			AssertEquals("Should delete Packages on Failure.", 0, order2.PackageJob.Packages.Count);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
		}

		#endregion

		#region TestAction_WhenConsigneeDoesNotHaveAutoPackEnabled

		public void TestAction_WhenConsigneeDoesNotHaveAutoPackEnabled()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			// setup orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			order.Consignee.MiscServ.OM_IsAutoPackAllowed = false;
			Factory.Save();

			ApplyApplicator(new[] { order }, @"WARNING: Warehouse Order W00000002 [HL W00000002] - was not auto-packed.
Order W00000002 cannot be Auto-Packed as it is not enabled on Consignee 111.");
		}

		#endregion

		#region TestAction_WhenPickIsReadyForPlanning

		public void TestAction_WhenPickIsReadyForPlanning()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			// setup orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			ApplyApplicator(new[] { order }, @"WARNING: Warehouse Order W00000002 [HL W00000002] - was not auto-packed.
Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.");
		}

		#endregion

		#region TestAction_WhenPickIsPlanned

		public void TestAction_WhenPickIsPlanned()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			// setup orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			ApplyApplicator(new[] { order }, @"WARNING: Warehouse Order W00000002 [HL W00000002] - was not auto-packed.
Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.");
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
