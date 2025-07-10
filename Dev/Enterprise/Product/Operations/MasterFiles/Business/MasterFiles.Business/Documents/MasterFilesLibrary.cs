using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business.Macros;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Documents
{
	public sealed class MasterFilesLibrary : MacroLibrary
	{
		public MasterFilesLibrary(BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.addressFormatter = new Lazy<OrganizationAddressFormatter>(() => new OrganizationAddressFormatter(factory));
			this.AddressFormatterWithCountry = new Lazy<OrganizationAddressFormatterWithCountryCode>(() => new OrganizationAddressFormatterWithCountryCode(factory));
			this.addInfoUSDateFormatter = new Lazy<AddInfoUSDateFormatter>(() => new AddInfoUSDateFormatter());
			this.usPhoneFormatter = new Lazy<USPhoneFormatter>(() => new USPhoneFormatter());
		}

		readonly Lazy<OrganizationAddressFormatter> addressFormatter;
		readonly Lazy<OrganizationAddressFormatterWithCountryCode> AddressFormatterWithCountry;
		readonly Lazy<AddInfoUSDateFormatter> addInfoUSDateFormatter;
		readonly Lazy<USPhoneFormatter> usPhoneFormatter;
		readonly BusinessObjectFactory factory;

		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<OrganizationAddressFormatter>>(
					"AddressFormatter",
					(NoResString)"Used to format organization address.",
					() => addressFormatter.Value);
				yield return new Handler<Func<OrganizationAddressFormatterWithCountryCode>>(
					"AddressFormatterWithCountry",
					(NoResString)"Used to format organization address with coountry code.",
					() => AddressFormatterWithCountry.Value);
				yield return new Handler<Func<AddInfoUSDateFormatter>>(
					"AddInfoUSDateFormatter",
					(NoResString)"Used to Convert AddInfo Date to US Date Format (MM-dd-yyyy).",
					() => addInfoUSDateFormatter.Value);
				yield return new Handler<Func<USPhoneFormatter>>(
					"USPhoneFormatter",
					(NoResString)"Used to Convert phone number to US format i.e. (800) 555-1212 ).",
					() => usPhoneFormatter.Value);
				yield return new Handler<Func<string, string>>(
					"GetRefSysConfigStringValue",
					(NoResString)"Get string value from RefSysConfig data.",
					(configCode) => GetRefSysConfigStringValue(configCode));
				yield return new Handler<Func<string, string>>(
					"GetVesselType",
					(NoResString)"Used to get a vessel type from a vessel code",
					(vesselCode) => GetVesselType(vesselCode));
				yield return new Handler<Func<string, string>>(
					"GetVesselCallSign",
					(NoResString)"Used to get a vessel's call sign from a vessel code",
					(vesselCode) => GetVesselCallSign(vesselCode));
				yield return new Handler<Func<string, string[]>>(
					"GetTaxNumberType",
					(NoResString)"Get tax number type for docType and currently logged in country code",
					(docType) => GetTaxNumberType(docType));
				yield return new Handler<Func<string, string, string[]>>(
					"GetTaxNumberType",
					(NoResString)"Get tax number type for docType and a particular country",
					(docType, country) => GetTaxNumberType(docType, country));
				yield return new Handler<Func<IAddress, string, IRegistrationNumber>>(
					"GetTaxNumber",
					(NoResString)"Get tax number for a particular address, docType and currently logged in country code",
					(address, docType) => GetTaxNumber(address, docType));
				yield return new Handler<Func<IAddress, string, string, IRegistrationNumber>>(
					"GetTaxNumber",
					(NoResString)"Get tax number for a particular address , docType and country",
					(address, docType, country) => GetTaxNumber(address, docType, country));
			}
		}
		string GetRefSysConfigStringValue(string configCode)
		{
			var refSysConfig = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IRefSysConfigLoader>("IRefSysConfigLoader", factory);
			return refSysConfig.GetStringValue(configCode, ZDateTime.UtcToday);
		}

		string GetVesselType(string vesselCode)
		{
			var vessel = RefVessel.LookupVesselByCode(vesselCode, factory);

			return vessel?.RV_VesselType ?? string.Empty;
		}

		string GetVesselCallSign(string vesselCode)
		{
			var vessel = RefVessel.LookupVesselByCode(vesselCode, factory);

			return vessel?.RV_RadioCallSign ?? string.Empty;
		}

		string[] GetTaxNumberType(ZString docType)
		{
			return GetTaxNumberType(GlbCompany.CurrentCompany.Country.Code, docType);
		}

		string[] GetTaxNumberType(ZString docType, ZString countryCode)
		{
			var helper = ObjectFactory.Get<IRequiredTaxNumberHelper>();
			return helper.GetTaxCodeTypes(docType, countryCode).ToArray();
		}

		IRegistrationNumber GetTaxNumber(IAddress address, ZString docType)
		{
			return GetTaxNumber(address, docType, GlbCompany.CurrentCompany.Country.Code);
		}

		IRegistrationNumber GetTaxNumber(IAddress address, ZString docType, ZString countryCode)
		{
			return GetTaxNumber(address, countryCode, GetTaxNumberType(docType, countryCode));
		}

		IRegistrationNumber GetTaxNumber(IAddress address, string countryCode, IEnumerable<string> types)
		{
			if (!types.Any())
			{
				return null;
			}

			var currentTaxType = types.First();
			var taxNum = address.RegistrationNumbers.FirstOrDefault(
				registrationNumber =>
					registrationNumber.CountryOfIssue.Code.Equals(countryCode) &&
					registrationNumber.Type.Code.Equals(currentTaxType)
			);
			return taxNum ?? GetTaxNumber(address, countryCode, types.Skip(1));
		}

		sealed class OrganizationAddressFormatter
		{
			public OrganizationAddressFormatter(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			[MacroInvokable]
			public string Format(object obj, string language = Core.Constants.Languages.English)
			{
				var macroObj = obj as IMacroObject;

				var orgAddress = macroObj?.Value ?? obj;

				var formatter = CreateFormatter(orgAddress as Company, language)
					?? CreateFormatter(orgAddress as IOrganizationAddress, language)
					?? CreateFormatter(orgAddress as IAddress, language);

				var postalAddress = formatter?.PostalAddress();

				// AddressFormatter is not using proper line breaks
				return !string.IsNullOrEmpty(postalAddress)
					? string.Join(System.Environment.NewLine, postalAddress.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
					: postalAddress;
			}

			AddressFormatter CreateFormatter(Company company, string language)
			{
				if (company == null)
				{
					return null;
				}

				return new AddressFormatter(factory,
					company.Organization.Name,
					company.Organization.MainAddress.AdditionalAddressInformation,
					company.Organization.MainAddress.AddressLine1,
					company.Organization.MainAddress.AddressLine2,
					!company.Organization.Unloco.Name.IsEmpty ? company.Organization.Unloco.Name : company.Organization.MainAddress.City,
					company.Organization.MainAddress.State,
					company.Organization.MainAddress.Postcode,
					(NoResString)company.Country.Name,
					language,
					true);
			}

			AddressFormatter CreateFormatter(IOrganizationAddress organizationAddress, string language)
			{
				if (organizationAddress == null)
				{
					return null;
				}

				var countryName = (NoResString)organizationAddress.Country?.Name.GetValueOrDefault();

				return new AddressFormatter(factory,
					organizationAddress.CompanyName.GetValueOrDefault(),
					organizationAddress.AdditionalAddressInformation.GetValueOrDefault(),
					organizationAddress.Address1.GetValueOrDefault(),
					organizationAddress.Address2.GetValueOrDefault(),
					organizationAddress.City.GetValueOrDefault(),
					organizationAddress.State.GetValueOrDefault(),
					organizationAddress.Postcode.GetValueOrDefault(),
					countryName,
					language,
					true);
			}

			AddressFormatter CreateFormatter(IAddress address, string language)
			{
				if (address == null)
				{
					return null;
				}

				return new AddressFormatter(factory,
					address.CompanyName,
					address.AdditionalAddressInformation,
					address.AddressLine1,
					address.AddressLine2,
					address.City,
					address.State,
					address.Postcode,
					(NoResString)address.Country.Name,
					language,
					true);
			}
		}

		sealed class OrganizationAddressFormatterWithCountryCode
		{
			public OrganizationAddressFormatterWithCountryCode(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			readonly BusinessObjectFactory factory;

			[MacroInvokable]
			public string Format(object obj, string language = Core.Constants.Languages.English)
			{
				var macroObj = obj as IMacroObject;

				var orgAddress = macroObj?.Value ?? obj;

				var address = orgAddress as IOrganizationAddress;

				if (address == null)
				{
					return string.Empty;
				}

				var countryName = (NoResString)address?.Country.Name.GetValueOrDefault();

				var formatter = new AddressFormatter(factory,
					address.CompanyName.GetValueOrDefault(),
					address.AdditionalAddressInformation.GetValueOrDefault(),
					address.Address1.GetValueOrDefault(),
					address.Address2.GetValueOrDefault(),
					address.City.GetValueOrDefault(),
					address.State.GetValueOrDefault(),
					address.Postcode.GetValueOrDefault(),
					countryName,
					language,
					true,
					address.Country.Code);

				var postalAddress = formatter.PostalAddress();

				// AddressFormatter is not using proper line breaks
				return !string.IsNullOrEmpty(postalAddress)
					? string.Join(System.Environment.NewLine, postalAddress.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
					: postalAddress;
			}
		}

		sealed class AddInfoUSDateFormatter
		{
			public AddInfoUSDateFormatter()
			{
			}

			[MacroInvokable]
			public string Format(object obj)
			{
				var result = string.Empty;
				var macroObj = obj as IMacroObject;
				var dateObj = macroObj?.Value ?? obj;
				var date = dateObj is ZString ? (ZString)dateObj : ZString.Empty;
				if (!date.IsEmpty)
				{
					DateTime usDate = DateTime.ParseExact(date, (NoResString)"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
					result = usDate.ToString("MM-dd-yyyy", CultureInfo.CurrentCulture);
				}
				return result;
			}
		}

		sealed class USPhoneFormatter
		{
			public USPhoneFormatter()
			{
			}

			[MacroInvokable]
			public string Format(object obj)
			{
				var macroObj = obj as IMacroObject;
				var phoneObj = macroObj?.Value ?? obj;
				var phone = phoneObj is ZString ? (ZString)phoneObj : ZString.Empty;
				var result = phone;
				if (!phone.IsEmpty)
				{
					PhoneNumberFormatterAndValidator phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
					result = phoneNumberFormatterAndValidator.FormatLocal(phone, string.Empty);
				}
				return result;
			}
		}
	}
}
