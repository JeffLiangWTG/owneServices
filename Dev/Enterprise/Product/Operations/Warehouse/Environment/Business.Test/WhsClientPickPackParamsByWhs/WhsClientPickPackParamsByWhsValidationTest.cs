using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsClientPickPackParamsByWhsValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWPP_F3_NKPackType

		public void TestCheckWPP_F3_NKPackType()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter.WPP_F3_NKPackTypeInfo);

			pickPackParameter.WPP_F3_NKPackType = "";
			AssertNoErrors(pickPackParameter.WPP_F3_NKPackTypeInfo);

			pickPackParameter.WPP_F3_NKPackType = "XXX";
			AssertHasError(pickPackParameter.WPP_F3_NKPackTypeInfo, "Enter a valid RF Pack Type.");

			pickPackParameter.WPP_F3_NKPackType = "PLT";
			AssertNoErrors(pickPackParameter.WPP_F3_NKPackTypeInfo);
		}

		#endregion

		#region TestCheckWPP_NumberOfLabelsToPrintOnClose

		public void TestCheckWPP_NumberOfLabelsToPrintOnClose()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 0;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = -1;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo, "No. Labels to Print on Close cannot be negative.");

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 1;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 0;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 0;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo, "Must print at least one Label for New or Close Package.");

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_IsPickAndPackEnabled = true;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo, "Must print at least one Label for New or Close Package.");

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 1;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 50;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 51;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo, "Should not print more than 50 Labels.");
		}

		#endregion

		#region TestCheckWPP_NumberOfLabelsToPrintOnNew

		public void TestCheckWPP_NumberOfLabelsToPrintOnNew()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 0;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = -1;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo, "No. Labels to Print on New cannot be negative.");

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 1;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 0;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 0;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo, "Must print at least one Label for New or Close Package.");

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_IsPickAndPackEnabled = true;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo, "Must print at least one Label for New or Close Package.");

			pickPackParameter.WPP_NumberOfLabelsToPrintOnClose = 1;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 50;
			AssertNoErrors(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo);

			pickPackParameter.WPP_NumberOfLabelsToPrintOnNew = 51;
			AssertHasError(pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo, "Should not print more than 50 Labels.");
		}

		#endregion

		#region TestCheckWPP_PromptForWeightAndDimensions

		public void TestCheckWPP_PromptForWeightAndDimensions()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter.WPP_PromptForWeightAndDimensionsInfo);

			pickPackParameter.WPP_PromptForWeightAndDimensions = false;
			AssertNoErrors(pickPackParameter.WPP_PromptForWeightAndDimensionsInfo);

			pickPackParameter.WPP_PromptForWeightAndDimensions = true;
			AssertNoErrors(pickPackParameter.WPP_PromptForWeightAndDimensionsInfo);

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertHasError(pickPackParameter.WPP_PromptForWeightAndDimensionsInfo, "This setting is only usable if Pick & Pack is enabled.");

			pickPackParameter.WPP_PromptForWeightAndDimensions = false;
			AssertNoErrors(pickPackParameter.WPP_PromptForWeightAndDimensionsInfo);
		}

		#endregion

		#region TestCheckWPP_WSH_SalesChannel

		public void TestCheckWPP_WSH_SalesChannel()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter1 = Helper.CreatePickPackParameter(client);
			pickPackParameter1.WPP_CycleCountOnShort = false;
			pickPackParameter1.WPP_WW_Warehouse = warehouse.PK;
			AssertNoErrors("Precondition:", pickPackParameter1.WPP_WSH_SalesChannelInfo);

			pickPackParameter1.Validation.ValidateWPP_WSH_SalesChannel();
			AssertNoErrors(pickPackParameter1.WPP_WSH_SalesChannelInfo);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter1.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertNoErrors(pickPackParameter1.WPP_WSH_SalesChannelInfo);

			pickPackParameter1.WPP_WSH_SalesChannel = ZGuid.Invalid;
			AssertHasError(pickPackParameter1.WPP_WSH_SalesChannelInfo, "Enter a valid Sales Channel.");

			pickPackParameter1.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertNoErrors(pickPackParameter1.WPP_WSH_SalesChannelInfo);

			var pickPackParameter2 = Helper.CreatePickPackParameter(client);
			pickPackParameter2.WPP_CycleCountOnShort = false;
			pickPackParameter2.WPP_WW_Warehouse = warehouse.PK;
			AssertNoErrors("Precondition:", pickPackParameter2.WPP_WSH_SalesChannelInfo);

			pickPackParameter2.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertHasError(pickPackParameter2.WPP_WSH_SalesChannelInfo, "The same Warehouse & Sales Channel can only be specified once per Client.");

			var otherSalesChannel = Helper.CreateWhsSalesChannel("DBR", "Distribution");
			pickPackParameter2.WPP_WSH_SalesChannel = otherSalesChannel.PK;
			AssertNoErrors(pickPackParameter2.WPP_WSH_SalesChannelInfo);

			pickPackParameter2.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertHasError(pickPackParameter2.WPP_WSH_SalesChannelInfo, "The same Warehouse & Sales Channel can only be specified once per Client.");

			var otherWarehouse = Helper.CreateWarehouse("COL");
			pickPackParameter2.WPP_WW_Warehouse = otherWarehouse.PK;
			AssertNoErrors(pickPackParameter2.WPP_WSH_SalesChannelInfo);
		}

		public void TestCheckWPP_WSH_SalesChannel_CycleCountOnShort()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors("Precondition:", pickPackParameter.WPP_WSH_SalesChannelInfo);

			pickPackParameter.Validation.ValidateWPP_WSH_SalesChannel();
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);

			pickPackParameter.WPP_CycleCountOnShort = true;
			AssertHasError(pickPackParameter.WPP_WSH_SalesChannelInfo, CycleCountOnShortWithSalesChannelError);

			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);
		}

		const string CycleCountOnShortWithSalesChannelError = "Cycle Count on Short is not valid with a Sales Channel.";

		public void TestCheckWPP_WSH_SalesChannel_AllowPickDockDoorLocationOverride()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = false;
			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors("Precondition:", pickPackParameter.WPP_WSH_SalesChannelInfo);

			pickPackParameter.Validation.ValidateWPP_WSH_SalesChannel();
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);

			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = true;
			AssertHasError(pickPackParameter.WPP_WSH_SalesChannelInfo, AllowPickDockDoorLocationWithSalesChannelError);

			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = false;
			AssertNoErrors(pickPackParameter.WPP_WSH_SalesChannelInfo);
		}

		#endregion

		#region TestCheckWPP_CycleCountOnShort

		public void TestCheckWPP_CycleCountOnShort()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors("Precondition:", pickPackParameter.WPP_CycleCountOnShortInfo);

			pickPackParameter.Validation.ValidateWPP_CycleCountOnShort();
			AssertNoErrors(pickPackParameter.WPP_CycleCountOnShortInfo);

			pickPackParameter.WPP_CycleCountOnShort = true;
			AssertNoErrors(pickPackParameter.WPP_CycleCountOnShortInfo);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertHasError(pickPackParameter.WPP_CycleCountOnShortInfo, CycleCountOnShortWithSalesChannelError);

			pickPackParameter.WPP_WSH_SalesChannel = ZGuid.Empty;
			AssertNoErrors(pickPackParameter.WPP_CycleCountOnShortInfo);

			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors(pickPackParameter.WPP_CycleCountOnShortInfo);
		}

		#endregion

		#region TestCheckWPP_AllowPickDockDoorLocationOverride

		public void TestCheckWPP_AllowPickDockDoorLocationOverride()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = false;
			pickPackParameter.WPP_CycleCountOnShort = false;
			AssertNoErrors("Precondition:", pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo);

			pickPackParameter.Validation.ValidateWPP_AllowPickDockDoorLocationOverride();
			AssertNoErrors(pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo);

			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = true;
			AssertNoErrors(pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertHasError(pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo, AllowPickDockDoorLocationWithSalesChannelError);

			pickPackParameter.WPP_WSH_SalesChannel = ZGuid.Empty;
			AssertNoErrors(pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo);

			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			pickPackParameter.WPP_AllowPickDockDoorLocationOverride = false;
			AssertNoErrors(pickPackParameter.WPP_AllowPickDockDoorLocationOverrideInfo);
		}

		const string AllowPickDockDoorLocationWithSalesChannelError = "Pick Dock Door Override is not valid with a Sales Channel.";

		#endregion

		#region TestCheckWPP_WW_Warehouse

		public void TestCheckWPP_WW_Warehouse()
		{
			var client = Helper.CreateClient();
			var pickPackParameter1 = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter1.WPP_WW_WarehouseInfo);

			pickPackParameter1.Validation.ValidateWPP_WW_Warehouse();
			AssertHasError(pickPackParameter1.WPP_WW_WarehouseInfo, "Please enter a Warehouse.");

			var warehouse = Helper.CreateWarehouse("WHS");
			pickPackParameter1.WPP_WW_Warehouse = warehouse.PK;
			AssertNoErrors(pickPackParameter1.WPP_WW_WarehouseInfo);

			pickPackParameter1.WPP_WW_Warehouse = ZGuid.Invalid;
			AssertHasError(pickPackParameter1.WPP_WW_WarehouseInfo, "Enter a valid Warehouse.");

			pickPackParameter1.WPP_WW_Warehouse = warehouse.PK;
			AssertNoErrors(pickPackParameter1.WPP_WW_WarehouseInfo);

			var pickPackParameter2 = Helper.CreatePickPackParameter(client);
			AssertNoErrors("Precondition:", pickPackParameter2.WPP_WW_WarehouseInfo);

			pickPackParameter2.WPP_WW_Warehouse = warehouse.PK;
			AssertHasError(pickPackParameter2.WPP_WW_WarehouseInfo, "The same Warehouse & Sales Channel can only be specified once per Client.");

			var otherWarehouse = Helper.CreateWarehouse("ABC");
			pickPackParameter2.WPP_WW_Warehouse = otherWarehouse.PK;
			AssertNoErrors(pickPackParameter2.WPP_WW_WarehouseInfo);

			pickPackParameter2.WPP_WW_Warehouse = warehouse.PK;
			AssertHasError(pickPackParameter2.WPP_WW_WarehouseInfo, "The same Warehouse & Sales Channel can only be specified once per Client.");

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			pickPackParameter2.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertNoErrors(pickPackParameter2.WPP_WW_WarehouseInfo);
		}

		#endregion

		#region TestCheckWPP_CartonizeByProductOrProductCategory

		public void TestCheckWPP_CartonizeByProductOrProductCategory()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);

			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductInfo);
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductCategoryInfo);

			pickPackParameter.WPP_CartonizeByProduct = true;
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductInfo);
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductCategoryInfo);

			pickPackParameter.WPP_CartonizeByProductCategory = true;
			AssertHasError(pickPackParameter.WPP_CartonizeByProductInfo, "Cartonize By Product and Cartonize By Product Category should not be selected at the same time.");
			AssertHasError(pickPackParameter.WPP_CartonizeByProductCategoryInfo, "Cartonize By Product and Cartonize By Product Category should not be selected at the same time.");

			pickPackParameter.WPP_CartonizeByProduct = false;
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductInfo);
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductCategoryInfo);

			pickPackParameter.WPP_CartonizeByProduct = true;
			AssertHasError(pickPackParameter.WPP_CartonizeByProductInfo, "Cartonize By Product and Cartonize By Product Category should not be selected at the same time.");
			AssertHasError(pickPackParameter.WPP_CartonizeByProductCategoryInfo, "Cartonize By Product and Cartonize By Product Category should not be selected at the same time.");

			pickPackParameter.WPP_CartonizeByProductCategory = false;
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductInfo);
			AssertNoErrors(pickPackParameter.WPP_CartonizeByProductCategoryInfo);
		}

		#endregion

		#region TestCheckWPP_UseDirectedPackingConsolidation

		public void TestCheckWPP_UseDirectedPackingConsolidation_Disabled()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");

			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;

			pickPackParameter.WPP_UseDirectedPackingConsolidation = false;
			AssertNoErrors(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo);

			pickPackParameter.WPP_WW_Warehouse = ZGuid.Invalid;
			AssertNoErrors(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo);

			pickPackParameter.Validation.ValidateWPP_WW_Warehouse();
			AssertNoErrors(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo);
		}

		public void TestCheckWPP_UseDirectedPackingConsolidation_Enabled()
		{
			const string ErrorMessage = "Use Directed Packing Consolidation should not be enabled as there are no Packing Consolidation Locations in this Warehouse.";

			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("WHS");

			var pickPackParameter = Helper.CreatePickPackParameter(client);
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			Factory.Save();

			pickPackParameter.WPP_UseDirectedPackingConsolidation = true;
			AssertHasError(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo, ErrorMessage);

			pickPackParameter.WPP_UseDirectedPackingConsolidation = false;
			AssertNoErrors(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo);

			var packingConsolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(warehouse, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			Factory.Save();

			pickPackParameter.WPP_UseDirectedPackingConsolidation = true;
			AssertNoErrors(pickPackParameter.WPP_UseDirectedPackingConsolidationInfo);
		}

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var client = Helper.CreateClient();
			var pickPackParameter = Helper.CreatePickPackParameter(client);
			var validation = new TestWhsClientPickPackParamsByWhsValidation(pickPackParameter);

			var list = new string[]
			{
				WhsClientPickPackParamsByWhsSchema.Constants.WPP_WSH_SalesChannel,
				WhsClientPickPackParamsByWhsSchema.Constants.WPP_F3_NKPackType
			};

			foreach (var propertyInfo in pickPackParameter.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		#region TestWhsClientPickPackParamsByWhsValidation

		class TestWhsClientPickPackParamsByWhsValidation : WhsClientPickPackParamsByWhsValidation
		{
			public TestWhsClientPickPackParamsByWhsValidation(WhsClientPickPackParamsByWhs parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion
	}
}
