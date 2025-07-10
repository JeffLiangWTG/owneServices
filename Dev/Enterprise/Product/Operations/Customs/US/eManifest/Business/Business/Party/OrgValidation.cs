using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	class OrgValidation : RelatedObjectValidation
	{
		OrgValidation(JobDocAddress party)
			: base(party.OrganisationPKInfo)
		{
			this.party = party;
		}

		public static void ValidateOrganization(Party party)
		{
			new OrgValidation(party).Validate();
		}

		public static void ValidateAddress(JobDocAddress address)
		{
			new OrgValidation(address).Validate(validateCompanyName: false);
		}

		void Validate(bool validateCompanyName = true)
		{
			if (!party.E2_AddressOverride && !notificationInfo.HasNotifications())
			{
				var errorBuilder = new ErrorStringBuilder(notificationInfo, party.AddressCaption);
				if (validateCompanyName)
				{
					MaxLengthValidation(party.E2_CompanyName, 40, errorBuilder, "name", true);
				}

				MessageErrorIfNotEntered(party.E2_Address1Info, errorBuilder, "address line 1");
				MessageErrorIfNotEntered(party.E2_CityInfo, errorBuilder, "city");
				MaxLengthValidation(party.E2_State, 3, errorBuilder, "state/province", true);
				MessageErrorIfNotEntered(party.E2_PostcodeInfo, errorBuilder, "postal/zip code");
				MessageErrorIfNotEntered(party.E2_RN_NKCountryCodeInfo, errorBuilder, "country");
				MessageErrorIfEnglishCharacters(errorBuilder);
				errorBuilder.FillErrorMessages();
			}
		}

		void MessageErrorIfEnglishCharacters(IZPropertyInfo notificationInfo)
		{
			ValidateCharacters(party.E2_CompanyNameInfo, notificationInfo);
			ValidateCharacters(party.E2_Address1Info, notificationInfo);
			ValidateCharacters(party.E2_Address2Info, notificationInfo);
			ValidateCharacters(party.E2_CityInfo, notificationInfo);
			ValidateCharacters(party.E2_PostcodeInfo, notificationInfo);
			ValidateCharacters(party.E2_StateInfo, notificationInfo);
		}

		static void ValidateCharacters(ZPropertyInfo validateInfo, IZPropertyInfo propertyInfo)
		{
			var stringValue = (ZString)validateInfo.Value.ToString();
			if (!stringValue.IsEnglishOnlyOrEmpty && !stringValue.RemoveDiacritics().IsEnglishOnlyOrEmpty)
			{
				propertyInfo.AddMessageError(validateInfo.HumanReadableName + InvalidCharactersMessageError);
			}
		}
		internal const string InvalidCharactersMessageError = " only accepts English language characters.";

		readonly JobDocAddress party;
	}
}
