using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models
{
	public sealed class ExchangeRateData : ISourceData
	{
		public DateTime PublishDate { get; set; }
		public IReadOnlyCollection<ExchangeRate> ExchangeRates { get; set; }

		DateTime ISourceData.PublicationDate => PublishDate;
	}
}
