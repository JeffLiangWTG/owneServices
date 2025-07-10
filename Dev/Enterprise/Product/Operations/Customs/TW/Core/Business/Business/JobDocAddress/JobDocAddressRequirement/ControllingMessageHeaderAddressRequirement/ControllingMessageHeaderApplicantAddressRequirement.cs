using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderApplicantAddressRequirement : ControllingMessageHeaderAddressRequirement
	{
		public ControllingMessageHeaderApplicantAddressRequirement(CusTWControllingMessageHeader header, ContactType defaultContactType)
			: base(header, DocAddressType.Applicant, defaultContactType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateIDCode += CheckIDCode;
			ValidateCompanyName += CheckCompanyName;
			ValidatePhone += CheckPhone;
			ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += CheckE2_OA_Address;
			ValidateOrganisationPK += CheckOrganisation;
		}

		void CheckE2_OA_Address(JobDocAddressValidation validation)
		{
			validation.ValidateOrganisationPK();
		}

		void CheckOrganisation(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				if (!parent.E2_AddressOverride)
				{
					var propertyInfo = parent.OrganisationPKInfo;
					CheckIDCodeCore(parent, propertyInfo);
					CheckPhoneCore(parent, propertyInfo);
					CheckCompanyNameCore(parent, propertyInfo);
					if (parent.ChineseTranslatedAddress == null)
					{
						if (Header.IsNX101 || (Header.IsNX201_01 && IsImport) || Header.IsNX301 || Header.IsNX603)
						{
							propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalAddressIsRequired);
						}
						if (Header.IsNX201_01 || Header.IsNX301 || Header.IsNX603)
						{
							propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.LocalCompanyNameIsRequired);
						}
					}
				}
			}
		}

		void CheckIDCode(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				CheckIDCodeCore(parent, parent.IDCodeInfo);
			}
		}

		void CheckIDCodeCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			var needValidation = Header.IsNX101 || Header.IsNX201_07 || Header.IsNX301 || Header.IsNX603;
			if (needValidation && parent.IDCode.IsEmpty)
			{
				propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.IDIsRequired);
			}
		}

		void CheckPhone(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				CheckPhoneCore(parent, parent.E2_PhoneInfo);
			}
		}

		void CheckPhoneCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if ((Header.IsNX301 || Header.IsNX603) && parent.E2_Phone.IsEmpty)
			{
				propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.TelephoneNumberIsRequired);
			}
		}

		void CheckCompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckCompanyNameCore(parent, parent.E2_CompanyNameInfo);
			}
		}

		void CheckCompanyNameCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if ((Header.IsNX301 || Header.IsNX603) && parent.E2_CompanyName.IsEmpty)
			{
				propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Applicant.CompanyNameIsRequired);
			}
		}
	}
}
