using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class JobPackProductValidation : AutoJobPackProductValidation
	{
		public JobPackProductValidation(AutoJobPackProduct parent) : base(parent)
		{
		}

		#region D2_ProductCode

		protected override void CheckD2_ProductCode()
		{
			base.CheckD2_ProductCode();
			ListValidation.WarnIfInvalidCode(Parent.D2_ProductCodeInfo);
		}

		#endregion

		#region D2_ProductQuantity

		protected override void CheckD2_ProductQuantity()
		{
			base.CheckD2_ProductQuantity();
			CompareValidation.CheckGreaterThanOrEqualTo(Parent.D2_ProductQuantityInfo, 0m);
		}

		#endregion

		#region D2_ProductUnitOfQty

		protected override void CheckD2_ProductUnitOfQty()
		{
			base.CheckD2_ProductUnitOfQty();
			ListValidation.ErrorIfInvalidCode(Parent.D2_ProductUnitOfQtyInfo);
			MandatoryValidation.CheckUnitEntered(Parent.D2_ProductUnitOfQtyInfo, Parent.D2_ProductQuantityInfo, Res.GetString("f6df89a9-8610-48c4-b1d1-57c8cff8d089", "Unit Of Quantity"));
		}

		#endregion
	}
}
