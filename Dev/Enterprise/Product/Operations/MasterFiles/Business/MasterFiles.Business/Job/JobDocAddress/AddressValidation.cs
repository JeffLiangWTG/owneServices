using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AddressValidation : ValidationProvider
	{
		#region CheckState

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007")]
		public void CheckState(ZPropertyInfo stateInfo, RefCountry countryOfAddress, AddressValidationSection section, string validationStatus = "")
		{
			ZString stateValue = (ZString)stateInfo.Value;

			if (countryOfAddress != null)
			{
				if (countryOfAddress.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered)
				{
					if (stateValue.IsEmpty && (!ShouldValidateAddress(countryOfAddress, section) || ShouldCountrySpecificRulesApplying(validationStatus)))
					{
						stateInfo.AddError(GenerateErrorMessage(true, Res.GetString("c13d5d0f-b22f-4894-aa11-c251bf508161", "state"), countryOfAddress.RN_DescMultilingual));
					}
					else if (countryOfAddress.States != null && countryOfAddress.States.Count > 0)
					{
						bool isStateOfAddressValid = countryOfAddress.States.Cast<RefCountryStates>().Any(s => s.RW_Code == stateValue);
						if (!isStateOfAddressValid)
						{
							var message = Res.GetString("508d2283-36b0-420f-9c1f-737f51f0a335", "Enter a valid State.");
							if (!ShouldValidateAddress(countryOfAddress, section) || ShouldCountrySpecificRulesApplying(validationStatus))
							{
								stateInfo.AddError(message);
							}
						}
					}
				}
				else if (countryOfAddress.RN_StateProvinceValidationRule == CountryAddressValidationRuleList.Codes.MustNotBeEntered)
				{
					if (!stateValue.IsEmpty)
					{
						stateInfo.AddError(GenerateErrorMessage(false, Res.GetString("c13d5d0f-b22f-4894-aa11-c251bf508161", "state"), countryOfAddress.RN_DescMultilingual));
					}
				}
			}
			else if (!stateValue.IsEmpty)
			{
				stateInfo.AddWarning(Res.GetString("c3214016-bb53-4059-a811-fe934e7f68ea", "This state code needs to be followed by valid country/region code."));
			}
		}

		#endregion

		#region Check Postcode

		public void CheckPostCode(ZPropertyInfo postCodeInfo, RefCountry countryOfAddress, AddressValidationSection section, string validationStatus = "")
		{
			if (countryOfAddress != null && countryOfAddress.RN_PostcodeValidationRule != CountryAddressValidationRuleList.Codes.NoValidationRule)
			{
				if (!ShouldValidateAddress(countryOfAddress, section) || ShouldCountrySpecificRulesApplying(validationStatus))
				{
					if (postCodeInfo.Value.IsEmpty)
					{
						var message = countryOfAddress.RN_PostcodeValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered ?
							GenerateErrorMessage(true, Res.GetString("8a7e5b30-5fea-49d7-8f3d-729f257104f9", "postcode"), countryOfAddress.RN_DescMultilingual)
							: InvalidPostcodeFormatMessage(countryOfAddress.PostcodeFormattingRule.Format, countryOfAddress.RN_DescMultilingual, countryOfAddress.Lookups.PostCodeValidationRules[countryOfAddress.RN_PostcodeValidationRule].Description);
						postCodeInfo.AddError(message);
					}
					else
					{
						var postcodeFormattingRule = countryOfAddress.PostcodeFormattingRule;
						if (postcodeFormattingRule != null && !postcodeFormattingRule.IsFormatted(postCodeInfo.Value.ToString()))
						{
							var message = InvalidPostcodeFormatMessage(countryOfAddress.PostcodeFormattingRule.Format, countryOfAddress.RN_DescMultilingual, countryOfAddress.Lookups.PostCodeValidationRules[countryOfAddress.RN_PostcodeValidationRule].Description);
							if (countryOfAddress.RN_PostcodeValidationRule == CountryAddressValidationRuleList.Codes.MustBeFormatted)
							{
								postCodeInfo.AddError(message);
							}
							else if (countryOfAddress.RN_PostcodeValidationRule == CountryAddressValidationRuleList.Codes.MustBeEntered)
							{
								postCodeInfo.AddWarning(message);
							}
						}
					}
				}
			}
		}

		string InvalidPostcodeFormatMessage(string format, string country, string postcodeValidationRule)
		{
			return Res.GetString("E9BBF741-C23E-4866-8CF2-1F9DA047C53B",
				"The entered postcode does not comply with the postcode format rules of the country/region ({0}). The postcode validation rule for the country/region {1} is currently set to \"{2}\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain->Locations->Countries/Regions.",
				format,
				country,
				postcodeValidationRule);
		}

		#endregion

		#region IsWebVerified

		public bool IsWebVerified(ZString validationStatus)
		{
			return validationStatus.Equals(AddressValidationStatus.Verified) || validationStatus.Equals(AddressValidationStatus.VerifiedToStreet);
		}

		#endregion

		readonly string[] countrySpecificRulesApplying = { AddressValidationStatus.ManuallyVerified, AddressValidationStatus.Unverifiable, AddressValidationStatus.CountryNotAvailable, AddressValidationStatus.ToBeVerified, AddressValidationStatus.ExcludeBackgroundValidation };

		bool ShouldCountrySpecificRulesApplying(string validationStatus)
		{
			bool shouldApplying;

			if (ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI && validationStatus == AddressValidationStatus.ManuallyVerified)
			{
				shouldApplying = false;
			}
			else
			{
				shouldApplying = countrySpecificRulesApplying.Contains(validationStatus);
			}

			return shouldApplying;
		}

		#region CheckEmail

		public void CheckEmail(ZPropertyInfo emailInfo)
		{
			if (!((ZString)emailInfo.Value).IsEmpty)
			{
				EmailAddressValidation.ValidateEmailAddress(emailInfo);
			}
		}

		#endregion

		string GenerateErrorMessage(bool mustBeEntered, string propertyName, string countryName)
		{
			return mustBeEntered
				? Res.GetString("cd024dc2-61cb-4875-bfce-78eb8fcd1062", "You must enter a {0}. The {0} validation rule for the country/region {1} is currently set to \"Must Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.", propertyName, countryName)
				: Res.GetString("4ed2405a-1bac-497c-a96b-c1631da5905c", "You must NOT enter a {0}. The {0} validation rule for the country/region {1} is currently set to \"Must Not Be Entered\".\r\n\r\nIf you wish to change this setting, please contact your system administrator to change the validation rule field on the country/region record in Maintain -> Locations -> Countries/Regions.", propertyName, countryName);
		}

		public void CheckPostcodeViaCity(ZPropertyInfo cityInfo, ZPropertyInfo postcodeInfo, RefCountry country)
		{
			// Check for uses that have been commented out for performance before reinstating

			//if (ShouldCheck(postcodeInfo, cityInfo, country))
			//{
			// Until all town names are included in the postcode list this check is invalid
			//CheckPostcodeAndCity(cityInfo, country, RefLatLongPostcodeSchema.RJ_CitySuburb, "There is no Postcode for City");
			//}
		}

		public void CheckCityViaPostcode(ZPropertyInfo postcodeInfo, ZPropertyInfo cityInfo, RefCountry country)
		{
			if (ShouldCheck(country))
			{
				CheckPostcodeAndCity(postcodeInfo, cityInfo, country, Res.GetString("366ad694-68b6-43a7-9274-e557014974e6", "There is no City with Postcode"));
			}
		}

		bool ShouldCheck(RefCountry country)
		{
			return RawDataRegistry.Instance.AllowCityPostcodeValidation.Value && ShouldCheckRefPostcodeForThisCountry(country);
		}

		void CheckPostcodeAndCity(ZPropertyInfo postcodeInfo, ZPropertyInfo cityInfo, RefCountry country, ZString errorMessage)
		{
			if (country != null && !postcodeInfo.Value.IsEmpty)
			{
				var postCode = RefPostCode.Load(Factory ?? postcodeInfo.BizObj.Factory, postcodeInfo.Value.ToString(), country.Code);
				if (postCode == null ||
						!postCode.CityTowns.Any(c => c.R9_InternationalName.EqualsIgnoringCase(cityInfo.Value.ToString())))
				{
					ZString message = Res.GetString("7e276903-bb01-4288-94a7-a41556103508", "{0} {1} in {2}", errorMessage,
						postcodeInfo.Value, country.RN_DescMultilingual);
					postcodeInfo.AddNotification(CargoWise.ComponentModel.NotificationType.Error, message);
				}
			}
			else if (country == null)
			{
				postcodeInfo.AddNotification(CargoWise.ComponentModel.NotificationType.Warning, Res.GetString("91841516-fd3b-47c0-8042-ae4229735f5d", "The Country/Region is not specified"));
			}
		}

		bool ShouldCheckRefPostcodeForThisCountry(RefCountry country)
		{
			if (country == null)
			{
				return true;
			}
			//WI00077458: Only check this when there is at least one record for this country
			return country.Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, country.Code)) != null;
		}

		bool ShouldValidateAddress(RefCountry country, AddressValidationSection section)
		{
			return
				country != null &&
				OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(country.PK.ToGuid(), section);
		}
	}
}
