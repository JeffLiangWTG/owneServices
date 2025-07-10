using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBondedWarehouseAttribute))]
	class WhsBondedWarehouseAttributeTest : WhsBusinessObjectTestCase
	{
		#region Constants

		public void TestConstants()
		{
			AssertEquals("WE", WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode);
			AssertEquals("AWAITING PAY RESPONSE", WhsBondedWarehouseAttribute.WaitingForCustomsPaymentMessage);
		}

		#endregion

		#region Statics

		public void TestBuildKey()
		{
			AssertEquals("", WhsBondedWarehouseAttribute.BuildKey("", 0));
			AssertEquals("", WhsBondedWarehouseAttribute.BuildKey("", 1));
			AssertEquals("E1", WhsBondedWarehouseAttribute.BuildKey("E1", 0));
			AssertEquals("E1-1", WhsBondedWarehouseAttribute.BuildKey("e1", 1));
			AssertEquals("ABCDE-100", WhsBondedWarehouseAttribute.BuildKey("AbCde", 100));
		}

		public void TestBreakUpKey()
		{
			var entry = new EntryNumber(string.Empty, 0);
			entry = WhsBondedWarehouseAttribute.BreakUpKey("AWFGT-10");
			AssertEquals("AWFGT", entry.EntryKey);
			AssertEquals((short)10, entry.EntryLineNo);

			entry = WhsBondedWarehouseAttribute.BreakUpKey("def-1");
			AssertEquals("DEF", entry.EntryKey);
			AssertEquals((short)1, entry.EntryLineNo);

			entry = WhsBondedWarehouseAttribute.BreakUpKey("AAA-BBB-CCC-05");
			AssertEquals("AAA-BBB-CCC", entry.EntryKey);
			AssertEquals((short)5, entry.EntryLineNo);

			entry = WhsBondedWarehouseAttribute.BreakUpKey("AAA-BBB");
			AssertEquals("AAA", entry.EntryKey);
			AssertEquals((short)0, entry.EntryLineNo);

			entry = WhsBondedWarehouseAttribute.BreakUpKey("AAA-25N");
			AssertEquals("AAA", entry.EntryKey);
			AssertEquals((short)0, entry.EntryLineNo);

			entry = WhsBondedWarehouseAttribute.BreakUpKey("AAA");
			AssertEquals("AAA", entry.EntryKey);
			AssertEquals((short)0, entry.EntryLineNo);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadFromParentIDWithNullFactory()
		{
			AssertEquals(null, WhsBondedWarehouseAttribute.LoadFromParentID(null, Factory.New<WhsReceiveLine>()));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadFromParentIDWithNullParent()
		{
			AssertEquals(null, WhsBondedWarehouseAttribute.LoadFromParentID(Factory, null));
		}

		public void TestLoadFromParentID()
		{
			var receive = Factory.New<WhsReceive>();
			var line = receive.Lines.AddNew();
			var bond1 = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			bond1.WB_ParentID = line.PK;
			AssertEquals(bond1.PK, WhsBondedWarehouseAttribute.LoadFromParentID(Factory, line).PK);
		}

		public void TestLoadFromParentID_FetchFromLocalCacheOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = receive.Lines.AddNew();
			line.WE_OP = data.Part1.PK;
			AssertNull(WhsBondedWarehouseAttribute.LoadFromParentID(Factory, line));
			AssertEquals("Receive Line is not in the DB. Should not have executed a DB query.", 0, Factory.GetTableHitCount(WhsBondedWarehouseAttributeSchema.Constants.TableName));

			Factory.Save();
			AssertNull(WhsBondedWarehouseAttribute.LoadFromParentID(Factory, line));
			AssertEquals("Receive Line is in the DB. Should have executed a DB query.", 1, Factory.GetTableHitCount(WhsBondedWarehouseAttributeSchema.Constants.TableName));
		}

		public void TestFindBestAttribute()
		{
			var list = new WhsBondedWarehouseAttribute[1];
			list[0] = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(list[0], WhsBondedWarehouseAttribute.FindBestAttribute(list));
		}

		public void TestFindBestAttributeWithEmptyList()
		{
			var list = Array.Empty<WhsBondedWarehouseAttribute>();
			AssertEquals(null, WhsBondedWarehouseAttribute.FindBestAttribute(list));
		}

		public void TestFindBestAttributeWithDeletedElements()
		{
			var list = new WhsBondedWarehouseAttribute[3];
			list[0] = Factory.New<WhsBondedWarehouseAttribute>();
			list[1] = Factory.New<WhsBondedWarehouseAttribute>();
			list[2] = Factory.New<WhsBondedWarehouseAttribute>();
			list[0].Delete();
			list[1].Delete();
			AssertEquals(list[2], WhsBondedWarehouseAttribute.FindBestAttribute(list));
		}

		public void TestFindBestAttributeWithDuplicates()
		{
			var list = new WhsBondedWarehouseAttribute[2];
			list[0] = Factory.New<WhsBondedWarehouseAttribute>();
			list[1] = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(list[0], WhsBondedWarehouseAttribute.FindBestAttribute(list));
			AssertEquals("Should be no silent exceptions", 0, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
		}

		#endregion

		#region Business Object Overrides

		public void TestDeleteWarehouseCustomsAdditionalAddInfo()
		{
			var org = Helper.CreateClient();
			var prod = Helper.CreateProduct(org, "P1");
			var whs = Helper.CreateWarehouse("1", "A");
			var receive = Helper.CreateWhsReceive(org, whs);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
			var attribute = inventoryLine.CustomsData;
			var addInfo1 = Factory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
			addInfo1.B7_ParentID = attribute.PK;
			addInfo1.B7_ParentTableCode = attribute.TablePrefix;
			addInfo1.B7_Type = "CCT";
			addInfo1.B7_AddInfoData = "WHO=BOB THE BUILDER";
			var addInfo2 = Factory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
			addInfo2.B7_ParentID = attribute.PK;
			addInfo2.B7_ParentTableCode = attribute.TablePrefix;
			addInfo2.B7_Type = "SUP";
			addInfo2.B7_AddInfoData = "WHO=WENDY THE DESTROYER";
			attribute.Delete();
			AssertEquals("No db hits when not in db.", 0, Factory.GetTableHitCount(CusAddInfoSchema.Constants.TableName));
			attribute = inventoryLine.CustomsData;
			addInfo1 = Factory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
			addInfo1.B7_ParentID = attribute.PK;
			addInfo1.B7_ParentTableCode = attribute.TablePrefix;
			addInfo1.B7_Type = "CCT";
			addInfo1.B7_AddInfoData = "WHO=BOB THE BUILDER";
			addInfo2 = Factory.New<Integration.IWarehouseCustomsAttributeAddInfo>();
			addInfo2.B7_ParentID = attribute.PK;
			addInfo2.B7_ParentTableCode = attribute.TablePrefix;
			addInfo2.B7_Type = "SUP";
			addInfo2.B7_AddInfoData = "WHO=WENDY THE DESTROYER";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			attribute = newFactory.Load<WhsBondedWarehouseAttribute>(attribute.PK);
			AssertEquals("No db hits when load.", 0, newFactory.GetTableHitCount(CusAddInfoSchema.Constants.TableName));
			attribute.Delete();
			AssertEquals("db hits when delete.", 1, newFactory.GetTableHitCount(CusAddInfoSchema.Constants.TableName));
			newFactory.Save();
			AssertEquals("addInfo1.IsDeleted", true, ((BusinessObject)addInfo1).IsDeleted);
			AssertEquals("addInfo2.IsDeleted", true, ((BusinessObject)addInfo2).IsDeleted);
		}

		public virtual void TestLightValidatonDisabled()
		{
			AssertEquals(false, Bond.LightValidationEnabled);
		}

		public void TestCopyPersistantValuesFromAnotherAttribute()
		{
			var attribute1 = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			attribute1.WB_EntryKey = "KEY1";
			attribute1.WB_EntryLineNo = 1;
			attribute1.WB_EntryDate = ZDateTime.Today;
			attribute1.WB_CustomsQty = 20;
			attribute1.WB_CustomsUnitOfQty = "LT";
			attribute1.WB_BondedWhsQty = 10;
			attribute1.WB_BondedWhsUnitOfQty = "KG";
			attribute1.WB_ValueForDuty = 100m;
			attribute1.WB_TILV = 20m;
			attribute1.WB_RN_NKCountryOfOrigin = "AU";
			attribute1.WB_AddInfo = "AddInfo";
			attribute1.WB_CustomsSecondQuantity = 50;
			attribute1.WB_CustomsSecondUnitQty = "GR";
			attribute1.WB_Tariff = "TRF";
			attribute1.WB_PrimaryPreference = "STANDARD";

			attribute1.WB_CustomsThirdQuantity = 10;
			attribute1.WB_CustomsThirdUnitQty = "GRM";
			var orgAddress = Factory.New<OrgAddress>();
			attribute1.WB_OA_ManufacturerAddress = orgAddress.PK;

			var attribute2 = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			attribute2.CopyPersistantValuesFromAnotherAttribute(attribute1);

			AssertEquals("EntryKey", attribute1.WB_EntryKey, attribute2.WB_EntryKey);
			AssertEquals("EntryLineNo", attribute1.WB_EntryLineNo, attribute2.WB_EntryLineNo);
			AssertEquals("EntryDate", attribute1.WB_EntryDate, attribute2.WB_EntryDate);
			AssertEquals("CustomsQty", attribute1.WB_CustomsQty, attribute2.WB_CustomsQty);
			AssertEquals("CustomsQtyUnit", attribute1.WB_CustomsUnitOfQty, attribute2.WB_CustomsUnitOfQty);
			AssertEquals("WhsQty", attribute1.WB_BondedWhsQty, attribute2.WB_BondedWhsQty);
			AssertEquals("WhsQtyUnit", attribute1.WB_BondedWhsUnitOfQty, attribute2.WB_BondedWhsUnitOfQty);
			AssertEquals("VFD", attribute1.WB_ValueForDuty, attribute2.WB_ValueForDuty);
			AssertEquals("TILV", attribute1.WB_TILV, attribute2.WB_TILV);
			AssertEquals("Origin", attribute1.WB_RN_NKCountryOfOrigin, attribute2.WB_RN_NKCountryOfOrigin);
			AssertEquals("AddInfo", attribute1.WB_AddInfo, attribute2.WB_AddInfo);
			AssertEquals("CustomsSecondQuantity", attribute1.WB_CustomsSecondQuantity, attribute2.WB_CustomsSecondQuantity);
			AssertEquals("CustomsSecondUnitQty", attribute1.WB_CustomsSecondUnitQty, attribute2.WB_CustomsSecondUnitQty);
			AssertEquals("Tariff", attribute1.WB_Tariff, attribute2.WB_Tariff);
			AssertEquals("PrimaryPreference", attribute1.WB_PrimaryPreference, attribute2.WB_PrimaryPreference);

			AssertEquals("CustomsThirdQuantity", attribute1.WB_CustomsThirdQuantity, attribute2.WB_CustomsThirdQuantity);
			AssertEquals("CustomsThirdUnitQty", attribute1.WB_CustomsThirdUnitQty, attribute2.WB_CustomsThirdUnitQty);
			AssertEquals("ManufacturerAddress", attribute1.WB_OA_ManufacturerAddress, attribute2.WB_OA_ManufacturerAddress);
		}

		public void TestSetDefaultValues()
		{
			var bond = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			AssertEquals(true, bond.WB_IsActive);
		}

		public void TestOnFactorySaving()
		{
			ZGuid bondPK = Bond.PK;
			AssertNull("Precondition", Bond.Parent);
			Factory.Save();
			AssertNull("Should be deleted because has no parent", Factory.Load<WhsBondedWarehouseAttribute>(bondPK));

			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart prod = Helper.CreateProduct(org, "P1");
			WhsWarehouse whs = Helper.CreateWarehouse("1", "A");
			WhsReceive receive = Helper.CreateWhsReceive(org, whs);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
			Factory.Save();

			Bond = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			bondPK = Bond.PK;
			Bond.SetParent(inventoryLine.InDocketLine);
			AssertEquals("Precondition", false, Bond.Parent.Docket.IsCustomsTransaction);
			Factory.Save();
			AssertNull("Should be deleted because docket is not bonded transaction", Factory.Load<WhsBondedWarehouseAttribute>(bondPK));

			inventoryLine.Delete();
			receive.WD_DocketSubType = CodeLists.ReceiveType.Codes.Customs;

			var customsInventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
			Bond = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			bondPK = Bond.PK;
			Helper.EnableWarehouseForBond(whs, true);
			Bond.SetParent(customsInventoryLine.InDocketLine);

			AssertEquals("Precondition", true, Bond.Parent.Docket.IsCustomsTransaction);
			Factory.Save();
			AssertNotNull("Should not be deleted because docket is a bonded transaction", Factory.Load<WhsBondedWarehouseAttribute>(bondPK));

			customsInventoryLine.Delete();
			Factory.Save();
			AssertNull("Should be deleted because parent is deleted", Factory.Load<WhsBondedWarehouseAttribute>(bondPK));
		}

		public void TestOnFactorySaving_RegistryItemEnabled()
		{
			Bond.Delete();
			using (WarehouseDataRegistry.Instance.EnableImprovedStorageOfCustomsData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var bond1 = Factory.New<WhsBondedWarehouseAttribute>();
				bond1.WB_ParentID = ZGuid.NewZGuid();
				bond1.WB_ParentTableCode = WhsDocketLineSchema.Constants.Prefix;
				AssertNull("Precondition", bond1.Parent);
				Factory.Save();
				AssertNotNull("Should not be deleted because registry item is enabled", Factory.Load<WhsBondedWarehouseAttribute>(bond1.PK));

				OrgHeader org = Helper.CreateClient();
				OrgSupplierPart prod = Helper.CreateProduct(org, "P1");
				WhsWarehouse whs = Helper.CreateWarehouse("1", "A");
				WhsReceive receive = Helper.CreateWhsReceive(org, whs);
				var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
				Factory.Save();

				var bond2 = Factory.New<WhsBondedWarehouseAttribute>();
				bond2.SetParent(inventoryLine.InDocketLine);
				AssertEquals("Precondition", false, bond2.Parent.Docket.IsCustomsTransaction);
				Factory.Save();
				AssertNotNull("Should not be deleted because registry item is enabled", Factory.Load<WhsBondedWarehouseAttribute>(bond2.PK));

				inventoryLine.Delete();
				receive.WD_DocketSubType = CodeLists.ReceiveType.Codes.Customs;

				var customsInventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
				var bond3 = Factory.New<WhsBondedWarehouseAttribute>();
				Helper.EnableWarehouseForBond(whs, true);
				bond3.SetParent(customsInventoryLine.InDocketLine);

				AssertEquals("Precondition", true, bond3.Parent.Docket.IsCustomsTransaction);
				Factory.Save();
				AssertNotNull("Should not be deleted because registry item is enabled", Factory.Load<WhsBondedWarehouseAttribute>(bond3.PK));

				customsInventoryLine.Delete();
				Factory.Save();
				AssertNull("Should be deleted because parent is deleted", Factory.Load<WhsBondedWarehouseAttribute>(bond3.PK));
			}
		}

		#endregion

		#region Related Entities

		#region TestReceiveEntry

		public void TestReceiveEntry()
		{
			WhsBondedWarehouseAttribute customsData = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
			WhsBondedWarehouseAttribute receiveAttribute = (WhsBondedWarehouseAttribute)GetNewBusinessObject();

			customsData.WB_WB_InwardsEntry = receiveAttribute.PK;
			AssertEquals("ReceiveEntry", receiveAttribute, customsData.ReceiveEntry);

			customsData.WB_WB_InwardsEntry = ZGuid.Empty;
			AssertEquals("ReceiveEntry", null, customsData.ReceiveEntry);
		}

		#endregion

		#region TestParent

		public void TestParent()
		{
			WhsDocket docket = Factory.New<WhsReceive>();
			WhsDocketLine docketLine = docket.Lines.AddNew();
			AssertNull("Parent", Bond.Parent);
			Bond.SetParent(docketLine);
			AssertEquals("Parent", docketLine, Bond.Parent);
		}

		#endregion

		#region TestSetParent

		public void TestSetParent()
		{
			var docket = Factory.New<WhsReceive>();
			var docketLine = docket.Lines.AddNew();
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("WB_ParentCode", "", bond.WB_ParentTableCode);
			AssertEquals("WB_ParentID", ZGuid.Empty, bond.WB_ParentID);

			bond.SetParent(docketLine);
			AssertEquals("WB_ParentCode", WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode, bond.WB_ParentTableCode);
			AssertEquals("WB_ParentID", docketLine.PK, bond.WB_ParentID);
			AssertEquals(true, docketLine.IsRegisteredEditableChildObject(bond));

			// Done to test the scenario where the Parent(WhsDocketLine) object is not yet added to the DataTable and but used
			// by WhsBondedWarehouseAttribute. Factory.Load(typeof(WhsDocketLine), WB_ParentID) will return null in this case. So
			// had to set the Parent field inside SetParent()
			((INeedRow)bond).Row.Table.Rows.Remove(((INeedRow)bond).Row);
			AssertEquals("Row Status", ((INeedRow)bond).Row.RowState, DataRowState.Detached);
			AssertEquals("Parent", docketLine, bond.Parent);
		}

		#endregion

		#region TestManufacturerAddress

		public void TestManufacturerAddress()
		{
			var manufacturerAddress = Factory.NewWithValidTestData<OrgAddress>();
			var bondAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", null, bondAttribute.ManufacturerAddress);

			bondAttribute.WB_OA_ManufacturerAddress = manufacturerAddress.PK;
			AssertEquals("WhsBondedWarehouseAttribute:ManufacturerAddress", manufacturerAddress, bondAttribute.ManufacturerAddress);
		}

		#endregion

		#endregion

		#region Properties

		#region TestWB_EntryKey

		public void TestWB_EntryKey()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryKey = "123";
			AssertEquals("123", bond.WB_EntryKey);

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			bond.SetParent(receiveLine);
			AssertEquals("", receiveLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "234";
			AssertEquals("234", receiveLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestWB_EntryKey_OnOrder

		public void TestWB_EntryKey_OnOrder()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryKey = "123";
			AssertEquals("123", bond.WB_EntryKey);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = "CUS";
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			bond.SetParent(orderLine);
			AssertEquals("", orderLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "234";
			AssertEquals("", orderLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestWB_EntryKey_OnDynamicWorkOrder

		public void TestWB_EntryKey_OnDynamicWorkOrder()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryKey = "123";
			AssertEquals("123", bond.WB_EntryKey);

			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);
			bond.SetParent(dynamicWorkOrderLine);
			AssertEquals("", dynamicWorkOrderLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "234";
			AssertEquals("", dynamicWorkOrderLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestWB_EntryLineNo

		public void TestWB_EntryLineNo()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryLineNo = 1;
			AssertEquals(new ZShort(1), bond.WB_EntryLineNo);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			bond.SetParent(receiveLine);
			AssertEquals("", receiveLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "123";
			AssertEquals("123-1", receiveLine.WE_BondedEntryKey);

			bond.WB_EntryLineNo = 2;
			AssertEquals("123-2", receiveLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestManufacturerCode

		public void TestManufacturerCode()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("ManufacturerCode", "", bond.ManufacturerCode);

			var manufacturer = Helper.CreateClient("MANU", "Manufacturer");
			var manufacturerAddress = manufacturer.MainAddress;

			bond.WB_OA_ManufacturerAddress = manufacturerAddress.PK;
			AssertEquals("ManufacturerCode", "MANU", bond.ManufacturerCode);
		}

		#endregion

		#region TestWB_EntryLineNo_OnOrder

		public void TestWB_EntryLineNo_OnOrder()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryLineNo = 1;
			AssertEquals(new ZShort(1), bond.WB_EntryLineNo);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = "CUS";
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			bond.SetParent(orderLine);
			AssertEquals("", orderLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "123";
			AssertEquals("", orderLine.WE_BondedEntryKey);

			orderLine.WE_BondedEntryKey = "123";
			bond.WB_EntryLineNo = 2;
			AssertEquals("123", orderLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestWB_EntryLineNo_OnDynamicWorkOrder

		public void TestWB_EntryLineNo_OnDynamicWorkOrder()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.WB_EntryLineNo = 1;
			AssertEquals(new ZShort(1), bond.WB_EntryLineNo);

			var data = new TestDataSimpleEnvironment(Factory);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);
			bond.SetParent(dynamicWorkOrderLine);
			AssertEquals("", dynamicWorkOrderLine.WE_BondedEntryKey);

			bond.WB_EntryKey = "123";
			AssertEquals("", dynamicWorkOrderLine.WE_BondedEntryKey);

			dynamicWorkOrderLine.WE_BondedEntryKey = "123";
			bond.WB_EntryLineNo = 2;
			AssertEquals("123", dynamicWorkOrderLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestWB_EntryLineNo_ReadOnly

		public void TestWB_EntryLineNoInfo_Transfer()
		{
			AssertPropertyIsReadonlyDuringTransfer(WhsBondedWarehouseAttributeSchema.Constants.WB_EntryLineNo);
		}

		#endregion

		#region TestWB_OA_ManufacturerAddress

		public void TestWB_OA_ManufacturerAddress()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(true, bond.WB_OA_ManufacturerAddressInfo.ReadOnly);

			var manufactuerer = Factory.NewWithValidTestData<OrgHeader>();
			bond.WB_OA_ManufacturerAddress = manufactuerer.MainAddress.PK;
			AssertEquals(true, bond.WB_OA_ManufacturerAddressInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestBondedAdditionalInfo

		public void TestBondedAdditionalInfo()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("WB_AddInfo", "", bond.WB_AddInfo);
			AssertEquals(0, bond.BondedAdditionalInfo.Count);

			bond.WB_AddInfo = "A=Apple*B=Banana";
			AssertNotNull(bond.BondedAdditionalInfo);
			AssertEquals(2, bond.BondedAdditionalInfo.Count);

			AssertEquals("A", bond.BondedAdditionalInfo[0].KeyString);
			AssertEquals("Apple", bond.BondedAdditionalInfo[0].ValueString);

			AssertEquals("B", bond.BondedAdditionalInfo[1].KeyString);
			AssertEquals("Banana", bond.BondedAdditionalInfo[1].ValueString);
		}

		#endregion

		#region TestSuspendUpdatingDocketLineEntryKey

		public void TestSuspendUpdatingDocketLineEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.SetParent(receiveLine);
			AssertEquals("Precondition", "", bond.WB_EntryKey);
			AssertEquals("Precondition", "", receiveLine.WE_BondedEntryKey);

			using (bond.SuspendUpdatingDocketLineEntryKey())
			{
				bond.WB_EntryKey = "234";
				AssertEquals("", receiveLine.WE_BondedEntryKey);
			}

			bond.WB_EntryKey = "456";
			AssertEquals("456", receiveLine.WE_BondedEntryKey);
		}

		#endregion

		#region TestSetDefaultOutwardTypeIfEmpty

		public void TestSetDefaultOutwardTypeIfEmpty()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", string.Empty, bond.WB_OutwardType);

			bond.SetDefaultOutwardTypeIfEmpty();
			AssertEquals(WhsBondedWarehouseAttributeOutwardType.Codes.CNN, bond.WB_OutwardType);
		}

		#endregion

		#region TestCheckIsZoneStatusDomestic

		public void TestCheckIsZoneStatusDomestic()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", true, bond.WB_ZoneStatus.IsEmpty);
			AssertEquals(false, bond.IsZoneStatusDomestic);
			TestCheckIsZoneStatusDomesticCore(bond, ZoneStatusList.Codes.Domestic, true);
			TestCheckIsZoneStatusDomesticCore(bond, ZoneStatusList.Codes.PrivilegedForeign, false);
			TestCheckIsZoneStatusDomesticCore(bond, ZoneStatusList.Codes.ZoneRestricted, false);
			TestCheckIsZoneStatusDomesticCore(bond, ZoneStatusList.Codes.NonPrivilegedForeign, false);
		}

		static void TestCheckIsZoneStatusDomesticCore(WhsBondedWarehouseAttribute bond, string status, bool expectedResult)
		{
			bond.WB_ZoneStatus = status;
			AssertEquals(expectedResult, bond.IsZoneStatusDomestic);
		}

		#endregion

		#region Clone

		public void TestClone()
		{
			var year = ZDateTime.Now.Year;

			AssertEquals("Precondition - New columns need testing.", 46, WhsBondedWarehouseAttributeSchema.All.Count - 4); // No need to test SystemCreate and SystemLastEdit columns
			var customsData = Factory.New<WhsBondedWarehouseAttribute>();
			customsData.WB_AddInfo = "addinfo";
			customsData.WB_BondedWhsQty = 19;
			customsData.WB_BondedWhsUnitOfQty = "KG";
			customsData.WB_CustomsQty = 3;
			customsData.WB_CustomsUnitOfQty = "LB";
			customsData.WB_DeclarationReference = "dec ref";
			customsData.WB_EntryDate = new ZDateTime(year, 3, 19);
			customsData.WB_EntryKey = "entry key";
			customsData.WB_EntryLineNo = 7;
			customsData.WB_IsActive = true;
			customsData.WB_ParentID = ZGuid.NewZGuid();
			customsData.WB_ParentTableCode = "WE";
			customsData.WB_RN_NKCountryOfOrigin = "AU";
			customsData.WB_RX_NKTILVCurrency = "AUD";
			customsData.WB_TILV = 11.5;
			customsData.WB_ValueForDuty = 100;
			customsData.WB_WB_InwardsEntry = customsData.PK;
			customsData.WB_PrimaryPreference = "STANDARD";
			customsData.WB_CustomsSecondQuantity = 99;
			customsData.WB_CustomsSecondUnitQty = "LB";
			customsData.WB_CustomsThirdQuantity = 5;
			customsData.WB_CustomsThirdUnitQty = "KG";
			customsData.WB_CustomsFourthQuantity = 150;
			customsData.WB_CustomsFourthUnitQty = "LB";
			customsData.WB_CustomsFifthQuantity = 8.5;
			customsData.WB_CustomsFifthUnitQty = "KG";
			customsData.WB_Tariff = "01010101";
			customsData.WB_OA_ManufacturerAddress = Factory.New<OrgAddress>().PK;
			customsData.WB_ZoneStatus = "P";
			customsData.WB_IsFromAnotherFTZWhs = true;
			customsData.WB_OutwardType = "TOF";
			customsData.WB_CustomsDeadline = ZDate.Today;
			customsData.WB_InwardStyle = "TT";
			customsData.WB_InwardProcedure = "TT";
			customsData.WB_MatchingKey = "12345678";
			customsData.WB_IsMainInwardsProcessedItem = true;
			customsData.WB_IsSecondaryInwardsProcessedItem = true;
			customsData.WB_Remarks = "Test";
			customsData.WB_RN_NKCountryOfDestination = "IE";
			customsData.WB_AllDutiesAmount = 10m;
			customsData.WB_VATAmount = 2.3m;

			var clone = (WhsBondedWarehouseAttribute)customsData.Clone();
			AssertEquals(customsData.WB_AddInfo, clone.WB_AddInfo);
			AssertEquals(customsData.WB_BondedWhsQty, clone.WB_BondedWhsQty);
			AssertEquals(customsData.WB_BondedWhsUnitOfQty, clone.WB_BondedWhsUnitOfQty);
			AssertEquals(customsData.WB_CustomsQty, clone.WB_CustomsQty);
			AssertEquals(customsData.WB_CustomsUnitOfQty, clone.WB_CustomsUnitOfQty);
			AssertEquals(customsData.WB_DeclarationReference, clone.WB_DeclarationReference);
			AssertEquals(customsData.WB_EntryDate, clone.WB_EntryDate);
			AssertEquals(customsData.WB_EntryKey, clone.WB_EntryKey);
			AssertEquals(customsData.WB_EntryLineNo, clone.WB_EntryLineNo);
			AssertEquals(customsData.WB_IsActive, clone.WB_IsActive);
			AssertEquals("ParentID should not be cloned.", ZGuid.Empty, clone.WB_ParentID);
			AssertEquals("ParentTableCode should not be cloned.", "", clone.WB_ParentTableCode);
			AssertEquals(customsData.WB_RN_NKCountryOfOrigin, clone.WB_RN_NKCountryOfOrigin);
			AssertEquals(customsData.WB_RX_NKTILVCurrency, clone.WB_RX_NKTILVCurrency);
			AssertEquals(customsData.WB_TILV, clone.WB_TILV);
			AssertEquals(customsData.WB_ValueForDuty, clone.WB_ValueForDuty);
			AssertEquals("WB_WB_InwardsEntry should not be cloned.", true, clone.WB_WB_InwardsEntry.IsEmpty);
			AssertEquals(customsData.WB_PrimaryPreference, clone.WB_PrimaryPreference);
			AssertEquals(customsData.WB_CustomsSecondQuantity, clone.WB_CustomsSecondQuantity);
			AssertEquals(customsData.WB_CustomsSecondUnitQty, clone.WB_CustomsSecondUnitQty);
			AssertEquals(customsData.WB_CustomsThirdQuantity, clone.WB_CustomsThirdQuantity);
			AssertEquals(customsData.WB_CustomsThirdUnitQty, clone.WB_CustomsThirdUnitQty);
			AssertEquals(customsData.WB_CustomsFourthQuantity, clone.WB_CustomsFourthQuantity);
			AssertEquals(customsData.WB_CustomsFourthUnitQty, clone.WB_CustomsFourthUnitQty);
			AssertEquals(customsData.WB_CustomsFifthQuantity, clone.WB_CustomsFifthQuantity);
			AssertEquals(customsData.WB_CustomsFifthUnitQty, clone.WB_CustomsFifthUnitQty);
			AssertEquals(customsData.WB_Tariff, clone.WB_Tariff);
			AssertEquals(customsData.WB_OA_ManufacturerAddress, clone.WB_OA_ManufacturerAddress);
			AssertEquals(customsData.WB_ZoneStatus, clone.WB_ZoneStatus);
			AssertEquals(customsData.WB_IsFromAnotherFTZWhs, clone.WB_IsFromAnotherFTZWhs);
			AssertEquals(customsData.WB_OutwardType, clone.WB_OutwardType);
			AssertEquals(customsData.WB_CustomsDeadline, clone.WB_CustomsDeadline);
			AssertEquals(customsData.WB_InwardStyle, clone.WB_InwardStyle);
			AssertEquals(customsData.WB_InwardProcedure, clone.WB_InwardProcedure);
			AssertEquals(customsData.WB_MatchingKey, clone.WB_MatchingKey);
			AssertEquals(customsData.WB_IsMainInwardsProcessedItem, clone.WB_IsMainInwardsProcessedItem);
			AssertEquals(customsData.WB_IsSecondaryInwardsProcessedItem, clone.WB_IsSecondaryInwardsProcessedItem);
			AssertEquals(customsData.WB_Remarks, clone.WB_Remarks);
			AssertEquals(customsData.WB_RN_NKCountryOfDestination, clone.WB_RN_NKCountryOfDestination);
			AssertEquals(customsData.WB_AllDutiesAmount, clone.WB_AllDutiesAmount);
			AssertEquals(customsData.WB_VATAmount, clone.WB_VATAmount);
		}

		#endregion

		#region ReadOnly

		#region TestReadOnly

		public void TestReadOnly()
		{
			var bondedAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", false, bondedAttribute.ReadOnly);
			AssertWhsBondedWarehouseAttributeColumnReadOnly("Precondition", bondedAttribute, expectedReadOnlyValue: false);

			bondedAttribute.ReadOnly = true;
			AssertWhsBondedWarehouseAttributeColumnReadOnly("When Bonded Warehouse Attribute is readonly, Preoperties should be readonly", bondedAttribute, expectedReadOnlyValue: true);
		}

		#endregion

		#region TestReadOnly_Receive

		public void TestReadOnly_Receive()
		{
			var bondedAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", false, bondedAttribute.ReadOnly);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = receive.Lines.AddNew();
			bondedAttribute.SetParent(receiveLine);
			AssertEquals(false, bondedAttribute.ReadOnly);

			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, bondedAttribute.ReadOnly);

			receive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(false, bondedAttribute.ReadOnly);

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, bondedAttribute.ReadOnly);

			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(false, bondedAttribute.ReadOnly);

			bondedAttribute.ReadOnly = true;
			AssertEquals(true, bondedAttribute.ReadOnly);
		}

		#endregion

		#region TestWB_EntryKey_Transfer

		public void TestWB_EntryKey_Transfer()
		{
			AssertPropertyIsReadonlyDuringTransfer(WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey);
		}

		#endregion

		#region TestWB_EntryKeyAndWB_EntryLineNo_ReadOnly

		#region TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CUSOrder

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CUSOrder_FTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.Customs, warehouseType: WarehouseTypes.Codes.FreeTradeZone, readOnly: false);
		}

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CUSOrder_NonFTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.Customs, warehouseType: WarehouseTypes.Codes.Product, readOnly: false);
		}

		#endregion

		#region TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CPSOrder

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CPSOrder_FTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.CustomsReleaseWithPermit, warehouseType: WarehouseTypes.Codes.FreeTradeZone, readOnly: true);
		}

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_CPSOrder_NonFTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.CustomsReleaseWithPermit, warehouseType: WarehouseTypes.Codes.Product, readOnly: false);
		}

		#endregion

		#region TestWB_EntryKeyAndWB_EntryLineNoReadOnly_NonCUSorCPSOrder

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_NonCUSorCPSOrder_FTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.Order, warehouseType: WarehouseTypes.Codes.FreeTradeZone, readOnly: false);
		}

		public void TestWB_EntryKeyAndWB_EntryLineNoReadOnly_NonCUSorCPSOrder_NonFTZ()
		{
			TestWB_EntryKeyAndWB_EntryLineNoReadOnly(orderType: OrderType.Codes.Order, warehouseType: WarehouseTypes.Codes.Product, readOnly: false);
		}

		#endregion

		void TestWB_EntryKeyAndWB_EntryLineNoReadOnly(string orderType, string warehouseType, bool readOnly)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("W2");
			var whs3 = Helper.CreateWarehouse("W3");
			SetupWarehouse(whs1, warehouseType, "USLAX");
			SetupWarehouse(whs2, warehouseType, "PRSJU");

			var orderInUS = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, "O1", data.Part1, 1);
			var orderInPR = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "O2", data.Part1, 1);
			orderInUS.WD_DocketSubType = orderType;
			orderInPR.WD_DocketSubType = orderType;
			var customsDataInUS = orderInUS.Lines.Single().CustomsData;
			var customsDataInPR = orderInPR.Lines.Single().CustomsData;

			AssertEquals($"{customsDataInUS.WB_EntryKeyInfo.Name} column is expected to be {(readOnly ? "readonly" : "editable")} for US warehouses.", readOnly, customsDataInUS.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"{customsDataInPR.WB_EntryKeyInfo.Name} column is expected to be {(readOnly ? "readonly" : "editable")} for PR warehouses.", readOnly, customsDataInPR.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"{customsDataInUS.WB_EntryLineNoInfo.Name} column is expected to be {(readOnly ? "readonly" : "editable")} for US warehouses.", readOnly, customsDataInUS.WB_EntryLineNoInfo.ReadOnly);
			AssertEquals($"{customsDataInPR.WB_EntryLineNoInfo.Name} column is expected to be {(readOnly ? "readonly" : "editable")} for PR warehouses.", readOnly, customsDataInPR.WB_EntryLineNoInfo.ReadOnly);

			if (!readOnly)
			{
				Helper.SetOutwardsEntryKeyForOrderLine(orderInUS.Lines[0], "ABC");
				Helper.SetOutwardsEntryKeyForOrderLine(orderInPR.Lines[0], "ABC");
			}

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Helper.CreatePickNew(orderInUS);
			}
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				Helper.CreatePickNew(orderInPR);
			}

			AssertEquals($"Since Order is in picking status, {customsDataInUS.WB_EntryKeyInfo.Name} column is expected to be readonly for US warehouses.", true, customsDataInUS.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"Since Order is in picking status, {customsDataInPR.WB_EntryKeyInfo.Name} column is expected to be readonly for US warehouses.", true, customsDataInPR.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"Since Order is in picking status, {customsDataInUS.WB_EntryLineNoInfo.Name} column is expected to be readonly for PR warehouses.", true, customsDataInUS.WB_EntryLineNoInfo.ReadOnly);
			AssertEquals($"Since Order is in picking status, {customsDataInPR.WB_EntryLineNoInfo.Name} column is expected to be readonly for PR warehouses.", true, customsDataInPR.WB_EntryLineNoInfo.ReadOnly);

			SetupWarehouse(whs3, warehouseType, "AUSYD");
			var orderInAU = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs3, "O1", data.Part1, 1);
			orderInAU.WD_DocketSubType = orderType;
			var customsData = orderInAU.Lines.Single().CustomsData;
			if (orderInAU.IsCustomsTransaction)
			{
				Helper.SetOutwardsEntryKeyForOrderLine(orderInAU.Lines[0], "ABC");
			}

			AssertEquals($"{customsData.WB_EntryKeyInfo.Name} column is expected to be editable for AU warehouses.", false, customsData.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"{customsData.WB_EntryLineNoInfo.Name} column is expected to be editable for AU warehouses.", false, customsData.WB_EntryLineNoInfo.ReadOnly);

			Helper.CreatePickNew(orderInAU);
			orderInAU.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals($"Since order is in picking status, {customsData.WB_EntryKeyInfo.Name} column is expected to be readonly for AU warehouses.", true, customsData.WB_EntryKeyInfo.ReadOnly);
			AssertEquals($"Since order is in picking status, {customsData.WB_EntryLineNoInfo.Name} column is expected to be readonly for AU warehouses.", true, customsData.WB_EntryLineNoInfo.ReadOnly);
		}

		void SetupWarehouse(WhsWarehouse warehouse, string warehouseType, string portCode)
		{
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = portCode;
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_WarehouseType = warehouseType;
		}

		#endregion

		#region TestWB_RN_NKCountryOfOrigin_ReadOnly

		public void TestWB_RN_NKCountryOfOrigin_Order()
		{
			AssertPropertyIsReadonlyDuringOrder(WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin);
		}

		public void TestWB_RN_NKCountryOfOrigin_Transfer()
		{
			AssertPropertyIsReadonlyDuringTransfer(WhsBondedWarehouseAttributeSchema.Constants.WB_RN_NKCountryOfOrigin);
		}

		#endregion

		#region TestWB_CustomsQty_ReadOnly

		public void TestWB_CustomsQty_Order()
		{
			AssertPropertyIsReadonlyDuringOrder(WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsQty);
		}

		public void TestWB_CustomsQty_Transfer()
		{
			AssertPropertyIsReadonlyDuringTransfer(WhsBondedWarehouseAttributeSchema.Constants.WB_CustomsQty);
		}

		#endregion

		#region TestWB_Tariff_ReadOnly

		public void TestWB_Tariff_Order()
		{
			AssertPropertyIsReadonlyDuringOrder(WhsBondedWarehouseAttributeSchema.Constants.WB_Tariff);
		}

		public void TestWB_Tariff_Transfer()
		{
			AssertPropertyIsReadonlyDuringTransfer(WhsBondedWarehouseAttributeSchema.Constants.WB_Tariff);
		}

		#endregion

		#region TestWB_IsMainInwardsProcessedItem_ReadOnly

		public void TestWB_IsMainInwardsProcessedItem_ReadOnly()
		{
			var attrib = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(true, attrib.WB_IsMainInwardsProcessedItemInfo.ReadOnly);
		}

		#endregion

		#region TestWB_IsSecondaryInwardsProcessedItem_ReadOnly

		public void TestWB_IsSecondaryInwardsProcessedItem_ReadOnly()
		{
			var attrib = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(true, attrib.WB_IsSecondaryInwardsProcessedItemInfo.ReadOnly);
		}

		#endregion

		#region AssertPropertyIsReadonlyDuringOrder

		public void AssertPropertyIsReadonlyDuringOrder(ZString bondedAttributeName)
		{
			var bondedAttr = Factory.New<WhsBondedWarehouseAttribute>();
			var info = bondedAttr.ZPropertyInfoHash[bondedAttributeName];
			AssertEquals(false, info.ReadOnly);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = "CUS";
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "", "ABC");
			bondedAttr.SetParent(orderLine);
			AssertEquals("Regardless of order status, Order must always be readonly.", true, info.ReadOnly);

			Helper.CreatePickNew(order);
			order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals("Regardless of order status, Order must always be readonly.", true, info.ReadOnly);

			Factory.Save();
			AssertEquals("Regardless of order status, Order must always be readonly.", true, info.ReadOnly);

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Regardless of order status, Order must always be readonly.", true, info.ReadOnly);
		}

		#endregion

		#region AssertPropertyIsReadonlyDuringTransfer

		public void AssertPropertyIsReadonlyDuringTransfer(ZString bondedAttributeName)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			receive.RunPreSaveValidation();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "Trf1", null, "CUS");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, data.Whs1.FindLocation("A-1"), data.Whs1.FindLocation("A-2"));

			var bondedAttr = transferLine.CustomsData;
			bondedAttr.SetParent(transferLine);
			var info = bondedAttr.ZPropertyInfoHash[bondedAttributeName];
			AssertEquals("Transfer line hasn't been picked yet so the bonded attribute should be editable", false, info.ReadOnly);

			// A valid pick time is needed to trigger the docket status transition to HFT
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Transfer line has been picked so the line should be held for transfer", true, transferLine.IsHeldForTransfer);
			AssertEquals("When a line is held for transfer, bonded attributes should be read only", true, info.ReadOnly);
		}

		#endregion

		#region TestReadOnly_Order

		public void TestReadOnly_CPSOrder_USFTZ()
		{
			TestReadOnly_Order_FTZ(OrderType.Codes.CustomsReleaseWithPermit, "USLAX");
		}

		public void TestReadOnly_CPSOrder_PRFTZ()
		{
			TestReadOnly_Order_FTZ(OrderType.Codes.CustomsReleaseWithPermit, "PRSJU");
		}

		void TestReadOnly_Order_FTZ(string orderType, string portCode)
		{
			var bondedAttr = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals("Precondition", false, bondedAttr.ReadOnly);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			SetupWarehouse(data.Whs1, WarehouseTypes.Codes.FreeTradeZone, portCode);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = orderType;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			bondedAttr.SetParent(orderLine);

			AssertEquals("FTZ Customs Order must be readonly.", true, bondedAttr.ReadOnly);
			AssertWhsBondedWarehouseAttributeColumnReadOnly("All properties for FTZ Customs Order must be readonly.", bondedAttr, expectedReadOnlyValue: true);
		}

		static void AssertWhsBondedWarehouseAttributeColumnReadOnly(string message, WhsBondedWarehouseAttribute bondedWarehouseAttribute, bool expectedReadOnlyValue)
		{
			AssertEquals("WB_OA_ManufacturerAddress must be readonly.", true, bondedWarehouseAttribute.WB_OA_ManufacturerAddressInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsThirdUnitQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsThirdQuantityInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsSecondUnitQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsSecondQuantityInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_WB_InwardsEntryInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_ValueForDutyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_TILVInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_RX_NKTILVCurrencyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_RN_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_ParentTableCodeInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_ParentIDInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_EntryLineNoInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_EntryKeyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_EntryDateInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_DeclarationReferenceInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsDeadlineInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_InwardStyleInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_InwardProcedureInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsUnitOfQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_CustomsQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_BondedWhsUnitOfQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_BondedWhsQtyInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_AddInfoInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_TariffInfo.ReadOnly);
			AssertEquals(message, expectedReadOnlyValue, bondedWarehouseAttribute.WB_PrimaryPreferenceInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestParentTableCodeConstraint

		public void TestParentTableCodeConstraint_Allow_WE() => AssertTestParentTableCodeConstraint("WE", true);
		public void TestParentTableCodeConstraint_OtherNotAllowed_VV() => AssertTestParentTableCodeConstraint("VV", false);
		public void TestParentTableCodeConstraint_OtherNotAllowed_ZZZ() => AssertTestParentTableCodeConstraint("ZZZ", false);

		public void AssertTestParentTableCodeConstraint(string parentTableCode, bool expectedAllowed)
		{
			using (WarehouseDataRegistry.Instance.EnableImprovedStorageOfCustomsData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Bond.WB_ParentTableCode = parentTableCode;
				Bond.WB_ParentID = ZGuid.NewZGuid();

				if (expectedAllowed)
				{
					AssertNoExceptionThrown(Factory.Save);
				}
				else
				{
					var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_WB_ParentTableCode_NoCheck\".";
					AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, Factory.Save, true);
				}
			}
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidation), bond.Validation.GetType());

			var data = new TestDataSimpleEnvironment(Factory);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var line = adjustment.Lines.AddNew();
			bond.SetParent(line);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForAdjustments), bond.Validation.GetType());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();
			bond.SetParent(orderLine);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForOrders), bond.Validation.GetType());

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var usBondedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var usOrderLine = order.Lines.AddNew();
			bond.SetParent(orderLine);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForOrders), bond.Validation.GetType());

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = transfer.Lines.AddNew();
			bond.SetParent(transferLine);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForTransfers), bond.Validation.GetType());

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			bond.SetParent(receive.Lines[0]);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForReceive), bond.Validation.GetType());

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = workOrder.Lines.AddNew();
			bond.SetParent(workOrderLine);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForComponentOrders), bond.Validation.GetType());

			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = workOrder.Lines.AddNew();
			bond.SetParent(dynamicWorkOrderLine);
			AssertEquals(typeof(WhsBondedWarehouseAttributeValidationForComponentOrders), bond.Validation.GetType());
		}

		#region TestValidateCustomsJobOnly

		public void TestValidateCustomsJobOnly_CUS()
		{
			TestValidateCustomsJobOnly_Core(OrderType.Codes.Customs, expectValidated: true);
		}

		public void TestValidateCustomsJobOnly_CPS()
		{
			TestValidateCustomsJobOnly_Core(OrderType.Codes.CustomsReleaseWithPermit, expectValidated: true);
		}

		public void TestValidateCustomsJobOnly_ORD()
		{
			TestValidateCustomsJobOnly_Core(OrderType.Codes.Order, expectValidated: false);
		}

		void TestValidateCustomsJobOnly_Core(string docketSubType, bool expectValidated)
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();
			bond.SetParent(orderLine);

			order.WD_DocketSubType = docketSubType;
			foreach (var propertyInfo in bond.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.ReadOnly))
			{
				AssertEquals($"Should only validate customs jobs, Type: {order.WD_DocketType} - {propertyInfo.Description}", expectValidated, bond.IsValidationEnabled(propertyInfo));
			}
		}

		public void TestIfParentIsNotSetYet()
		{
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			foreach (var propertyInfo in bond.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.ReadOnly))
			{
				AssertEquals($"Should validate if parent is not set yet, prop: {propertyInfo.Description}.", true, bond.IsValidationEnabled(propertyInfo));
			}
		}

		#endregion

		#endregion

		#region Implementation

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.SetParent(receiveLine);
			return bond;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Bond = (WhsBondedWarehouseAttribute)GetNewBusinessObject();
		}

		WhsBondedWarehouseAttribute Bond;

		#endregion
	}
}
