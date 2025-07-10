using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderSupplierAddressRequirement : ControllingMessageHeaderAddressRequirement
	{
		public ControllingMessageHeaderSupplierAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(header, defaultDocAddressType, defaultContactType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateIDCode += ValidateIDCodeCore;
			ValidateAddress1 += ValidateE2_Address1;
			ValidateCompanyName += ValidateE2_CompanyName;
			ValidateState += ValidateE2_State;
			ValidateCountry += ValidateE2_RN_NKCountryCode;
		}

		void ValidateIDCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			var propertyInfo = parent.IDCodeInfo;
			var idCode = parent.IDCode;
			if (parent.E2_AddressOverride)
			{
				if (Header.IsNX101)
				{
					if (IsSupplierIDIsReruired && idCode.IsEmpty)
					{
						propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.AValidIDIsReruired);
					}

					if (IsSupplierIdShouldSameAsApplicantId && Header.ApplicantDocumentaryAddress.IDCode != idCode)
					{
						propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.IdShouldBeSameAsApplicantId);
					}
				}
				else if (Header.IsNX401 && !IsImport && idCode.IsEmpty)
				{
					propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.AValidIDIsReruired);
				}
			}
		}

		protected override void ValidateE2_OA_AddressForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateE2_OA_AddressForNX101(parent, targetInfo);
			var chineseTranslatedAddress = parent.ChineseTranslatedAddress;
			if (parent.CompanyName.IsEmpty && (chineseTranslatedAddress?.CompanyName.IsEmpty ?? true))
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalCompanyNameResString));
			}

			if (chineseTranslatedAddress?.Address1.IsEmpty ?? true)
			{
				if (parent.Address?.Address1.IsEmpty ?? true)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalAddressResString));
				}
				else if (Header.IsCertificate15)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalAddressResString));
				}
			}
		}

		protected override void ValidateOrganizationForNX301_DN(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX301_DN(parent, targetInfo);
			if (parent.Organisation == null || (!parent.E2_AddressOverride && parent.E2_RN_NKCountryCode.IsEmpty))
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CountryRegionResString));
			}
		}

		protected override void ValidateOrganizationForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX601(parent, targetInfo);
			if (parent.Organisation == null)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CountryRegionResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
			}
			else if (!parent.E2_AddressOverride)
			{
				if (parent.E2_RN_NKCountryCode.IsEmpty)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CountryRegionResString));
				}

				if (parent.CompanyName.IsEmpty)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
				}

				if (!parent.HasRealAddress || parent.Address == null)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
				}
			}
		}

		protected override void ValidateOrganizationForNX401(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX401(parent, targetInfo);
			if (!IsImport)
			{
				if (parent.Organisation == null)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.AValidIDIsReruired);
				}
				else if (!parent.E2_AddressOverride)
				{
					if (parent.IDCode.IsEmpty)
					{
						targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.AValidIDIsReruired);
					}

					if (parent.CompanyName.IsEmpty)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
					}

					if (!parent.HasRealAddress || parent.Address == null)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
					}
				}
			}
		}

		protected override void ValidateOrganizationForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX101(parent, targetInfo);
			if (parent.Organisation == null)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalCompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalAddressResString));
			}
			else
			{
				if (IsSupplierIDIsReruired && parent.IDCode.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.AValidIDIsReruired);
				}

				if (IsSupplierIdShouldSameAsApplicantId)
				{
					var applicantId = Header.ApplicantDocumentaryAddress.IDCode;
					if (applicantId != parent.IDCode)
					{
						targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Supplier.IdShouldBeSameAsApplicantId);
					}
				}

				if (Header.IsCertificate15)
				{
					if (parent.CompanyChineseName.IsEmpty)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalCompanyNameResString));
					}

					if (parent.ChineseTranslatedAddress == null)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.LocalAddressResString));
					}

					if (!parent.E2_AddressOverride && parent.HasRealAddress && (parent.Address?.OA_Email.IsEmpty ?? false))
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactEmailResString));
					}
				}

				if (IsPhoneOrFaxRequiredForSupplier && !parent.E2_AddressOverride && parent.HasRealAddress && parent.Address is OrgAddress address)
				{
					if (address.OA_Phone.IsEmpty)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactPhoneResString));
					}

					if (address.OA_Fax.IsEmpty)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactFaxResString));
					}
				}
			}
		}

		protected override void CheckContact(TWJobDocAddress parent, ZPropertyInfo info)
		{
			if (parent.E2_AddressOverride && Header.IsNX101)
			{
				if (Header.IsCertificate15 && parent.E2_Email.IsEmpty && parent.IsSupplierDocumentaryAddress)
				{
					info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactEmailResString));
				}

				if (IsPhoneOrFaxRequiredForSupplier && parent.IsSupplierDocumentaryAddress)
				{
					if (parent.E2_Fax.IsEmpty)
					{
						info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactFaxResString));
					}

					if (parent.E2_Phone.IsEmpty)
					{
						info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.ContactPhoneResString));
					}
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				if (Header.IsNX101)
				{
					if (parent.E2_Address1.IsEmpty && (parent.LocalAddress?.E2_Address1.IsEmpty ?? true))
					{
						parent.E2_Address1Info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
					}
				}
				else if ((Header.IsNX201_01 && IsImport) || Header.IsNX301_AX || Header.IsNX401 || Header.IsNX601)
				{
					if (parent.E2_Address1.IsEmpty)
					{
						parent.E2_Address1Info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Supplier.AddressResString));
					}
				}
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (parent.E2_AddressOverride)
				{
					if (!Header.IsNX101 || (parent.LocalAddress?.E2_CompanyName.IsEmpty ?? true))
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString);
					}
					if (parent.E2_CompanyName.IsEmpty && (Header.IsNX301_AX || Header.IsNX601 || (Header.IsNX401 && !IsImport)))
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo, ValidationConstants.CusTWControllingMessageHeader.Supplier.CompanyNameResString);
					}
				}

				var maxLangth = 80;
				if (parent.E2_CompanyName.Length > maxLangth)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
				}
			}
		}

		void ValidateE2_State(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateState();
			}
		}

		void ValidateE2_RN_NKCountryCode(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride && parent.E2_RN_NKCountryCode.IsEmpty)
			{
				if ((Header.IsNX201_01 && IsImport) || Header.IsNX301 || Header.IsNX301_AX || Header.IsNX301_DN || Header.IsNX601 || Header.IsNX603)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.E2_RN_NKCountryCodeInfo, ValidationConstants.CusTWControllingMessageHeader.Supplier.CountryRegionResString);
				}
			}
		}

		bool IsSupplierIDIsReruired
		{
			get
			{
				var certificateType = Header.TW1_CertificateType;
				return certificateType == CertificateTypeList.Codes.Code9
				|| certificateType == CertificateTypeList.Codes.Code11
				|| certificateType == CertificateTypeList.Codes.Code13
				|| certificateType == CertificateTypeList.Codes.Code14
				|| certificateType == CertificateTypeList.Codes.Code15
				|| certificateType == CertificateTypeList.Codes.Code18
				|| certificateType == CertificateTypeList.Codes.Code1
				|| certificateType == CertificateTypeList.Codes.Code16
				|| certificateType == CertificateTypeList.Codes.Code19;
			}
		}

		bool IsPhoneOrFaxRequiredForSupplier
		{
			get
			{
				var certificateType = Header.TW1_CertificateType;
				return certificateType == CertificateTypeList.Codes.Code9
				|| certificateType == CertificateTypeList.Codes.Code11
				|| certificateType == CertificateTypeList.Codes.Code13
				|| certificateType == CertificateTypeList.Codes.Code14
				|| certificateType == CertificateTypeList.Codes.Code15
				|| certificateType == CertificateTypeList.Codes.Code18
				|| certificateType == CertificateTypeList.Codes.Code19;
			}
		}

		bool IsSupplierIdShouldSameAsApplicantId
		{
			get
			{
				var certificateType = Header.TW1_CertificateType;
				return certificateType == CertificateTypeList.Codes.Code1
				|| certificateType == CertificateTypeList.Codes.Code16;
			}
		}
	}
}
