using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseCusPermitHeaderValidation : SharedCusPermitHeaderValidation
	{
		public BaseCusPermitHeaderValidation(BaseCusPermitHeader parent)
			: base(parent)
		{
		}

		public new BaseCusPermitHeader Parent => (BaseCusPermitHeader)base.Parent;

		protected override void CheckTransactionCategory()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TransactionCategoryInfo);
		}
	}
}
