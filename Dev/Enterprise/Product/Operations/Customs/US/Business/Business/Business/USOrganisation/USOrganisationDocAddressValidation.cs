using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public class USOrganisationDocAddressValidation : JobDocAddressValidation
	{
		public USOrganisationDocAddressValidation(JobDocAddress parent)
			: base(parent)
		{
		}

		public new USOrganisationDocAddress Parent
		{
			get { return (USOrganisationDocAddress)base.Parent; }
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			var addressType = Parent.E2_AddressType;
			var header = Parent.Invoice;
			if (header != null && header.IsAttachedToPersistentExportDeclaration)
			{
				if (addressType == AutoDocAddressTypes.Codes.UltimateConsignee || addressType == AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress)
				{
					if (Parent.OrganisationPK.IsEmpty
						&& !Parent.E2_AddressOverride)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
					}
				}

				if (addressType == AutoDocAddressTypes.Codes.USPrincipalPartyInInterest)
				{
					header.Validation.ValidateExportSupplier();
				}

				if (!IsAddressOverridenValidationEnabled)
				{
					ValidatePickUpAddressIsUS(Parent.OrganisationPKInfo);
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();

			if (Parent.E2_AddressType != AutoDocAddressTypes.Codes.USPrincipalPartyInInterest)
			{
				if (!Parent.E2_AddressOverride && Parent.Address != null)
				{
					OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.E2_OA_AddressInfo, Parent.Address);
				}

				if (!Parent.OrganisationPK.IsEmpty && !Parent.OrganisationPKInfo.HasMessageErrors())
				{
					AESAddressValidator.Validate(Parent.E2_OA_AddressInfo, Parent.Address);
				}
			}
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();

			if (IsAddressOverridenValidationEnabled)
			{
				if (MessageBlockStringDataCorrector.KeepOnlyValidCharacters(Parent.E2_City, ABICharacterTypeString.Constants.Alphabetic, JobDocAddress.Schema.E2_CityMaxLength).IsEmpty)
				{
					Parent.E2_CityInfo.AddMessageError(CityRequired);
				}
			}
		}
		internal const string CityRequired = "Please enter a valid city name.";

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();

			if (IsAddressOverridenValidationEnabled)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_RN_NKCountryCodeInfo);
				ValidatePickUpAddressIsUS(Parent.E2_RN_NKCountryCodeInfo);
			}
		}

		protected override void CheckE2_State()
		{
			base.CheckE2_State();

			if (IsAddressOverridenValidationEnabled)
			{
				var countryCode = Parent.E2_RN_NKCountryCode;
				if (!countryCode.IsEmpty)
				{
					if (AESAddressValidator.IsStateRequired(countryCode))
					{
						if (Parent.E2_State.IsEmpty)
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_StateInfo);
						}
					}
				}
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();

			if (IsAddressOverridenValidationEnabled)
			{
				var countryCode = Parent.E2_RN_NKCountryCode;
				if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.UnitedStates)
				{
					var postCode = Parent.E2_Postcode;
					if (postCode.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_PostcodeInfo);
					}
					else
					{
						postCode = postCode.Replace(" ", "").Replace("-", "");
						int postCodeLength = postCode.Length;

						if (postCode.ContainsAnyLetters || (postCodeLength != 5 && postCodeLength != 9))
						{
							Parent.E2_PostcodeInfo.AddMessageError(AESAddressValidator.PostalCodeInvalidFormat);
						}
						else
						{
							AESAddressValidator.CheckAddressHasValidPostCodeForUSOrPR(Parent.E2_PostcodeInfo, postCode, countryCode == Core.Constants.CountryCodes.PuertoRico ? Core.Constants.CountryCodes.PuertoRico : Parent.E2_Postcode.ToString(), PostalCodeNotValidForState);
						}
					}
				}
			}
		}
		internal const string PostalCodeNotValidForState = "The entered Postal Code that is not valid for '{0}'. The first 3 digits of the Postal Code for '{0}' should be between {1}.";

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			var header = Parent.Invoice;
			if (header != null && header.IsAttachedToPersistentDeclaration && header.IsExport)
			{
				var addressType = Parent.E2_AddressType;
				var contactInfo = Parent.E2_ContactInfo;
				if (addressType == AutoDocAddressTypes.Codes.USPrincipalPartyInInterest)
				{
					if (!IsAddressOverridenValidationEnabled)
					{
						ValidatePhoneNumberForUSPPI(header, contactInfo);
					}
					ContactValidator.CheckContactDetails(JobComInvoiceHeader.Schema.US_SupplierContactMaxLength, contactInfo, header.US_USPPI);
				}
				else if (!Parent.E2_Contact.IsEmpty)
				{
					if (addressType == AutoDocAddressTypes.Codes.UltimateConsignee)
					{
						ContactValidator.CheckContactDetails(JobComInvoiceHeader.Schema.US_BuyerContactMaxLength, contactInfo, header.US_ExportUltimateConsignee);
					}
					else if (addressType == AutoDocAddressTypes.Codes.IntermediateConsignee)
					{
						ContactValidator.CheckContactDetails(JobComInvoiceHeader.Schema.US_IntermConsigneeContactMaxLength, contactInfo, header.US_IntermediateConsignee);
					}
				}
			}
		}

		protected override void CheckE2_Phone_Formatted()
		{
			base.CheckE2_Phone_Formatted();

			if (IsAddressOverridenValidationEnabled)
			{
				var header = Parent.Invoice;
				var addressType = Parent.E2_AddressType;
				if (header != null && addressType == AutoDocAddressTypes.Codes.USPrincipalPartyInInterest)
				{
					ValidatePhoneNumberForUSPPI(header, Parent.E2_Phone_FormattedInfo);
				}
			}
		}

		void ValidatePhoneNumberForUSPPI(JobComInvoiceHeader header, ZPropertyInfo property)
		{
			var parent = Parent;
			if (parent.E2_Phone_Formatted.IsEmpty)
			{
				property.AddMessageError(ContactPhoneRequired);
			}
			else if (parent.E2_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates && !IsValidUSPPIPhoneForAES(header.US_USPPI))
			{
				property.AddMessageError(ContactPhoneExactLength);
			}
		}

		internal const string ContactPhoneExactLength = "The USPPI Contact Phone Number must be ten numeric digits in length.";
		internal const string ContactPhoneRequired = "Contact Phone Number is required.";

		bool IsValidUSPPIPhoneForAES(USOrganisation org)
		{
			return PhoneNumberCalculator.GetUnformattedPhoneNumber(org.ZO_Phone, false).Length == 10;
		}

		void ValidatePickUpAddressIsUS(ZPropertyInfo propertyInfo)
		{
			var addressType = Parent.E2_AddressType;
			if (addressType == AutoDocAddressTypes.Codes.SupplierPickupDeliveryAddress)
			{
				var countryCode = Parent.E2_RN_NKCountryCode;
				if (!countryCode.IsEmpty && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) != Core.Constants.CountryCodes.UnitedStates)
				{
					propertyInfo.AddMessageError(SupplierPickupAddressMustBeUS);
				}
			}
		}
		internal const string SupplierPickupAddressMustBeUS = "Pickup address must be a US address.";

		protected override INotificationType UnmatchedOrgNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;
	}
}
