using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsDocketFlattenedValidation : AutoWhsDocketFlattenedValidation
	{
		public WhsDocketFlattenedValidation(AutoWhsDocketFlattened parent)
			: base(parent)
		{
		}

		protected new WhsDocketFlattened Parent => (WhsDocketFlattened)base.Parent;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateLine_WE_ExpiryDate();
			ValidateLine_WE_PackingDate();
		}

		#region Line_WE_ExpiryDate

		public void ValidateLine_WE_ExpiryDate()
		{
			ValidateCalculatedProperty(Parent.Line_WE_ExpiryDateInfo);
		}

		protected void CheckLine_WE_ExpiryDate()
		{
			CheckLine_WE_ExpiryDateIsValidZDate();
			CheckLine_WE_ExpiryDateIsValidZDateRange();
		}

		protected void CheckLine_WE_ExpiryDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.Line_WE_ExpiryDateInfo);
		}

		protected void CheckLine_WE_ExpiryDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateRange(Parent.Line_WE_ExpiryDateInfo);
		}

		#endregion

		#region Line_WE_PackingDate

		public void ValidateLine_WE_PackingDate()
		{
			ValidateCalculatedProperty(Parent.Line_WE_PackingDateInfo);
		}

		protected void CheckLine_WE_PackingDate()
		{
			CheckLine_WE_PackingDateIsValidZDate();
			CheckLine_WE_PackingDateIsValidZDateRange();
		}

		protected void CheckLine_WE_PackingDateIsValidZDate()
		{
			TypeValidation.CheckValidZDateWithoutRange(Parent.Line_WE_PackingDateInfo);
		}

		protected void CheckLine_WE_PackingDateIsValidZDateRange()
		{
			TypeValidation.CheckValidZDateRange(Parent.Line_WE_PackingDateInfo);
		}

		#endregion
	}
}
