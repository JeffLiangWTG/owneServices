using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models
{
	public class Header : ISourceData
	{
		public TransactionType TransactionType { get; set; }
		public DateTime PublicationDate { get; set; }
		public string GovernmentGazettePublicationNumber { get; set; }

		public List<TariffData> Tariffs { get; set; } = new List<TariffData>();

		public void ProcessUpdates(ICountryCodeLoader countryLoader, ITariffHelper tariffHelper, ILogger logger)
		{
			var countryCodes = countryLoader.GetCountryData(GetUniqueCountryNames());

			Tariffs.ForEach(t => t.ProcessUpdates(countryCodes, tariffHelper, logger));

			AddTariffsToBeExpired(tariffHelper);
		}

		public IEnumerable<string> GetUniqueCountryNames()
		{
			var all = Tariffs.Select(x => (x.ImportedFrom ?? string.Empty).ToUpperInvariant()).Distinct().ToList();
			var rateCountries = Tariffs.SelectMany(x => x.Rates.Select(r => (r.Countries ?? string.Empty).ToUpperInvariant())).Distinct().ToList();

			all.AddRange(rateCountries);

			return all.SelectMany(x => x.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
						.Select(x => x.Trim())
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.Distinct()
						.OrderBy(x => x)
						.ToList();
		}

		void AddTariffsToBeExpired(ITariffHelper tariffHelper)
		{
			if (TransactionType != TransactionType.Deletion)
			{
				var headings = Tariffs.Where(t => t.IsHeading && t.AllowForExpiration && !t.Rates.Any()).ToList();
				foreach (var heading in headings)
				{
					var expireDate = heading.StartDate.AddMinutes(-1);
					var tariffsToExpire = tariffHelper.GetTariffs(heading.TariffCode)
						.Where(t => t.AllowForExpiration && t.StartDate < expireDate && t.EndDate == CommonHelper.MaximumDateTime)
						.ToList();
					foreach (var tariff in tariffsToExpire)
					{
						tariff.IsAddedForExpiration = true;
						tariff.EndDate = expireDate;
						Tariffs.Add(tariff);
					}
				}
			}
		}
	}
}
