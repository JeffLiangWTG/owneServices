//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsVASOrderLineValidation
//
//    This class should be used for overriding validation in AutoWhsVASOrderLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Warehouse.Transactions.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public class WhsVASOrderLineValidation : AutoWhsVASOrderLineValidation
	{
		public WhsVASOrderLineValidation(AutoWhsVASOrderLine parent)
			: base(parent)
		{
		}

		#region CheckWVL_OP_Product

		protected override void CheckWVL_OP_Product()
		{
			base.CheckWVL_OP_Product();
			CheckProductIsValid();
		}

		void CheckProductIsValid()
		{
			if (!Parent.ReadOnly)
			{
				var vasOrder = Parent.VASOrder;
				var clientPK = vasOrder?.WVO_OH_Client ?? ZGuid.Empty;
				WhsValidationHelper.CheckProductIsValid(Parent.Product, clientPK, Parent.WVL_OP_ProductInfo);
			}
		}

		#endregion

		#region CheckWVL_Quantity

		protected override void CheckWVL_Quantity()
		{
			if (Parent.WVL_Quantity <= 0)
			{
				Parent.WVL_QuantityInfo.AddError(Res.GetString("1a847b89-d153-4bfb-9906-6533a0eb50f3", "Please enter a quantity greater than zero."));
			}
			else if (!Parent.WVL_QuantityInfo.HasErrors() && Parent.WVL_Quantity > 1 && !Parent.WVL_SerialNumberInfo.Value.IsEmpty)
			{
				Parent.WVL_QuantityInfo.AddError(PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
			}
		}

		#endregion

		#region CheckWVL_ExpiryDateIsValidZDateRange

		protected override void CheckWVL_ExpiryDateIsValidZDateRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };
			new DateRangeValidation(limits).ValidateDateValueHasChanged(Parent.WVL_ExpiryDateInfo);
		}

		#endregion

		#region Implementation

		new WhsVASOrderLine Parent
		{
			get { return (WhsVASOrderLine)base.Parent; }
		}

		#endregion
	}
}
