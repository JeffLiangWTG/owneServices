using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ImporterLocalAddressRequirement : TWJobDocAddressRequirement
	{
		public ImporterLocalAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateAddress1 += ValidateE2_Address1;
			this.ValidateState += ValidateCheckE2_State;
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation) => ValidateE2_CompanyNameCore(validation);

		protected virtual void ValidateE2_CompanyNameCore(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var maxLangth = 70;
				if (parent.IsImport && parent.E2_CompanyName.Length > maxLangth)
				{
					parent.E2_CompanyNameInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation) => ValidateE2_Address1Core(validation);

		protected virtual void ValidateE2_Address1Core(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateLocalAddressE2_Address1Maxlength();
			}
		}

		void ValidateCheckE2_State(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateState();
			}
		}
	}
}
