using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawDailyNomenclatureRecord : IRawNomenclatureDailyRecord
	{
		public string Publish { get; }

		public DateTime DeclarableStartDate { get; }

		public DateTime EndDate { get; }

		public int HierarchyPosition { get; }

		public int Level { get; }

		public string Description { get; }

		public string Language { get; }

		public string TariffHeader { get; }

		public DateTime StartDate { get; }

		public int SequenceNumber { get; }

		public bool IsLeaf => TariffHeader.EndsWith(LeafSuffixTariffHeader, StringComparison.InvariantCulture);

		public string FileName { get; }

		public RawDailyNomenclatureRecord(string tariffHeader,
			DateTime startDate,
			DateTime endDate,
			string language,
			string hierarchyPosition,
			string level,
			string description,
			string publish,
			DateTime declarableStartDate,
			int sequenceNumber,
			string fileName)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(hierarchyPosition, nameof(hierarchyPosition));
			Argument.NotNullOrEmpty(publish, nameof(publish));

			TariffHeader = tariffHeader;
			StartDate = startDate;
			EndDate = endDate;
			Language = language;
			Description = description;
			HierarchyPosition = int.Parse(hierarchyPosition, CultureInfo.InvariantCulture);
			Level = string.IsNullOrEmpty(level) ? 0 : level.Count(c => c == '-');
			Publish = publish;
			DeclarableStartDate = declarableStartDate;
			SequenceNumber = sequenceNumber;
			FileName = fileName;
		}

		const string LeafSuffixTariffHeader = "80";
	}
}
