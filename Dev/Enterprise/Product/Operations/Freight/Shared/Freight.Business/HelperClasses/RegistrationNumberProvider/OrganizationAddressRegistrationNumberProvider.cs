using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Business
{
	sealed class OrganizationAddressRegistrationNumberProvider : IRegistrationNumberProvider
	{
		public OrganizationAddressRegistrationNumberProvider(OrganizationAddress organizationAddress)
		{
			org = organizationAddress;
		}

		readonly OrganizationAddress org;

		ZString IRegistrationNumberProvider.GetTaxNumber(ZString countryCode, ZString typeCode)
		{
			return GetTaxNumber(countryCode, typeCode);
		}

		ZString GetTaxNumber(ZString countryCode, ZString typeCode)
		{
			if (org == null || countryCode.IsEmpty || typeCode.IsEmpty)
			{
				return ZString.Empty;
			}

			if ((org.GovRegNumType?.Code.GetValueOrDefault() ?? ZString.Empty) == typeCode
					&& (org.Country?.Code.GetValueOrDefault() ?? ZString.Empty) == countryCode)
			{
				return org.GovRegNum.GetValueOrDefault();
			}

			var taxNumber = org.RegistrationNumberCollection
				?.OfType<RegistrationNumber>()
				.FirstOrDefault(registrationNumber => (registrationNumber.Type?.Code.GetValueOrDefault() ?? ZString.Empty) == typeCode
					&& (registrationNumber.CountryOfIssue?.Code.GetValueOrDefault() ?? ZString.Empty) == countryCode
					&& registrationNumber.Value.HasValue);

			return taxNumber?.Value ?? ZString.Empty;
		}

		public IReadOnlyCollection<(ZString taxNumber, ZString countryOfIssue)> GetTaxNumbers(ZString typeCode)
		{
			if (org?.RegistrationNumberCollection == null || typeCode.IsEmpty)
			{
				return Array.Empty<(ZString taxNumber, ZString countryOfIssue)>();
			}

			return org.RegistrationNumberCollection
				?.OfType<RegistrationNumber>()
				.Where(registrationNumber => typeCode.EqualsIgnoringCase(registrationNumber.Type?.Code.GetValueOrDefault() ?? ZString.Empty)
					&& registrationNumber.Value.HasValue)
				.Select(c => (c.Value.Value, c.CountryOfIssue?.Code ?? ZString.Empty))
				.ToArray();
		}
	}
}
