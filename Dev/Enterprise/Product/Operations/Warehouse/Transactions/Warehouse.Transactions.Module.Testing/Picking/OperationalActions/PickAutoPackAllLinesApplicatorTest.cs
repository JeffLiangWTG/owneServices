using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickAutoPackAllLinesApplicator))]
	public class PickAutoPackAllLinesApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			// setup data and receive
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			// setup orders
			var order_NoPackableItems = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OO", data.Part2, 100m);
			var order_AlreadyPacked = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order_Packed1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			var order_Packed2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m);
			var order_Packed3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 4m);
			var order_AutoPackDisabled = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			order_AutoPackDisabled.ConsigneePK = Helper.CreateClient("Consignee 2").PK;
			order_AutoPackDisabled.Consignee.MiscServ.OM_IsAutoPackAllowed = false;
			var order_Packed4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 6m);
			var order_Packed5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O7", data.Part1, 7m);
			Factory.Save();

			// setup picks
			var pick_AlreadyPacked = Helper.CreatePickNew(order_AlreadyPacked);
			var pick_WithoutOrders = Helper.CreatePickNew();
			var pick_WithNoPackableItems = Helper.CreatePickNew(order_NoPackableItems);
			var pick_WithPacks = Helper.CreatePickNew(order_Packed1, order_Packed2, order_Packed3);
			var pick_WithAutoPackDisabled = Helper.CreatePickNew(order_AutoPackDisabled);
			var pick_IsReadyForPlanning = Helper.CreatePickNew(order_Packed4);
			pick_IsReadyForPlanning.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			var pick_IsPlanned = Helper.CreatePickNew(order_Packed5);
			pick_IsPlanned.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			pick_AlreadyPacked.AutoAllocateItems();
			pick_WithPacks.AutoAllocateItems();
			pick_WithNoPackableItems.AutoAllocateItems();
			pick_WithAutoPackDisabled.AutoAllocateItems();
			pick_IsReadyForPlanning.AutoAllocateItems();
			pick_IsPlanned.AutoAllocateItems();

			// add packs to picks
			var notify = new NotificationBuffer();
			pick_AlreadyPacked.AutoPackOrdersAndPrintLabels(notify);

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var picks = new[] { pick_AlreadyPacked, pick_WithoutOrders, pick_WithNoPackableItems, pick_WithPacks, pick_WithAutoPackDisabled, pick_IsReadyForPlanning, pick_IsPlanned };
			ApplyApplicator(picks, string.Format(
@"WARNING: Pick {0} [HL {0}] - had the following errors when trying to Auto-Pack:
Order {1} cannot be Auto-Packed as it already has Packages.

WARNING: Pick {2} [HL {2}] - has no Orders attached and cannot be Auto-Packed.
WARNING: Pick {3} [HL {3}] - had the following errors when trying to Auto-Pack:
Order {4} cannot be Auto-Packed as it has no Packable Items caused by a shortfall.

INFO: Pick {5} [HL {5}] - was successfully Auto-Packed.
WARNING: Pick P00000005 [HL P00000005] - had the following errors when trying to Auto-Pack:
Order W00000007 cannot be Auto-Packed as it is not enabled on Consignee Consignee 2.

WARNING: Pick {6} [HL {6}] - had the following errors when trying to Auto-Pack:
Order {7} cannot be Auto-Packed as it has a read only Packing Job.

WARNING: Pick {8} [HL {8}] - had the following errors when trying to Auto-Pack:
Order {9} cannot be Auto-Packed as it has a read only Packing Job.",
				pick_AlreadyPacked.WP_PickNo,
				order_AlreadyPacked.WD_DocketID,
				pick_WithoutOrders.WP_PickNo,
				pick_WithNoPackableItems.WP_PickNo,
				order_NoPackableItems.WD_DocketID,
				pick_WithPacks.WP_PickNo,
				pick_IsReadyForPlanning.WP_PickNo,
				order_Packed4.WD_DocketID,
				pick_IsPlanned.WP_PickNo,
				order_Packed5.WD_DocketID));
		}

		#endregion

		#region TestAction_WorkOrders

		public void TestAction_WorkOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 2m);
			Factory.Save();
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(workOrder);
			Factory.Save();

			ApplyApplicator(new[] { pick }, string.Format(
@"WARNING: Pick {0} [HL {0}] - is a Work Order Pick and cannot be Auto-Packed", pick.WP_PickNo));
		}

		#endregion

		#region TestAction_ArithmeticOverflowOfPackageDimensions

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingPackageWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = Core.Constants.Length.Metres;

			data.Part1.OP_Weight = 500m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Grams;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 10m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);

			// setup picks
			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			pick1.AutoAllocateItems();

			var pick2 = Helper.CreatePickNew(order4);
			pick2.AutoAllocateItems();

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
				ApplyApplicator(new[] { pick1, pick2 }, string.Format(
@"ERROR: Pick {0} [HL {0}] - had the following errors when trying to Auto-Pack:
Order {1} cannot be saved due to the following validation errors:
Error - KP_Weight: The number 5,000,100 is too large, the maximum value allowed for Gross Weight is 999,999.999.
".Trim(), pick1.WP_PickNo, order2.WD_DocketID));
			}

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);
			var order4InNewFactory = newFactory.Load<WhsOrder>(order4.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for fourth order.", 0, order4InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingPackageVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = Core.Constants.Length.Metres;

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
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);

			// setup picks
			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			pick1.AutoAllocateItems();

			var pick2 = Helper.CreatePickNew(order4);
			pick2.AutoAllocateItems();

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
				ApplyApplicator(new[] { pick1, pick2 }, string.Format(
@"ERROR: Pick {0} [HL {0}] - had the following errors when trying to Auto-Pack:
Order {1} cannot be saved due to the following validation errors:
Error - KP_Volume: The number 1,000,000 is too large, the maximum value allowed for Volume is 999,999.999.
".Trim(), pick1.WP_PickNo, order2.WD_DocketID));
			}

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);
			var order4InNewFactory = newFactory.Load<WhsOrder>(order4.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for fourth order.", 0, order4InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingOrderWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = Core.Constants.Length.Metres;

			data.Part1.OP_Weight = 1000m;
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Grams;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 10m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);

			// setup pick
			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			pick1.AutoAllocateItems();

			var pick2 = Helper.CreatePickNew(order4);
			pick2.AutoAllocateItems();

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

			ApplyApplicator(new[] { pick1, pick2 }, string.Format(
	@"ERROR: Pick {0} [HL {0}] - had the following errors when trying to Auto-Pack:
Order {1} cannot be saved due to the following validation errors:
Error - WD_WeightSent: The number 1,100,000 is too large, the maximum value allowed for Weight Sent is 999,999.999.
".Trim(), pick1.WP_PickNo, order2.WD_DocketID));

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);
			var order4InNewFactory = newFactory.Load<WhsOrder>(order4.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for fourth order.", 0, order4InNewFactory.PackageJob.Packages.Count);
		}

		public void TestAction_ArithmeticOverflowOfPackageDimensions_OverflowingOrderVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = Core.Constants.Length.Metres;
			
			data.Part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;
			data.Part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 2m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 110m);
			Factory.Save();

			// setup orders
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order2.Lines[0].WE_F3_NKPackType = "PLT";

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 1m);

			// setup pick
			var pick1 = Helper.CreatePickNew(order1, order2, order3);
			pick1.AutoAllocateItems();

			var pick2 = Helper.CreatePickNew(order4);
			pick2.AutoAllocateItems();

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

				ApplyApplicator(new[] { pick1, pick2 }, string.Format(
	@"ERROR: Pick {0} [HL {0}] - had the following errors when trying to Auto-Pack:
Order {1} cannot be saved due to the following validation errors:
Error - WD_CubicSent: The number 1,000,000 is too large, the maximum value allowed for Volume Sent is 999,999.999.
".Trim(), pick1.WP_PickNo, order2.WD_DocketID));

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			var order3InNewFactory = newFactory.Load<WhsOrder>(order3.PK);
			var order4InNewFactory = newFactory.Load<WhsOrder>(order4.PK);

			AssertEquals("Packages should have been created and saved for first order.", 1, order1InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for third order.", 0, order3InNewFactory.PackageJob.Packages.Count);
			AssertEquals("Packages should not have been created and saved for fourth order.", 0, order4InNewFactory.PackageJob.Packages.Count);
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
