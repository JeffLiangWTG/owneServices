using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxRateValidation : AutoAccTaxRateValidation
	{
		public AccTaxRateValidation(AutoAccTaxRate parent) : base(parent)
		{
		}

		protected override void CheckAT_Code()
		{
			base.CheckAT_Code();
			MandatoryValidation.CheckEntered(Parent.AT_CodeInfo);
		}

		protected override void CheckAT_Description()
		{
			base.CheckAT_Description();
			MandatoryValidation.CheckEntered(Parent.AT_DescriptionInfo);
		}
	}
}
