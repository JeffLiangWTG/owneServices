using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class SlaughterDateValidation : CusCodeDataValidation
	{
		public SlaughterDateValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected new PackingDate Parent => (PackingDate)base.Parent;

		protected override void CheckCY_Code()
		{
		}
	}
}
