using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISAdditionalNumberValidation : AutoDISAdditionalNumberValidation
	{
		public DISAdditionalNumberValidation(AutoDISAdditionalNumber bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckNumber()
		{
			base.CheckNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.NumberInfo);
		}
	}
}
