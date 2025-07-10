using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsProductParamsByWhsAndClient))]
	class WhsProductParamsByWhsAndClientTestCase : WhsBusinessObjectTestCase
	{
		#region TestOperationalActionsFieldToShowVisibility

		public void TestOperationalActionsFieldToShowVisibility()
		{
			AssertEquals(true, ActionFieldAttribute.Get(typeof(WhsProductParamsByWhsAndClient).GetProperty(WhsProductParamsByWhsAndClientSchema.W3_OH.Name)).ReadOnly);

			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty("Warehouse")));
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty("StagingLocationBOM")));
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty("Product")));
		}

		#endregion

		#region Related Entities

		#region TestWarehouse

		public void TestWarehouse()
		{
			var whs = Factory.New<WhsWarehouse>();
			var productParam = (WhsProductParamsByWhsAndClient)GetNewBusinessObject();
			productParam.W3_WW = whs.PK;
			AssertEquals(whs.PK, productParam.Warehouse.PK);
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty(nameof(WhsProductParamsByWhsAndClient.Warehouse))));
		}

		#endregion

		#region TestStagingLocationBOM

		public void TestStagingLocationBOM()
		{
			var productParam = (WhsProductParamsByWhsAndClient)GetNewBusinessObject();
			AssertNull(productParam.StagingLocationBOM);

			var location = Factory.New<WhsLocation>();
			productParam.W3_WL_StagingLocationBOM = location.PK;
			AssertEquals(location.PK, productParam.W3_WL_StagingLocationBOM);
			AssertEquals(location, productParam.StagingLocationBOM);
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty(nameof(WhsProductParamsByWhsAndClient.StagingLocationBOM))));
		}

		#endregion

		#region TestInwardProcessingStagingLocationBOM

		public void TestInwardProcessingStagingLocationBOM()
		{
			var productParam = (WhsProductParamsByWhsAndClient)GetNewBusinessObject();
			AssertNull(productParam.InwardProcessingStagingLocationBOM);

			var location = Factory.New<WhsLocation>();
			productParam.W3_WL_InwardsProcessingStagingLocationBOM = location.PK;
			AssertEquals(location, productParam.InwardProcessingStagingLocationBOM);
			AssertEquals(false, ActionFieldFollowAttribute.ShouldFollow(typeof(WhsProductParamsByWhsAndClient).GetProperty(nameof(WhsProductParamsByWhsAndClient.InwardProcessingStagingLocationBOM))));
		}

		#endregion

		#region TestPutawayGroup

		public void TestPutawayGroup()
		{
			var putawayGroup = Helper.CreatePutawayGroup("ABC", "ABC");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();
			AssertNull(param.PutawayGroup);

			param.W3_WPG_PutawayGroup = putawayGroup.PK;
			AssertEquals(putawayGroup.PK, param.PutawayGroup.PK);
		}

		#endregion

		#endregion

		#region Properties

		#region TestPickGroupForBinding

		public void TestPickGroupForBinding()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var productParams = Factory.New<WhsProductParamsByWhsAndClient>();
				AssertEquals("", productParams.PickGroupForBinding);

				productParams.W3_PickGroup = 1;
				AssertEquals("1 - Desc", productParams.PickGroupForBinding);

				productParams.PickGroupForBinding = "A";
				AssertEquals(new ZShort(0), productParams.W3_PickGroup);

				productParams.PickGroupForBinding = "2";
				AssertEquals(new ZShort(2), productParams.W3_PickGroup);
				AssertEquals("2", productParams.PickGroupForBinding);

				productParams.PickGroupForBinding = "";
				AssertEquals(new ZShort(0), productParams.W3_PickGroup);

				productParams.PickGroupForBinding = "1.1";
				AssertEquals(new ZShort(11), productParams.W3_PickGroup);

				productParams.PickGroupForBinding = "111111";
				AssertEquals(short.MaxValue, productParams.W3_PickGroup);
			}
		}

		#endregion

		#region TestPickGroupForBindingInfo

		public void TestPickGroupForBindingInfo()
		{
			var productParams = Factory.New<WhsProductParamsByWhsAndClient>();
			AssertEquals(PickGroupHelper.PickGroupForBindingMaxLength, productParams.PickGroupForBindingInfo.MaxLength);
		}

		#endregion

		#region TestW3_EconomicQuantity

		public void TestW3_EconomicQuantity()
		{
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			AssertEquals("Precondition", 0m, param.W3_EconomicQuantity);
			AssertEquals("Precondition", 0m, param.W3_ReplenishmentMultiple);

			param.W3_ReplenishmentMultiple = 2m;
			param.W3_EconomicQuantity = 5m;
			AssertEquals(5m, param.W3_EconomicQuantity);
			AssertEquals(2m, param.W3_ReplenishmentMultiple);

			param.W3_ReplenishmentMultiple = 0m;
			param.W3_EconomicQuantity = 10m;
			AssertEquals(10m, param.W3_EconomicQuantity);
			AssertEquals(1m, param.W3_ReplenishmentMultiple);

			param.W3_ReplenishmentMultiple = -1m;
			param.W3_EconomicQuantity = 10m;
			AssertEquals(10m, param.W3_EconomicQuantity);
			AssertEquals(1m, param.W3_ReplenishmentMultiple);
		}

		#endregion

		#region TestW3_ABCAnalysisCategory

		[TestDate(2012, 1, 1)]
		public void TestW3_ABCAnalysisCategory()
		{
			var oldAbcCategory = Factory.New<WhsABCCategory>();
			oldAbcCategory.WJ_OP_Product = Param.W3_OP;
			oldAbcCategory.WJ_OH_Client = Param.W3_OH;
			oldAbcCategory.WJ_WW_Warehouse = Param.W3_WW;
			oldAbcCategory.WJ_Category = "XXX";
			oldAbcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2011, 1, 1);

			var abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_OP_Product = Param.W3_OP;
			abcCategory.WJ_OH_Client = Param.W3_OH;
			abcCategory.WJ_WW_Warehouse = Param.W3_WW;
			abcCategory.WJ_Category = "ABC";
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 1, 1);
			AssertEquals("ABC", Param.W3_ABCAnalysisCategory);
		}

		#endregion

		#region TestW3_ABCAnalysisPeriod

		[TestDate(2012, 1, 1)]
		public void TestW3_ABCAnalysisPeriod()
		{
			var oldAbcCategory = Factory.New<WhsABCCategory>();
			oldAbcCategory.WJ_OP_Product = Param.W3_OP;
			oldAbcCategory.WJ_OH_Client = Param.W3_OH;
			oldAbcCategory.WJ_WW_Warehouse = Param.W3_WW;
			oldAbcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2011, 1, 1, 0, 0, 0, TimeSpan.FromHours(-5));

			var abcCategory = Factory.New<WhsABCCategory>();
			abcCategory.WJ_OP_Product = Param.W3_OP;
			abcCategory.WJ_OH_Client = Param.W3_OH;
			abcCategory.WJ_WW_Warehouse = Param.W3_WW;
			abcCategory.WJ_AnalysisDateTo = new ZDateTimeOffset(2012, 1, 1, 0, 0, 0, TimeSpan.FromHours(-5));
			abcCategory.WJ_Category = "A";
			abcCategory.WJ_AnalysisPeriod = "LCW";
			AssertEquals("25-Dec-11 to 31-Dec-11", Param.W3_ABCAnalysisPeriod);
		}

		#endregion

		#region TestUQ

		public void TestUQ()
		{
			AssertEquals(ZString.Empty, Param.UQ);
			Param.W3_OP = Factory.New<OrgSupplierPart>().PK;
			Param.SupplierPart.OP_StockKeepingUnit = "CTN";
			AssertEquals("CTN", Param.UQ);
		}

		#endregion

		#region TestUQInfo

		public void TestUQInfo()
		{
			AssertEquals("UQ", Param.UQInfo.Name);
			AssertEquals(true, Param.UQInfo.ReadOnly);
		}

		#endregion

		#region TestW3_WW

		public void TestW3_WW()
		{
			var whs1 = Helper.CreateWarehouse("Whs1");
			var whs2 = Helper.CreateWarehouse("Whs2");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();
			param.W3_WW = whs1.PK;
			AssertEquals("Precondition", whs1, param.Warehouse);

			param.W3_WW = whs1.PK;
			AssertEquals(whs1, param.Warehouse);

			param.W3_WW = whs2.PK;
			AssertEquals(whs2, param.Warehouse);
		}

		public void TestW3_WW_SetExpiryNotificationPeriodToClientDefault_WhenClientAndWarehousePopulatedAndValid()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = whs.PK;
			AssertEquals("Should set Notification Period to client default value.", 10, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_SetExpiryNotificationPeriodToClientDefault_DoNotSetIfValueUnchanged()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			param.W3_WW = whs.PK;
			param.W3_ExpiryNotificationPeriod = 0;
			AssertEquals("Precondition.", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = whs.PK;
			AssertEquals("Should not set Notification Period to client default value as warehouse unchanged.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_DoNotSetExpiryNotificationPeriodToDefault_WhenClientInvalid()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = ZGuid.NewZGuid();
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = whs.PK;
			AssertEquals("Should not set Notification Period to client default value if client not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_DoNotSetExpiryNotificationPeriodToDefault_WhenWarehouseInvalid()
		{
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = ZGuid.NewZGuid();
			AssertEquals("Should not set Notification Period to client default value if warehouse invalid.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_DoNotSetExpiryNotificationPeriodToDefault_WhenClientNotPopulated()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = ZGuid.Empty;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = whs.PK;
			AssertEquals("Should not set Notification Period to client default value if client not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_DoNotSetExpiryNotificationPeriodToDefault_WhenWarehouseNotPopulated()
		{
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = ZGuid.Empty;
			AssertEquals("Should not set Notification Period to client default value if warehouse not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_WW_DoNotSetExpiryNotificationPeriodToDefault_WhenExpiryNotificationPeriodNotZero()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			param.W3_ExpiryNotificationPeriod = 5;
			AssertEquals("Precondition", 5, param.W3_ExpiryNotificationPeriod);

			param.W3_WW = whs.PK;
			AssertEquals("Should not set Notification Period to client default value if value previously set.", 5, param.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#region TestW3_WL_StagingLocationBOM_List

		public void TestW3_WL_StagingLocationBOM_List()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsProductParamsByWhsAndClient), nameof(WhsProductParamsByWhsAndClient.W3_WL_StagingLocationBOM), false,
				la => la.ListDataSourceMember == "Lookups.StagingLocationsBOM");
		}

		#endregion

		#region TestW3_WL_InwardsProcessingStagingLocationBOM_List

		public void TestW3_WL_InwardsProcessingStagingLocationBOM_List()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsProductParamsByWhsAndClient), nameof(WhsProductParamsByWhsAndClient.W3_WL_InwardsProcessingStagingLocationBOM), false,
				la => la.ListDataSourceMember == "Lookups.StagingLocationsBOM");
		}

		#endregion

		#region TestW3_MaximumShelfLifeReadonly

		public void TestW3_MaximumShelfLifeReadonly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true); // part 2 with any attribute
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // part 1 with Julian Batch Number
			var part3 = Helper.CreateProduct(data.Org1, "P3"); // part 3 with no attributes
			var part4 = Helper.CreateProduct(client2, "P4"); // part 4 without client relationship

			var clientParamWithAnyAttribute = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var clientParamWithJulianBatchNumber = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);
			var clientParamWithNoAttributes = Helper.CreateProductParamsByWhsAndClient(part3, data.Org1, data.Whs1);
			var clientParamWithoutClientRelationship = Helper.CreateProductParamsByWhsAndClient(part4, data.Org1, data.Whs1);

			AssertEquals(true, clientParamWithAnyAttribute.W3_MaximumShelfLifeInfo.ReadOnly);
			AssertEquals(false, clientParamWithJulianBatchNumber.W3_MaximumShelfLifeInfo.ReadOnly);
			AssertEquals(true, clientParamWithNoAttributes.W3_MaximumShelfLifeInfo.ReadOnly);
			AssertEquals(true, clientParamWithoutClientRelationship.W3_MaximumShelfLifeInfo.ReadOnly);
		}

		public void TestW3_MaximumShelfLifeReadonly_ProductWithPackingAndExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var clientParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			AssertEquals("Maximum shelf life is not read-only if product uses both packing and expiry date.", false, clientParam.W3_MaximumShelfLifeInfo.ReadOnly);
		}

		public void TestW3_MaximumShelfLifeReadonly_ProductWithPackingDateOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var clientParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			AssertEquals("Maximum shelf life is read-only.", true, clientParam.W3_MaximumShelfLifeInfo.ReadOnly);
		}

		public void TestW3_MaximumShelfLifeReadonly_ProductWithExpiryDateOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var clientParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			AssertEquals("Maximum shelf life is read-only.", true, clientParam.W3_MaximumShelfLifeInfo.ReadOnly);
		}

		#endregion

		#region TestW3_OH

		public void TestW3_OH_SetExpiryNotificationPeriodToClientDefault_WhenClientAndWarehousePopulatedAndValid()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = whs.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = org.PK;
			AssertEquals("Should set Notification Period to client default value.", 10, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_SetExpiryNotificationPeriodToClientDefault_DoNotSetIfValueUnchanged()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_OH = org.PK;
			param.W3_WW = whs.PK;
			param.W3_ExpiryNotificationPeriod = 0;
			AssertEquals("Precondition.", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = org.PK;
			AssertEquals("Should not set Notification Period to client default value as client unchanged.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_DoNotSetExpiryNotificationPeriodToDefault_WhenClientInvalid()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = whs.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = ZGuid.NewZGuid();
			AssertEquals("Should not set Notification Period to client default value if client not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_DoNotSetExpiryNotificationPeriodToDefault_WhenWarehouseInvalid()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = ZGuid.NewZGuid();
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = org.PK;
			AssertEquals("Should not set Notification Period to client default value if warehouse invalid.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_DoNotSetExpiryNotificationPeriodToDefault_WhenClientNotPopulated()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = whs.PK;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = ZGuid.Empty;
			AssertEquals("Should not set Notification Period to client default value if client not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_DoNotSetExpiryNotificationPeriodToDefault_WhenWarehouseNotPopulated()
		{
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = ZGuid.Empty;
			AssertEquals("Precondition", 0, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = org.PK;
			AssertEquals("Should not set Notification Period to client default value if warehouse not populated.", 0, param.W3_ExpiryNotificationPeriod);
		}

		public void TestW3_OH_DoNotSetExpiryNotificationPeriodToDefault_WhenExpiryNotificationPeriodNotZero()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var org = Helper.CreateClient("Org");
			var miscServ = Factory.New<OrgMiscServ>();
			miscServ.OM_WhsDefaultExpiryNotificationPeriodInDays = 10;
			org.MiscServ = miscServ;
			var param = Factory.New<WhsProductParamsByWhsAndClient>();

			param.W3_WW = whs.PK;
			param.W3_ExpiryNotificationPeriod = 5;
			AssertEquals("Precondition", 5, param.W3_ExpiryNotificationPeriod);

			param.W3_OH = org.PK;
			AssertEquals("Should not set Notification Period to client default value if value previously set.", 5, param.W3_ExpiryNotificationPeriod);
		}

		#endregion

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);

			Factory.Save();

			AssertEquals(true, productParam_WithoutStock.CanDelete);
			AssertEquals(false, productParam_WithStock.CanDelete);

			var otherClient = Helper.CreateClient("CLIENT2");
			productParam_WithStock.W3_OH = otherClient.PK;
			AssertEquals(false, productParam_WithStock.CanDelete);
			productParam_WithStock.W3_OH = data.Org1.PK; // clean up

			var otherWhs = Helper.CreateWarehouse("WH2");
			productParam_WithStock.W3_WW = otherWhs.PK;
			AssertEquals(false, productParam_WithStock.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete

		public void TestReasonForNotAbleToDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);
			var productParam_WithoutStock = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			var productParam_WithStock = Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m, true, false);

			Factory.Save();

			var expectedErrorMessage = "There is current inventory using Julian Batch numbers. This inventory must be removed from the warehouse before this record can be deleted.";

			AssertEquals("", productParam_WithoutStock.ReasonForNotAbleToDelete);
			AssertEquals(expectedErrorMessage, productParam_WithStock.ReasonForNotAbleToDelete);

			var otherClient = Helper.CreateClient("CLIENT2");
			productParam_WithStock.W3_OH = otherClient.PK;
			AssertEquals(expectedErrorMessage, productParam_WithStock.ReasonForNotAbleToDelete);
			productParam_WithStock.W3_OH = data.Org1.PK; // clean up

			var otherWhs = Helper.CreateWarehouse("WH2");
			productParam_WithStock.W3_WW = otherWhs.PK;
			AssertEquals(expectedErrorMessage, productParam_WithStock.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestTriggers

		#region TestTrigger_TG_PreventMismatchWarehouseAndAreaOrLocation

		[ExpectNoExceptions]
		public void TestTrigger_TG_PreventMismatchWarehouseAndAreaOrLocation_Insert()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Whs2");
			var area1 = Helper.CreateArea(whs1, "A");
			var area2 = Helper.CreateArea(whs2, "B");
			var location2 = Helper.CreateRowAndGenerateLocations(whs2, "R").Locations[0];

			var param = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, whs1);
			param.W3_WL_StagingLocationBOM = location2.PK;

			var expectedMsg = "All locations and areas must belongs to same warehouse on WhsProductParamsByWhsAndClient.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedMsg), "Trigger error should be thrown.");
		}

		[ExpectNoExceptions]
		public void TestTrigger_TG_PreventMismatchWarehouseAndAreaOrLocation_StagingLocationBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Whs2");
			var area1 = Helper.CreateArea(whs1, "A");
			var location1 = Helper.CreateRowAndGenerateLocations(whs1, "R1").Locations[0];
			var area2 = Helper.CreateArea(whs2, "B");
			var location2 = Helper.CreateRowAndGenerateLocations(whs2, "R2").Locations[0];

			var param = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			param.W3_WL_StagingLocationBOM = location1.PK;
			Factory.Save();

			param.W3_WL_StagingLocationBOM = location2.PK;

			var expectedMsg = "All locations and areas must belongs to same warehouse on WhsProductParamsByWhsAndClient.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedMsg), "Trigger error should be thrown.");
		}

		[ExpectNoExceptions]
		public void TestTrigger_TG_PreventMismatchWarehouseAndAreaOrLocation_Warehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			var whs2 = Helper.CreateWarehouse("Whs2");
			var area1 = Helper.CreateArea(whs1, "A");
			var location1 = Helper.CreateRowAndGenerateLocations(whs1, "R1").Locations[0];
			var area2 = Helper.CreateArea(whs2, "B");

			var param = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			param.W3_WL_StagingLocationBOM = location1.PK;
			Factory.Save();

			param.W3_WW = whs2.PK;

			var expectedMsg = "All locations and areas must belongs to same warehouse on WhsProductParamsByWhsAndClient.";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), expectedMsg), "Trigger error should be thrown.");
		}

		#endregion

		#endregion

		#region Implementation

		protected WhsProductParamsByWhsAndClient Param
		{
			get { return param ?? (param = (WhsProductParamsByWhsAndClient)GetNewBusinessObject()); }
		}

		WhsProductParamsByWhsAndClient param;

		#endregion
	}
}
