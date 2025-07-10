using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class AddressExtensions
	{
		#region IsToOrder

		public static ZBool IsToOrder(this Address address)
		{
			return IsToOrder(address.CompanyName);
		}

		public static bool IsToOrder(string addressCompanyName)
		{
			var identifiers = new string[] { (NoResString)"TO ORDER", (NoResString)"TO ORDER OF", (NoResString)"TO THE ORDER", (NoResString)"TO THE ORDER OF" }; // non-translatable validation message
			return identifiers.Any(i => addressCompanyName.ToUpperInvariant().StartsWith(i));
		}

		#endregion

		#region IsSameAsConsignee

		public static ZBool IsSameAsConsignee(this Address address)
		{
			return IsSameAsConsignee(address.CompanyName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static bool IsSameAsConsignee(string addressCompanyName)
		{
			return addressCompanyName.ToUpperInvariant() == "SAME AS CONSIGNEE";
		}

		#endregion

		#region AddSameAsOtherAddressSupport

		public static Address AddToOrderSupport(this Address address)
		{
			return address.AddSameAsOtherAddressSupport(IsToOrder);
		}

		public static Address AddSameAsConsigneeSupport(this Address address)
		{
			return address.AddSameAsOtherAddressSupport(IsSameAsConsignee);
		}

		static Address AddSameAsOtherAddressSupport(this Address address, Func<string, bool> isSameAs)
		{
			if (address == null || isSameAs == null)
			{
				return null;
			}

			var originalAddress1 = address.AddressLine1;
			var originalAddress2 = address.AddressLine2;
			var originalCity = address.City;
			var originalState = address.State;
			var originalCountryCode = address.Country.Code;
			var originalCountryName = address.Country.Name;
			var originalPostcode = address.Postcode;
			var originalContact = address.Contact;
			var originalContactPhone = address.Phone;
			var originalContactFax = address.Fax;
			var originalContactEmail = address.Email;
			var originalTaxNumber = address.TaxNumber;
			var originalTaxNumberType = address.TaxNumberType?.Code ?? ZString.Empty;

			void HandleToOrder(object sender, EventArgs e)
			{
				var args = e as ValueChangedEventArgs;

				if (args == null)
				{
					return;
				}

				var oldValue = args.OldValue.ToString();
				var newValue = args.NewValue.ToString();

				if (isSameAs(oldValue) && !isSameAs(newValue))
				{
					address.AddressLine1 = originalAddress1;
					address.AddressLine2 = originalAddress2;
					address.City = originalCity;
					address.State = originalState;
					address.Country.Code = originalCountryCode;
					address.Country.Name = originalCountryName;
					address.Postcode = originalPostcode;
					address.Contact = originalContact;
					address.Phone = originalContactPhone;
					address.Fax = originalContactFax;
					address.Email = originalContactEmail;
					address.TaxNumber = originalTaxNumber;
					if (address.TaxNumberType != null)
					{
						address.TaxNumberType.Code = originalTaxNumberType;
					}
				}
				else if (!isSameAs(oldValue) && isSameAs(newValue))
				{
					address.AddressLine1 = string.Empty;
					address.AddressLine2 = string.Empty;
					address.City = string.Empty;
					address.State = string.Empty;
					address.Country.Code = string.Empty;
					address.Country.Name = string.Empty;
					address.Postcode = string.Empty;
					address.Contact = string.Empty;
					address.Phone = string.Empty;
					address.Fax = string.Empty;
					address.Email = string.Empty;
					address.TaxNumber = string.Empty;
					if (address.TaxNumberType != null)
					{
						address.TaxNumberType.Code = string.Empty;
					}
				}
			}

			address.CompanyNameInfo.ValueChanged += HandleToOrder;

			return address;
		}

		#endregion

		#region AddAsAgentInfoToCompanyName

		public static Address AddAsAgentInfoToCompanyName(this Address address, object addressObject)
		{
			switch (addressObject)
			{
				case OrgAddress orgAddress:

					AddAsAgentInfoToCompanyNameCore(address, orgAddress);

					break;

				case JobDocAddress jobDocAddress:

					if (jobDocAddress.HasRealAddress)
					{
						AddAsAgentInfoToCompanyNameCore(address, jobDocAddress.Address);
					}

					break;

				case ZAddressWithContact zAddressWithContact:

					AddAsAgentInfoToCompanyNameCore(address, zAddressWithContact.OrgAddress as OrgAddress);

					break;
			}

			return address;
		}

		static void AddAsAgentInfoToCompanyNameCore(Address address, OrgAddress orgAddress)
		{
			if (orgAddress != null)
			{
				var miscInfo = orgAddress.Header?.MiscServ;

				if (miscInfo != null && !miscInfo.OM_FWAsAgentOption.IsEmpty)
				{
					var asAgentOption = miscInfo.Lookups.AsAgentOptions.GetDescriptionFromCode(miscInfo.OM_FWAsAgentOption);

					address.CompanyName = address.CompanyName + " " + asAgentOption + (miscInfo.OM_FWAsAgentName.IsEmpty ? string.Empty : " " + miscInfo.OM_FWAsAgentName);
				}
			}
		}

		#endregion

		#region Registration Numbers

		public static ZString GetRegistrationNumberWithFallbackToOrgHeader(this OrgAddress address, string codeType, string countryCode = null, bool useDefaultCountry = true)
		{
			var value = ZString.Empty;
			if (address != null)
			{
				value = address.GetRegistrationNumber(countryCode ?? (useDefaultCountry ? address.OA_RN_NKCountryCode : ZString.Empty), codeType);
				if (value.IsEmpty)
				{
					value = address.Header.GetRegistrationNumber(countryCode ?? (useDefaultCountry ? address.Header.CountryCode : ZString.Empty), codeType);
				}
			}

			return value;
		}

		public static OrgCusCode GetRegistrationNumberObjectWithFallbackToOrgHeader(this OrgAddress address, string codeType, string countryCode = null, bool useDefaultCountry = true)
		{
			OrgCusCode orgCusCode = null;
			if (address != null)
			{
				orgCusCode = address.GetRegistrationNumberObject(countryCode ?? (useDefaultCountry ? address.OA_RN_NKCountryCode : ZString.Empty), codeType);
				if (orgCusCode == null)
				{
					orgCusCode = address.Header.GetRegistrationNumberObject(countryCode ?? (useDefaultCountry ? address.Header.CountryCode : ZString.Empty), codeType);
				}
			}

			return orgCusCode;
		}

		static ZString GetRegistrationNumber(this OrgAddress address, ZString countryCode, ZString type)
		{
			var orgCusCode = address.GetRegistrationNumberObject(countryCode, type);

			return orgCusCode != null
				? orgCusCode.OK_CustomsRegNo
				: ZString.Empty;
		}

		static OrgCusCode GetRegistrationNumberObject(this OrgAddress address, ZString countryCode, ZString type)
		{
			return address != null
				? address.CustomsCodes.OfType<OrgCusCode>()
					.FirstOrDefault(c => (countryCode.IsEmpty || c.OK_RN_NKCodeCountry == countryCode) && c.OK_CodeType == type)
				: null;
		}

		#endregion
	}
}
