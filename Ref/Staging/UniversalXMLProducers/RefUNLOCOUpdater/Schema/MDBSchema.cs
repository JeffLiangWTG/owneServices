namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Schema
{
	public static class MDBSchema
	{
		public const string Country = "Country";
		public const string Location = "Location";
		public const string Name = "Name";
		public const string NameWoDiacritics = "NameWoDiacritics";
		public const string Subdivision = "Subdivision";
		public const string Status = "Status";
		public const string Function = "Function";
		public const string IATA = "IATA";
		public const string Date = "Date";
		public const string Coordinates = "Coordinates";

		public const int CountryMaxLength = 2;
		public const int LocationMaxLength = 5;
		public const int NameMaxLength = 35;
		public const int NameWoDiacriticsMaxLength = 35;
		public const int SubdivisionMaxLength = 35;
		public const int FunctionMaxLength = 10;
		public const int DateMaxLength = 10;
		public const int IATAMaxLength = 3;
		public const int StatusMaxLength = 3;
		public const int CoordinatesMaxLength = 12;
	}
}
