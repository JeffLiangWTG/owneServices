using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExchangeRates
{
	public class ExchangeRateMetaData
	{
		public string Content { get; set; }
		public string DataLocation { get; set; }
		public string Source { get; set; }
		public string Version { get; set; }
		public string OutputPath { get; set; }
		public string OutputFilePrefix { get; set; }
		public string DataSourceName { get; set; }
		public DateTime PublicationTime { get; set; }
	}
}
