using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsClientPickPackParamsByWhs))]
	class WhsClientPickPackParamsByWhsTest : EnterpriseBusinessObjectTestCase
	{
		#region Related Entities

		#region TestSalesChannel

		public void TestSalesChannel()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertNull(pickPackParameter.SalesChannel);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "Ecommerce");
			pickPackParameter.WPP_WSH_SalesChannel = salesChannel.PK;
			AssertEquals("Sales Channel should be returned when Sales Channel FK is set.", salesChannel, pickPackParameter.SalesChannel);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertNull(pickPackParameter.Warehouse);

			var warehouse = Helper.CreateWarehouse("WHS");
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse should be returned when Warehouse FK is set.", warehouse, pickPackParameter.Warehouse);
		}

		#endregion

		#endregion

		#region Properties

		#region TestWarehouseName

		public void TestWarehouseName()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Warehouse Name should be empty if no Warehouse is specified.", "", pickPackParameter.WarehouseName);

			var warehouse = Helper.CreateWarehouse("Big Warehouse");
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse Name should return Warehouse Name if Warehouse is specified.", "Big Warehouse", pickPackParameter.WarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			var warehouse = Helper.CreateWarehouse("Test Warehouse");
			pickPackParameter.WPP_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse Name in English.", "Test Warehouse", pickPackParameter.WarehouseName);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("Warehouse Name in Chinese.", "测试仓库", pickPackParameter.WarehouseName);
			}
		}

		#endregion

		#region TestWPP_NumberOfLabelsToPrintOnCloseInfo

		public void TestWPP_NumberOfLabelsToPrintOnCloseInfo()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Precondition", true, pickPackParameter.WPP_IsPickAndPackEnabled);
			AssertEquals("Should be editable if Pick And Pack is enabled.", false, pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo.ReadOnly);

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertEquals("Should be read only if Pick And Pack is not enabled.", true, pickPackParameter.WPP_NumberOfLabelsToPrintOnCloseInfo.ReadOnly);
		}

		#endregion

		#region TestWPP_NumberOfLabelsToPrintOnNewInfo

		public void TestWPP_NumberOfLabelsToPrintOnNewInfo()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Precondition", true, pickPackParameter.WPP_IsPickAndPackEnabled);
			AssertEquals("Should be editable if Pick And Pack is enabled.", false, pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo.ReadOnly);

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertEquals("Should be read only if Pick And Pack is not enabled.", true, pickPackParameter.WPP_NumberOfLabelsToPrintOnNewInfo.ReadOnly);
		}

		#endregion

		#region TestWPP_F3_NKPackTypeInfo

		public void TestWPP_F3_NKPackTypeInfo()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Precondition", true, pickPackParameter.WPP_IsPickAndPackEnabled);
			AssertEquals("Should be editable if Pick And Pack is enabled.", false, pickPackParameter.WPP_F3_NKPackTypeInfo.ReadOnly);

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertEquals("Should be read only if Pick And Pack is not enabled.", true, pickPackParameter.WPP_F3_NKPackTypeInfo.ReadOnly);
		}

		#endregion

		#region TestWPP_IsUsingOwnLabel

		public void TestWPP_IsUsingOwnLabel()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Precondition", true, pickPackParameter.WPP_IsPickAndPackEnabled);
			AssertEquals("Should be editable if Pick And Pack is enabled.", false, pickPackParameter.WPP_IsUsingOwnLabelInfo.ReadOnly);

			pickPackParameter.WPP_IsPickAndPackEnabled = false;
			AssertEquals("Should be read only if Pick And Pack is not enabled.", true, pickPackParameter.WPP_IsUsingOwnLabelInfo.ReadOnly);
		}

		#endregion

		#region TestWPP_AllowPickFinalizationWithUnpackedTotes

		public void TestWPP_AllowPickFinalizationWithUnpackedTotes()
		{
			var pickPackParameter = Factory.New<WhsClientPickPackParamsByWhs>();
			AssertEquals("Default value of new PickPackParam should be false", false, pickPackParameter.WPP_AllowPickFinalizationWithUnpackedTotes);
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
