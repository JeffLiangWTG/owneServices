using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ManufacturerAddressRequirement : TWJobDocAddressRequirement
	{
		public ManufacturerAddressRequirement(DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
		}

		protected override void Initialize()
		{
			base.Initialize();
			ValidateIDCode += ValidateIDCodeCore;
			ValidateFRICode += ValidateFRICodeCore;
			ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += ValidateE2_OA_Address;
			ValidateAddress1 += ValidateE2_Address1;
			ValidateCompanyName += ValidateE2_CompanyName;
			ValidateCountry += ValidateE2_RN_NKCountryCode;
			ValidateOrganisationPK += ValidateOrganization;
			ValidateEmail += ValidateE2_Email;
			ValidatePhone += ValidateE2_Phone;
			ValidateFax += ValidateE2_Fax;
		}

		void ValidateIDCodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			CheckManufacturerID(parent, parent.IDCodeInfo);
		}

		void ValidateFRICodeCore(TWJobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			CheckManufacturerID(parent, parent.FRICodeInfo);
		}

		void ValidateE2_OA_Address(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_OA_AddressInfo;
				var address = parent.Address;
				if (!parent.E2_AddressOverride && parent.Parent is JobComInvoiceLine line)
				{
					if (address != null)
					{
						if (line.IsImport && address.OA_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
						{
							targetInfo.AddMessageError(ValidationConstants.TWJobDocAddressValidationMessages.CountryOfForeignManufacturerMustNotBeTW);
						}
						else if (line.IsExport && address.OA_RN_NKCountryCode != Core.Constants.CountryCodes.Taiwan)
						{
							targetInfo.AddMessageError(ValidationConstants.TWJobDocAddressValidationMessages.CountryOfManufacturerMustBeTW);
						}

						CheckOrganizationCompanyAndAddressLength(targetInfo, address, parent.IsImport);

						if (line.IsExport && line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader)
						{
							var chineseTranslatedAddress = parent.ChineseTranslatedAddress;
							if (chineseTranslatedAddress?.CompanyName.IsEmpty ?? true)
							{
								if (parent.Address?.CompanyName.IsEmpty ?? true)
								{
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerCompanyNameResString));
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerLocalCompanyNameResString));
								}
								else if (cMHeader.IsCertificate15)
								{
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerLocalCompanyNameResString));
								}
							}

							if (chineseTranslatedAddress?.Address1.IsEmpty ?? true)
							{
								if (parent.Address?.Address1.IsEmpty ?? true)
								{
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerAddressResString));
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TWJobDocAddressValidation.ManufacturerLocalAddressResString));
								}
								else if (cMHeader.IsCertificate15)
								{
									targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TWJobDocAddressValidation.ManufacturerLocalAddressResString));
								}
							}

							if (cMHeader.IsCertificate15 && parent.ContactDetail.IsEmpty)
							{
								targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerContactInformationResString));
							}
						}
					}

					if (line.IsForCMHeaderMessageTypeNX301 || line.IsForCMHeaderMessageTypeNX301_DN || line.IsForCMHeaderMessageTypeNX601 || line.IsForCMHeaderMessageTypeNX603)
					{
						MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
					}
				}
			}
		}

		void ValidateE2_Address1(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.E2_Address1Info;
				if (parent.IsExport &&
					parent.E2_AddressOverride &&
					parent.Parent is JobComInvoiceLine line &&
					line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader &&
					!parent.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader) &&
					parent.E2_Address1.IsEmpty
					&& (parent.LocalAddress?.E2_Address1.IsEmpty ?? true))
				{
					targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerAddressResString));
				}
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				ValidateE2_CompanyNameCore(parent, parent.E2_CompanyNameInfo);
			}
		}

		void ValidateE2_CompanyNameCore(TWJobDocAddress parent, ZPropertyInfo propertyInfo)
		{
			if (parent.E2_CompanyName.IsEmpty && parent.Parent is JobComInvoiceLine line)
			{
				if (parent.IsImport)
				{
					if (line.IsForCMHeaderByMessageTypes(ControllingMessageTypeList.Codes.NX301, ControllingMessageTypeList.Codes.NX301_AX, ControllingMessageTypeList.Codes.NX301_DN, ControllingMessageTypeList.Codes.NX601, ControllingMessageTypeList.Codes.NX603))
					{
						propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ForeignManufacturerCompanyNameResString));
					}
				}
				else if (parent.IsExport &&
					line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader)
				{
					if (parent.E2_AddressOverride &&
					!parent.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader) &&
					(parent.LocalAddress?.E2_CompanyName.IsEmpty ?? true))
					{
						propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerCompanyNameResString));
					}
				}
			}
		}

		void ValidateE2_RN_NKCountryCode(JobDocAddressValidation validation)
		{
			var parent = validation.Parent;
			if (parent.Parent is JobComInvoiceLine line && parent.E2_AddressOverride)
			{
				var targetInfo = parent.E2_RN_NKCountryCodeInfo;
				if (line.IsImport && parent.E2_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					targetInfo.AddMessageError(ValidationConstants.TWJobDocAddressValidationMessages.CountryOfForeignManufacturerMustNotBeTW);
				}
				else if (line.IsExport && parent.E2_RN_NKCountryCode != Core.Constants.CountryCodes.Taiwan)
				{
					targetInfo.AddMessageError(ValidationConstants.TWJobDocAddressValidationMessages.CountryOfManufacturerMustBeTW);
				}
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				var targetInfo = parent.OrganisationPKInfo;
				ValidateE2_CompanyNameCore(parent, targetInfo);

				if (parent.IsExport &&
					!parent.E2_AddressOverride &&
					parent.Parent is JobComInvoiceLine line &&
					line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader)
				{
					var organisation = parent.Organisation;
					if (ConfidentialCertificateTypes.Contains(cMHeader.TW1_CertificateType))
					{
						if (organisation == null)
						{
							targetInfo.AddMessageError(AValidManufactureIDIsReruired);
							targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerCompanyNameResString));
							targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerLocalCompanyNameResString));
							targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerAddressResString));
							targetInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(TWJobDocAddressValidation.ManufacturerLocalAddressResString));
						}
						else if (!OrgHeaderHelper.CheckHasCusCode(organisation, Core.Constants.CountryCodes.Taiwan, parent.ManufactureIDTypes))
						{
							targetInfo.AddMessageError(AValidManufactureIDIsReruired);
						}
					}
					else if (organisation != null && !OrgHeaderHelper.CheckHasCusCode(organisation, Core.Constants.CountryCodes.Taiwan, parent.ManufactureIDTypes))
					{
						targetInfo.AddMessageError(AValidManufactureIDIsReruired);
					}
				}
			}
		}

		void ValidateE2_Email(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckManufacturerContact(parent, validation.Parent.E2_EmailInfo);
			}
		}

		void ValidateE2_Phone(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckManufacturerContact(parent, validation.Parent.E2_PhoneInfo);
			}
		}

		void ValidateE2_Fax(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckManufacturerContact(parent, validation.Parent.E2_FaxInfo);
			}
		}

		void CheckManufacturerContact(TWJobDocAddress parent, ZPropertyInfo info)
		{
			if (parent.IsExport &&
				parent.E2_AddressOverride &&
				parent.Parent is JobComInvoiceLine line &&
				line.GetCMHeaderByMessageType(ControllingMessageTypeList.Codes.NX101)?.ControllingMessageHeader is CusTWControllingMessageHeader cMHeader &&
				cMHeader.IsCertificate15 &&
				!parent.IsIdentificationSameAsLocalProcessAddressForExport(cMHeader) &&
				parent.E2_Email.IsEmpty &&
				parent.E2_Phone.IsEmpty &&
				parent.E2_Fax.IsEmpty)
			{
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(ManufacturerContactInformationResString));
			}
		}

		public ImmutableHashSet<string> ConfidentialCertificateTypes => confidentialCertificateTypes ??= new[]
		{
			CertificateTypeList.Codes.Code9,
			CertificateTypeList.Codes.Code11,
			CertificateTypeList.Codes.Code13,
			CertificateTypeList.Codes.Code14,
			CertificateTypeList.Codes.Code15,
			CertificateTypeList.Codes.Code18,
			CertificateTypeList.Codes.Code19,
		}.ToImmutableHashSet();

		ImmutableHashSet<string> confidentialCertificateTypes;

		string ManufacturerCompanyNameResString => Res.GetString("6EAFE77E-2F57-458D-A765-D91AE5503320", "Manufacturer Company Name");

		string ForeignManufacturerCompanyNameResString => Res.GetString("3F02ADDD-B55B-47A3-86FA-696E133CC1E3", "Foreign Manufacturer Company Name");

		string ManufacturerAddressResString => Res.GetString("84A2C04F-80C7-43CD-B839-6F4EF69586FB", "Manufacturer Address");

		string ManufacturerContactInformationResString => Res.GetString("9BF76D2D-3102-43F1-B2F5-79FB1643FE51", "Manufacturer Contact Information");

		public static string ManufacturerLocalCompanyNameResString => Res.GetString("EC32B23C-A240-4824-B318-A9D2CEFD281C", "Manufacturer Local Company Name");

		string AValidManufactureIDIsReruired => Res.GetString("2E48E8D7-29E4-4521-83A3-E07C8558F585", "You have not entered a Manufacturer ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Manufacturer. To create a valid TW-VAT or TW-PAS or TW-PID or TW-FRI, visit Organization > Details > Config > Registration.");

		void CheckOrganizationCompanyAndAddressLength(ZPropertyInfo targetInfo, OrgAddress address, bool isImport)
		{
			if (address != null)
			{
				IPartyDetails partyDetailsWrapper = new PartyDetailsWrapper("", "", "", address);
				if (!isImport && partyDetailsWrapper.ChineseName.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength));
				}
				if (isImport && partyDetailsWrapper.Address.Line.Length > ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength)
				{
					targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxForeignAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ForeignAddressMaxlength));
				}
			}
		}

		void CheckManufacturerID(TWJobDocAddress address, ZPropertyInfo info)
		{
			if (address.E2_AddressOverride)
			{
				var parent = address;
				if (parent.IsExport &&
					parent.Parent is JobComInvoiceLine line &&
					line.IsForCMHeaderMessageTypeNX101 &&
					(!parent.ManufactureIDTypes.Contains(parent.IDCodeType) || parent.IDCode.IsEmpty) && (!parent.AddressCodeTypes.FRICodeTypes.Contains<string>(parent.FRICodeType) || parent.FRICode.IsEmpty))
				{
					info.AddMessageError(Res.GetString("20A33102-E6B8-4683-AFD7-6D11C53A38C1", "You have not entered a Manufacturer ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Manufacturer."));
				}
			}
		}
	}
}
