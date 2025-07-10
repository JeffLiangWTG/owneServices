using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class OrderLineFilterControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestColumns()
		{
			var order = Factory.New<WhsOrder>();
			var orderLines = order.Lines;
			var filterBO = new OrderLineFilterBusinessObject();

			using (var form = new ZForm())
			using (var control = new OrderLineFilterStripControl(orderLines, filterBO))
			{
				form.Controls.Add(control);
				form.Show();

				var systemColumns = new HashSet<ZString>
				{ // In this test these columns are added by parent classes but not functionally.
					WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc,
					WhsDocketLineSchema.Constants.WE_SystemCreateUser,
					WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc,
					WhsDocketLineSchema.Constants.WE_SystemLastEditUser
				};

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"ClientDetail",
					"WarehousePK",
					"Order+WD_DocketSubType",
					"ConsigneeDetail",
					"Order+WD_ExternalReference",
					"Order+WarehouseOrderStatus",
					"WE_OP",
					"ProductDesc",
					"WE_PackQuantity",
					"WE_F3_NKPackType",
					"SumOfUnitsMet",
					"WE_LineComment",
					"WE_ExpiryDate",
					"CustomsTariffLookup",
					"CustomsTariffDesc",
					"CustomsTariffItem",
					"CustomsData+WB_EntryKey",
					"CustomsData+WB_EntryLineNo",
					"CustomsData+WB_EntryDate",
					"CustomsData+WB_DeclarationReference",
					"CustomsData+WB_CustomsDeadline",
					"CustomsData+WB_InwardStyle",
					"CustomsData+WB_InwardProcedure",
					"CustomsData+WB_RN_NKCountryOfOrigin",
					"CustomsData+WB_CustomsQty",
					"CustomsData+WB_CustomsUnitOfQty",
					"CustomsData+WB_ValueForDuty",
					"CustomsData+WB_TILV",
					"CustomsData+WB_AddInfo",
					"CustomsData+WB_CustomsSecondQuantity",
					"CustomsData+WB_CustomsSecondUnitQty",
					"CustomsData+WB_CustomsThirdQuantity",
					"CustomsData+WB_CustomsThirdUnitQty",
					"CustomsData+WB_Tariff",
					"CustomsData+WB_PrimaryPreference",
					"CustomsData+ManufacturerCode",
					"CustomsData+WB_ZoneStatus",
					"CustomsData+WB_IsFromAnotherFTZWhs",
					"CustomsData+WB_OutwardType",
					"CustomsData+WB_OA_ManufacturerAddress",
					"WE_BondedEntryKey"
				}, control.Grid.Columns.Where(s => !systemColumns.Contains(s.ColumnName)).Select(x => x.ColumnName));
			}
		}

		public void TestCustomsDataShouldBeGrouped()
		{
			var order = Factory.New<WhsOrder>();
			var orderLines = order.Lines;
			var filterBO = new OrderLineFilterBusinessObject();

			using (var form = new ZForm())
			using (var control = new OrderLineFilterStripControl(orderLines, filterBO))
			{
				form.Controls.Add(control);
				form.Show();

				var customsDataColumnNames = new[]
				{
					"CustomsTariffLookup",
					"CustomsTariffDesc",
					"CustomsTariffItem",
					"CustomsData+WB_EntryKey",
					"CustomsData+WB_EntryLineNo",
					"CustomsData+WB_EntryDate",
					"CustomsData+WB_DeclarationReference",
					"CustomsData+WB_CustomsDeadline",
					"CustomsData+WB_InwardStyle",
					"CustomsData+WB_InwardProcedure",
					"CustomsData+WB_RN_NKCountryOfOrigin",
					"CustomsData+WB_CustomsQty",
					"CustomsData+WB_CustomsUnitOfQty",
					"CustomsData+WB_ValueForDuty",
					"CustomsData+WB_TILV",
					"CustomsData+WB_AddInfo",
					"CustomsData+WB_CustomsSecondQuantity",
					"CustomsData+WB_CustomsSecondUnitQty",
					"CustomsData+WB_CustomsThirdQuantity",
					"CustomsData+WB_CustomsThirdUnitQty",
					"CustomsData+WB_Tariff",
					"CustomsData+WB_PrimaryPreference",
					"CustomsData+ManufacturerCode",
					"CustomsData+WB_ZoneStatus",
					"CustomsData+WB_IsFromAnotherFTZWhs",
					"CustomsData+WB_OutwardType",
					"CustomsData+WB_OA_ManufacturerAddress",
					"WE_BondedEntryKey"
				};
				var customsDataColumns = control.Grid.Columns.Where(x => customsDataColumnNames.Contains(x.ColumnName));
				var groupNames = customsDataColumns.Select(x => x.GroupName.Caption).Distinct();
				AssertEquals("All customs data columns should share same group name.", "Customs Data", groupNames.Single());
				var groupNameKeys = customsDataColumns.Select(x => x.GroupName.Key).Distinct();
				AssertEquals("All customs data columns should share same group key.", 1, groupNameKeys.Count());
				var otherColumns = control.Grid.Columns.Except(customsDataColumns);
				Assert("Other columns should not be in group customs data.", otherColumns.All(x => x.ColumnName != "Customs Data"));
			}
		}
	}
}
