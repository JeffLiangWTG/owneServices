using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTransferValidationTestCase : WhsDocketValidationTestCase<WhsTransfer>
	{
		protected override void AssertWD_OH_ClientCreditCheck(bool isForWeb)
		{
			Assert("Transfers don't require credit check", true);
		}

		public void TestValidateWD_DocketType()
		{
			Docket.WD_DocketType = "";
			AssertEquals("Docket type validation accepting blank", true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Transfer;
			AssertEquals("Docket type validation not accepting valid docket type: " + Docket.WD_DocketType, false, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Adjustment;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Receive;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			Docket.WD_DocketType = DocketType.Codes.Order;
			AssertEquals("Docket type validation accepting invalid docket type: " + Docket.WD_DocketType, true, Docket.WD_DocketTypeInfo.HasErrors());

			// also test a dodgy random type
			Docket.WD_DocketType = ";;;";
			AssertEquals("Docket type validation accepting junk", true, Docket.WD_DocketTypeInfo.HasErrors());
		}

		public void TestValidateWD_DocketStatus()
		{
			Docket.WD_DocketStatus = "";
			AssertEquals("Docket status validation accepting blank", true, Docket.WD_DocketStatusInfo.HasErrors());

			// here we test each type, but only Receive should be accepted
			for (int i = 0; i < Docket.Statuses.Count; i++)
			{
				Docket.WD_DocketStatus = (ZString)Docket.Statuses[i].Code;
				if ((Docket.WD_DocketStatus == DocketStatus.Codes.New) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Entered) ||
					(Docket.WD_DocketStatus == DocketStatus.Codes.Finalised))
				{
					AssertEquals("Docket status validation NOT accepting valid docket status: " + Docket.WD_DocketStatus, false, Docket.WD_DocketStatusInfo.HasErrors());
				}
				else
				{
					AssertEquals("Docket status validation accepting invalid docket status: " + Docket.WD_DocketStatus, true, Docket.WD_DocketStatusInfo.HasErrors());
				}
			}

			// also test a dodgy random type
			Docket.WD_DocketStatus = ";;;";
			AssertEquals("Docket status validation accepting junk", true, Docket.WD_DocketStatusInfo.HasErrors());
		}

		public void TestWD_WP_PickBeingReplenished_MustBeSameWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row1", 2, 2);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 915m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse2, "R2", data.Part1, 85m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick1 = Factory.New<WhsPick>();
			pick1.AddOrders(new[] { order1 });

			var order2 = Helper.CreateWhsOrder(data.Org1, warehouse2, "O6");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Factory.New<WhsPick>();
			pick2.AddOrders(new[] { order2 });
			Factory.Save();

			var expectedWarehouseErrorMessage = "Enter a valid Pick To Replenish.";

			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			AssertEquals("Precondition", ZGuid.Empty, docket.WD_WP_PickBeingReplenished);
			AssertNoError("Precondition", docket.WD_WP_PickBeingReplenishedInfo, expectedWarehouseErrorMessage);

			docket.WD_WP_PickBeingReplenished = pick1.PK;
			AssertNoError(docket.WD_WP_PickBeingReplenishedInfo, expectedWarehouseErrorMessage);

			docket.WD_WP_PickBeingReplenished = pick2.PK;
			AssertHasError(docket.WD_WP_PickBeingReplenishedInfo, expectedWarehouseErrorMessage);

			docket.WD_WP_PickBeingReplenished = pick1.PK;
			AssertNoError(docket.WD_WP_PickBeingReplenishedInfo, expectedWarehouseErrorMessage);
		}

		#region TestFinaliseDocket_UNDG_Putaway

		public void TestFinaliseDocket_UNDG_Putaway_UnderLimit()
		{
			TestFinaliseDocket_UNDG_PutawayCore(1m, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_Putaway_OverLimit()
		{
			TestFinaliseDocket_UNDG_PutawayCore(2m, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_PutawayCore(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 9m, location, "Pallet-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, quantity, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1");
			Factory.Save();

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, location, "Pallet-1", quantity);
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_InterWhsTransferSource

		public void TestFinaliseDocket_UNDG_InterWhsTransferSource_UnderLimit()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferSourceCore(10m, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_InterWhsTransferSource_OverLimit()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferSourceCore(11m, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_InterWhsTransferSourceCore(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, quantity * 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, quantity * 2);

			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit2 = Helper.CreateWhsUNDGLimit(whs2, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs2.UNDGLimits.Add(undgLimit2);

			var whs3 = Helper.CreateWarehouse("WH3", "C");
			whs3.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit3 = Helper.CreateWhsUNDGLimit(whs3, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs3.UNDGLimits.Add(undgLimit3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNotNull(whs3.FindLocation("C"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, quantity, "A", whs2.PK, "B");
			transferLine1.WE_TransferFromPalletId = "";
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, quantity, "A", whs3.PK, "C");
			transferLine2.WE_TransferFromPalletId = "";
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2.PK, quantity, "A", whs3.PK, "C");
			transferLine3.WE_TransferFromPalletId = "";
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Warehouse WH2
Substance Code 'AAA'

Warehouse WH3
Substance Code 'AAA'

", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement

		public void TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement_NotEnabled()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(false, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement_Enabled()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(true, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 22);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R2", data.Part2, 22);

			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs2.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit2 = Helper.CreateWhsUNDGLimit(whs2, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs2.UNDGLimits.Add(undgLimit2);

			var whs3 = Helper.CreateWarehouse("WH3", "C");
			whs3.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit3 = Helper.CreateWhsUNDGLimit(whs3, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs3.UNDGLimits.Add(undgLimit3);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNotNull(whs3.FindLocation("C"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11, "A", whs2.PK, "B");
			transferLine1.WE_TransferFromPalletId = "";
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11, "A", whs3.PK, "C");
			transferLine2.WE_TransferFromPalletId = "";
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part2.PK, 11, "A", whs3.PK, "C");
			transferLine3.WE_TransferFromPalletId = "";
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Warehouse WH2
Substance Code 'AAA'

Warehouse WH3
Substance Code 'AAA'

", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_InterWhsTransferDest

		public void TestFinaliseDocket_UNDG_InterWhsTransferDest_UnderLimit()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferDestCore(10m, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_InterWhsTransferDest_OverLimit()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferDestCore(11m, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_InterWhsTransferDestCore(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, quantity);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNull(whs1.FindLocation("B"));
				AssertNull(whs2.FindLocation("A"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, quantity, "B", whs2.PK, "A");
			transferLine.WE_TransferFromPalletId = "";
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement

		public void TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement_NotEnabled()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(false, shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement_Enabled()
		{
			TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(true, shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 11m);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNull(whs1.FindLocation("B"));
				AssertNull(whs2.FindLocation("A"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11, "B", whs2.PK, "A");
			transferLine.WE_TransferFromPalletId = "";
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocket_UNDG_ChangeProduct

		public void TestFinaliseDocket_UNDG_ChangeProduct()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 11);
			Factory.Save();

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11, "B", whs2.PK, "A");
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
			AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);

			line.WE_OP = data.Part2.PK;
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
		}

		#endregion

		#region TestFinaliseDocket_UNDG_UQ

		public void TestFinaliseDocket_UNDG_WeightUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "T", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_WeightUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitWeightUQ: "G", shouldHaveError: true);
		}

		public void TestFinaliseDocket_UNDG_VolumetUQ_UnderLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "ML", shouldHaveError: false);
		}

		public void TestFinaliseDocket_UNDG_VolumeUQ_OverLimit()
		{
			TestFinaliseDocket_UNDG_UQCore(undgLimitVolumeUQ: "L", shouldHaveError: true);
		}

		void TestFinaliseDocket_UNDG_UQCore(string undgLimitWeightUQ = "KG", string undgLimitVolumeUQ = "M3", bool shouldHaveError = false)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 1m, totalWeightLimitUQ: undgLimitWeightUQ, totalVolumeLimit: 2m, totalVolumeLimitUQ: undgLimitVolumeUQ);
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 1);
			Factory.Save();

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1, "B", whs2.PK, "A");
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			transfer.FinaliseDocketWithoutUserConfirmation();

			// Assert
			if (shouldHaveError)
			{
				AssertEquals(true, transfer.WD_DocketStatusInfo.HasErrors());
				AssertEquals(@"The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", transfer.WD_DocketStatusInfo.GetErrors().Single().Message);
			}
			else
			{
				AssertEquals(false, transfer.WD_DocketStatusInfo.HasErrors());
			}
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_InterWhsTransferSource

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferSource_UnderLimit()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferSourceCore(10m, false);
		}

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferSource_OverLimit()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferSourceCore(11m, true);
		}

		void TestFinaliseDocketLines_UNDG_InterWhsTransferSourceCore(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, quantity);

			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs2.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit2 = Helper.CreateWhsUNDGLimit(whs2, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs2.UNDGLimits.Add(undgLimit2);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, quantity, "B", whs2.PK, "A");
			transferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			transferLine.WE_F3_NKPackType = "XXX";

			AssertEquals("Precondition", false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);

			// Assert
			if (shouldHaveError)
			{
				var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
				AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
				AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Warehouse WH2
Substance Code 'AAA'

", lastEvent.Message);
			}
			else
			{
				AssertEquals(false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			}
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement_NotEnabled()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(false, false);
		}

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagement_Enabled()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(true, true);
		}

		void TestFinaliseDocketLines_UNDG_InterWhsTransferSource_DestWarehouseDangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs1, "R1", data.Part1, 11);

			var whs2 = Helper.CreateWarehouse("WH2", "B");
			whs2.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit2 = Helper.CreateWhsUNDGLimit(whs2, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs2.UNDGLimits.Add(undgLimit2);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsSource;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11m, "B", whs2.PK, "A");
			transferLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			transferLine.WE_F3_NKPackType = "XXX";

			AssertEquals("Precondition", false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);

			// Assert
			if (shouldHaveError)
			{
				var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
				AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
				AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Warehouse WH2
Substance Code 'AAA'

", lastEvent.Message);
			}
			else
			{
				AssertEquals(false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			}
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_InterWhsTransferDest

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferDest_UnderLimit()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferDestCore(10m, shouldHaveError: false);
		}

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferDest_OverLimit()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferDestCore(11m, shouldHaveError: true);
		}

		void TestFinaliseDocketLines_UNDG_InterWhsTransferDestCore(decimal quantity, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, quantity);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNull(whs1.FindLocation("B"));
				AssertNull(whs2.FindLocation("A"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, quantity, "B", whs2.PK, "A");
			transferLine.WE_TransferFromPalletId = "";
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);

			// Assert
			if (shouldHaveError)
			{
				var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
				AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
				AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", lastEvent.Message);
			}
			else
			{
				AssertEquals(false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			}
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement_NotEnabled()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(false, shouldHaveError: false);
		}

		public void TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagement_Enabled()
		{
			TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(true, shouldHaveError: true);
		}

		void TestFinaliseDocketLines_UNDG_InterWhsTransferDest_DestWarehouseDangerousGoodsManagementCore(bool isDangerousGoodsManagementEnabled, bool shouldHaveError)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = isDangerousGoodsManagementEnabled;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 20m);
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 11m);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull(whs1.FindLocation("A"));
				AssertNotNull(whs2.FindLocation("B"));
				AssertNull(whs1.FindLocation("B"));
				AssertNull(whs2.FindLocation("A"));
			});

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11m, "B", whs2.PK, "A");
			transferLine.WE_TransferFromPalletId = "";
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);

			// Assert
			if (shouldHaveError)
			{
				var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
				AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
				AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", lastEvent.Message);
			}
			else
			{
				AssertEquals(false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			}
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_ChangeProduct

		public void TestFinaliseDocketLines_UNDG_ChangeProduct()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 0m);
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 11);
			Factory.Save();

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 11, "B", whs2.PK, "A");
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);
			var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
			AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", lastEvent.Message);

			transferLine.WE_OP = data.Part2.PK;
			((NotificationBuffer)transfer.NotificationManager.Peek).Clear();
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { transferLine }, false);

			// Assert
			AssertEquals(false, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
		}

		#endregion

		#region TestFinaliseDocketLines_UNDG_UQ

		public void TestFinaliseDocketLines_UNDG_WeightUQ_UnderLimit()
		{
			TestFinaliseDocketLines_UNDG_UQCore(undgLimitWeightUQ: "T", shouldHaveError: false);
		}

		public void TestFinaliseDocketLines_UNDG_WeightUQ_OverLimit()
		{
			TestFinaliseDocketLines_UNDG_UQCore(undgLimitWeightUQ: "G", shouldHaveError: true);
		}

		public void TestFinaliseDocketLines_UNDG_VolumetUQ_UnderLimit()
		{
			TestFinaliseDocketLines_UNDG_UQCore(undgLimitVolumeUQ: "ML", shouldHaveError: false);
		}

		public void TestFinaliseDocketLines_UNDG_VolumetUQ_OverLimit()
		{
			TestFinaliseDocketLines_UNDG_UQCore(undgLimitVolumeUQ: "L", shouldHaveError: true);
		}

		void TestFinaliseDocketLines_UNDG_UQCore(string undgLimitWeightUQ = "KG", string undgLimitVolumeUQ = "M3", bool shouldHaveError = false)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", undgClass, undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 2m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var whs1 = data.Whs1;
			whs1.WW_WarehouseCode = "WH1";
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit = Helper.CreateWhsUNDGLimit(whs1, undgCode, totalWeightLimit: 1m, totalWeightLimitUQ: undgLimitWeightUQ, totalVolumeLimit: 2m, totalVolumeLimitUQ: undgLimitVolumeUQ);
			whs1.WW_IsDangerousGoodsManagementEnabled = true;
			whs1.UNDGLimits.Add(undgLimit);

			var whs2 = Helper.CreateWarehouse("W2", "B");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R1", data.Part1, 1);
			Factory.Save();

			// Act
			var transfer = Helper.CreateWhsTransfer(data.Org1, whs1);
			transfer.DocketSubType = TransferType.Codes.InterWhsDest;
			var line = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1, "B", whs2.PK, "A");
			line.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			line.WE_F3_NKPackType = "XXX";
			transfer.ValidateAndFinaliseDocketLines(new List<WhsTransferLine> { line }, false);

			// Assert
			if (shouldHaveError)
			{
				var lastEvent = ((TestNotificationBuffer)(transfer.NotificationManager.Peek)).LastEvent;
				AssertEquals(true, ((TestNotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
				AssertEquals("Should have error", @"Error: The following UNDG Limits will be exceeded by finalizing this job:
Substance Code 'AAA'
", lastEvent.Message);
			}
			else
			{
				AssertEquals(false, ((NotificationBuffer)transfer.NotificationManager.Peek).HasErrors);
			}
		}

		#endregion
	}
}
