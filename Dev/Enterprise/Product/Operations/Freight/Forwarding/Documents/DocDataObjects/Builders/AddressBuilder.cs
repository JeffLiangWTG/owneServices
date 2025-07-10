using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class AddressBuilder
	{
		public static Address Create(IContext context, object address, bool includeContactDetail = true)
		{
			switch (address)
			{
				case JobDocAddress jobDocAddress:
					return Create(context, jobDocAddress, includeContactDetail);

				case OrgAddress orgAddress:
					return Create(context, orgAddress, includeContactDetail);

				case ZAddressWithContact zAddressWithContact:
					return Create(context, zAddressWithContact, includeContactDetail);

				case IAddress docAddress:
					return Create(context, docAddress, includeContactDetail);
			}

			return Create(context, (OrgAddress)null);
		}

		public static Address Create(IContext context, bool createEmptyInfo = false) => CreateEmptyAddress(context, createEmptyInfo);

		public static Address Create(IContext context, IAddress address, bool includeContactDetail = true)
		{
			if (address == null)
			{
				return CreateEmptyAddress(context);
			}

			var result = new Address(context.Factory);
			result.AddressIdentifier = address.AddressIdentifier;
			result.HeaderIdentifier = address.HeaderIdentifier;
			result.CompanyName = address.CompanyName;
			result.AddressLine1 = address.AddressLine1;
			result.AddressLine2 = address.AddressLine2;
			result.AdditionalAddressInformation = address.AdditionalAddressInformation;
			result.City = address.City;
			result.State = address.State;
			result.Postcode = address.Postcode;
			result.Country = Country.Create(context, address.Country);
			result.Unloco = Unloco.Create(context, address.Unloco);
			if (includeContactDetail)
			{
				result.Fax = address.Fax;
				result.Phone = address.Phone;
				result.Email = address.Email;
				result.Contact = address.Contact;
			}

			result.RegistrationNumbers = address.RegistrationNumbers
			.Select(x => new RegistrationNumber()
			{
				Type = new CodeDescription(x.Type?.Codes as CargoWise.Integration.ICodeDescriptionPairList ?? new CodeDescriptionPairList()),
				Value = x.Value,
				CountryOfIssue = Country.Create(context, x.CountryOfIssue)
			}).ToArray();

			result.TaxNumber = address.TaxNumber;

			result.TaxNumberType = new CodeDescription(address.TaxNumberType?.Codes as CargoWise.Integration.ICodeDescriptionPairList ?? new CodeDescriptionPairList())
			{
				Code = address.TaxNumberType?.Code ?? ZString.Empty,
				Description = address.TaxNumberType?.Description ?? ZString.Empty
			};

			return result;
		}

		public static Address Create(IContext context, JobDocAddress jobDocAddress, bool includeContactDetail = true)
		{
			if (jobDocAddress == null
				|| jobDocAddress.IsEmpty)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
			address.AddressIdentifier = jobDocAddress.E2_OA_Address;
			address.HeaderIdentifier = jobDocAddress.OrganisationPK;
			address.CompanyName = jobDocAddress.CompanyName;
			address.AddressLine1 = jobDocAddress.Address1;
			address.AddressLine2 = jobDocAddress.Address2;
			address.AdditionalAddressInformation = jobDocAddress.UnrestrictedAdditionalAddressInformation;
			address.City = jobDocAddress.City;
			address.State = jobDocAddress.StateCode;
			address.Postcode = jobDocAddress.Postcode;
			address.Country = Country.Create(context, jobDocAddress.Country);
			var docAddressUnloco = !jobDocAddress.E2_AddressOverride && jobDocAddress.HasRealAddress ? jobDocAddress.Address.RelatedPortCode : null;
			address.Unloco = Unloco.Create(context, docAddressUnloco);
			if (includeContactDetail)
			{
				address.Fax = jobDocAddress.E2_Fax;
				address.Phone = jobDocAddress.E2_Phone;
				address.Email = jobDocAddress.E2_Email;
				address.Contact = jobDocAddress.E2_Contact;
			}
			address.RegistrationNumbers = CreateRegistrationNumbers(jobDocAddress.Organisation);

			SetTaxInfo(address, jobDocAddress.Organisation);

			return address;
		}

		public static Address Create(IContext context, OrgAddress orgAddress, bool includeContactInfo = true)
		{
			if (orgAddress == null)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
			address.AddressIdentifier = orgAddress.PK;
			address.HeaderIdentifier = orgAddress.OA_OH;
			address.CompanyName = orgAddress.EffectiveCompanyName;
			address.AddressLine1 = orgAddress.Address1;
			address.AddressLine2 = orgAddress.Address2;
			address.AdditionalAddressInformation = orgAddress.UnrestrictedAdditionalAddressInformation;
			address.City = orgAddress.City;
			address.State = orgAddress.StateCode;
			address.Postcode = orgAddress.Postcode;
			address.Country = Country.Create(context, orgAddress.Country);
			address.Unloco = Unloco.Create(context, orgAddress.RelatedPortCode);
			address.RegistrationNumbers = CreateRegistrationNumbers(orgAddress.Header);

			SetTaxInfo(address, orgAddress.Header);

			if (includeContactInfo)
			{
				address.Fax = orgAddress.OA_Fax;
				address.Phone = orgAddress.OA_Phone;
				address.Email = orgAddress.OA_Email;
			}

			return address;
		}

		public static Address Create(IContext context, OrgHeader orgHeader, OrgContact orgContact)
		{
			var orgAddress = orgHeader?.MainAddress;
			if (orgAddress == null)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
			address.AddressIdentifier = orgAddress.PK;
			address.HeaderIdentifier = orgHeader.PK;
			address.CompanyName = orgAddress.EffectiveCompanyName;
			address.AddressLine1 = orgAddress.Address1;
			address.AddressLine2 = orgAddress.Address2;
			address.AdditionalAddressInformation = orgAddress.UnrestrictedAdditionalAddressInformation;
			address.City = orgAddress.City;
			address.State = orgAddress.StateCode;
			address.Postcode = orgAddress.Postcode;
			address.Country = Country.Create(context, orgAddress.Country);
			address.Unloco = Unloco.Create(context, orgAddress.RelatedPortCode);
			address.RegistrationNumbers = CreateRegistrationNumbers(orgAddress.Header);

			SetTaxInfo(address, orgHeader);

			if (orgContact != null)
			{
				address.Fax = orgContact.OC_Fax;
				address.Phone = orgContact.OC_Phone;
				address.Email = orgContact.OC_Email;
				address.Contact = orgContact.OC_ContactName;
			}
			else
			{
				address.Fax = orgAddress.OA_Fax;
				address.Phone = orgAddress.OA_Phone;
				address.Email = orgAddress.OA_Email;
			}

			return address;
		}

		public static Address Create(IContext context, UniversalOrgAddress orgAddress)
		{
			if (orgAddress == null)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
			address.CompanyName = orgAddress.CompanyName.GetValueOrDefault();
			address.AddressLine1 = orgAddress.Address1.GetValueOrDefault();
			address.AddressLine2 = orgAddress.Address2.GetValueOrDefault();
			address.AdditionalAddressInformation = orgAddress.AdditionalAddressInformation.GetValueOrDefault();
			address.City = orgAddress.City.GetValueOrDefault();
			address.State = ((ZString?)orgAddress.State).GetValueOrDefault();
			address.Postcode = orgAddress.Postcode.GetValueOrDefault();

			address.Country = Country.Create(context, (ICountry)null);
			address.Country.Code = orgAddress.Country?.Code.GetValueOrDefault() ?? ZString.Empty;
			address.Country.Name = orgAddress.Country?.Name.GetValueOrDefault() ?? ZString.Empty;

			address.Unloco = Unloco.Create(context, (IUnloco)null);
			address.Unloco.Code = orgAddress.Port?.Code.GetValueOrDefault() ?? ZString.Empty;
			address.Unloco.Name = orgAddress.Port?.Name.GetValueOrDefault() ?? ZString.Empty;

			address.TaxNumber = orgAddress.GovRegNum.GetValueOrDefault();
			address.TaxNumberType = CodeDescription.Create(orgAddress.GovRegNumType);

			address.Contact = orgAddress.Contact.GetValueOrDefault();
			address.Fax = orgAddress.Fax.GetValueOrDefault();
			address.Phone = orgAddress.Phone.GetValueOrDefault();
			address.Email = orgAddress.Email.GetValueOrDefault();

			address.RegistrationNumbers = orgAddress.RegistrationNumberCollection != null
				? orgAddress.RegistrationNumberCollection.Select(n => RegistrationNumber.Create(context, n)).ToArray()
				: Array.Empty<IRegistrationNumber>();

			return address;
		}

		public static Address Create(IContext context, ZAddressWithContact zAddressWithContact, bool includeContactInfo = true, bool useAddressContactInfoFallbackToOrganisation = false)
		{
			if (zAddressWithContact == null)
			{
				return new Address(context.Factory)
				{
					Country = new Country(context.Factory, context.Countries),
					Unloco = new Unloco(context.Factory, context.Unlocos, context.Countries),
					RegistrationNumbers = Array.Empty<IRegistrationNumber>()
				};
			}

			var address = Create(context, zAddressWithContact.OrgAddress, false);

			if (includeContactInfo)
			{
				if (useAddressContactInfoFallbackToOrganisation)
				{
					var orgAddress = zAddressWithContact.OrgAddress as OrgAddress;
					var mainAddress = orgAddress?.Header?.MainAddress;

					address.Email = string.IsNullOrEmpty(orgAddress?.OA_Email) ? (mainAddress?.OA_Email ?? ZString.Empty) : orgAddress.OA_Email;
					address.Phone = string.IsNullOrEmpty(orgAddress?.OA_Phone) ? (mainAddress?.OA_Phone ?? ZString.Empty) : orgAddress.OA_Phone;
					address.Fax = string.IsNullOrEmpty(orgAddress?.OA_Fax) ? (mainAddress?.OA_Fax ?? ZString.Empty) : orgAddress.OA_Fax;
				}
				else if (zAddressWithContact.OrgContact != null)
				{
					address.Contact = zAddressWithContact.OrgContact.OC_ContactName;
					address.Email = zAddressWithContact.OrgContact.EmailFallbackToOrganisation;
					address.Phone = zAddressWithContact.OrgContact.PhoneFallbackToOrganisation;
					address.Fax = zAddressWithContact.OrgContact.FaxFallbackToOrganisation;
				}
			}

			return address;
		}

		static IReadOnlyCollection<IRegistrationNumber> CreateRegistrationNumbers(OrgHeader header)
		{
			var result = header?
				.CustomsCodes
				.OfType<OrgCusCode>()
				.Select(code => new MasterFiles.Business.Macros.RegistrationNumber(code))
				.ToArray();

			result.ForEach(code =>
			{
				if (code.Type.Code == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && !code.Value.IsEmpty)
				{
					if (!code.CountryOfIssue.Code.EqualsIgnoringCase(code.Value.SubstringSafe(0, 2))
						&& !(code.CountryOfIssue.Code.EqualsIgnoringCase(CountryCodes.UnitedKingdom) && code.Value.StartsWith("XI", StringComparison.OrdinalIgnoreCase)))
					{
						code.Value = (code.CountryOfIssue.Code + code.Value).ToUpper();
					}
					else
					{
						code.Value = code.Value.ToUpper();
					}
				}
			});

			return result ?? Array.Empty<IRegistrationNumber>();
		}

		static void SetTaxInfo(Address address, OrgHeader header)
		{
			if (header == null)
			{
				return;
			}

			var provider = new OrgHeaderRegistrationNumberProvider(header);
			var taxInfo = ChinaCustomsTaxNumberHelper.GetTaxNumberInfo(address.Country.Code, provider);

			address.TaxNumber = taxInfo?.Number ?? ZString.Empty;

			var taxTypesList = new CodeDescriptionPairList();
			if (taxInfo != null)
			{
				taxTypesList.AddPair(taxInfo.Code, taxInfo.Description);
			}

			address.TaxNumberType = new CodeDescription(taxTypesList)
			{
				Code = taxInfo?.Code ?? ZString.Empty,
				Description = taxInfo?.Description ?? ZString.Empty
			};
		}

		public static Address CreateForCurrentUser(IContext context)
		{
			var address = CreateAddressForCurrentUser(context);

			address.Contact = GlbStaff.CurrentUser?.GS_FullName ?? ZString.Empty;
			address.Email = GlbStaff.CurrentUser?.GS_EmailAddress ?? ZString.Empty;
			address.Phone = GlbStaff.CurrentUser?.GS_WorkPhone ?? ZString.Empty;
			address.Fax = GlbStaff.CurrentUser?.GS_FaxNum ?? ZString.Empty;

			return address;
		}

		static Address CreateAddressForCurrentUser(IContext context)
		{
			Address address = null;

			if (GlbBranch.CurrentBranch?.OrgProxy?.MainAddress != null
				&& !GlbBranch.CurrentBranch.OrgProxy.MainAddress.Address1.IsEmpty)
			{
				address = Create(context, GlbBranch.CurrentBranch.OrgProxy.MainAddress);

				if (address.Country.Code.IsEmpty)
				{
					address.Country.Code = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
				}
			}
			else if (GlbCompany.CurrentCompany?.OrgProxy?.MainAddress != null)
			{
				address = Create(context, GlbCompany.CurrentCompany.OrgProxy.MainAddress);

				if (address.Country.Code.IsEmpty)
				{
					address.Country.Code = GlbCompany.CurrentCompany.Country?.Code ?? ZString.Empty;
				}
			}

			return address ?? Create(context, (OrgAddress)null);
		}

		static Address CreateEmptyAddress(IContext context, bool createEmptyInfo = true)
		{
			var address = new Address(context.Factory);
			if (createEmptyInfo)
			{
				address.Country = new Country(context.Factory, context.Countries);
				address.Unloco = new Unloco(context.Factory, context.Unlocos, context.Countries);
				address.RegistrationNumbers = Array.Empty<IRegistrationNumber>();
			}
			return address;
		}
	}
}
