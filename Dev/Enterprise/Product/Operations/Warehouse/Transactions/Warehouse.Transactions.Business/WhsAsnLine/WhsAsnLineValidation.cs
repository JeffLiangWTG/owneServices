//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsAsnLineValidation
//
//    This class should be used for overriding validation in AutoWhsAsnLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
namespace Enterprise.Warehouse.Transactions.Business
{
	using CargoWise.EntityFramework;

	public class WhsAsnLineValidation : AutoWhsAsnLineValidation
	{
		public WhsAsnLineValidation(AutoWhsAsnLine parent) : base(parent)
		{
		}

		#region CheckWN_ExpiryDateIsValidZDateRange

		protected override void CheckWN_ExpiryDateIsValidZDateRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WN_ExpiryDateInfo);
		}

		#endregion
	}
}
