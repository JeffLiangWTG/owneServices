using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Warehouse.Transit.Document
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
			}

			return Create(context, (OrgAddress)null);
		}

		public static Address Create(IContext context, JobDocAddress jobDocAddress, bool includeContactDetail = true)
		{
			if (jobDocAddress == null
				|| jobDocAddress.IsEmpty)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
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

			return address;
		}

		public static Address Create(IContext context, OrgAddress orgAddress, bool includeContactInfo = true)
		{
			if (orgAddress == null)
			{
				return CreateEmptyAddress(context);
			}

			var address = new Address(context.Factory);
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

			if (includeContactInfo)
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

			address.Fax = orgAddress.Fax.GetValueOrDefault();
			address.Phone = orgAddress.Phone.GetValueOrDefault();
			address.Email = orgAddress.Email.GetValueOrDefault();

			address.RegistrationNumbers = orgAddress.RegistrationNumberCollection != null
				? orgAddress.RegistrationNumberCollection.Select(n => RegistrationNumber.Create(context, n)).ToArray()
				: Array.Empty<IRegistrationNumber>();

			return address;
		}

		static IReadOnlyCollection<IRegistrationNumber> CreateRegistrationNumbers(OrgHeader header)
		{
			var result = header?
				.CustomsCodes
				.OfType<OrgCusCode>()
				.Select(code => new MasterFiles.Business.Macros.RegistrationNumber(code))
				.ToArray();

			return result ?? Array.Empty<IRegistrationNumber>();
		}

		static Address CreateEmptyAddress(IContext context)
		{
			var address = new Address(context.Factory);
			address.Country = new Country(context.Factory, context.Countries);
			address.Unloco = new Unloco(context.Factory, context.Unlocos, context.Countries);
			address.RegistrationNumbers = Array.Empty<IRegistrationNumber>();
			return address;
		}
	}
}
