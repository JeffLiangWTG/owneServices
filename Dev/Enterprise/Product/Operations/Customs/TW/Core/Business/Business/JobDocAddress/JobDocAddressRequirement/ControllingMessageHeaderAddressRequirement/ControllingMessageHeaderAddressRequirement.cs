using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderAddressRequirement : TWJobDocAddressRequirement
	{
		public ControllingMessageHeaderAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType)
			: base(defaultDocAddressType)
		{
			Header = header;
		}

		public ControllingMessageHeaderAddressRequirement(CusTWControllingMessageHeader header, DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(defaultDocAddressType, defaultContactType)
		{
			Header = header;
		}

		protected CusTWControllingMessageHeader Header { get; }

		public bool IsImport => Header.Declaration?.IsImport ?? false;

		protected override void Initialize()
		{
			base.Initialize();
			ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address += ValidateE2_OA_Address;
			ValidateOrganisationPK += ValidateOrganization;
			ValidateEmail += ValidateE2_Email;
			ValidatePhone += ValidateE2_Phone;
			ValidateFax += ValidateE2_Fax;
		}

		void ValidateE2_OA_Address(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride)
			{
				var targetInfo = parent.E2_OA_AddressInfo;
				if (Header.IsNX601)
				{
					ValidateE2_OA_AddressForNX601(parent, targetInfo);
				}
				else if (Header.IsNX101)
				{
					ValidateE2_OA_AddressForNX101(parent, targetInfo);
				}

				if (parent.Organisation != null && parent.IsImport)
				{
					CheckE2_OA_AddressCompanyNameAndAddressLength(parent.Address, targetInfo);
				}
			}
		}

		void CheckE2_OA_AddressCompanyNameAndAddressLength(OrgAddress address, ZPropertyInfo targetInfo)
		{
			IPartyDetails partyDetailsWrapper = new PartyDetailsWrapper("", "", "", address);
			var chineseName = partyDetailsWrapper.ChineseName;
			var chineseAddress = partyDetailsWrapper.Address.ChineseLine;
			if (chineseName.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseCompanyNameCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseNameMaxlength));
			}
			if (chineseAddress.Length > ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength)
			{
				targetInfo.AddWarning(ValidationConstants.TWJobDocAddressValidationMessages.MaxChineseAddressCharactersLength(ValidationConstants.TWJobDocAddressValidationMessages.ChineseAddressMaxlength));
			}
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent && !parent.E2_AddressOverride)
			{
				var targetInfo = parent.OrganisationPKInfo;
				if (Header.IsNX601)
				{
					ValidateOrganizationForNX601(parent, targetInfo);
				}
				else if (Header.IsNX101)
				{
					ValidateOrganizationForNX101(parent, targetInfo);
				}
				else if (Header.IsNX301_DN)
				{
					ValidateOrganizationForNX301_DN(parent, targetInfo);
				}
				else if (Header.IsNX401)
				{
					ValidateOrganizationForNX401(parent, targetInfo);
				}
			}
		}

		void ValidateE2_Email(JobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckContact(parent, parent.E2_EmailInfo);
			}
		}

		void ValidateE2_Phone(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckContact(parent, parent.E2_PhoneInfo);
			}
		}

		void ValidateE2_Fax(TWJobDocAddressValidation validation)
		{
			if (validation.Parent is TWJobDocAddress parent)
			{
				CheckContact(parent, parent.E2_FaxInfo);
			}
		}

		protected virtual void ValidateE2_OA_AddressForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo) { }

		protected virtual void ValidateE2_OA_AddressForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			if (parent.Address is OrgAddress address)
			{
				if (address.Language != Core.SharedConstants.Languages.ChineseTraditional)
				{
					var translatedAddressInChinese = address.TranslatedAddresses.FirstOrDefault(x => x.Language == Core.SharedConstants.Languages.ChineseTraditional);
					if (translatedAddressInChinese == null)
					{
						if (DefaultDocAddressType != DocAddressType.ImporterDocumentaryAddress)
						{
							targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Address_ChineseAddressIsRequired);
						}
					}
					else if (translatedAddressInChinese.CompanyName.IsEmpty)
					{
						targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Address_ChineseCompanyNameIsRequired);
					}
				}

				if (DefaultDocAddressType != DocAddressType.ImporterDocumentaryAddress && address.OA_Phone.IsEmpty)
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.Address_PhoneIsRequire);
				}
			}
		}

		protected virtual void ValidateOrganizationForNX101(TWJobDocAddress parent, ZPropertyInfo targetInfo) { }
		protected virtual void ValidateOrganizationForNX301_DN(TWJobDocAddress parent, ZPropertyInfo targetInfo) { }
		protected virtual void ValidateOrganizationForNX401(TWJobDocAddress parent, ZPropertyInfo targetInfo) { }
		protected virtual void ValidateOrganizationForNX601(TWJobDocAddress parent, ZPropertyInfo targetInfo) { }
		protected virtual void CheckContact(TWJobDocAddress parent, ZPropertyInfo info) { }
	}
}
