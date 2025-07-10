using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawDeclarableCodeRecord : IRawDeclarableCodeRecord
	{
		public string TariffHeader { get; }
		public DateTime StartDate { get; }
		public DateTime DeclarableStartDate { get; }
		public bool IsLeaf { get; }
		public DateTime EndDate { get; }

		public RawDeclarableCodeRecord(string tariffHeader, DateTime startDate, DateTime declarableStartDate, string isLeaf, DateTime endDate)
		{
			this.TariffHeader = tariffHeader;
			this.StartDate = startDate;
			this.DeclarableStartDate = declarableStartDate;
			this.IsLeaf = isLeaf == "1";
			this.EndDate = endDate;
		}
	}
}
