using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ExpireCommissionAgreementActionValidation : AutoExpireCommissionAgreementActionValidation
	{
		public ExpireCommissionAgreementActionValidation(AutoExpireCommissionAgreementAction parent)
			: base(parent)
		{
		}

		#region Date

		protected override void CheckDate()
		{
			base.CheckDate();
			MandatoryValidation.CheckEntered(Parent.DateInfo);
		}

		protected override void CheckDateIsValidZDateTimeRange()
		{
			// Should not check date time range
		}

		#endregion
	}
}
