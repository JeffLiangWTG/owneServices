using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderImporterAddressRequirement : ControllingMessageHeaderAddressRequirement
	{
		public ControllingMessageHeaderImporterAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(header, defaultDocAddressType, defaultContactType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateIDCode += CheckIDCode;
			ValidateAddress1 += ValidateE2_Address1;
			ValidateCompanyName += ValidateE2_CompanyName;
			ValidateState += ValidateE2_State;
			ValidateCountry += ValidateE2_RN_NKCountryCode;
			ValidatePhone += ValidateE2_Phone;
			ValidateOrganisationPK += ValidateOrganization;
		}

		#region CheckIDCode
		void CheckIDCode(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				CheckIDCodeCore(parent, parent.IDCodeInfo);
			}
		}

		void CheckIDCodeCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			var needValidation = (Header.IsNX101 && IsNX101ImporterIDReruired) || IsImporterIDReruired;
			if (needValidation && parent.IDCode.IsEmpty)
			{
				propertyInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Importer.AValidIDIsReruired);
			}
		}
		#endregion

		protected override void ValidateE2_OA_AddressForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateE2_OA_AddressForNX101(parent, targetInfo);
			var chineseTranslatedAddress = parent.ChineseTranslatedAddress;
			if (parent.CompanyName.IsEmpty && (chineseTranslatedAddress?.CompanyName.IsEmpty ?? true))
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.CompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalCompanyNameResString));
			}

			if (chineseTranslatedAddress?.Address1.IsEmpty ?? true)
			{
				if (parent.Address?.Address1.IsEmpty ?? true)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.AddressResString));
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString));
				}
				else if (Header.IsCertificate15)
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString));
				}
			}
		}

		protected override void ValidateOrganizationForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX601(parent, targetInfo);
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("89565DBC-8C05-4638-91EE-9BCBDDCC4050", "Importer"));
		}

		protected override void ValidateOrganizationForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX101(parent, targetInfo);
			if (parent.Organisation == null)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.CompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalCompanyNameResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.AddressResString));
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString));
			}
			else
			{
				if (IsNX101ImporterIDReruired && parent.IDCode.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Importer.AValidIDIsReruired);
				}

				if (Header.IsCertificate15)
				{
					if (parent.CompanyChineseName.IsEmpty)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalCompanyNameResString));
					}

					if (parent.ChineseTranslatedAddress == null)
					{
						targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString));
					}

					if (!parent.E2_AddressOverride && parent.HasRealAddress && parent.Address is OrgAddress address)
					{
						if (address.OA_Email.IsEmpty && address.OA_Phone.IsEmpty && address.OA_Fax.IsEmpty)
						{
							targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.ContactInformationResString));
						}
					}
				}
			}
		}

		protected override void CheckContact(TWJobDocAddress parent, ZPropertyInfo info)
		{
			if (parent.E2_AddressOverride &&
				Header.IsNX101 &&
				Header.IsCertificate15 &&
				parent.E2_Email.IsEmpty &&
				parent.E2_Phone.IsEmpty &&
				parent.E2_Fax.IsEmpty)
			{
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.ContactInformationResString));
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				ValidateE2_Address1Core(parent, parent.E2_Address1Info);
			}
		}

		void ValidateE2_Address1Core(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if ((Header.IsNX101 && (parent.LocalAddress?.E2_Address1.IsEmpty ?? true)) || IsImporterAddressReruired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.Importer.AddressResString);
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (parent.E2_AddressOverride)
				{
					ValidateE2_CompanyNameCore(parent, targetInfo);
				}

				var maxLangth = 80;
				if (parent.E2_CompanyName.Length > maxLangth)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxCharactersLength(maxLangth));
				}
			}
		}

		void ValidateE2_CompanyNameCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if ((Header.IsNX101 && (parent.LocalAddress?.E2_CompanyName.IsEmpty ?? true)) || IsImporterCompanyNameReruired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, ValidationConstants.CusTWControllingMessageHeader.Importer.CompanyNameResString);
			}
		}

		void ValidateE2_State(JobDocAddressValidation validation)
		{
			if (validation is TWJobDocAddressValidation twValidation)
			{
				twValidation.ValidateState();
			}
		}

		#region ValidateE2_RN_NKCountryCode
		void ValidateE2_RN_NKCountryCode(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				ValidateE2_RN_NKCountryCodeCore(parent, parent.E2_RN_NKCountryCodeInfo);
			}
		}

		void ValidateE2_RN_NKCountryCodeCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			var needValidation = Header.IsNX201_01 && Header.IsExport;
			if (needValidation && parent.E2_RN_NKCountryCode.IsEmpty)
			{
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.CountryCodeResString));
			}
		}
		#endregion

		#region ValidateE2_Phone
		void ValidateE2_Phone(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride)
			{
				ValidateE2_PhoneCore(parent, parent.E2_PhoneInfo);
			}
		}

		void ValidateE2_PhoneCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if (IsImporterPhoneReruired && parent.E2_Phone.IsEmpty)
			{
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.PhoneResString));
			}
		}
		#endregion

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride)
			{
				var propertyInfo = parent.OrganisationPKInfo;
				CheckIDCodeCore(parent, propertyInfo);
				ValidateE2_RN_NKCountryCodeCore(parent, propertyInfo);
				ValidateE2_PhoneCore(parent, propertyInfo);
				ValidateE2_CompanyNameCore(parent, propertyInfo);
				ValidateE2_Address1Core(parent, propertyInfo);
				if (ControllingMessageHeaderImporterLocalAddressRequirement.IsImporterLocalCompanyNameReruired(Header) && !parent.Address.HasCompanyNameOfLanguage(Core.Constants.Languages.ChineseTraditional))
				{
					propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalCompanyNameResString));
				}
				if (ControllingMessageHeaderImporterLocalAddressRequirement.IsImporterLocalAddressReruired(Header) && !parent.Address.HasAddressOfLanguage(Core.Constants.Languages.ChineseTraditional))
				{
					propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.Importer.LocalAddressResString));
				}
			}
		}

		bool IsNX101ImporterIDReruired
		{
			get
			{
				var certificateType = Header.TW1_CertificateType;
				return certificateType == CertificateTypeList.Codes.Code9
				|| certificateType == CertificateTypeList.Codes.Code11
				|| certificateType == CertificateTypeList.Codes.Code13
				|| certificateType == CertificateTypeList.Codes.Code14
				|| certificateType == CertificateTypeList.Codes.Code18
				|| certificateType == CertificateTypeList.Codes.Code19;
			}
		}

		bool IsImporterIDReruired =>
			Header.IsNX301 ||
			Header.IsNX301_AX ||
			Header.IsNX301_DN ||
			(Header.IsNX401 && Header.IsImport) ||
			Header.IsNX601 ||
			Header.IsNX603;

		bool IsImporterCompanyNameReruired =>
			(Header.IsNX201_01 && Header.IsExport) ||
			Header.IsNX301 ||
			Header.IsNX301_AX ||
			(Header.IsNX401 && Header.IsImport) ||
			Header.IsNX601 ||
			Header.IsNX603;

		bool IsImporterAddressReruired =>
			Header.IsNX301_AX ||
			(Header.IsNX401 && Header.IsImport) ||
			Header.IsNX601;

		bool IsImporterPhoneReruired =>
			Header.IsNX301_AX ||
			Header.IsNX301_DN ||
			Header.IsNX601;
	}
}
