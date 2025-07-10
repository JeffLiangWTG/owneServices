using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISBondDataValidation : AutoDISBondDataValidation
	{
		public DISBondDataValidation(AutoDISBondData bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckBondName()
		{
			base.CheckBondName();
			ListValidation.MessageErrorIfInvalidCode(Parent.BondNameInfo);
		}

		protected override void CheckDefaultBondCode()
		{
			base.CheckDefaultBondCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.DefaultBondCodeInfo);
		}
	}
}
