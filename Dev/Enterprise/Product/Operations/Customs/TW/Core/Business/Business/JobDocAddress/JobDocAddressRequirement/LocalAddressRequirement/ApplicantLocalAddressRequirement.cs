using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ApplicantLocalAddressRequirement : TWJobDocAddressRequirement
	{
		public ApplicantLocalAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateCompanyName += CheckE2_CompanyName;
			ValidateAddress1 += CheckAddress1;
		}

		void CheckAddress1(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride && parent.Parent is CusTWControllingMessageHeader header)
			{
				var needValidation = header.IsNX101 || header.IsNX301 || header.IsNX603 || (header.IsNX201_01 && (header.Declaration?.IsImport ?? false));
				if (needValidation && parent.Address1.IsEmpty)
				{
					parent.Address1Info.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalAddressIsRequired);
				}
			}
		}

		void CheckE2_CompanyName(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.E2_AddressOverride && parent.Parent is CusTWControllingMessageHeader header)
			{
				var needValidation = header.IsNX201_01 || header.IsNX301 || header.IsNX603;
				if (needValidation && parent.CompanyName.IsEmpty)
				{
					parent.CompanyNameInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalCompanyNameIsRequired);
				}
			}
		}
	}
}
