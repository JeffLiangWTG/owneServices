using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class CountryIgnoreRegistrationNumberProvider : IRegistrationNumberProvider
	{
		readonly IRegistrationNumberProvider registrationNumberProvider;

		public CountryIgnoreRegistrationNumberProvider(IRegistrationNumberProvider registrationNumberProvider)
		{
			this.registrationNumberProvider = registrationNumberProvider;
		}

		public ZString GetTaxNumber(ZString countryCode, ZString typeCode)
		{
			var taxNumbers = registrationNumberProvider.GetTaxNumbers(typeCode);
			var bestTaxNumber = taxNumbers?.FirstOrDefault(x => x.countryOfIssue == countryCode).taxNumber ?? ZString.Empty;
			return bestTaxNumber.IsEmpty ? (taxNumbers?.FirstOrDefault().taxNumber ?? ZString.Empty) : bestTaxNumber;
		}

		public IReadOnlyCollection<(ZString taxNumber, ZString countryOfIssue)> GetTaxNumbers(ZString typeCode)
		{
			return registrationNumberProvider.GetTaxNumbers(typeCode);
		}

		public ZString FindCountryByTaxNumber(ZString taxNumber, ZString typeCode)
		{
			var taxNumbers = registrationNumberProvider.GetTaxNumbers(typeCode);
			return taxNumbers.Where(x => x.taxNumber == taxNumber).Select(x => x.countryOfIssue).FirstOrDefault();
		}
	}
}
