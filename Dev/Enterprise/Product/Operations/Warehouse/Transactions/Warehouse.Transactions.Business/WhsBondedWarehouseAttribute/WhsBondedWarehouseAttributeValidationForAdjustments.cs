using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsBondedWarehouseAttributeValidationForAdjustments : WhsBondedWarehouseAttributeValidation
	{
		public WhsBondedWarehouseAttributeValidationForAdjustments(WhsBondedWarehouseAttribute parent)
			: base(parent)
		{
		}

		#region CheckWB_EntryDateIsValidZDateTimeRange

		protected override void CheckWB_EntryDateIsValidZDateTimeRange()
		{
			var limits = new TypeValidationLimits { PastYearsBeforeError = DateRangeValidation.MaximumPastYears };
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WB_EntryDateInfo);
		}

		#endregion

		#region IsDocketLineMissingBondedEntryKey

		/// <summary>
		/// Old bonded functionality & tests use Adjustments where the Bonded Entry Key is set on the line
		/// only and not on the WhsBondedWarehouseAttribute (Customs Data). The adjustment process copies
		/// the value across so having the Bonded Entry Key on the Adjustment Line is good enough.
		/// </summary>
		protected override bool IsDocketLineMissingBondedEntryKey(WhsDocketLine line)
		{
			return line.WE_TransactionQuantity > 0 && line.WE_BondedEntryKey.IsEmpty;
		}

		#endregion

		#region IsNegativeValueAllowedForAdjustments

		protected override bool IsNegativeValueAllowedForAdjustments
		{
			get { return true; }
		}

		#endregion
	}
}
