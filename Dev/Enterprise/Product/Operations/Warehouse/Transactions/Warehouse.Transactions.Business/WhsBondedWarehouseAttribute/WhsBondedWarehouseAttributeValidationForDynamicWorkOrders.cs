using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeValidationForDynamicWorkOrders : WhsBondedWarehouseAttributeValidationForComponentOrders
	{
		public WhsBondedWarehouseAttributeValidationForDynamicWorkOrders(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		WhsDynamicWorkOrder DynamicWorkOrder => (WhsDynamicWorkOrder)Parent?.Parent?.Docket;

		#region CheckWB_IsMainInwardsProcessedItem

		protected override void CheckWB_IsMainInwardsProcessedItem()
		{
			CheckExactlyOne_InwardProcessedItemType_IsSet(Parent.WB_IsMainInwardsProcessedItemInfo);
			CheckAtLeastOne_IsMainInwardProcessedItem();
			CheckExactlyOne_MainInwardProcessedItem_IsSet();
		}

		void CheckExactlyOne_InwardProcessedItemType_IsSet(ZPropertyInfo property)
		{
			if (!property.HasErrors()
				&& !Parent.Parent.WE_WE_ParentDocketLine.IsValid
				&& !(Parent.WB_IsMainInwardsProcessedItem ^ Parent.WB_IsSecondaryInwardsProcessedItem))
			{
				property.AddError(AtLeastOneInwardProcessedItemTypeIsSet);
			}
		}

		public static string AtLeastOneInwardProcessedItemTypeIsSet => Res.GetString(
			"ce7c0e14-cc4a-401f-99c3-dcb7272b0d34",
			"Every line on Dynamic Work Order must have either 'Is Main Inward Processed Item' or 'Is Secondary Inward Processed Item' set.");

		void CheckAtLeastOne_IsMainInwardProcessedItem()
		{
			if (!Parent.WB_IsMainInwardsProcessedItemInfo.HasErrors())
			{
				var validationCache = DynamicWorkOrder?.GetPreSaveValidationCache();
				if (validationCache != null && validationCache.DoesDynamicWorkOrderHaveNoMainProcessedItems())
				{
					Parent.WB_IsMainInwardsProcessedItemInfo.AddError(AtLeastOneMainInwardsProcessedItem);
				}
			}
		}

		public static string AtLeastOneMainInwardsProcessedItem => Res.GetString("5de033de-a482-45d6-a41d-3a36b3a45fe5", "Should have at least one 'Is Main Processed Item' per Dynamic Work Order.");

		void CheckExactlyOne_MainInwardProcessedItem_IsSet()
		{
			if (!Parent.WB_IsMainInwardsProcessedItemInfo.HasErrors())
			{
				var dynamicWorkOrder = DynamicWorkOrder;
				var validationCache = dynamicWorkOrder?.GetPreSaveValidationCache();
				if (!(dynamicWorkOrder?.IsFinalised ?? false) && validationCache != null && validationCache.DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem())
				{
					Parent.WB_IsMainInwardsProcessedItemInfo.AddError(OnlyOneMainInwardsProcessedItem);
				}
			}
		}

		public static string OnlyOneMainInwardsProcessedItem => Res.GetString("e30f2371-eda7-4fe0-afe9-ffc80b682ce3", "Should have only one 'Is Main Processed Item' per Dynamic Work Order.");

		#endregion

		#region CheckWB_IsSecondaryInwardsProcessedItem

		protected override void CheckWB_IsSecondaryInwardsProcessedItem()
		{
			CheckExactlyOne_InwardProcessedItemType_IsSet(Parent.WB_IsSecondaryInwardsProcessedItemInfo);
		}

		#endregion
	}
}
