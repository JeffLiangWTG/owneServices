//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSWarehouseDetailAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSWarehouseDetailAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USWarehouseDetailAddInfoValidation : AutoUSWarehouseDetailAddInfoValidation
	{
		public USWarehouseDetailAddInfoValidation(AutoUSWarehouseDetailAddInfo parent)
			: base(parent)
		{
		}

		protected WarehouseDetail Detail
		{
			get { return Parent.Parent; }
		}

		protected new USWarehouseDetailAddInfo Parent
		{
			get { return (USWarehouseDetailAddInfo)base.Parent; }
		}

		protected override void CheckUS_WarehouseBondedQuantity()
		{
			base.CheckUS_WarehouseBondedQuantity();
			MandatoryValidation.WarnIfIsNegative(Parent.US_WarehouseBondedQuantityInfo);
		}

		protected override void CheckUS_WarehouseWithdrawQuantity()
		{
			base.CheckUS_WarehouseWithdrawQuantity();
			MandatoryValidation.WarnIfIsNegative(Parent.US_WarehouseWithdrawQuantityInfo);
			if (Parent.US_WarehouseWithdrawQuantity > Parent.US_WarehouseBondedQuantity)
			{
				Parent.US_WarehouseWithdrawQuantityInfo.AddWarning(WithdrawQuantityShouldNotBeGreaterThanBondedQuantity);
			}
		}

		internal const string WithdrawQuantityShouldNotBeGreaterThanBondedQuantity = "Withdraw Quantity should not be greater than Bonded Quantity.";
	}
}
