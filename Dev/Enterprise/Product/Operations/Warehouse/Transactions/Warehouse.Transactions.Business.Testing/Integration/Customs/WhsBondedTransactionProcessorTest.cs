using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedTransactionProcessorTest : WhsTestCaseWithFactory
	{
		#region Constructors

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				new WhsBondedTransactionProcessor(null, new WhsBondedWarehouseTransaction());
			});
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				new WhsBondedTransactionProcessor(new BusinessObjectFactory(), null);
			});
		}

		#endregion

		#region TestProcess

		public void TestProcess()
		{
			// setup existing receive data
			var data1 = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			var processor1 = new WhsBondedTransactionProcessor(Factory, data1.IReceive);
			ProcessWithMock(processor1);
			Factory.Save();
			Helper.SetClientAttributeType(data1.Org, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data1.Org, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data1.Org, data1.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data1.Org, data1.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data1.Org, data1.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data1.Org, data1.Part2, AttributeNumber.Two, true);
			// create some manual adjustments
			var adjustment = Helper.CreateWhsAdjustment(data1.Org, data1.Whs);
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			// create a few adjustment lines
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustment, data1.Part1, 5m,
				data1.Whs.DefaultLocation.ToLocationString());
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(adjustment, data1.Part2, 10m,
				data1.Whs.DefaultLocation.ToLocationString());
			adjustmentLine1.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey(data1.IReceiveLine1.EntryKey,
				data1.IReceiveLine1.EntryLineNumber);
			adjustmentLine1.WE_PartAttrib1 = "VIN1";
			adjustmentLine1.WE_PartAttrib2 = "ENGINE1";
			adjustmentLine2.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey(data1.IReceiveLine2.EntryKey,
				data1.IReceiveLine2.EntryLineNumber);
			adjustmentLine2.WE_PartAttrib1 = "VIN2";
			adjustmentLine2.WE_PartAttrib2 = "ENGINE2";
			adjustment.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			adjustment.FinaliseDocket();
			Factory.Save();
			var processor2 = new WhsBondedTransactionProcessor(Factory, data1.IReceive);
			ProcessWithMock(processor2);
			var receiveList = new WhsReceiveCollection(Factory);
			var adjustmentList = new WhsAdjustmentCollection(Factory);
			var filter = new ZQuery(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, adjustment.PK);
			adjustmentList.AdditionalFilter = filter;
			AssertEquals("Factory should have 2 Receive objects", 2, receiveList.Count);
			AssertEquals("Factory should have 1 system generated Adjustment object", 1, adjustmentList.Count);
			AssertEquals("Receive1 should have 6 lines", 6, receiveList[0].Lines.Count);
			AssertEquals("Receive2 should have 6 lines", 6, receiveList[1].Lines.Count);
			AssertEquals("System generated adjustment should have 8 lines", 8, adjustmentList[0].Lines.Count);
		}

		#region TestProcess_Receive

		public void TestProcess_Receive()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			var receiveList = new WhsReceiveCollection(Factory);
			var receive = receiveList[0];
			AssertEquals("Incorrect number of Receive dockets created", 1, receiveList.Count);
			AssertEquals("Receive was not finalised", true, receive.IsFinalised);
			AssertEquals("Incorrect number of Receive Lines created", 6, receive.Lines.Count);
			AssertEquals("Receive warehouse does not match Bonded Transaction warehouse", data.Whs.PK,
				receive.Warehouse.PK);
			AssertEquals("Receive client does not match Bonded Transaction client", data.Org.PK, receive.Client.PK);
			AssertEquals("Receive External Reference is incorrect", data.IReceiveLine1.EntryKey + "-INW",
				receive.WD_ExternalReference);
			AssertEquals("Receive Arrival Date is incorrect", data.Whs.GetWarehouseBranchLocalDateTimeOffset(data.IReceive.Date), receive.WD_ArrivalDate);
			short transactionLineIndex = 0;
			foreach (var line in receive.Lines)
			{
				IWhsBondedWarehouseTransactionLine transactionLine = data.IReceive.Lines[transactionLineIndex];
				AssertReceiveLineCustomsData(line, transactionLine, data.IReceive);
				AssertLine(line, transactionLine.Product.PK, transactionLine.Quantity, transactionLine.QuantityUnit,
					data.Whs.DefaultLocation.ToLocationString(), "", "",
					WhsBondedWarehouseAttribute.BuildKey(transactionLine.EntryKey, transactionLine.EntryLineNumber),
					ZDate.Empty, ZDate.Empty, transactionLine.PartAttrib1, transactionLine.PartAttrib2,
					transactionLine.PartAttrib3);
				transactionLineIndex++;
			}
		}

		public void TestProcess_ReceiveMultiWarehouse()
		{
			var data = new TestDataForBondedEntriesMultiWarehouse(Factory,
				processBondedInwardDuringConstruction: false);
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive1);

			var putawayEngineMock = ProcessWithMock(processor);

			var receiveList = new WhsReceiveCollection(Factory);
			AssertEquals("Incorrect number of Receive dockets created", 3, receiveList.Count);
			var receive1 = receiveList.Cast<WhsReceive>().FindDocket(data.Whs1, data.Org);
			var receive2 = receiveList.Cast<WhsReceive>().FindDocket(data.Whs2, data.Org);
			var receive3 = receiveList.Cast<WhsReceive>().FindDocket(data.Whs3, data.Org);

			putawayEngineMock.Verify(p => p.Putaway(It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive1.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(), It.IsAny<INotifications>(), It.IsAny<RefEquipment>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<bool>(), It.IsAny<bool>()));

			putawayEngineMock.Verify(p => p.Putaway(It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive2.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(), It.IsAny<INotifications>(), It.IsAny<RefEquipment>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<bool>(), It.IsAny<bool>()));

			putawayEngineMock.Verify(p => p.Putaway(It.Is<IEnumerable<WhsReceive>>(rc => rc.Any(r => r.PK == receive3.PK)),
					It.IsAny<IEnumerable<WhsReceiveLine>>(), It.IsAny<INotifications>(), It.IsAny<RefEquipment>(), It.IsAny<IEnumerable<ZGuid>>(), It.IsAny<bool>(), It.IsAny<bool>()));

			AssertEquals("Receive1 warehouse does not match Bonded Transaction Line warehouse", data.Whs1.PK,
				receive1.Warehouse.PK);
			AssertEquals("Receive1 was not finalised", true, receive1.IsFinalised);
			AssertEquals("Incorrect number of Receive1 Lines created", 2, receive1.Lines.Count);
			AssertEquals("Receive1 client does not match Bonded Transaction client", data.Org.PK, receive1.Client.PK);
			AssertEquals("Receive1 External Reference incorrect ", data.IReceiveLine11.EntryKey + "-INW",
				receive1.WD_ExternalReference);
			AssertEquals("Receive2 warehouse does not match Bonded Transaction Line warehouse", data.Whs2.PK,
				receive2.Warehouse.PK);
			AssertEquals("Receive2 was not finalised", true, receive2.IsFinalised);
			AssertEquals("Incorrect number of Receive2 Lines created", 2, receive2.Lines.Count);
			AssertEquals("Receive2 client does not match Bonded Transaction client", data.Org.PK, receive2.Client.PK);
			AssertEquals("Receive2 External Reference incorrect ", data.IReceiveLine21.EntryKey + "-INW",
				receive2.WD_ExternalReference);
			AssertEquals("Receive3 warehouse does not match Bonded Transaction Line warehouse", data.Whs3.PK,
				receive3.Warehouse.PK);
			AssertEquals("Receive3 was not finalised", true, receive3.IsFinalised);
			AssertEquals("Incorrect number of Receive3 Lines created", 1, receive3.Lines.Count);
			AssertEquals("Receive3 client does not match Bonded Transaction client", data.Org.PK, receive3.Client.PK);
			AssertEquals("Receive3 External Reference incorrect ", data.IReceiveLine31.EntryKey + "-INW",
				receive3.WD_ExternalReference);
			short transactionLineIndex = 0;
			var whs1ExpectedLocationString = data.Whs1.DefaultLocationInBondedArea.ToLocationString();
			foreach (var line in receive1.Lines)
			{
				var transactionLine = data.IReceive1.Lines[transactionLineIndex];
				AssertReceiveLineCustomsData(line, transactionLine, data.IReceive1);
				AssertLine(line, transactionLine.Product.PK, transactionLine.Quantity, transactionLine.QuantityUnit,
					whs1ExpectedLocationString, "", "",
					WhsBondedWarehouseAttribute.BuildKey(transactionLine.EntryKey, transactionLine.EntryLineNumber),
					ZDate.Empty, ZDate.Empty, transactionLine.PartAttrib1, transactionLine.PartAttrib2,
					transactionLine.PartAttrib3);
				transactionLineIndex++;
			}

			var whs2ExpectedLocationString = data.Whs2.DefaultLocationInBondedArea.ToLocationString();
			foreach (var line in receive2.Lines)
			{
				var transactionLine = data.IReceive1.Lines[transactionLineIndex];
				AssertReceiveLineCustomsData(line, transactionLine, data.IReceive1);
				transactionLineIndex++;
				AssertLine(line, transactionLine.Product.PK, transactionLine.Quantity, transactionLine.QuantityUnit,
					whs2ExpectedLocationString, "", "",
					WhsBondedWarehouseAttribute.BuildKey(transactionLine.EntryKey, transactionLine.EntryLineNumber),
					ZDate.Empty, ZDate.Empty, transactionLine.PartAttrib1, transactionLine.PartAttrib2,
					transactionLine.PartAttrib3);
			}

			var whs3ExpectedLocationString = data.Whs3.DefaultLocationInBondedArea.ToLocationString();
			foreach (var line in receive3.Lines)
			{
				var transactionLine = data.IReceive1.Lines[transactionLineIndex];
				AssertReceiveLineCustomsData(line, transactionLine, data.IReceive1);
				transactionLineIndex++;
				AssertLine(line, transactionLine.Product.PK, transactionLine.Quantity, transactionLine.QuantityUnit,
					whs3ExpectedLocationString, "", "",
					WhsBondedWarehouseAttribute.BuildKey(transactionLine.EntryKey, transactionLine.EntryLineNumber),
					ZDate.Empty, ZDate.Empty, transactionLine.PartAttrib1, transactionLine.PartAttrib2,
					transactionLine.PartAttrib3);
			}
		}

		#endregion

		#region TestProcess_Adjustments

		public void TestProcess_Adjustments()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			Factory.Save();
			Helper.SetClientAttributeType(data.Org, AttributeNumber.One, false);
			Helper.SetClientAttributeType(data.Org, AttributeNumber.Two, false);
			Helper.SetProductAttributeUse(data.Org, data.Part2, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org, data.Part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org, data.Part3, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org, data.Part3, AttributeNumber.Two, true);
			var receiveList = new WhsReceiveCollection(Factory);
			var receive = receiveList[0];
			var adjustment1 = Helper.CreateWhsAdjustment(data.Org, data.Whs);
			adjustment1.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine11 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part2, 5m,
				data.Whs.DefaultLocation.ToLocationString());
			adjustmentLine11.WE_BondedEntryKey =
				WhsBondedWarehouseAttribute.BuildKey(data.IReceiveLine1.EntryKey, data.IReceiveLine1.EntryLineNumber);
			adjustmentLine11.WE_PartAttrib1 = "VIN11";
			adjustmentLine11.WE_PartAttrib2 = "ENGINE11";
			var adjustmentLine12 = Helper.CreateWhsAdjustmentLine(adjustment1, data.Part2, 10m,
				data.Whs.DefaultLocation.ToLocationString());
			adjustmentLine12.WE_BondedEntryKey =
				WhsBondedWarehouseAttribute.BuildKey(data.IReceiveLine1.EntryKey, data.IReceiveLine1.EntryLineNumber);
			adjustmentLine12.WE_PartAttrib1 = "VIN12";
			adjustmentLine12.WE_PartAttrib2 = "ENGINE12";
			adjustment1.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			adjustment1.FinaliseDocket();
			var adjustment2 = Helper.CreateWhsAdjustment(data.Org, data.Whs);
			adjustment2.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var adjustmentLine21 = Helper.CreateWhsAdjustmentLine(adjustment2, data.Part3, 15m,
				data.Whs.DefaultLocation.ToLocationString());
			adjustmentLine21.WE_BondedEntryKey =
				WhsBondedWarehouseAttribute.BuildKey(data.IReceiveLine1.EntryKey, data.IReceiveLine1.EntryLineNumber);
			adjustmentLine21.WE_PartAttrib1 = "VIN21";
			adjustmentLine21.WE_PartAttrib2 = "ENGINE21";
			adjustment2.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			adjustment2.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment1);
			AssertIsFinalisedPrecondition(adjustment2);
			ProcessWithMock(processor);
			var adjustmentList = new WhsAdjustmentCollection(Factory);
			AssertEquals("Should have only three adjustments", 3, adjustmentList.Count);
			var filter = new ZQuery();
			filter.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, adjustment1.PK);
			filter.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, adjustment2.PK);
			adjustmentList = new WhsAdjustmentCollection(Factory, filter);
			var adjustment = adjustmentList[0];
			AssertEquals("Should find only 1 adjustment", 1, adjustmentList.Count);
			AssertEquals("Adjustment was not finalised", true, adjustment.IsFinalised);
			AssertEquals("Incorrect number of Adjustment Lines created", 9, adjustment.Lines.Count);
			AssertEquals("Adjustment warehouse does not match Bonded Transaction warehouse", data.Whs.PK,
				adjustment.Warehouse.PK);
			AssertEquals("Adjustment client does not match Bonded Transaction client", data.Org.PK,
				adjustment.Client.PK);
			//AssertEquals("Adjustment External Reference is incorrect", data.IReceiveLine6.EntryKey + "-AMD", adjustment.WD_ExternalReference);
			var hasAMatch = false;
			var lines = new WhsDocketLineCollectionND(Factory, new ZQuery());
			lines.AddRange(receive.Lines);
			lines.AddRange(adjustment1.Lines);
			lines.AddRange(adjustment2.Lines);
			foreach (WhsAdjustmentLine adjustmentLine in adjustment.Lines)
			{
				hasAMatch = false;
				foreach (WhsDocketLine line in lines)
				{
					try
					{
						AssertAdjustmentLine(adjustmentLine, line);
						AssertAdjustmentLineCustomsData(adjustmentLine, line);
						hasAMatch = true;
						break;
					}
					catch
					{
					}
				}

				if (!hasAMatch)
				{
					Fail("Adjustment Line: " + adjustmentLine.WE_BondedEntryKey +
						 " does not have a matching transaction");
				}
			}
		}

		public void TestProcess_AdjustmentsMultiWarehouse()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false,
				saveFactory_doNotUseForNewTests: false);
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "2 BOND ST";
			address1.OA_OH = data.Org.PK;
			var whs1 = Helper.CreateWarehouse("1", "A", 10, 2);
			whs1.WW_OA_WarehouseAddress = address1.PK;
			Helper.EnableWarehouseForBond(whs1, true);
			Helper.EnableWarehouseForFreeStore(whs1, false);
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "3 BOND ST";
			address2.OA_OH = data.Org.PK;
			var whs2 = Helper.CreateWarehouse("1", "A", 10, 2);
			whs2.WW_OA_WarehouseAddress = address2.PK;
			Helper.EnableWarehouseForBond(whs2, true);
			Helper.EnableWarehouseForFreeStore(whs2, false);
			Array.ForEach(whs1.Rows.Cast<WhsRow>().SelectMany(r => r.Locations).ToArray(),
				l => l.WLV_WA_PickingArea = whs1.Areas.Single(a => a.WA_AreaType == "BON").PK);
			Array.ForEach(whs2.Rows.Cast<WhsRow>().SelectMany(r => r.Locations).ToArray(),
				l => l.WLV_WA_PickingArea = whs2.Areas.Single(a => a.WA_AreaType == "BON").PK);
			data.IReceiveLine1.EntryKey = "X";
			data.IReceiveLine1.EntryLineNumber = 1;
			data.IReceiveLine1.Warehouse = address1;
			data.IReceiveLine2.EntryKey = "Y";
			data.IReceiveLine2.EntryLineNumber = 1;
			data.IReceiveLine2.Warehouse = address2;
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			Factory.Save();
			ProcessWithMock(processor);
			var adjustmentList = new WhsAdjustmentCollection(Factory);
			AssertEquals("Should have only three adjustments", 3, adjustmentList.Count);
			adjustmentList = new WhsAdjustmentCollection(Factory);
			adjustmentList.ApplySort(WhsDocketSchema.WD_ExternalReference.Name,
				System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Should find 3 adjustments", 3, adjustmentList.Count);
			var newAdjustment1 = adjustmentList[0];
			var newAdjustment2 = adjustmentList[1];
			var newAdjustment3 = adjustmentList[2];
			AssertEquals("Adjustment was not finalised", true, newAdjustment1.IsFinalised);
			AssertEquals("Incorrect number of Adjustment Lines created", 4, newAdjustment1.Lines.Count);
			AssertEquals("Adjustment warehouse does not match Bonded Transaction warehouse", data.Whs.PK,
				newAdjustment1.Warehouse.PK);
			AssertEquals("Adjustment was not finalised", true, newAdjustment2.IsFinalised);
			AssertEquals("Incorrect number of Adjustment Lines created", 1, newAdjustment2.Lines.Count);
			AssertEquals("Adjustment warehouse does not match Bonded Transaction warehouse", whs1.PK,
				newAdjustment2.Warehouse.PK);
			AssertEquals("Adjustment was not finalised", true, newAdjustment3.IsFinalised);
			AssertEquals("Incorrect number of Adjustment Lines created", 1, newAdjustment3.Lines.Count);
			AssertEquals("Adjustment warehouse does not match Bonded Transaction warehouse", whs2.PK,
				newAdjustment3.Warehouse.PK);
		}

		#endregion

		#region TestProcess_ValidateMandatoryData

		Tuple<TestDataForBondedEntriesWithBondIDs, WhsBondedTransactionProcessor>
			GetProcess_ValidateMandatoryDataSetup()
		{
			var tempFactory = new BusinessObjectFactory();
			var data = new TestDataForBondedEntriesWithBondIDs(tempFactory,
				processBondedInwardDuringConstruction: false);
			var processor = new WhsBondedTransactionProcessor(tempFactory, data.IReceive);
			return new Tuple<TestDataForBondedEntriesWithBondIDs, WhsBondedTransactionProcessor>(data, processor);
		}

		public void TestProcess_ValidateClient()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			AssertNoExceptionOnProcess(processor);
			data.IReceive.Client = null;
			AssertExceptionOnProcess(processor, "Missing data for field: Client");
		}

		public void TestProcess_ValidateLines()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceive.Lines = null;
			AssertExceptionOnProcess(processor, "Missing data for field: Lines");
		}

		public void TestProcess_ValidateBondedTransactionLines()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceive.Lines = new WhsBondedWarehouseTransactionLineCollection();
			AssertExceptionOnProcess(processor, "Missing data for field: Lines");
		}

		public void TestProcess_ValidateTransactionWarehouseAddress()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceiveLine1.Warehouse = null;
			AssertNotNull("Transaction warehouse address should not be null", data.IReceive.Warehouse);
			AssertNoExceptionOnProcess(processor);
		}

		public void TestProcess_ValidateTransactionLineWarehouseAddress()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceive.Warehouse = null;
			AssertNotNull("Transaction Line1 warehouse address should not be null", data.IReceiveLine1.Warehouse);
			AssertNoExceptionOnProcess(processor);
		}

		public void TestProcess_ValidateWarehouseData()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceiveLine1.Warehouse = null;
			data.IReceive.Warehouse = null;
			AssertExceptionOnProcess(processor, "Missing data for field: Warehouse");
		}

		public void TestProcess_ValidateBondedWarehouse()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			Helper.EnableWarehouseForBond(data.Whs, false);
			AssertExceptionOnProcess(processor,
				"The warehouse must be a Bonded Warehouse. You can set this in Config -> Warehouse -> Warehouses");
		}

		public void TestProcess_ValidateMandatoryData()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceiveLine1.Product = null;
			data.IReceiveLine1.EntryKey = ZString.Empty;
			data.IReceiveLine1.EntryLineNumber = ZShort.Zero;
			data.IReceiveLine1.Quantity = ZDecimal.Zero;
			AssertExceptionOnProcess(processor,
				"Missing data for field: Product" + System.Environment.NewLine + "Missing data for field: EntryKey" +
				System.Environment.NewLine + "Missing data for field: EntryLineNumber" + System.Environment.NewLine +
				"Incorrect data for field: Quantity, is zero");
		}

		public void TestProcess_ValidateAttributeDetail()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.Org.PartAttributeManager.SetProductToUseAttribute((OrgSupplierPart)data.IReceiveLine1.Product, 1,
				true);
			data.IReceiveLine1.PartAttrib1 = ZString.Empty;
			AssertExceptionOnProcess(processor,
				"Missing data for field: BondID1, Product(" + data.IReceiveLine1.Product.OP_PartNum +
				") requires this attribute detail");
		}

		public void TestProcess_ValidateAttributeDetailNotRequired()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.Org.PartAttributeManager.SetProductToUseAttribute((OrgSupplierPart)data.IReceiveLine1.Product, 1,
				false);
			data.IReceiveLine1.PartAttrib1 = "SOMETHING";
			AssertExceptionOnProcess(processor,
				"Incorrect data for field: BondID1 should be empty as Product(" +
				data.IReceiveLine1.Product.OP_PartNum + ") does not require this attribute detail");
		}

		public void TestProcess_ValidateAttributeDetail_SerialNumber()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.Org.MiscServ.OM_IMUseSerialNumber = true;
			data.Org.PartAttributeManager.SetProductToUseAttribute((OrgSupplierPart)data.IReceiveLine1.Product, 6,
				true);
			data.IReceiveLine1.SerialNumber = ZString.Empty;
			AssertExceptionOnProcess(processor,
				"Missing data for field: BondID4, Product(" + data.IReceiveLine1.Product.OP_PartNum +
				") requires this attribute detail");
		}

		public void TestProcess_ValidateAttributeDetailNotRequired_SerialNumber()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.Org.MiscServ.OM_IMUseSerialNumber = true;
			data.Org.PartAttributeManager.SetProductToUseAttribute((OrgSupplierPart)data.IReceiveLine1.Product, 6,
				false);
			data.IReceiveLine1.SerialNumber = "SOMETHING";
			AssertExceptionOnProcess(processor,
				"Incorrect data for field: BondID4 should be empty as Product(" +
				data.IReceiveLine1.Product.OP_PartNum + ") does not require this attribute detail");
		}

		public void TestProcess_ValidateReferenceType()
		{
			var setup = GetProcess_ValidateMandatoryDataSetup();
			var data = setup.Item1;
			var processor = setup.Item2;
			data.IReceive.AdditionalReferences =
				new AdditionalReference[] { new AdditionalReference("Invalid", "Test") };
			AssertExceptionOnProcess(processor,
				"Incorrect data for field: Additional Reference, incorrect reference type: " +
				data.IReceive.AdditionalReferences.ElementAt(0).Type);
		}

		#endregion

		#region TestProcess_CheckAndCorrectProductOwnerRelationship

		public void TestProcess_CheckAndCorrectProductOwnerRelationship()
		{
			TestDataForBondedEntriesWithBondIDs data =
				new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			WhsBondedTransactionProcessor processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			data.Part1.RelatedOrganisations.RemoveAll();
			AssertNull("Precondition: Should have no Part1 to Client relationship",
				data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			data.Part2.RelatedOrganisations.RemoveAll();
			AssertNull("Precondition: Should have no Part2 to Client relationship",
				data.Part2.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			ProcessWithMock(processor);
			AssertNotNull("Part1 to Client relationship should be created",
				data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			AssertNotNull("Part2 to Client relationship should be created",
				data.Part2.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org.PK,
					OrgPartRelation.RelationshipTypes.Owner));
		}

		#endregion

		#region TestProcess_BuildDistinctEntryKeyList

		public void TestProcess_BuildDistinctEntryKeyList()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			var receive = Helper.CreateWhsReceive(data.Org, data.Whs);
			receive.WD_DocketSubType = "CUS";
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "A-1");
			receiveLine1.WI_WL = data.Whs.DefaultLocation.PK;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, "A-2");
			receiveLine2.WI_WL = data.Whs.DefaultLocation.PK;
			receive.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			data.IReceiveLine1.EntryKey = "A";
			data.IReceiveLine1.EntryLineNumber = 1;
			data.IReceiveLine2.EntryKey = "A";
			data.IReceiveLine2.EntryLineNumber = 1;
			data.IReceiveLine3.EntryKey = "A";
			data.IReceiveLine3.EntryLineNumber = 1;
			data.IReceiveLine4.EntryKey = "A";
			data.IReceiveLine4.EntryLineNumber = 2;
			data.IReceiveLine5.EntryKey = "A";
			data.IReceiveLine5.EntryLineNumber = 2;
			data.IReceiveLine6.EntryKey = "A";
			data.IReceiveLine6.EntryLineNumber = 2;
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			var adjustmentsList = new WhsAdjustmentCollection(Factory);
			AssertEquals("Should have only one Adjustment object", 1, adjustmentsList.Count);
			var newAdjustment = adjustmentsList[0];
			AssertEquals("Adjustment object should have two lines", 2, newAdjustment.Lines.Count);
			newAdjustment.Lines.ApplySort(WhsDocketLineSchema.WE_BondedEntryKey.Name,
				System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Adjustment line1 bonded key should be " + WhsBondedWarehouseAttribute.BuildKey("A", 1),
				WhsBondedWarehouseAttribute.BuildKey("A", 1), newAdjustment.Lines[0].WE_BondedEntryKey);
			AssertEquals("Adjustment line2 bonded key should be " + WhsBondedWarehouseAttribute.BuildKey("A", 2),
				WhsBondedWarehouseAttribute.BuildKey("A", 2), newAdjustment.Lines[1].WE_BondedEntryKey);
		}

		public void TestProcess_BuildDistinctEntryKeyList_CheckIfItISTooLateToAdjust()
		{
			TestDataForBondedEntries data = new TestDataForBondedEntries(Factory,
				processBondedInwardDuringConstruction: false, saveFactory_doNotUseForNewTests: false);
			WhsReceive receive = Helper.CreateWhsReceive(data.Org, data.Whs);
			WhsInventoryView receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receiveLine.WI_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey("A", 1);
			receiveLine.WI_WL = data.Whs.DefaultLocation.PK;
			receive.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsOrder order = Helper.CreateWhsOrder(data.Org, data.Whs);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "", "ABC");
			orderLine.WE_BondedEntryKey = WhsBondedWarehouseAttribute.BuildKey("A", 1);
			Helper.CreatePickNew(order);
			order.FinaliseDocket();
			data.IReceiveLine1.EntryKey = "A";
			data.IReceiveLine1.EntryLineNumber = 1;
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			bool noException = false;
			try
			{
				ProcessWithMock(processor);
				noException = true;
			}
			catch (Exception e)
			{
				AssertEquals("Invalid exception " + e.Message, (new CannotUpdateStockException("")).GetType(),
					e.GetType());
			}

			if (noException)
			{
				Fail("CannotUpdateStockException should be thrown");
			}
		}

		#endregion

		#region TestProcess_BuildExistingTransactions

		public void TestProcess_BuildExistingTransactions()
		{
			var data = new TestDataForBondedEntries(Factory, processBondedInwardDuringConstruction: false,
				saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org, data.Whs);
			receive.WD_DocketSubType = "CUS";
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "A-1");
			receiveLine1.WI_WL = data.Whs.DefaultLocation.PK;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, "A-3");
			receiveLine2.WI_WL = data.Whs.DefaultLocation.PK;
			receive.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			data.IReceiveLine1.EntryKey = "A";
			data.IReceiveLine1.EntryLineNumber = 1;
			data.IReceiveLine2.EntryKey = "A";
			data.IReceiveLine2.EntryLineNumber = 2;
			data.IReceiveLine3.EntryKey = "A";
			data.IReceiveLine3.EntryLineNumber = 3;
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			var adjustmentsList = new WhsAdjustmentCollection(Factory);
			AssertEquals("Should have only one Adjustment object", 1, adjustmentsList.Count);
			var newAdjustment = adjustmentsList[0];
			AssertEquals("Adjustment object should have one line", 2, newAdjustment.Lines.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "A-1", "A-3" },
				newAdjustment.Lines.Select(l => l.WE_BondedEntryKey));
		}

		#endregion

		#region TestProcess_FindOrCreateVirtualWarehouse

		public void TestProcess_FindOrCreateVirtualWarehouse()
		{
			//TestDataForBondedEntries Data
			// create Transaction Header with valid OrgAddress - WhsWarehouse pair
			// fudge line 1 with null orgaddress (should use TransactionHeader warehouse)
			// fudge line 2 with a orgaddress (diff from header) but no matching WhsWarehouse
			// fudge line 3 with a orgaddress (diff from header) with matching WhsWarehouse
			// Assert line1 uses Header warehouse
			// assert line2 creates a WhsWarehouse for OrgAddress
			// assert line3 does not create a new WhsWarehouse (uses existing)
			var data = new TestDataForBondedEntries(Factory, processBondedInwardDuringConstruction: false);
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "2 BOND ST";
			address1.OA_OH = data.Org.PK;
			var warehouse1 = WhsBondedTransactionProcessor.CreateVirtualWarehouse(Factory, address1);
			AssertEquals("As part of the process, it should be saved but not in CreateVirtualWarehouse.", false, warehouse1.IsInDatabase);
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Address1 = "3 BOND ST";
			address2.OA_OH = data.Org.PK;
			var warehouse2 = Helper.CreateWarehouse("2", "A", 10, 2);
			warehouse2.WW_OA_WarehouseAddress = address2.PK;
			Helper.EnableWarehouseForBond(warehouse2, true);
			data.IReceiveLine1.EntryKey = "A";
			data.IReceiveLine2.EntryKey = "B";
			data.IReceiveLine3.EntryKey = "C";
			data.IReceiveLine1.Warehouse = null;
			data.IReceiveLine2.Warehouse = address1;
			data.IReceiveLine3.Warehouse = address2;
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			var receiveCollection = new WhsReceiveCollection(Factory);
			receiveCollection.ApplySort(WhsDocketSchema.WD_ExternalReference.Name,
				System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Should create 3 Receive objects", 3, receiveCollection.Count);
			var receive1 = receiveCollection[0];
			var receive2 = receiveCollection[1];
			var receive3 = receiveCollection[2];
			AssertEquals("Receive1 should use transaction warehouse", data.Whs, receive1.Warehouse);
			AssertNotEquals("Receive2 warehouse should be different from transaction warehouse", data.Whs,
				receive2.Warehouse);
			AssertNotEquals("Receive2 warehouse should be different from address2 warehouse", warehouse2,
				receive2.Warehouse);
			AssertEquals("Receive2 should use warehouse for address1", address1, receive2.Warehouse.WarehouseAddress);
			AssertEquals("Receive3 should be using Warehouse2", warehouse2, receive3.Warehouse);
			AssertEquals("Receive3 should use existing warehouse for address2", address2,
				receive3.Warehouse.WarehouseAddress);
		}

		public void TestProcess_FindOrCreateVirtualWarehouseCreated()
		{
			// create Transaction Header with OrgAddress but no WhsWarehouse match
			// create a dummy line (details don't matter, but ensure warehouse is null)
			// assert WhsWarehouse is created for Header OrgAddress
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			var address3 = Factory.NewWithValidTestData<OrgAddress>();
			address3.OA_Address1 = "5 BOND ST";
			address3.OA_OH = data.Org.PK;
			data.IReceive.Warehouse = address3;
			data.IReceiveLine1.EntryKey = "D";
			data.IReceiveLine1.Warehouse = null;
			data.IReceive.Lines = new WhsBondedWarehouseTransactionLineCollection();
			data.IReceive.Lines.Add(data.IReceiveLine1);
			Factory.Save();
			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);
			var receiveCollection = new WhsReceiveCollection(Factory);
			AssertEquals("Should create 1 Receive object", 1, receiveCollection.Count);
			var receive1 = receiveCollection[0];
			AssertEquals("Receive should create new warehouse for address3", address3,
				receive1.Warehouse.WarehouseAddress);
			AssertEquals("New Warehouse should be bonded", true, receive1.Warehouse.IsWarehouseBondEnabled);
			AssertEquals("New Warehouse should be virtual", true, receive1.Warehouse.WW_IsVirtualWarehouse);
		}

		public void TestProcess_CreateVirtualWarehouse_DoCallSaveFactory()
		{
			// create Transaction Header with OrgAddress but no WhsWarehouse match
			// create a dummy line (details don't matter, but ensure warehouse is null)
			// assert WhsWarehouse is created for Header OrgAddress
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "5 BOND ST";
			address.OA_OH = data.Org.PK;
			Factory.Save();

			data.IReceive.Warehouse = address;

			data.IReceiveLine1.EntryKey = "D";
			data.IReceiveLine1.Warehouse = null;

			data.IReceive.Lines = new WhsBondedWarehouseTransactionLineCollection();
			data.IReceive.Lines.Add(data.IReceiveLine1);

			Factory.Saving += f =>
			{
				Assert("When creating a virtual warehouse, do not hit save.", false);
			};

			AssertNull("Precondition", findWarehouse());

			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			ProcessWithMock(processor);

			AssertEquals("Should create in memory.", false, findWarehouse().IsInDatabase);

			WhsWarehouse findWarehouse() => Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, address.PK));
		}

		#endregion

		#region TestProcess_FinaliseAllDockets_NotifyUserWithFinaliseErrors

		public void TestProcess_FinaliseAllDockets_NotifyUserWithFinaliseErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var bondedReceive = CreateBondedWhsReceive(data.Org1, data.Whs1, "R1");
			var bondedReceiveLine =
				CreateBondedWhsReceiveInventoryLine(bondedReceive, data.Part1, 10m, "EN0001", 1, "", "",
					""); // 10 units for serial no. product is invalid.
			bondedReceiveLine.SerialNumber = "SN1";
			var processor = new WhsBondedTransactionProcessor(Factory, bondedReceive);
			AssertExceptionThrown("Must always be 1 or less for serial number controlled products",
				typeof(CannotUpdateStockException), () => ProcessWithMock(processor));
		}

		WhsBondedWarehouseTransaction CreateBondedWhsReceive(OrgHeader client, WhsWarehouse whs, ZString reference)
		{
			var bondedReceive = new WhsBondedWarehouseTransaction();
			bondedReceive.Client = client;
			bondedReceive.Warehouse = whs.WarehouseAddress;
			bondedReceive.Reference = reference;
			bondedReceive.Date = ZDateTime.Now;
			bondedReceive.AdditionalReferences = Array.Empty<AdditionalReference>();
			return bondedReceive;
		}

		WhsBondedWarehouseTransactionLine CreateBondedWhsReceiveInventoryLine(
			WhsBondedWarehouseTransaction bondedReceive, OrgSupplierPart part, ZDecimal qty, string entryKey,
			ZShort entryLineNo, string pA1, string pA2, string pA3)
		{
			var bondedReceiveLine = new WhsBondedWarehouseTransactionLine();
			bondedReceiveLine.Warehouse = bondedReceive.Warehouse;
			bondedReceiveLine.Product = part;
			bondedReceiveLine.Quantity = qty;
			bondedReceiveLine.QuantityUnit = part.OP_StockKeepingUnit;
			bondedReceiveLine.EntryKey = entryKey;
			bondedReceiveLine.EntryLineNumber = entryLineNo;
			bondedReceiveLine.PartAttrib1 = pA1;
			bondedReceiveLine.PartAttrib2 = pA2;
			bondedReceiveLine.PartAttrib3 = pA3;
			bondedReceive.Lines.Add(bondedReceiveLine);
			return bondedReceiveLine;
		}

		#endregion

		#region TestProcess_InValidDefaultLocationInBondedArea

		public void TestProcess_InValidDefaultLocationInBondedArea()
		{
			var data = new TestDataForBondedEntriesWithBondIDs(Factory, processBondedInwardDuringConstruction: false);
			Factory.Save();

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "2 BOND ST";
			address.OA_OH = data.Org.PK;

			var warehouse = WhsBondedTransactionProcessor.CreateVirtualWarehouse(Factory, address);
			Factory.Save();

			warehouse.DefaultLocationInBondedArea.Delete(); // in case warehouse does not have valid location
			Factory.Save();
			AssertNull(warehouse.DefaultLocationInBondedArea);
			data.IReceiveLine1.Warehouse = address;

			var processor = new WhsBondedTransactionProcessor(Factory, data.IReceive);
			AssertExceptionThrown(typeof(CannotUpdateStockException), "The Bonded Warehouse has not been updated. This warehouse (2 BOND ST) does not have a default location in a Bonded Pick Area defined for stock receipt.", () => ProcessWithMock(processor), assertStartsWith: true);
		}

		#endregion

		#endregion

		#region WhsBondedOldProcessorTests
		#region BondedReceiveProcessorOldTests

		public void TestFirstCreateDoesNotInterfereWithTheSecond()
		{
			var tran1 = TestDataBuilder.GetSimpleTestData();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var receive1 = BondedHelperClass.LoadLastCreatedReceive(tran1, Factory);
			AssertInputDataWithDocketCreated(tran1, receive1);
			var tran2 = TestDataBuilder.GetDataWithMultipleLines();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran2);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var receive2 = BondedHelperClass.LoadLastCreatedReceive(tran2, Factory);
			AssertEquals("TRAN2 - check reference", "TRAN2-INW W00000002", receive2.WD_ExternalReference);
			AssertEquals("TRAN2 - Lines created", 3, receive2.Lines.Count);
			// reload to see docket created by tran 1 still there and unaltered in any manner
			receive1 = BondedHelperClass.LoadLastCreatedReceive(tran1, Factory);
			AssertInputDataWithDocketCreated(tran1, receive1);
		}

		[ExpectNoExceptions()]
		public void TestCallingFunctionTwiceDoesNotThrowException()
		{
			// Create First Entry
			var tran1 = TestDataBuilder.GetSimpleTestData();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
		}

		void AssertInputDataWithDocketCreated(IWhsBondedWarehouseTransaction bondedInputTransaction,
			WhsReceive whsReceiveCreated)
		{
			AssertEquals("Client value matches", bondedInputTransaction.Client.PK, whsReceiveCreated.Client.PK);
			AssertEquals("Warehouse address matches", bondedInputTransaction.Warehouse.PK,
				whsReceiveCreated.Warehouse.WW_OA_WarehouseAddress);
			AssertEquals("External reference matches", "TRAN1-INW W00000001", whsReceiveCreated.WD_ExternalReference);
			AssertEquals("Number of line matches", bondedInputTransaction.Lines.Count, whsReceiveCreated.Lines.Count);
			AssertEquals("Line product matches", bondedInputTransaction.Lines[0].Product.PK,
				whsReceiveCreated.Lines[0].WE_OP);
			AssertEquals("Line quantity matches", bondedInputTransaction.Lines[0].Quantity,
				whsReceiveCreated.Lines[0].WE_TransactionQuantity);
			AssertEquals("Line quantity unit matches", bondedInputTransaction.Lines[0].QuantityUnit,
				whsReceiveCreated.Lines[0].WE_F3_NKPackType);
			AssertEquals("Line quantity matches", bondedInputTransaction.Lines[0].Quantity,
				whsReceiveCreated.Lines[0].WE_TransactionQuantity);
			AssertEquals("Line quantity unit matches", bondedInputTransaction.Lines[0].QuantityUnit,
				whsReceiveCreated.Lines[0].WE_F3_NKPackType);
			AssertEquals("Line Entry matches", bondedInputTransaction.Lines[0].EntryKey,
				whsReceiveCreated.Lines[0].CustomsData.WB_EntryKey);
			AssertEquals("Line Entry line matches", bondedInputTransaction.Lines[0].EntryLineNumber,
				whsReceiveCreated.Lines[0].CustomsData.WB_EntryLineNo);
		}

		public void TestStockLevelOfSingleLineAfterMultipleTransactions()
		{
			var transaction = TestDataBuilder.GetSimpleTestData();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var initialReceive = BondedHelperClass.LoadLastCreatedReceive(transaction, Factory);
			AssertEquals("WD_ExternalReference", "TRAN1-INW W00000001", initialReceive.WD_ExternalReference);
			Assert("Check Finalised", initialReceive.IsFinalised);
			var line = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			line.Quantity = 5;
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var firstAdjustment = BondedHelperClass.LoadLastCreatedAdjustment(transaction, Factory);
			AssertEquals("WD_ExternalReference", "TRAN1-AMD W00000002", firstAdjustment.WD_ExternalReference);
			Assert("Check Finalised", firstAdjustment.IsFinalised);
			line = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			line.Quantity = 20;
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save(); // need to save as finalise uses a 2nd factory
			var secondAdjustment = BondedHelperClass.LoadLastCreatedAdjustment(transaction, Factory);
			AssertEquals("WD_ExternalReference", "TRAN1-AMD W00000004", secondAdjustment.WD_ExternalReference);
			Assert("Check Finalised", secondAdjustment.IsFinalised);
			var inventories =
				Helper.LoadInventory(transaction.Lines[0].EntryKey + "-" + transaction.Lines[0].EntryLineNumber);
			AssertEquals("Check Stock", 20m, inventories.UnitsTotal);
		}

		public void TestStockLevelsAfterMultipleEntriesAndCorrections()
		{
			var lines = new WhsBondedWarehouseTransactionLineCollection();
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 1, 100m));
			var tran1 = TestDataBuilder.SetUpTransaction(lines);
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran1);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			lines = new WhsBondedWarehouseTransactionLineCollection();
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 2, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 2, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 2, 100m));
			var tran2 = TestDataBuilder.SetUpTransaction(lines);
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran2);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			lines = new WhsBondedWarehouseTransactionLineCollection();
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 1, 150m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 2, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 2, 150m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 2, 100m));
			var tran3 = TestDataBuilder.SetUpTransaction(lines);
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran3);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			lines = new WhsBondedWarehouseTransactionLineCollection();
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod2, "TRAN2", 1, 150m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod3, "TRAN2", 1, 100m));
			lines.Add(TestDataBuilder.SetUpLine(TestDataBuilder.Prod1, "TRAN2", 2, 100m));
			var tran4 = TestDataBuilder.SetUpTransaction(lines);
			InWarehouse = new WhsBondedTransactionProcessor(Factory, tran4);
			ProcessWithMock(InWarehouse);
			Factory.Save(); // need to save as finalise uses a 2nd factory
			var inventories1 = Helper.LoadInventory("TRAN2-1");
			foreach (var inv in inventories1.Inventory)
			{
				if (inv.WI_OP == TestDataBuilder.Prod1.PK)
				{
					AssertEquals("Prod1 Stock", 100m, inv.WI_TotalUnits);
				}
				else if (inv.WI_OP == TestDataBuilder.Prod2.PK)
				{
					AssertEquals("Prod2 Stock", 150m, inv.WI_TotalUnits);
				}
				else if (inv.WI_OP == TestDataBuilder.Prod3.PK)
				{
					AssertEquals("Prod3 Stock", 100m, inv.WI_TotalUnits);
				}
				else
				{
					Fail("Not expected stock for this product:" + inv.SupplierPart.OP_PartNum);
				}
			}

			var inventories2 = Helper.LoadInventory("TRAN2-2");
			foreach (var inv in inventories2.Inventory)
			{
				if (inv.WI_OP == TestDataBuilder.Prod1.PK)
				{
					AssertEquals("Prod1 Stock", 100m, inv.WI_TotalUnits);
				}
				else
				{
					Fail("Not expected stock for this product:" + inv.SupplierPart.OP_PartNum);
				}
			}

			var inventories3 = Helper.LoadInventory(TestDataBuilder.Client, TestDataBuilder.Prod1);
			AssertEquals("Prod1 Stock", 200m, inventories3.UnitsTotal);
			var inventories4 = Helper.LoadInventory(TestDataBuilder.Client, TestDataBuilder.Prod2);
			AssertEquals("Prod2 Stock", 150m, inventories4.UnitsTotal);
			var inventories5 = Helper.LoadInventory(TestDataBuilder.Client, TestDataBuilder.Prod3);
			AssertEquals("Prod3 Stock", 100m, inventories5.UnitsTotal);
		}

		public void TestReference()
		{
			var transaction = TestDataBuilder.GetSimpleTestData();
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var lastReceive = BondedHelperClass.LoadLastCreatedReceive(transaction, Factory);
			AssertEquals("First Receive Created", "TRAN1-INW W00000001", lastReceive.WD_ExternalReference);
			var line1 = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			var line2 = line1.Clone();
			line2.EntryKey = "TRAN1";
			line2.EntryLineNumber = 2;
			line2.Quantity = 5;
			var bondedTransaction = (WhsBondedWarehouseTransaction)transaction;
			bondedTransaction.Lines = WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2);
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			lastReceive = BondedHelperClass.LoadLastCreatedReceive(transaction, Factory);
			AssertEquals("First Receive Created", "TRAN1-INW W00000003", lastReceive.WD_ExternalReference);
			line1 = (WhsBondedWarehouseTransactionLine)transaction.Lines[0];
			line1.EntryKey = "TRAN1";
			line1.EntryLineNumber = 2;
			line1.Quantity = 10;
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			var lastAdjustment = BondedHelperClass.LoadLastCreatedAdjustment(transaction, Factory);
			AssertEquals("First Adjustment Created", "TRAN1-AMD W00000004", lastAdjustment.WD_ExternalReference);
			line1.EntryKey = "TRAN1";
			line1.EntryLineNumber = 2;
			line1.Quantity = 15;
			InWarehouse = new WhsBondedTransactionProcessor(Factory, transaction);
			ProcessWithMock(InWarehouse);
			Factory.Save();
			lastAdjustment = BondedHelperClass.LoadLastCreatedAdjustment(transaction, Factory);
			AssertEquals("Second Adjustment Created", "TRAN1-AMD W00000006", lastAdjustment.WD_ExternalReference);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			TestDataBuilder = new DummyBondedWarehouseDataBuilder(Factory);
		}

		DummyBondedWarehouseDataBuilder TestDataBuilder;
		WhsBondedTransactionProcessor InWarehouse;

		#endregion

		#region BondedHelperClass

		public class BondedHelperClass
		{
			BondedHelperClass()
			{
			}

			static ZQuery GetFilter(IWhsBondedWarehouseTransaction inWarehouseTransaction, ZString docketType)
			{
				ZQuery filter = new ZQuery(WhsDocketSchema.WD_OH_Client, inWarehouseTransaction.Client.PK);
				filter.AddToFilter(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith,
					inWarehouseTransaction.Lines[0].EntryKey);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, docketType);
				filter.OrderBy = WhsDocketSchema.WD_ExternalReference.Name + OrderByClause.Descending;
				return filter;
			}

			public static WhsReceive[] LoadAllReceiveCreated(IWhsBondedWarehouseTransaction inWarehouseTransaction,
				BusinessObjectFactory factory)
			{
				return (WhsReceive[])factory.Load(typeof(WhsReceive),
					GetFilter(inWarehouseTransaction, CodeLists.DocketType.Codes.Receive));
			}

			public static WhsReceive LoadLastCreatedReceive(IWhsBondedWarehouseTransaction inWarehouseTransaction,
				BusinessObjectFactory factory)
			{
				return factory.LoadTop1<WhsReceive>(GetFilter(inWarehouseTransaction, CodeLists.DocketType.Codes.Receive));
			}

			public static WhsAdjustment[] LoadAllAdjustmentCreated(
				IWhsBondedWarehouseTransaction inWarehouseTransaction, BusinessObjectFactory factory)
			{
				return (WhsAdjustment[])factory.Load(typeof(WhsAdjustment),
					GetFilter(inWarehouseTransaction, CodeLists.DocketType.Codes.Adjustment));
			}

			public static WhsAdjustment LoadLastCreatedAdjustment(IWhsBondedWarehouseTransaction inWarehouseTransaction,
				BusinessObjectFactory factory)
			{
				return factory.LoadTop1<WhsAdjustment>(GetFilter(inWarehouseTransaction, CodeLists.DocketType.Codes.Adjustment));
			}
		}

		public class DummyBondedWarehouseDataBuilder
		{
			public DummyBondedWarehouseDataBuilder(BusinessObjectFactory factory)
			{
				this.Factory = factory;
				Helper = new WhsTestHelperFunctions(factory);
				Client = Helper.CreateClient("AKA", "PRAS");
				Helper.SetClientAttributeType(Client, AttributeNumber.One, true);
				Helper.SetClientAttributeType(Client, AttributeNumber.Two, true);
				Helper.SetClientAttributeType(Client, AttributeNumber.Three, true);
				var warehouseAddress = Client.MainAddress;
				warehouseAddress.OA_Address1 = "Eagle Rd";
				Warehouse = Helper.CreateWarehouse("AAA");
				Warehouse.SetUpBondedWarehouse(warehouseAddress);
				Warehouse2 = Helper.CreateWarehouse("WHS2");
				Warehouse2.SetUpBondedWarehouse(Warehouse2.WarehouseAddress);
				Prod1 = Helper.CreateProduct(Client, "PROD1");
				Prod2 = Helper.CreateProduct(Client, "PROD2");
				Prod3 = Helper.CreateProduct(Client, "PROD3");
				Prod4 = Helper.CreateProduct(Client, "PROD4");
				ProdAttr1 = Helper.CreateProduct(Client, "ProdAttr1");
				ProdAttr2 = Helper.CreateProduct(Client, "ProdAttr2");
				ProdAttr3 = Helper.CreateProduct(Client, "ProdAttr3");
				ProdAttrAll = Helper.CreateProduct(Client, "ProdAttrAll");
				Helper.SetProductAttributeUse(Client, ProdAttr1, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Client, ProdAttr2, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(Client, ProdAttr3, AttributeNumber.Three, true);
				Helper.SetProductAttributeUse(Client, ProdAttrAll, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(Client, ProdAttrAll, AttributeNumber.Two, true);
				Helper.SetProductAttributeUse(Client, ProdAttrAll, AttributeNumber.Three, true);
				factory.Save();
			}

			public WhsWarehouse Warehouse;
			public WhsWarehouse Warehouse2;
			public readonly OrgHeader Client;
			public readonly OrgSupplierPart Prod1;
			public readonly OrgSupplierPart Prod2;
			public readonly OrgSupplierPart Prod3;
			public readonly OrgSupplierPart Prod4;
			public readonly OrgSupplierPart ProdAttr1;
			public readonly OrgSupplierPart ProdAttr2;
			public readonly OrgSupplierPart ProdAttr3;
			public readonly OrgSupplierPart ProdAttrAll;
			public readonly WhsTestHelperFunctions Helper;
			readonly BusinessObjectFactory Factory;

			public IWhsBondedWarehouseTransaction GetSimpleTestData()
			{
				var line1 = SetUpLine("TRAN1", 1, 10m);
				return SetUpTransaction(line1);
			}

			public IWhsBondedWarehouseTransaction GetSimpleTestDataWithAttributes()
			{
				// the entry key and attributes have lower case characters for a reason, dont change this - CM
				var line1 = SetUpLine(ProdAttrAll, "Tran1", 2, 10m, "Pa1", "Pa2", "Pa3");
				return SetUpTransaction(line1);
			}

			public IWhsBondedWarehouseTransaction GetSimpleTestDataVOC()
			{
				var tran = (WhsBondedWarehouseTransaction)GetSimpleTestData();
				var line = (WhsBondedWarehouseTransactionLine)tran.Lines[0];
				line.CustomsQuantity = 5;
				line.BondedWarehouseQuantity = 5;
				line.Quantity = 5;
				return tran;
			}

			public IWhsBondedWarehouseTransaction GetDataWithMultipleLines()
			{
				var line1 = SetUpLine("TRAN2", 1, 5m);
				var line2 = SetUpLine("TRAN2", 2, 10m);
				var line3 = SetUpLine("TRAN2", 3, 15m);
				return SetUpTransaction(WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2, line3));
			}

			public IWhsBondedWarehouseTransaction GetDataWithMultipleLinesVOC()
			{
				var line1 = SetUpLine("TRAN2", 1, 5m);
				var line2 = SetUpLine("TRAN2", 2, 20m);
				var line3 = SetUpLine("TRAN2", 3, 15m);
				return SetUpTransaction(WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2, line3));
			}

			public IWhsBondedWarehouseTransaction GetDataWithMultipleLinesVOCDifferentWarehouse()
			{
				var transaction = (WhsBondedWarehouseTransaction)GetDataWithMultipleLinesVOC();
				transaction.Warehouse = Warehouse2.WarehouseAddress;
				return transaction;
			}

			public IWhsBondedWarehouseTransaction GetTestDataWithNoExistingWarehouse()
			{
				var testData = (WhsBondedWarehouseTransaction)GetSimpleTestData();
				var address = Factory.New<OrgAddress>();
				address.OA_Address1 = "Eagle Ltd";
				testData.Warehouse = address;
				return testData;
			}

			public IWhsBondedWarehouseTransaction GetTestDataForNonVirtualWarehouse()
			{
				Warehouse.WW_GB_RelatedCompanyBranch = Factory.New(typeof(GlbBranch)).PK;
				return GetSimpleTestData();
			}

			public IWhsBondedWarehouseTransaction GetTestDataForBothWarehouses()
			{
				// Non Virtual / Normal Warehouse
				var nonVirtualWarehouse = Helper.CreateWarehouse("NONVIRTUAL1");
				nonVirtualWarehouse.WW_OA_WarehouseAddress = Warehouse.WW_OA_WarehouseAddress;
				var nonVirtualWarehouse2 = Helper.CreateWarehouse("NONVIRTUAL2");
				nonVirtualWarehouse2.WW_OA_WarehouseAddress = Warehouse.WW_OA_WarehouseAddress;
				// Virtual Warehouse
				var virtualWarehouse = Helper.CreateWarehouse("VIRTUAL1");
				virtualWarehouse.WW_OA_WarehouseAddress = Warehouse.WW_OA_WarehouseAddress;
				virtualWarehouse.WW_GB_RelatedCompanyBranch = ZGuid.Empty;
				virtualWarehouse.WW_IsVirtualWarehouse = true;
				var virtualWarehouse2 = Helper.CreateWarehouse("VIRTUAL2");
				virtualWarehouse2.WW_OA_WarehouseAddress = Warehouse.WW_OA_WarehouseAddress;
				virtualWarehouse2.WW_GB_RelatedCompanyBranch = ZGuid.Empty;
				virtualWarehouse2.WW_IsVirtualWarehouse = true;
				// Clear the old warehose
				Warehouse.WW_OA_WarehouseAddress = ZGuid.NewZGuid();
				Warehouse = nonVirtualWarehouse2;
				return GetSimpleTestData();
			}

			public IWhsBondedWarehouseTransaction GetDataProductWithoutOwner()
			{
				var line1 = SetUpLine("TRAN1", 1, 10m);
				var line2 = SetUpLine("TRAN1", 2, 10m);
				((OrgSupplierPart)line1.Product).RelatedOrganisations.RemoveAndDeleteAll();
				return SetUpTransaction(WhsBondedWarehouseTransactionLineCollection.GetNew(line1, line2));
			}

			public IWhsBondedWarehouseTransaction GetProductWithAttribute1()
			{
				var line1 = SetUpLine(ProdAttr1, "TRAN1", 1, 10m, "A01", "", "");
				return SetUpTransaction(line1);
			}

			public IWhsBondedWarehouseTransaction GetProductWithAttribute2()
			{
				var line1 = SetUpLine(ProdAttr2, "TRAN1", 1, 10m, "", "A02", "");
				return SetUpTransaction(line1);
			}

			public IWhsBondedWarehouseTransaction GetProductWithAttribute3()
			{
				var line1 = SetUpLine(ProdAttr3, "TRAN1", 1, 10m, "", "", "A03");
				return SetUpTransaction(line1);
			}

			public IWhsBondedWarehouseTransaction GetEntryLineWithMultipleProduct()
			{
				var lines = new WhsBondedWarehouseTransactionLineCollection();
				lines.Add(SetUpLine(Prod1, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod1, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod4, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod1, "TRAN2", 3, 20m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 3, 20m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 3, 20m));
				return SetUpTransaction(lines);
			}

			public IWhsBondedWarehouseTransaction GetEntryLineWithMultipleProductVOC()
			{
				var lines = new WhsBondedWarehouseTransactionLineCollection();
				lines.Add(SetUpLine(Prod1, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 1, 10m));
				lines.Add(SetUpLine(Prod1, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 2, 5m));
				lines.Add(SetUpLine(Prod1, "TRAN2", 3, 20m));
				lines.Add(SetUpLine(Prod2, "TRAN2", 3, 20m));
				lines.Add(SetUpLine(Prod3, "TRAN2", 3, 20m));
				return SetUpTransaction(lines);
			}

			public WhsBondedWarehouseTransaction SetUpTransaction(IWhsBondedWarehouseTransactionLine line)
			{
				return SetUpTransaction(WhsBondedWarehouseTransactionLineCollection.GetNew(line));
			}

			public WhsBondedWarehouseTransaction SetUpTransaction(WhsBondedWarehouseTransactionLineCollection lines)
			{
				var inward = new WhsBondedWarehouseTransaction();
				inward.Client = Client;
				inward.Warehouse = Warehouse.WarehouseAddress;
				inward.Date = ZDateTime.Today;
				inward.TransportCompany = Client;
				inward.Reference = "DeclarationReference1";
				inward.AdditionalReferences = new AdditionalReference[]
				{
					new AdditionalReference("MAR", "555555"), new AdditionalReference("MAR", "666666")
				};
				inward.Lines = lines;
				return inward;
			}

			public WhsBondedWarehouseTransactionLine SetUpLine(OrgSupplierPart prod, ZString entryKey,
				ZShort entryLineNo, ZDecimal quantity)
			{
				return SetUpLine(prod, entryKey, entryLineNo, quantity, "", "", "");
			}

			public WhsBondedWarehouseTransactionLine SetUpLine(ZString entryKey, ZShort entryLineNo, ZDecimal quantity)
			{
				return SetUpLine(Prod1, entryKey, entryLineNo, quantity, "", "", "");
			}

			WhsBondedWarehouseTransactionLine SetUpLine(OrgSupplierPart part, ZString entryKey, ZShort entryLineNo,
				ZDecimal quantity, ZString attr1, ZString attr2, ZString attr3)
			{
				var inwardLine = new WhsBondedWarehouseTransactionLine();
				inwardLine.Product = part;
				inwardLine.EntryKey = entryKey;
				inwardLine.EntryLineNumber = entryLineNo;
				inwardLine.EntryDate = ZDateTime.Today;
				inwardLine.Quantity = quantity;
				inwardLine.QuantityUnit = "UNT";
				inwardLine.PartAttrib1 = attr1;
				inwardLine.PartAttrib2 = attr2;
				inwardLine.PartAttrib3 = attr3;
				inwardLine.ValueForDuty = 5.00m;
				inwardLine.TILV = new Money(5.00m, GlbCompany.CurrentCompany.LocalCurrency);
				inwardLine.CountryOfOrigin = RefCountry.LoadFromCountryCode(Factory, "AU");
				inwardLine.AddInfo = "Bonded AddInfo";
				inwardLine.BondedWarehouseQuantity = quantity;
				inwardLine.BondedWarehouseQuantityUnit = "L";
				inwardLine.CustomsQuantity = quantity;
				inwardLine.CustomsQuantityUnit = "L";
				inwardLine.CustomsSecondQuantity = quantity;
				inwardLine.CustomsSecondQuantityUnit = "M3";
				inwardLine.CustomsThirdQuantity = quantity;
				inwardLine.CustomsThirdQuantityUnit = "CM3";
				return inwardLine;
			}
		}

		#endregion

		#endregion

		#region BondedReceiveCreatorOldTests

		public void TestProduce()
		{
			var bondedTransaction = TestDataBuilder.GetSimpleTestData();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			ProcessWithMock(inwardCreator);
			Factory.Save();
			var whsReceiveCreated = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			// Check all the data are created properly
			AssertHeaderData(bondedTransaction, whsReceiveCreated);
			AssertLineData(bondedTransaction, whsReceiveCreated);
			Assert("Docket was putaway", whsReceiveCreated.Inventory.Count > 0);
			Assert("Docket created must be finalise", whsReceiveCreated.IsFinalised);
		}

		void AssertHeaderData(IWhsBondedWarehouseTransaction bondedTransaction, WhsReceive whsReceiveCreated)
		{
			AssertEquals("Client value matches", bondedTransaction.Client.PK, whsReceiveCreated.Client.PK);
			AssertEquals("Warehouse address matches", bondedTransaction.Warehouse.PK,
				whsReceiveCreated.Warehouse.WW_OA_WarehouseAddress);
			AssertEquals("External reference matches", "TRAN1-INW W00000001", whsReceiveCreated.WD_ExternalReference);
			AssertEquals("Arrival date matches", whsReceiveCreated.Warehouse.GetWarehouseBranchLocalDateTimeOffset(bondedTransaction.Date), whsReceiveCreated.WD_ArrivalDate);
			AssertEquals("Transport company matches", bondedTransaction.TransportCompany.PK,
				whsReceiveCreated.TransportCoPK);
			AssertEquals("Same number of reference for the docket", bondedTransaction.AdditionalReferences.Count(),
				whsReceiveCreated.References.Count);
			AssertEquals("Reference Type matches", bondedTransaction.AdditionalReferences.ElementAt(0).Type,
				whsReceiveCreated.References[0].WX_RefType);
			AssertEquals("Reference Value matches", bondedTransaction.AdditionalReferences.ElementAt(0).Value,
				whsReceiveCreated.References[0].WX_Reference);
		}

		void AssertLineData(IWhsBondedWarehouseTransaction bondedTransaction, WhsReceive whsReceiveCreated)
		{
			var line = bondedTransaction.Lines[0];
			AssertEquals("Number of line matches", bondedTransaction.Lines.Count, whsReceiveCreated.Lines.Count);
			AssertEquals("Line product matches", line.Product.PK, whsReceiveCreated.Lines[0].WE_OP);
			AssertEquals("Line quantity matches", line.Quantity, whsReceiveCreated.Lines[0].WE_TransactionQuantity);
			AssertEquals("PartAttrib1", line.PartAttrib1, whsReceiveCreated.Lines[0].WE_PartAttrib1);
			AssertEquals("PartAttrib2", line.PartAttrib2, whsReceiveCreated.Lines[0].WE_PartAttrib2);
			AssertEquals("PartAttrib3", line.PartAttrib3, whsReceiveCreated.Lines[0].WE_PartAttrib3);
			AssertEquals("Line quantity unit matches", line.QuantityUnit, whsReceiveCreated.Lines[0].WE_F3_NKPackType);
			AssertEquals("Line Entry Key", line.EntryKey, whsReceiveCreated.Lines[0].CustomsData.WB_EntryKey);
			AssertEquals("Line Entry line matches", line.EntryLineNumber,
				whsReceiveCreated.Lines[0].CustomsData.WB_EntryLineNo);
			AssertEquals("Line country of origin matches", line.CountryOfOrigin.RN_Code,
				whsReceiveCreated.Lines[0].CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("Line DutiableAmount matches", line.ValueForDuty,
				whsReceiveCreated.Lines[0].CustomsData.WB_ValueForDuty);
			AssertEquals("TILV Amount", line.TILV.Amount, whsReceiveCreated.Lines[0].CustomsData.WB_TILV);
			AssertEquals("TILV Currency", line.TILV.Currency.Code,
				whsReceiveCreated.Lines[0].CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("Add Info matches", line.AddInfo, whsReceiveCreated.Lines[0].CustomsData.WB_AddInfo);
			AssertEquals("Bonded Whs quantity matches", line.BondedWarehouseQuantity,
				whsReceiveCreated.Lines[0].CustomsData.WB_BondedWhsQty);
			AssertEquals("Bonded Whs quantity unit matches", line.BondedWarehouseQuantityUnit,
				whsReceiveCreated.Lines[0].CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("Custom quantity matches", line.CustomsQuantity,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsQty);
			AssertEquals("Custom quantity unit matches", line.CustomsQuantityUnit,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("Custom Second quantity matches", line.CustomsSecondQuantity,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("Custom Second quantity unit matches", line.CustomsSecondQuantityUnit,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("Custom Third quantity matches", line.CustomsThirdQuantity,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("Custom Third quantity unit matches", line.CustomsThirdQuantityUnit,
				whsReceiveCreated.Lines[0].CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("Declaration Reference", bondedTransaction.Reference,
				whsReceiveCreated.Lines[0].CustomsData.WB_DeclarationReference);
		}

		public void TestProductWithoutOwner()
		{
			var bondedTransaction = TestDataBuilder.GetDataProductWithoutOwner();
			var receiveCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			ProcessWithMock(receiveCreator);
			var whsReceiveCreated = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			var part = Factory.Load<OrgSupplierPart>(whsReceiveCreated.Lines[0].WE_OP);
			AssertNotNull("Owner must be created",
				part.RelatedOrganisations.FindByOrganisationPKAndRelationship(whsReceiveCreated.Client.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			Assert("Docket was putaway", whsReceiveCreated.Inventory.Count > 0);
			Assert("Docket created must be finalise", whsReceiveCreated.IsFinalised);
		}

		public void TestProductWithOwnerCausesNoProblem()
		{
			var bondedTransaction = TestDataBuilder.GetSimpleTestData();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			var part = Factory.Load<OrgSupplierPart>(bondedTransaction.Lines[0].Product.PK);
			AssertNotNull("Precondtion - Owner must be created",
				part.RelatedOrganisations.FindByOrganisationPKAndRelationship(bondedTransaction.Client.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			var filter = new ZQuery();
			filter.AddToFilter(OrgPartRelationSchema.OU_OH, bondedTransaction.Client.PK);
			filter.AddToFilter(OrgPartRelationSchema.OU_OP, part.PK);
			filter.AddToFilter(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Owner);
			var relationshipList = (OrgPartRelation[])part.RelatedOrganisations.Find(filter);
			AssertEquals("Precondition - single relationship", 1, relationshipList.Length);
			ProcessWithMock(inwardCreator);
			var whsReceiveCreated = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			Assert("Docket was putaway", whsReceiveCreated.Inventory.Count > 0);
			Assert("Docket created must be finalise", whsReceiveCreated.IsFinalised);
			part = Factory.Load<OrgSupplierPart>(whsReceiveCreated.Lines[0].WE_OP);
			AssertNotNull("Has Product Client Relationship",
				part.RelatedOrganisations.FindByOrganisationPKAndRelationship(whsReceiveCreated.Client.PK,
					OrgPartRelation.RelationshipTypes.Owner));
			relationshipList = (OrgPartRelation[])part.RelatedOrganisations.Find(filter);
			AssertEquals("Make sure it hasnt added another one", 1, relationshipList.Length);
		}

		#endregion

		#region BondedReceiveUpdaterOldTests

		public void TestSimpleInWarehouse()
		{
			var bondedTransaction = TestDataBuilder.GetSimpleTestData();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			ProcessWithMock(inwardCreator);
			Factory.Save();
			var initialEntry = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			AssertIsFinalisedPrecondition(initialEntry);
			var correctionTransaction = TestDataBuilder.GetSimpleTestDataVOC();
			var inwardUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			ProcessWithMock(inwardUpdater);
			var correctionEntry = BondedHelperClass.LoadLastCreatedAdjustment(bondedTransaction, Factory);
			Assert("Adjustment Finalised", correctionEntry.IsFinalised);
		}

		public void TestAdjustmentCreated()
		{
			var initialTransaction = TestDataBuilder.GetSimpleTestData();
			var receiveCreator = new WhsBondedTransactionProcessor(Factory, initialTransaction);
			ProcessWithMock(receiveCreator);
			Factory.Save();
			var originalReceive = BondedHelperClass.LoadLastCreatedReceive(initialTransaction, Factory);
			AssertIsFinalisedPrecondition(originalReceive);
			var correctionTransaction = TestDataBuilder.GetSimpleTestDataVOC();
			var receiveUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			ProcessWithMock(receiveUpdater);
			Factory.Save();
			var adjustmentToCorrect = BondedHelperClass.LoadLastCreatedAdjustment(correctionTransaction, Factory);
			Assert("Adjustment Finalised", adjustmentToCorrect.IsFinalised);
			AssertEquals("Client value matches", correctionTransaction.Client.PK, adjustmentToCorrect.Client.PK);
			AssertEquals("Warehouse address matches", correctionTransaction.Warehouse.PK,
				adjustmentToCorrect.Warehouse.WW_OA_WarehouseAddress);
			AssertEquals("External reference matches", "TRAN1-AMD W00000002", adjustmentToCorrect.WD_ExternalReference);
			AssertEquals("No Ref", 0, adjustmentToCorrect.References.Count);
			AssertEquals("1 Line Created", 1, adjustmentToCorrect.Lines.Count);
			var removeStockLine =
				GetLineMatchinTheseCase(adjustmentToCorrect, "TRAN1", 1, AdjustmentLineType.Negative);
			AssertAdjustmentLineCreated(initialTransaction,
				initialTransaction.Lines[0], removeStockLine,
				AdjustmentLineType.Negative);
			//WhsAdjustmentLine AddStockLine = GetLineMatchinTheseCase(AdjustmentToCorrect, "TRAN1", (short)1, AdjustmentLineType.Positive);
			//AssertAdjustmentLineCreated(CorrectionTransaction, (IWhsBondedWarehouseTransactionLine)CorrectionTransaction.Lines[0], AddStockLine, AdjustmentLineType.Positive);
		}

		public void TestAdjustmentCreatedToDeleteLinesOnly()
		{
			var initialTransaction = TestDataBuilder.GetSimpleTestData();
			var receiveCreator = new WhsBondedTransactionProcessor(Factory, initialTransaction);
			ProcessWithMock(receiveCreator);
			Factory.Save();
			var originalReceiveCreated = BondedHelperClass.LoadLastCreatedReceive(initialTransaction, Factory);
			AssertIsFinalisedPrecondition(originalReceiveCreated);
			AssertEquals("External Reference", "TRAN1-INW W00000001", originalReceiveCreated.WD_ExternalReference);
			var correctionTransaction = TestDataBuilder.GetSimpleTestData();
			var line = (WhsBondedWarehouseTransactionLine)correctionTransaction.Lines[0];
			//Line.EntryLineNumber = 2;
			var receiveUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			ProcessWithMock(receiveUpdater);
			Factory.Save();
			var adjustmentToCorrect = BondedHelperClass.LoadLastCreatedAdjustment(correctionTransaction, Factory);
			Assert("Adjustment Finalised", adjustmentToCorrect.IsFinalised);
			AssertEquals("Client value matches", correctionTransaction.Client.PK, adjustmentToCorrect.Client.PK);
			AssertEquals("Warehouse address matches", correctionTransaction.Warehouse.PK,
				adjustmentToCorrect.Warehouse.WW_OA_WarehouseAddress);
			AssertEquals("External reference matches", "TRAN1-AMD W00000002", adjustmentToCorrect.WD_ExternalReference);
			AssertEquals("AdjustmentToCorrect.Lines.Count", 1, adjustmentToCorrect.Lines.Count);
			var removeStockLine =
				GetLineMatchinTheseCase(adjustmentToCorrect, "TRAN1", 1, AdjustmentLineType.Negative);
			AssertAdjustmentLineCreated(initialTransaction,
				initialTransaction.Lines[0], removeStockLine,
				AdjustmentLineType.Negative);
		}

		public void TestNoAdjustmentRequired()
		{
			var initialTransaction = TestDataBuilder.GetSimpleTestData();
			var receiveCreator = new WhsBondedTransactionProcessor(Factory, initialTransaction);
			ProcessWithMock(receiveCreator);
			Factory.Save();
			var originalReceive = BondedHelperClass.LoadLastCreatedReceive(initialTransaction, Factory);
			AssertIsFinalisedPrecondition(originalReceive);
			//WhsBondedTransactionProcessor InwardUpdater = new WhsBondedTransactionProcessor(Factory, InitialTransaction);
			//InwardUpdater.Process();
			var adjustment = BondedHelperClass.LoadLastCreatedAdjustment(initialTransaction, Factory);
			AssertNull("Adjustment is not created", adjustment);
		}

		//public void TestWithMultipleEntryNumber()
		//{
		//    IWhsBondedWarehouseTransaction InitialTransaction = TestDataBuilder.GetDataWithMultipleLines();
		//    WhsBondedTransactionProcessor InwardCreator = new WhsBondedTransactionProcessor(Factory, InitialTransaction);
		//    InwardCreator.Process();
		//    Factory.Save();
		//    WhsReceive InitialEntry = BondedHelperClass.LoadLastCreatedInward(InitialTransaction, Factory);
		//    AssertIsFinalisedPrecondition(InitialEntry);
		//    IWhsBondedWarehouseTransaction CorrectionTransaction = TestDataBuilder.GetDataWithMultipleLinesVOC();
		//    WhsBondedTransactionProcessor InwardUpdater = new WhsBondedTransactionProcessor(Factory, CorrectionTransaction);
		//    InwardUpdater.Process();
		//    WhsAdjustment CorrectionEntry = BondedHelperClass.LoadLastCreatedAdjustment(CorrectionTransaction, Factory);
		//    Assert("Adjustment Finalised", CorrectionEntry.IsFinalised);
		//    AssertEquals("One entry recreated = 3 adj lines", 3, CorrectionEntry.Lines.Count);
		//    WhsAdjustmentLine RemoveStockLine = GetLineMatchinTheseCase(CorrectionEntry, "TRAN2", (short)2, AdjustmentLineType.Negative);
		//    AssertEquals("EntryNo", "TRAN2", RemoveStockLine.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)2, (short)RemoveStockLine.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", -10m, RemoveStockLine.WE_TransactionQuantity);
		//    WhsAdjustmentLine AddStockLine = GetLineMatchinTheseCase(CorrectionEntry, "TRAN2", (short)2, AdjustmentLineType.Positive);
		//    AssertEquals("EntryNo", "TRAN2", AddStockLine.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)2, (short)AddStockLine.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", 20m, AddStockLine.WE_TransactionQuantity);
		//}
		//public void TestWithChangingWarehouse()
		//{
		//    IWhsBondedWarehouseTransaction InitialTransaction = TestDataBuilder.GetDataWithMultipleLines();
		//    WhsBondedTransactionProcessor InwardCreator = new WhsBondedTransactionProcessor(Factory, InitialTransaction);
		//    InwardCreator.Process();
		//    Factory.Save();
		//    WhsReceive InitialEntry = BondedHelperClass.LoadLastCreatedInward(InitialTransaction, Factory);
		//    AssertIsFinalisedPrecondition(InitialEntry);
		//    IWhsBondedWarehouseTransaction CorrectionTransaction = TestDataBuilder.GetDataWithMultipleLinesVOCDifferentWarehouse();
		//    WhsBondedTransactionProcessor InwardUpdater = new WhsBondedTransactionProcessor(Factory, CorrectionTransaction);
		//    InwardUpdater.Process();
		//    WhsAdjustment[] AdjustmentCreated = BondedHelperClass.LoadAllAdjustmentCreated(CorrectionTransaction, Factory);
		//    AssertEquals("Two adjustment were created", 2, AdjustmentCreated.Length);
		//    Assert("Adjustment1 Finalised", AdjustmentCreated[0].IsFinalised);
		//    Assert("Adjustment2 Finalised", AdjustmentCreated[1].IsFinalised);
		//    AssertEquals("Both should have 3 line", 3, AdjustmentCreated[0].Lines.Count);
		//    AssertEquals("Both should have 3 line", 3, AdjustmentCreated[1].Lines.Count);
		//    WhsAdjustment AdjustInStock = GetDocketWithReference(AdjustmentCreated, "TRAN2-AMD-01");
		//    WhsAdjustment AdjustOutStock = GetDocketWithReference(AdjustmentCreated, "TRAN2-AMD-02");
		//    AssertEquals("AdjustOut Same Client", CorrectionTransaction.Client, AdjustOutStock.Client);
		//    AssertEquals("AdjustOut Old Warehouse", TestDataBuilder.Warehouse, AdjustOutStock.Warehouse);
		//    AssertEquals("AdjustIn Same Client", CorrectionTransaction.Client, AdjustInStock.Client);
		//    AssertEquals("AdjustIn New Warehouse", TestDataBuilder.Warehouse2, AdjustInStock.Warehouse);
		//    WhsAdjustmentLine RemoveStockLine1 = GetLineMatchinTheseCase(AdjustOutStock, "TRAN2", (short)1, AdjustmentLineType.Negative);
		//    AssertEquals("EntryNo", "TRAN2", RemoveStockLine1.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)1, (short)RemoveStockLine1.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", -5m, RemoveStockLine1.WE_TransactionQuantity);
		//    WhsAdjustmentLine RemoveStockLine2 = GetLineMatchinTheseCase(AdjustOutStock, "TRAN2", (short)2, AdjustmentLineType.Negative);
		//    AssertEquals("EntryNo", "TRAN2", RemoveStockLine2.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)2, (short)RemoveStockLine2.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", -10m, RemoveStockLine2.WE_TransactionQuantity);
		//    WhsAdjustmentLine RemoveStockLine3 = GetLineMatchinTheseCase(AdjustOutStock, "TRAN2", (short)3, AdjustmentLineType.Negative);
		//    AssertEquals("EntryNo", "TRAN2", RemoveStockLine3.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)3, (short)RemoveStockLine3.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", -15m, RemoveStockLine3.WE_TransactionQuantity);
		//    WhsAdjustmentLine AddStockLine1 = GetLineMatchinTheseCase(AdjustInStock, "TRAN2", (short)1, AdjustmentLineType.Positive);
		//    AssertEquals("EntryNo", "TRAN2", AddStockLine1.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)1, (short)AddStockLine1.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", 5m, AddStockLine1.WE_TransactionQuantity);
		//    WhsAdjustmentLine AddStockLine2 = GetLineMatchinTheseCase(AdjustInStock, "TRAN2", (short)2, AdjustmentLineType.Positive);
		//    AssertEquals("EntryNo", "TRAN2", AddStockLine2.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)2, (short)AddStockLine2.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", 20m, AddStockLine2.WE_TransactionQuantity);
		//    WhsAdjustmentLine AddStockLine3 = GetLineMatchinTheseCase(AdjustInStock, "TRAN2", (short)3, AdjustmentLineType.Positive);
		//    AssertEquals("EntryNo", "TRAN2", AddStockLine3.CustomsData.WB_EntryKey);
		//    AssertEquals("LineNo", (short)3, (short)AddStockLine3.CustomsData.WB_EntryLineNo);
		//    AssertEquals("Units", 15m, AddStockLine3.WE_TransactionQuantity);
		//}
		public void TestMultipleProductInSingleEntryLineNo()
		{
			var initialTransaction = TestDataBuilder.GetEntryLineWithMultipleProduct();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, initialTransaction);
			ProcessWithMock(inwardCreator);
			Factory.Save();
			var initialEntry = BondedHelperClass.LoadLastCreatedReceive(initialTransaction, Factory);
			AssertIsFinalisedPrecondition(initialEntry);
			var correctionTransaction = TestDataBuilder.GetEntryLineWithMultipleProductVOC();
			var inwardUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			ProcessWithMock(inwardUpdater);
			Factory.Save(); // need to save as finalise uses a 2nd factory
			var correctionEntry = BondedHelperClass.LoadLastCreatedAdjustment(correctionTransaction, Factory);
			AssertNotNull("Adjustment Created", correctionEntry);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod1, "TRAN2-1", 10m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod2, "TRAN2-1", 10m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod3, "TRAN2-1", 10m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod1, "TRAN2-2", 5m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod2, "TRAN2-2", 5m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod3, "TRAN2-2", 5m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod1, "TRAN2-3", 20m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod2, "TRAN2-3", 20m);
			AssertStockLevelByBondedEntryKey(TestDataBuilder.Client, TestDataBuilder.Prod3, "TRAN2-3", 20m);
		}

		public void TestUpdateProcessIsWorkingAfterManualAdjustment()
		{
			var bondedTransaction = TestDataBuilder.GetSimpleTestData();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			ProcessWithMock(inwardCreator);
			Factory.Save();
			var manualAdjustment = Helper.CreateWhsAdjustment(TestDataBuilder.Client, TestDataBuilder.Warehouse,
				"MANADJ", Helper.Notify);
			manualAdjustment.WD_DocketSubType = AdjustmentType.Codes.Customs;
			var line = Helper.CreateWhsAdjustmentLine(manualAdjustment, TestDataBuilder.Prod1, -4, "BOND");
			line.WE_BondedEntryKey = "TRAN1-1";
			manualAdjustment.FinaliseDocket();
			Factory.Save();
			Assert("Docket successfully finalised", manualAdjustment.IsFinalised);
			var initialEntry = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			AssertIsFinalisedPrecondition(initialEntry);
			var correctionTransaction = TestDataBuilder.GetSimpleTestDataVOC();
			var inwardUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			ProcessWithMock(inwardUpdater);
			Factory.Save(); // need to save as finalise uses a 2nd factory
			var correctionEntry = BondedHelperClass.LoadLastCreatedAdjustment(bondedTransaction, Factory);
			Assert("Adjustment Finalised", correctionEntry.IsFinalised);
			var inventories = Helper.LoadInventory("TRAN1-1");
			AssertEquals("Final Unit count", 5m, inventories.UnitsAvailable);
		}

		void AssertStockLevelByBondedEntryKey(OrgHeader client, OrgSupplierPart part, string bondedEntryKey,
			decimal units)
		{
			var stockByBondedEntryKey = Helper.LoadInventory(client, part, bondedEntryKey);
			AssertEquals("Stock Level", units, stockByBondedEntryKey.UnitsTotal);
		}

		[ExpectExceptionMessage(typeof(CannotUpdateStockException),
			"Stock Correction failed on Entry Key:TRAN1 and Entry Line Number:1 as ExWarehouse transactions have already taken place for this Entry Number. Users need to manually correct stock in the Warehouse -> Adjustment screen.")]
		public void TestCannotUpdateStockException()
		{
			var bondedTransaction = TestDataBuilder.GetSimpleTestData();
			var inwardCreator = new WhsBondedTransactionProcessor(Factory, bondedTransaction);
			ProcessWithMock(inwardCreator);
			Factory.Save();
			var initialEntry = BondedHelperClass.LoadLastCreatedReceive(bondedTransaction, Factory);
			AssertIsFinalisedPrecondition(initialEntry);
			var order = TestDataBuilder.Helper.CreateWhsOrder(TestDataBuilder.Client, TestDataBuilder.Warehouse);
			TestDataBuilder.Helper.CreateWhsOrderLine(order, TestDataBuilder.Prod1, 10, "TRAN1-1", "DummyOutward-1",
				"");
			var pick = TestDataBuilder.Helper.CreatePick_OLD(order);
			Factory.Save();
			var correctionTransaction = TestDataBuilder.GetSimpleTestDataVOC();
			var inwardUpdater = new WhsBondedTransactionProcessor(Factory, correctionTransaction);
			inwardUpdater.Process();
		}

		enum AdjustmentLineType
		{
			Negative,
			Positive
		}

		WhsAdjustmentLine GetLineMatchinTheseCase(WhsAdjustment adjustmentDocket, string entryKey, short entryLineNo,
			AdjustmentLineType lineType)
		{
			WhsAdjustmentLine result = null;
			foreach (WhsAdjustmentLine line in adjustmentDocket.Lines)
			{
				if (entryKey == line.CustomsData.WB_EntryKey && entryLineNo == line.CustomsData.WB_EntryLineNo)
				{
					if (lineType == AdjustmentLineType.Positive && line.IsAdjustmentIn)
					{
						result = line;
					}
					else if (lineType == AdjustmentLineType.Negative && line.IsAdjustmentOut)
					{
						result = line;
					}
				}
			}

			return result;
		}

		void AssertAdjustmentLineCreated(IWhsBondedWarehouseTransaction originalTransaction,
			IWhsBondedWarehouseTransactionLine originalLine, WhsAdjustmentLine actualLineCreated,
			AdjustmentLineType type)
		{
			AssertEquals("Product", originalLine.Product.PK, actualLineCreated.WE_OP);
			AssertEquals("Bond1", originalLine.PartAttrib1, actualLineCreated.WE_PartAttrib1);
			AssertEquals("Bond2", originalLine.PartAttrib2, actualLineCreated.WE_PartAttrib2);
			AssertEquals("Bond3", originalLine.PartAttrib3, actualLineCreated.WE_PartAttrib3);
			AssertEquals("Units", (type == AdjustmentLineType.Negative ? -1 : 1) * originalLine.Quantity,
				actualLineCreated.WE_TransactionQuantity);
			AssertEquals("BondedWarehouseQuantity", originalLine.BondedWarehouseQuantity,
				actualLineCreated.CustomsData.WB_BondedWhsQty);
			AssertEquals("CustomsQuantity", originalLine.CustomsQuantity, actualLineCreated.CustomsData.WB_CustomsQty);
			AssertEquals("CustomsSecondQuantity", originalLine.CustomsSecondQuantity,
				actualLineCreated.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsThirdQuantity", originalLine.CustomsThirdQuantity,
				actualLineCreated.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("QuantityUnit", originalLine.QuantityUnit, actualLineCreated.WE_F3_NKPackType);
			AssertEquals("LineComment", "AMD", actualLineCreated.WE_LineComment);
			AssertEquals("AddInfo", originalLine.AddInfo, actualLineCreated.CustomsData.WB_AddInfo);
			AssertEquals("BondedWarehouseQuantityUnit", actualLineCreated.CustomsData.WB_BondedWhsUnitOfQty,
				originalLine.BondedWarehouseQuantityUnit);
			AssertEquals("CustomsQuantityUnit", originalLine.CustomsQuantityUnit,
				actualLineCreated.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsSecondQuantityUnit", originalLine.CustomsSecondQuantityUnit,
				actualLineCreated.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsThirdQuantityUnit", originalLine.CustomsThirdQuantityUnit,
				actualLineCreated.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("ValueForDuty", originalLine.ValueForDuty, actualLineCreated.CustomsData.WB_ValueForDuty);
			AssertEquals("TILV Amount", originalLine.TILV.Amount, actualLineCreated.CustomsData.WB_TILV);
			AssertEquals("TILV Currency", originalLine.TILV.Currency.Code,
				actualLineCreated.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("Reference", originalTransaction.Reference,
				actualLineCreated.CustomsData.WB_DeclarationReference);
			AssertEquals("EntryKey", originalLine.EntryKey, actualLineCreated.CustomsData.WB_EntryKey);
			AssertEquals("EntryLineNumber", originalLine.EntryLineNumber, actualLineCreated.CustomsData.WB_EntryLineNo);
			AssertEquals("EntryDate", originalLine.EntryDate, actualLineCreated.CustomsData.WB_EntryDate);
			AssertEquals("CountryOfOrigin", originalLine.CountryOfOrigin,
				actualLineCreated.CustomsData.CountryOfOrigin);
		}

		#endregion
		#endregion

		#region Implementation

		protected void AssertNoExceptionOnProcess(WhsBondedTransactionProcessor processor)
		{
			try
			{
				ProcessWithMock(processor);
			}
			catch (Exception e)
			{
				AssertEquals("Should not get any exception", null, e);
			}
		}

		Mock<IPutawayEngineManagerForReceive> ProcessWithMock(WhsBondedTransactionProcessor processor)
		{
			var mockPutawayEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();
			using (ObjectFactory.Substitute(mockPutawayEngine.Object))
			{
				processor.Process();
			}
			return mockPutawayEngine;
		}

		protected void AssertExceptionOnProcess(WhsBondedTransactionProcessor processor, string exceptionMessage)
		{
			AssertExceptionThrown("Should throw an exception that contains '" + exceptionMessage + "'",
				typeof(MissingDataException), exceptionMessage, processor.Process, assertStartsWith: true);
		}

		protected void AssertAdjustmentLine(WhsAdjustmentLine adjustmentLine, WhsDocketLine line)
		{
			AssertLine(adjustmentLine, line.WE_OP, -line.WE_TransactionQuantity, line.WE_F3_NKPackType,
				line.LocationString, "AMD", "AMD", line.WE_BondedEntryKey,
				line.WE_ExpiryDate, line.WE_PackingDate, line.WE_PartAttrib1, line.WE_PartAttrib2, line.WE_PartAttrib3);
			AssertEquals("AssertAdjustmentLine: PackType is incorrect", line.WE_F3_NKPackType,
				adjustmentLine.WE_F3_NKPackType);
		}

		protected void AssertAdjustmentLineCustomsData(WhsAdjustmentLine adjustmentLine, WhsDocketLine line)
		{
			AssertAdjustmentLineCustomsData(adjustmentLine, line.CustomsData.WB_AddInfo,
				line.CustomsData.WB_BondedWhsQty, line.CustomsData.WB_BondedWhsUnitOfQty,
				line.CustomsData.WB_CustomsQty, line.CustomsData.WB_CustomsUnitOfQty,
				line.CustomsData.WB_CustomsSecondQuantity, line.CustomsData.WB_CustomsSecondUnitQty,
				line.CustomsData.WB_CustomsThirdQuantity, line.CustomsData.WB_CustomsThirdUnitQty,
				line.CustomsData.WB_EntryDate, line.CustomsData.WB_EntryKey, line.CustomsData.WB_EntryLineNo,
				line.CustomsData.WB_ParentTableCode, line.CustomsData.WB_RN_NKCountryOfOrigin,
				line.CustomsData.WB_ValueForDuty, line.CustomsData.WB_TILV, line.CustomsData.WB_RX_NKTILVCurrency,
				line.CustomsData.WB_DeclarationReference);
		}

		protected void AssertAdjustmentLineCustomsData(WhsAdjustmentLine adjustmentLine, ZString addInfo,
			ZDecimal bondedWhsQty, ZString bondedWhsUnitOfQty, ZDecimal customsQty, ZString customsUnitOfQty,
			ZDecimal customsSecondQty, ZString customsSecondUnitOfQty, ZDecimal customsThirdQty,
			ZString customsThirdUnitOfQty, ZDateTime entryDate, ZString entryKey, ZShort entryLineNo,
			ZString parentTableCode, ZString rN_NKCountryOfOrigin, ZDecimal valueForDuty, ZDecimal tILV,
			ZString rX_NKTILVCurrency, ZString declarationReference)
		{
			AssertEquals("AssertAdjustmentLineCustomsData: AddInfo incorrect", addInfo,
				adjustmentLine.CustomsData.WB_AddInfo);
			if (adjustmentLine.WE_TransactionQuantity >
				0) // only match below when transaction qty is greater than 0 otherwise they may be different
			{
				AssertEquals("AssertAdjustmentLineCustomsData: CustomsQty incorrect", customsQty,
					adjustmentLine.CustomsData.WB_CustomsQty);
				AssertEquals("AssertAdjustmentLineCustomsData: BondedWhsQty incorrect", bondedWhsQty,
					adjustmentLine.CustomsData.WB_BondedWhsQty);
				AssertEquals("AssertAdjustmentLineCustomsData: ValueForDuty incorrect", valueForDuty,
					adjustmentLine.CustomsData.WB_ValueForDuty);
				AssertEquals("AssertAdjustmentLineCustomsData: TILV incorrect", tILV,
					adjustmentLine.CustomsData.WB_TILV);
			}

			AssertEquals("AssertAdjustmentLineCustomsData: BondedWhsUnitOfQty incorrect", bondedWhsUnitOfQty,
				adjustmentLine.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("AssertAdjustmentLineCustomsData: CustomsUnitOfQty incorrect", customsUnitOfQty,
				adjustmentLine.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("AssertAdjustmentLineCustomsData: CustomsSecondQty incorrect", customsSecondQty,
				adjustmentLine.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("AssertAdjustmentLineCustomsData: CustomsSecondUnitOfQty incorrect", customsSecondUnitOfQty,
				adjustmentLine.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("AssertAdjustmentLineCustomsData: CustomsThirdQty incorrect", customsThirdQty,
				adjustmentLine.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("AssertAdjustmentLineCustomsData: CustomsThirdUnitOfQty incorrect", customsThirdUnitOfQty,
				adjustmentLine.CustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("AssertAdjustmentLineCustomsData: EntryDate incorrect", entryDate,
				adjustmentLine.CustomsData.WB_EntryDate);
			AssertEquals("AssertAdjustmentLineCustomsData: EntryKey incorrect", entryKey,
				adjustmentLine.CustomsData.WB_EntryKey);
			AssertEquals("AssertAdjustmentLineCustomsData: EntryLineNo incorrect", entryLineNo,
				adjustmentLine.CustomsData.WB_EntryLineNo);
			AssertEquals("AssertAdjustmentLineCustomsData: ParentTableCode incorrect", parentTableCode,
				adjustmentLine.CustomsData.WB_ParentTableCode);
			AssertEquals("AssertAdjustmentLineCustomsData: RN_NKCountryOfOrigin incorrect", rN_NKCountryOfOrigin,
				adjustmentLine.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("AssertAdjustmentLineCustomsData: RX_NKTILVCurrency incorrect", rX_NKTILVCurrency,
				adjustmentLine.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("AssertAdjustmentLineCustomsData: DeclarationReference incorrect", declarationReference,
				adjustmentLine.CustomsData.WB_DeclarationReference);
		}

		protected void AssertLine(WhsDocketLine line, ZGuid productPK, ZDecimal units, ZString packType,
			ZString locationString2, ZString lineComment, ZString reasonCode, ZString entryKey,
			ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3)
		{
			AssertEquals("AssertLine: Product incorrect", productPK, line.WE_OP);
			AssertEquals("AssertLine: Location2 incorrect", locationString2, line.LocationString);
			AssertLineAttributes(line, units, packType, lineComment, reasonCode, entryKey, expiryDate, packingDate,
				partAttrib1, partAttrib2, partAttrib3);
		}

		protected void AssertLineAttributes(WhsDocketLine line, ZDecimal units, ZString packType, ZString lineComment,
			ZString reasonCode, ZString entryKey, ZDate expiryDate, ZDate packingDate, ZString partAttrib1,
			ZString partAttrib2, ZString partAttrib3)
		{
			AssertEquals("AssertLineAttributes: Units incorrect", units, line.WE_TransactionQuantity);
			AssertEquals("AssertLineAttributes: Pack Type incorrect", packType, line.WE_F3_NKPackType);
			AssertEquals("AssertLineAttributes: Line Comment incorrect", lineComment, line.WE_LineComment);
			AssertEquals("AssertLineAttributes: Reason Code incorrect", reasonCode, line.WE_ReasonCode);
			AssertEquals("AssertLineAttributes: Expiry Date incorrect", expiryDate, line.WE_ExpiryDate);
			AssertEquals("AssertLineAttributes: Entry Key incorrect", entryKey, line.WE_BondedEntryKey);
			AssertEquals("AssertLineAttributes: Packing Date incorrect", packingDate, line.WE_PackingDate);
			AssertEquals("AssertLineAttributes: Packing Date incorrect", packingDate, line.WE_PackingDate);
			AssertEquals("AssertLineAttributes: Part Attrib1 incorrect", partAttrib1, line.WE_PartAttrib1);
			AssertEquals("AssertLineAttributes: Part Attrib2 incorrect", partAttrib2, line.WE_PartAttrib2);
			AssertEquals("AssertLineAttributes: Part Attrib3 incorrect", partAttrib3, line.WE_PartAttrib3);
		}

		protected void AssertReceiveLineCustomsData(WhsDocketLine docketLine,
			IWhsBondedWarehouseTransactionLine transactionLine, IWhsBondedWarehouseTransaction transaction)
		{
			AssertReceiveLineCustomsData(docketLine, transactionLine.EntryKey, transactionLine.EntryLineNumber,
				WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode, transactionLine.EntryDate,
				transactionLine.ValueForDuty, transactionLine.TILV.Amount,
				((transactionLine.TILV.Currency == null) ? "" : transactionLine.TILV.Currency.Code),
				transactionLine.AddInfo, docketLine.WE_TransactionQuantity, transactionLine.BondedWarehouseQuantityUnit,
				transactionLine.CustomsQuantity, transactionLine.CustomsQuantityUnit, transaction.Reference,
				((transactionLine.CountryOfOrigin == null) ? ZString.Empty : transactionLine.CountryOfOrigin.RN_Code));
		}

		protected void AssertReceiveLineCustomsData(WhsDocketLine docketLine, ZString entryKey, ZShort entryLineNumber,
			ZString parentTableCode, ZDateTime entryDate, ZDecimal valueForDuty, ZDecimal tILVAmount,
			ZString tILVCurrencyCode, ZString addInfo, ZDecimal bondedWarehouseQuantity,
			ZString bondedWarehouseQuantityUnit, ZDecimal customsQuantity, ZString customsQuantityUnit,
			ZString reference, ZString countryOfOriginCode)
		{
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_EntryKey incorrect", entryKey,
				docketLine.CustomsData.WB_EntryKey);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_EntryLineNo incorrect", entryLineNumber,
				docketLine.CustomsData.WB_EntryLineNo);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_ParentID incorrect", docketLine.PK,
				docketLine.CustomsData.WB_ParentID);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_ParentTableCode incorrect", parentTableCode,
				docketLine.CustomsData.WB_ParentTableCode);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_EntryDate incorrect", entryDate,
				docketLine.CustomsData.WB_EntryDate);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_ValueForDuty incorrect", valueForDuty,
				docketLine.CustomsData.WB_ValueForDuty);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_TILV incorrect", tILVAmount,
				docketLine.CustomsData.WB_TILV);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_RX_NKTILVCurrency incorrect",
				tILVCurrencyCode, docketLine.CustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_AddInfo incorrect", addInfo,
				docketLine.CustomsData.WB_AddInfo);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_BondedWhsQty incorrect",
				bondedWarehouseQuantity, docketLine.CustomsData.WB_BondedWhsQty);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_BondedWhsUnitOfQty incorrect",
				bondedWarehouseQuantityUnit, docketLine.CustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_CustomsQty incorrect", customsQuantity,
				docketLine.CustomsData.WB_CustomsQty);
			AssertEquals("AssertReceiveLineBodedAttributes: DocketLine.CustomsData.WB_CustomsUnitOfQty incorrect",
				customsQuantityUnit, docketLine.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("AssertReceiveLineBodedAttributes: DocketLine.CustomsData.WB_DeclarationReference incorrect",
				reference, docketLine.CustomsData.WB_DeclarationReference);
			AssertEquals("AssertReceiveLineBodedAttributes: CustomsData.WB_RN_NKCountryOfOrigin incorrect",
				countryOfOriginCode, docketLine.CustomsData.WB_RN_NKCountryOfOrigin);
		}

		#endregion
	}
}
