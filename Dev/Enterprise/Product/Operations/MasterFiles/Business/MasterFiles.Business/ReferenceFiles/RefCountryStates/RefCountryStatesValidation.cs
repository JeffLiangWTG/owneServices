using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryStatesValidation : AutoRefCountryStatesValidation
	{
		public RefCountryStatesValidation(AutoRefCountryStates parent)
			: base(parent)
		{
		}

		protected override void CheckRW_RN_NKCountryCode()
		{
			base.CheckRW_RN_NKCountryCode();
			MandatoryValidation.CheckEntered(Parent.RW_RN_NKCountryCodeInfo);
		}

		protected override void CheckRW_Code()
		{
			base.CheckRW_Code();
			MandatoryValidation.CheckEntered(Parent.RW_CodeInfo);
		}

		protected override void CheckRW_Description()
		{
			base.CheckRW_Description();
			MandatoryValidation.CheckEntered(Parent.RW_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.RW_DescriptionInfo);
		}
	}
}
