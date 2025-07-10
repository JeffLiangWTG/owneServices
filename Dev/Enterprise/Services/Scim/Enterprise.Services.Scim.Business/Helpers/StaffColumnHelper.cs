using System;
using CargoWise.Schema;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.Scim.Business
{
	internal static class StaffColumnHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "json expression to speed things up")]
		internal static SchemaColumn GetStaffColumn(string scimColumn, string parent = "")
		{
			if (!string.IsNullOrEmpty(parent))
			{
				switch (parent.ToLower())
				{
					case AttributeNames.Emails:
						if (scimColumn.Equals(AttributeNames.Value, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_EmailAddress;
						}
						else
						{
							return null;
						}

					case AttributeNames.Addresses:
						if (scimColumn.Equals(AttributeNames.FormattedPart, StringComparison.InvariantCultureIgnoreCase)
							|| scimColumn.Equals(AttributeNames.AddressesStreetAddressPart, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_UserAddress1;
						}

						if (scimColumn.Equals(AttributeNames.AddressesLocalityPart, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_City;
						}

						if (scimColumn.Equals(AttributeNames.AddressesRegionPart, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_State;
						}

						if (scimColumn.Equals(AttributeNames.AddressesPostalCodePart, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_Postcode;
						}

						if (scimColumn.Equals(AttributeNames.AddressesCountryPart, StringComparison.InvariantCultureIgnoreCase))
						{
							return GlbStaffSchema.GS_RN_NKCountryCode;
						}

						return null;
					default:
						return null;
				}
			}

			if (scimColumn.Equals(AttributeNames.UserName, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_LoginName;
			}

			if (scimColumn.Equals(AttributeNames.Active, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_IsActive;
			}

			if (scimColumn.Equals(AttributeNames.GivenName, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.GivenNamePart, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_GivenName;
			}

			if (scimColumn.Equals(AttributeNames.MiddleName, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.MiddleNamePart, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_MiddleName;
			}
			if (scimColumn.Equals(AttributeNames.FamilyName, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.FamilyNamePart, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_Surname;
			}
			if (scimColumn.Equals(AttributeNames.NameHonorificPrefix, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.NameHonorificPrefixPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.NameHonorificPrefixFlat, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_NameTitle;
			}
			if (scimColumn.Equals(AttributeNames.NameHonorificSuffix, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.NameHonorificSuffixPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.NameHonorificSuffixFlat, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_NameSuffix;
			}
			if (scimColumn.Equals(AttributeNames.NameFormattedFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.FormattedPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.NameFormatted, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_FullName;
			}
			if (scimColumn.Equals(AttributeNames.PhoneNumbersMobile, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.PhoneNumbersMobileFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("phonenumbers[type eq \"mobile\"].value", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_MobilePhone;
			}

			if (scimColumn.Equals(AttributeNames.PhoneNumbersHome, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.PhoneNumbersHomeFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("phonenumbers[type eq \"home\"].value", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_HomePhone;
			}

			if (scimColumn.Equals(AttributeNames.PhoneNumbersWork, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.PhoneNumbersWorkFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("phonenumbers[type eq \"work\"].value", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_WorkPhone;
			}

			if (scimColumn.Equals(AttributeNames.PhoneNumbersFax, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.PhoneNumbersFaxFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("phonenumbers[type eq \"fax\"].value", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_FaxNum;
			}

			if (scimColumn.Equals(AttributeNames.AddressesStreetAddress, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesStreetAddressFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesStreetAddressPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("addresses[type eq \"work\"].streetAddress", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_UserAddress1;
			}

			if (scimColumn.Equals(AttributeNames.AddressesLocality, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesLocalityFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesLocalityPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("addresses[type eq \"work\"].locality", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_City;
			}

			if (scimColumn.Equals(AttributeNames.AddressesRegion, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesRegionFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesRegionPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("addresses[type eq \"work\"].region", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_State;
			}

			if (scimColumn.Equals(AttributeNames.AddressesPostalCode, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesPostalCodeFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesPostalCodePart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("addresses[type eq \"work\"].postalcode", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_Postcode;
			}

			if (scimColumn.Equals(AttributeNames.AddressesCountry, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesCountryFlat, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals(AttributeNames.AddressesCountryPart, StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("addresses[type eq \"work\"].country", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_RN_NKCountryCode;
			}

			if (scimColumn.Equals(AttributeNames.Title, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_Title;
			}

			if (scimColumn.Equals("email", StringComparison.InvariantCultureIgnoreCase)
				|| scimColumn.Equals("emails[type eq \"work\"].value", StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_EmailAddress;
			}

			if (scimColumn.Equals(AttributeNames.NickName, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_FriendlyName;
			}

			if (scimColumn.Equals(AttributeNames.PreferredLanguage, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_WorkingLanguage;
			}

			if (scimColumn.Equals(AttributeNames.ExternalId, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_ExternalId;
			}

			if (scimColumn.Equals(AttributeNames.MetaCreated, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_SystemCreateTimeUtc;
			}

			if (scimColumn.Equals(AttributeNames.MetaLastModified, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_SystemLastEditTimeUtc;
			}

			if (scimColumn.Equals(AttributeNames.Id, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.PK;
			}

			if (scimColumn.Equals(AttributeNames.OtherReferences, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbStaffSchema.GS_Pager;
			}

			return null;
		}
	}
}
