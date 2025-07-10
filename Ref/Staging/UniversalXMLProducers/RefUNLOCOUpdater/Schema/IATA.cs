using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema
{
	public class IATA
	{
		public string IATACode { get; set; }
		public string IATACityCode { get; set; }
		public string PortName { get; set; }
		public string CityName { get; set; }
		public string StateName { get; set; }
		public string CountryName { get; set; }
		public string LatitudeCoordinates { get; set; }
		public string LongitudeCoordinates { get; set; }
		public string TimeZoneName { get; set; }
		public string FunctionType { get; set; }
	}
}
