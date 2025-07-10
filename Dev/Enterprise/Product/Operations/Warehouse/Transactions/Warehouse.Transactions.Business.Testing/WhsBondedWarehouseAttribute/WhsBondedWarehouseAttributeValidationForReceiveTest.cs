using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsBondedWarehouseAttributeValidationForReceiveTest : WhsBondedWarehouseAttributeValidationTest
	{
		public void TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_NotSame()
		{
			TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_Core(ReceiveType.Codes.Customs, line1EntryKey: "E01",
				line2EntryKey: "E02", expectError: true);
		}

		public void TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_Empty()
		{
			TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_Core(ReceiveType.Codes.Customs, line1EntryKey: "",
				line2EntryKey: "E02", expectError: false);
		}

		public void TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_NotCustoms()
		{
			TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_Core(ReceiveType.Codes.Receipt, line1EntryKey: "E01",
				line2EntryKey: "E02", expectError: false);
		}

		void TestCustomsReceiveLine_WB_EntryKey_SameForAllLines_Core(ZString receiveType, ZString line1EntryKey,
			ZString line2EntryKey, bool expectError)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = receiveType;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);

			var expectedError = "Only one Entry Key allowed per Receive.";
			receiveLine1.CustomsData.WB_EntryKey = line1EntryKey;
			receiveLine2.CustomsData.WB_EntryKey = line2EntryKey;

			receive.RunPreSaveValidation();
			var errors = receive.NotificationsIncludingChildren.GetErrors().ToArray();
			if (expectError)
			{
				AssertHasError(receiveLine1.CustomsData.WB_EntryKeyInfo, expectedError);
				AssertHasError(receiveLine2.CustomsData.WB_EntryKeyInfo, expectedError);

				AssertEquals("Should have 2 errors, one per line.", 2, errors.Length);
				AssertEquals("Should be Entry Number Message", $"Error - WB_EntryKey: {expectedError}",
					errors[0].Message);
				AssertEquals("Should be Entry Number Message", $"Error - WB_EntryKey: {expectedError}",
					errors[1].Message);
			}
			else
			{
				AssertNoError(receiveLine1.CustomsData.WB_EntryKeyInfo, expectedError);
				AssertNoError(receiveLine2.CustomsData.WB_EntryKeyInfo, expectedError);

				AssertEquals(
					"Should not have 'Only one Entry Key allowed per Receive.' error.(they may have some other error e.g. Should be enter.)",
					0, errors.Count(e => e.Message == expectedError));

				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				receiveLine1.CustomsData.WB_EntryKey = "E01";
				receiveLine2.CustomsData.WB_EntryKey = "E02";

				AssertHasError(receiveLine1.CustomsData.WB_EntryKeyInfo, expectedError);
				AssertHasError(receiveLine2.CustomsData.WB_EntryKeyInfo, expectedError);
			}
		}

		public void TestCustomsReceiveLine_WB_EntryKey_NotSameForAllLines_DisassemblyWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var part3 = Helper.CreateProduct("P3", data.Org1);
			Helper.CreateProductBOM(data.Part2, data.Part1, 2m, "UNT");
			Helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");

			var area = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var componentReceive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, "BEK-1", allocateLocations: false, finalise: false);
			var componentReceive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part3, 10m, "OTHERBEK-1", allocateLocations: false, finalise: false);
			componentReceive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			componentReceive1.WD_IsInwardsProcessingJob = true;
			componentReceive2.WD_IsInwardsProcessingJob = true;
			componentReceive1.Lines[0].WE_WL = location.PK;
			componentReceive2.Lines[0].WE_WL = location.PK;
			componentReceive1.FinaliseDocket();
			componentReceive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive1);
			AssertIsFinalisedPrecondition(componentReceive2);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			workOrder.WD_IsInwardsProcessingJob = true;

			var productParam = workOrder.Lines[0].Product.ParamsByWhsAndClient.AddNew();
			productParam.W3_OH = data.Org1.PK;
			productParam.W3_WW = data.Whs1.PK;
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, receive.WD_DocketSubType);
			AssertEquals("New Receive should be Inwards Processing Job.", true, receive.WD_IsInwardsProcessingJob);
			AssertEquals("Should have created kit Product.", 1, receive.Lines.Count);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "ExtRef2");
			disassemblyWorkOrder.WD_DocketSubType = WorkOrderType.Codes.Disassemble;
			disassemblyWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			disassemblyWorkOrder.WD_IsInwardsProcessingJob = true;
			Helper.CreateWhsWorkOrderLine(disassemblyWorkOrder, data.Part2, 5m);

			Helper.CreatePickNew(disassemblyWorkOrder);
			disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(disassemblyWorkOrder);

			var disassemblyReceive = disassemblyWorkOrder.Receive;
			AssertEquals("New Receive should be a Customs Receive.", ReceiveType.Codes.Customs, disassemblyReceive.WD_DocketSubType);
			AssertEquals("New Receive should be Inwards Processing Job.", true, disassemblyReceive.WD_IsInwardsProcessingJob);
			AssertEquals("Should have created two Component Products.", 2, disassemblyReceive.Lines.Count);

			var line1 = disassemblyReceive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			var line2 = disassemblyReceive.Lines.Single(l => l.WE_OP == part3.PK);
			AssertEquals("Component Bonded Entry Key should be correct.", "BEK-1", line1.WE_BondedEntryKey);
			AssertEquals("Component Bonded Entry Key should be correct.", "OTHERBEK-1", line2.WE_BondedEntryKey);
			disassemblyReceive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Disassembly should succeed, Errors were:\r\n" + disassemblyReceive.NotificationsIncludingChildren.GetErrors().ToUniqueMessageListString(), true, disassemblyReceive.IsFinalised);
		}

		public void TestCustomsReceiveLine_WE_BondedEntryKeyValidation_WithEntry_WithNoQuantity()
		{
			TestCustomsReceiveLine_WE_BondedEntryKeyValidationCore("ABC1", 0m, false);
		}

		public void TestCustomsReceiveLine_WE_BondedEntryKeyValidation_WithEntry_WithQuantity()
		{
			TestCustomsReceiveLine_WE_BondedEntryKeyValidationCore("ABC2", 10m, false);
		}

		public void TestCustomsReceiveLine_WE_BondedEntryKeyValidation_WithoutEntry_WithNoQuantity()
		{
			TestCustomsReceiveLine_WE_BondedEntryKeyValidationCore("", 0m, false);
		}

		public void TestCustomsReceiveLine_WE_BondedEntryKeyValidation_WithoutEntry_WithQuantity()
		{
			TestCustomsReceiveLine_WE_BondedEntryKeyValidationCore("", 10m, true);
		}

		void TestCustomsReceiveLine_WE_BondedEntryKeyValidationCore(ZString entryKey, ZDecimal units, bool expectError)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, units, entryKey);
			receive.RunPreSaveValidation();
			var errors = receive.NotificationsIncludingChildren.GetErrors().ToArray();
			var expectedError = "Entry Number is mandatory for Customs Jobs.";

			if (expectError)
			{
				AssertHasError(receiveLine.CustomsData.WB_EntryKeyInfo, expectedError);

				AssertEquals("Should have an error.", 1, errors.Length);
				AssertEquals("Should be Entry Number Message", $"Error - WB_EntryKey: {expectedError}",
					errors[0].Message);
			}
			else
			{
				AssertNoErrors(receiveLine.CustomsData.WB_EntryKeyInfo);
				AssertEquals($"Should not have '{expectedError}' error(they may have some other error).", 0,
					errors.Count(e => e.Message == $"Error - WB_EntryKey: {expectedError}"));
			}
		}

		public void TestCustomsReceiveLine_WE_BondedEntryKeyValidation_NoEntryKey_IsCustomsReceiveFromWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.RunPreSaveValidation();
			AssertHasError(receiveLine.CustomsData.WB_EntryKeyInfo, "Entry Number is mandatory for Customs Jobs.");

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			receive.WD_WD_ParentDocket = workOrder.PK;
			receiveLine.CustomsData.Validation.ValidateWB_EntryKey();
			AssertNoErrors(receiveLine.CustomsData.WB_EntryKeyInfo);
		}

		public void TestCheckWB_EntryKeyCached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine1.CustomsData.WB_EntryKey = "E01";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine2.CustomsData.WB_EntryKey = "E01";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine3.CustomsData.WB_EntryKey = "E01";

			Factory.Save();
			AssertNoErrors(receiveLine2.CustomsData.WB_EntryKeyInfo);

			var newFactory = new BusinessObjectFactory();
			var receiveLine2InNewFactory = newFactory.Load<WhsDocketLine>(receiveLine2.PK);
			receiveLine2InNewFactory.CustomsData.WB_EntryKey = "E02";

			AssertHasError(receiveLine2InNewFactory.CustomsData.WB_EntryKeyInfo, "Only one Entry Key allowed per Receive.");
			AssertEquals("Should only have 1 table select from setting the invalid customs data.", 1, newFactory.TableSelects.Single(table => table.TableName == WhsBondedWarehouseAttributeSchema.Constants.TableName).Value);

			receiveLine2InNewFactory.CustomsData.WB_EntryKey = "E01";
			AssertNoErrors(receiveLine2InNewFactory.CustomsData.WB_EntryKeyInfo);
			AssertEquals("Should only have 1 table select from setting the invalid customs data.", 1, newFactory.TableSelects.Single(table => table.TableName == WhsBondedWarehouseAttributeSchema.Constants.TableName).Value);
		}

		public void TestCheckWB_EntryKeyCaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine1.CustomsData.WB_EntryKey = "Efg01";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine2.CustomsData.WB_EntryKey = "EFG01";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine3.CustomsData.WB_EntryKey = "EfG01";

			receive.RunPreSaveValidation();
			AssertNoErrors(receiveLine1.CustomsData.WB_EntryKeyInfo);
			AssertNoErrors(receiveLine2.CustomsData.WB_EntryKeyInfo);
			AssertNoErrors(receiveLine3.CustomsData.WB_EntryKeyInfo);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveLine2InNewFactory = newFactory.Load<WhsDocketLine>(receiveLine2.PK);
			receiveLine2InNewFactory.CustomsData.WB_EntryKey = "EFG02";

			AssertHasError(receiveLine2InNewFactory.CustomsData.WB_EntryKeyInfo, "Only one Entry Key allowed per Receive.");
			
			receiveLine2InNewFactory.CustomsData.WB_EntryKey = "efg01";
			AssertNoErrors(receiveLine2InNewFactory.CustomsData.WB_EntryKeyInfo);
		}
	}
}
