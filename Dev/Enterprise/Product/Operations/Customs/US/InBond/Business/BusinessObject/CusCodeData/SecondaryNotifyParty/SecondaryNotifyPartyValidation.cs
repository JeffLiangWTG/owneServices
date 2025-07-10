using CargoWise.EntityFramework;
namespace Enterprise.Customs.US.InBond.Business
{
	public class SecondaryNotifyPartyValidation : Customs.Business.CusCodeDataValidation
	{
		public SecondaryNotifyPartyValidation(SecondaryNotifyParty parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			ListValidation.ErrorIfInvalidCode(Parent.CY_CodeInfo);
		}
	}
}
