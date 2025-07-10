using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclatureRecord : INomenclatureRecord
	{
		public string TariffHeader { get; }
		public DateTime DeclarableStartDate { get; }
		public DateTime EndDate { get; }
		public int HierarchyPosition { get; }
		public int Level { get; }
		public string Description { get; }
		public bool IsTariff { get; private set; }

		public ICollection<(string language, string description)> Language { get; }

		public NomenclatureRecord(string tariffHeader, DateTime declarableStartDate, DateTime endDate, int hierarchyPosition, int level, string description, ICollection<(string language, string description)> language, bool isTariff)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(description, nameof(description));
			Argument.NotNull(language, nameof(language));

			this.TariffHeader = tariffHeader;
			this.DeclarableStartDate = declarableStartDate;
			this.EndDate = endDate;
			this.Description = description;
			this.HierarchyPosition = hierarchyPosition;
			this.Level = level;
			this.Language = language;
			this.IsTariff = isTariff;
		}
	}
}
