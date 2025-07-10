namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class LocationsItem : ILocationsItem
	{
		public LocationsItem(string location, string name, string startDate, string endDate)
		{
			Location = location;
			Name = name;
			StartDate = startDate;
			EndDate = endDate;
		}

		public string Location { get; }
		public string Name { get; }
		public string StartDate { get; }
		public string EndDate { get; }
	}
}
