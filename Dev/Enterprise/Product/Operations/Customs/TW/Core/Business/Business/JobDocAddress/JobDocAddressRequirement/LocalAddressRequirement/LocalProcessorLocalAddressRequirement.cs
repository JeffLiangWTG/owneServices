using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class LocalProcessorLocalAddressRequirement : TWJobDocAddressRequirement
	{
		public LocalProcessorLocalAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateCompanyName += ValidateE2_CompanyName;
			ValidateAddress1 += ValidateE2_Address1;
			ValidateState += ValidateCheckE2_State;
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (parent.E2_AddressOverride && parent.Parent is CusTWControllingMessageHeader cMHeader &&
					(ValidationCheckForNX101(cMHeader, cMHeader.LocalProcessorAddress.CompanyName) || cMHeader.IsNX601))
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.LocalCompanyNameResString);
				}

				var maxLangth = 70;
				if (parent.IsImport && parent.E2_CompanyName.Length > maxLangth)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent &&
				parent.E2_AddressOverride &&
				parent.Parent is CusTWControllingMessageHeader cMHeader &&
				(ValidationCheckForNX101(cMHeader, cMHeader.LocalProcessorAddress.E2_Address1) || cMHeader.IsNX601))
			{
				var targetInfo = parent.E2_Address1Info;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.LocalAddressResString);
			}

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

		bool ValidationCheckForNX101(CusTWControllingMessageHeader cMHeader, ZString releventValue) => cMHeader.IsNX101 && (releventValue.IsEmpty || cMHeader.IsCertificate15);
	}
}
