using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISPGAValidation : AutoDISPGAValidation
	{
		public DISPGAValidation(AutoDISPGA bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckCode()
		{
			base.CheckCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CodeInfo);
		}
	}
}
