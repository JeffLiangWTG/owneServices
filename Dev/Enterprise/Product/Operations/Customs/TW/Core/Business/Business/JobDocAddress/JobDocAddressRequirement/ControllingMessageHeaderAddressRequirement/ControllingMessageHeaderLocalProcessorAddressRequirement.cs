using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderLocalProcessorAddressRequirement : ControllingMessageHeaderAddressRequirement
	{
		public ControllingMessageHeaderLocalProcessorAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType)
		: base(header, defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateIDCode += ValidateIDCodeCore;
			ValidateAddress1 += ValidateE2_Address1;
			ValidateCompanyName += ValidateE2_CompanyName;
			ValidatePhone += ValidateE2_Phone;
		}

		void ValidateIDCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (Header.IsNX101 &&
				IsNX101LocalProcessorIDRequired &&
				(!parent.ManufactureIDTypes.Contains(parent.IDCodeType) || parent.IDCode.IsEmpty))
			{
				parent.IDCodeInfo.AddMessageError(Res.GetString("FDD13BA2-2265-4ABD-816E-77B0C47D57CA", "You have not entered a Local Processor ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Local Processor."));
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
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.ContactInformationResString));
			}
		}

		protected override void ValidateE2_OA_AddressForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateE2_OA_AddressForNX101(parent, targetInfo);
			var chineseTranslatedAddress = parent.ChineseTranslatedAddress;
			if (chineseTranslatedAddress?.CompanyName.IsEmpty ?? true)
			{
				if (parent.CompanyName.IsEmpty)
				{
					targetInfo.AddMessageError(ErrorMessageForCompanyNameIsRequire);
					targetInfo.AddMessageError(ErrorMessageForAddress_ChineseCompanyNameIsRequired);
				}
				else if (Header.IsCertificate15)
				{
					targetInfo.AddMessageError(ErrorMessageForAddress_ChineseCompanyNameIsRequired);
				}
			}
			if (chineseTranslatedAddress?.Address1.IsEmpty ?? true)
			{
				if (parent.Address?.Address1.IsEmpty ?? true)
				{
					targetInfo.AddMessageError(ErrorMessageForAddressIsRequire);
					targetInfo.AddMessageError(ErrorMessageForAddress_ChineseAddressIsRequired);
				}
				else if (Header.IsCertificate15)
				{
					targetInfo.AddMessageError(ErrorMessageForAddress_ChineseAddressIsRequired);
				}
			}
			if (Header.IsCertificate15 && parent.ContactDetail.IsEmpty)
			{
				targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.ContactInformationResString));
			}
		}

		protected override void ValidateE2_OA_AddressForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			if (parent.Address is OrgAddress address)
			{
				if (address.Language != Core.SharedConstants.Languages.ChineseTraditional)
				{
					var translatedAddressInChinese = address.TranslatedAddresses.FirstOrDefault(x => x.Language == Core.SharedConstants.Languages.ChineseTraditional);
					if (translatedAddressInChinese?.Address1.IsEmpty ?? true)
					{
						targetInfo.AddMessageError(ErrorMessageForAddress_ChineseAddressIsRequired);
					}
					if (translatedAddressInChinese?.CompanyName.IsEmpty ?? true)
					{
						targetInfo.AddMessageError(ErrorMessageForAddress_ChineseCompanyNameIsRequired);
					}
				}

				if (address.OA_Phone.IsEmpty)
				{
					targetInfo.AddMessageError(ErrorMessageForAddress_PhoneIsRequire);
				}
			}
		}

		protected override void ValidateOrganizationForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX601(parent, targetInfo);
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("226E0F26-3FA0-4AC7-87B8-1037E28F5572", "Local Processor"));

			if (parent.Organisation == null)
			{
				targetInfo.AddMessageError(ErrorMessageForAddress_ChineseCompanyNameIsRequired);
				targetInfo.AddMessageError(ErrorMessageForAddress_ChineseAddressIsRequired);
				targetInfo.AddMessageError(ErrorMessageForAddress_PhoneIsRequire);
			}
		}

		protected override void ValidateOrganizationForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			base.ValidateOrganizationForNX101(parent, targetInfo);
			var organisation = parent.Organisation;
			if (organisation == null)
			{
				targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.AValidIDIsReruired);
				targetInfo.AddMessageError(ErrorMessageForCompanyNameIsRequire);
				targetInfo.AddMessageError(ErrorMessageForAddress_ChineseCompanyNameIsRequired);
				targetInfo.AddMessageError(ErrorMessageForAddressIsRequire);
				targetInfo.AddMessageError(ErrorMessageForAddress_ChineseAddressIsRequired);
			}
			else if (!OrgHeaderHelper.CheckHasCusCode(organisation, Core.Constants.CountryCodes.Taiwan, parent.ManufactureIDTypes))
			{
				targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.AValidIDIsReruired);
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent &&
				parent.E2_AddressOverride &&
				Header.IsNX101 &&
				parent.E2_Address1.IsEmpty &&
				(parent.LocalAddress?.E2_Address1.IsEmpty ?? true))
			{
				parent.E2_Address1Info.AddMessageError(ErrorMessageForAddressIsRequire);
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && parent.E2_AddressOverride && Header.IsNX101 && (parent.LocalAddress?.E2_CompanyName.IsEmpty ?? true))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.E2_CompanyNameInfo, ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.CompanyNameResString);
			}
		}

		void ValidateE2_Phone(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent &&
				parent.E2_AddressOverride &&
				Header.IsNX601 &&
				parent.E2_Phone.IsEmpty &&
				(parent.LocalAddress?.E2_Phone.IsEmpty ?? true))
			{
				parent.E2_PhoneInfo.AddMessageError(ErrorMessageForAddress_PhoneIsRequire);
			}
		}

		bool IsNX101LocalProcessorIDRequired
		{
			get
			{
				switch (Header.TW1_CertificateType)
				{
					case CertificateTypeList.Codes.Code9:
					case CertificateTypeList.Codes.Code11:
					case CertificateTypeList.Codes.Code13:
					case CertificateTypeList.Codes.Code14:
						return false;
					default:
						return true;
				}
			}
		}

		string ErrorMessageForAddress_ChineseCompanyNameIsRequired { get; } = MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.LocalCompanyNameResString);

		string ErrorMessageForAddress_ChineseAddressIsRequired { get; } = MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.LocalAddressResString);

		string ErrorMessageForAddress_PhoneIsRequire { get; } = MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.LocalPhoneResString);

		string ErrorMessageForAddressIsRequire { get; } = MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.AddressResString);

		string ErrorMessageForCompanyNameIsRequire { get; } = MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.CusTWControllingMessageHeader.LocalProcessor.CompanyNameResString);
	}
}
