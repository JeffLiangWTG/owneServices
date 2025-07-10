using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class AESAddressValidator
	{
		public static void Validate(ZPropertyInfo addressInfo, OrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				addressInfo.AddMessageError(AddressRequired);
			}
			else
			{
				if (orgAddress.OA_Address1.IsEmpty)
				{
					addressInfo.AddMessageError(Address1Required);
				}

				if (!orgAddress.OA_IsActive)
				{
					addressInfo.AddMessageError("You have selected an inactive address.");
				}

				if (MessageBlockStringDataCorrector.KeepOnlyValidCharacters(orgAddress.OA_City, ABICharacterTypeString.Constants.Alphabetic, OrgAddress.Schema.OA_Address1MaxLength).IsEmpty)
				{
					addressInfo.AddMessageError(CityRequired);
				}

				var relatedPortCode = orgAddress.EffectiveRelatedPortCode;
				ZString countryCode = relatedPortCode != null && relatedPortCode.Country != null ? relatedPortCode.Country.Code : ZString.Empty;
				if (countryCode.IsEmpty)
				{
					addressInfo.AddMessageError(CountryRequired);
				}
				else
				{
					if (IsStateRequired(countryCode))
					{
						if (orgAddress.OA_State.IsEmpty)
						{
							addressInfo.AddMessageError(StateRequired);
						}
						else
						{
							OrganisationValidation.ValidateStateForExportAddress(addressInfo, orgAddress);
						}
					}

					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						if (orgAddress.OA_PostCode.IsEmpty)
						{
							addressInfo.AddMessageError(PostalCodeRequired);
						}
						else
						{
							ZString postCode = orgAddress.OA_PostCode.Replace(" ", "").Replace("-", "");
							int postCodeLength = postCode.Length;

							if (postCode.ContainsAnyLetters || (postCodeLength != 5 && postCodeLength != 9))
							{
								addressInfo.AddMessageError(PostalCodeInvalidFormat);
							}
							else
							{
								CheckAddressHasValidPostCodeForUSOrPR(addressInfo, postCode, countryCode == Core.Constants.CountryCodes.PuertoRico ? Core.Constants.CountryCodes.PuertoRico : orgAddress.OA_State.ToString(), PostalCodeNotValidForState);
							}
						}
					}
				}
			}
		}
		public const string AddressRequired = "Address is required, please select one.";
		public const string Address1Required = "The address selected does not have an Address line 1.";
		public const string CityRequired = "The address selected does not have a valid City.";
		public const string CountryRequired = "The address selected does not have a Country.";
		public const string StateRequired = "The address selected does not have a State. A State is required for addresses located in the USA, Mexico, and Puerto Rico.";
		public const string PostalCodeRequired = "The address selected does not have a Postal Code, a Postal Code is required for addresses in USA/PR.";
		public const string PostalCodeInvalidFormat = "The address selected does not have a valid US Postal Code. A valid US Postal Code must be 5 or 9 digits.";

		internal static void CheckAddressHasValidPostCodeForUSOrPR(ZPropertyInfo addressInfo, ZString postCode, ZString stateCode, ZString postCodeInvalidNotificationText)
		{
			if (!stateCode.IsEmpty)
			{
				bool isValidPostcodeForState = false;
				ZString left3Postcode = postCode.Left(3);
				ZStringBuilder builder = new ZStringBuilder();
				ZQuery query = new ZQuery(USCZipCodeSchema.UZ_State, stateCode);
				query.OrderBy = USCZipCodeSchema.Constants.UZ_BeginZipCodeRange;
				foreach (USCZipCode zipCode in addressInfo.BizObj.Factory.Load<USCZipCode>(query))
				{
					var subBeginZipCode = zipCode.UZ_BeginZipCodeRange.Left(3);
					var subEndZipCode = zipCode.UZ_EndZipCodeRange.Left(3);
					builder.Append(ZString.Format("'{0}' and '{1}'", subBeginZipCode, subEndZipCode));
					if (left3Postcode.CompareTo(subBeginZipCode) >= 0 && left3Postcode.CompareTo(subEndZipCode) <= 0)
					{
						isValidPostcodeForState = true;
						break;
					}
				}

				if (!isValidPostcodeForState && !builder.IsEmpty)
				{
					addressInfo.AddMessageError(ZString.Format(postCodeInvalidNotificationText, stateCode, builder.ToStringWithDelimiterBetweenAppends(", or ")));
				}
			}
		}
		internal const string PostalCodeNotValidForState = "The address selected has a Postal Code that is not valid for '{0}'. The first 3 digits of the Postal Code for '{0}' should be between {1}.";

		internal static bool IsStateRequired(ZString countryCode)
		{
			return countryCode == Core.Constants.CountryCodes.UnitedStates ||
				countryCode == Core.Constants.CountryCodes.Mexico;
		}

		public static void ValidateUSAddressForUSRoutedTransaction(ZString routedTransaction, ZPropertyInfo propertyInfo, ZString countryCode)
		{
			if (routedTransaction == YesNoDefaultList.Codes.Yes && (countryCode == Core.Constants.CountryCodes.UnitedStates
				|| countryCode == Core.Constants.CountryCodes.PuertoRico || countryCode == Core.Constants.CountryCodes.VirginIslands))
			{
				propertyInfo.AddMessageError(ZString.Format(FPPIRequiredForRoutedTransaction, countryCode));
			}
		}
		internal const string FPPIRequiredForRoutedTransaction = "Non-{0} organization is required for a Routed Transaction.";
	}
}
