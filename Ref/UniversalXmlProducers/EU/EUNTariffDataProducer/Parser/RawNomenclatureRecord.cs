using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawNomenclatureRecord : IRawNomenclatureRecord
	{
		public string TariffHeader { get; }
		public DateTime StartDate { get; private set; }
		public DateTime EndDate { get; }
		public string Language { get; }
		public int HierarchyPosition { get; }
		public int Level { get; }
		public string Description { get; }

		public RawNomenclatureRecord(string tariffHeader, DateTime startDate, DateTime endDate, string language, string hierarchyPosition, string level, string description)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(description, nameof(description));
			Argument.NotNullOrEmpty(hierarchyPosition, nameof(hierarchyPosition));
			Argument.NotNullOrEmpty(language, nameof(language));

			TariffHeader = tariffHeader;
			StartDate = startDate;
			EndDate = endDate;
			Language = language;
			Description = description;
			HierarchyPosition = int.Parse(hierarchyPosition, CultureInfo.InvariantCulture);
			Level = string.IsNullOrEmpty(level) ? 0 : level.Count(c => c == '-');
		}

		public RawNomenclatureRecord(string tariffHeader, DateTime startDate, DateTime endDate, string language, int hierarchyPosition, int level, string description)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(description, nameof(description));
			Argument.NotNullOrEmpty(language, nameof(language));

			TariffHeader = tariffHeader;
			StartDate = startDate;
			EndDate = endDate;
			Language = language;
			Description = description;
			HierarchyPosition = hierarchyPosition;
			Level = level;
		}
	}
}
