using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeValidation : AutoWhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidation(AutoWhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		protected new WhsBondedWarehouseAttribute Parent
		{
			get { return (WhsBondedWarehouseAttribute)base.Parent; }
		}

		#region CheckWB_EntryKey

		protected override void CheckWB_EntryKey()
		{
			base.CheckWB_EntryKey();

			if (!Parent.WB_EntryKeyInfo.HasErrors() && ShouldCheckIfEntryNumberIsEntered && Parent.WB_EntryKey.IsEmpty)
			{
				var docketLine = Parent.Parent;
				if (docketLine != null && IsDocketLineMissingBondedEntryKey(docketLine) && docketLine.IsCustomsTransaction)
				{
					Parent.WB_EntryKeyInfo.AddError(Res.GetString("32f5dc7a-c44a-4757-b65c-4b1adfe6ce1f", "Entry Number is mandatory for Customs Jobs."));
				}
			}
		}

		protected virtual bool ShouldCheckIfEntryNumberIsEntered
		{
			get { return true; }
		}

		protected virtual bool IsDocketLineMissingBondedEntryKey(WhsDocketLine line)
		{
			return true;
		}

		#endregion

		protected override void CheckWB_CustomsQty()
		{
			base.CheckWB_CustomsQty();

			if (!IsNegativeValueAllowedForAdjustments && Parent.WB_CustomsQty < 0m)
			{
				Parent.WB_CustomsQtyInfo.AddError(Res.GetString("be927bab-8908-4cce-b78b-628821594f8c", "Customs Qty cannot be negative"));
			}
		}

		protected override void CheckWB_ValueForDuty()
		{
			base.CheckWB_ValueForDuty();

			if (!IsNegativeValueAllowedForAdjustments && Parent.WB_ValueForDuty < 0m)
			{
				Parent.WB_ValueForDutyInfo.AddError(Res.GetString("3801bc96-7c42-417f-b2eb-b45745e58de2", "Value For Duty cannot be negative"));
			}
		}

		protected override void CheckWB_TILV()
		{
			base.CheckWB_TILV();

			if (!IsNegativeValueAllowedForAdjustments && Parent.WB_TILV < 0m)
			{
				Parent.WB_TILVInfo.AddError(Res.GetString("95efad7b-5104-4a12-9384-eb040b1db573", "TILV cannot be negative"));
			}
		}

		protected override void CheckWB_OutwardType()
		{
			base.CheckWB_OutwardType();
			var parent = Parent;
			if (!parent.WB_OutwardType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(parent.WB_OutwardTypeInfo, parent.Lookups.OutwardTypes);
			}
			else if (parent.Parent is ICustomsDataParent customsDataParent && customsDataParent.IsOutwardTypeRequired)
			{
				MandatoryValidation.CheckEntered(parent.WB_OutwardTypeInfo);
			}
		}

		protected override void CheckWB_ZoneStatus()
		{
			base.CheckWB_ZoneStatus();
			ListValidation.ErrorIfInvalidCode(Parent.WB_ZoneStatusInfo, Parent.Lookups.ZoneStatusList);
		}

		protected override void CheckWB_IsFromAnotherFTZWhs()
		{
			base.CheckWB_IsFromAnotherFTZWhs();
			if (Parent.WB_IsFromAnotherFTZWhs && !IsUSFTZWarehouse)
			{
				Parent.WB_IsFromAnotherFTZWhsInfo.AddError(Res.GetString("155f0753-39de-4f9b-b478-b2b1e31bbcea", "Only US FTZ warehouse can select from other US FTZ."));
			}
		}

		protected virtual bool IsNegativeValueAllowedForAdjustments
		{
			get { return false; }
		}

		WhsWarehouse Warehouse => Parent.Parent?.Docket?.Warehouse;

		public bool IsUSFTZWarehouse => Warehouse?.IsFTZAndUSJurisdiction() ?? false;
	}
}
