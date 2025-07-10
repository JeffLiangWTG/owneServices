using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseCusPermitLineTransactionValidation : SharedCusPermitLineTransactionValidation
	{
		public BaseCusPermitLineTransactionValidation(BaseCusPermitLineTransaction parent) : base(parent)
		{
		}

		public new BaseCusPermitLineTransaction Parent => (BaseCusPermitLineTransaction)base.Parent;

		protected override void CheckCPL_TranQty()
		{
			var quantityBalance = Parent.PermitHeader?.QuantityBalance ?? ZDecimal.Zero;
			if (Parent.CPL_TranQty < ZDecimal.Zero && quantityBalance < ZDecimal.Zero)
			{
				Parent.CPL_TranQtyInfo.AddError(BalanceCannotBeNegative);
			}
		}
	}
}
