namespace Enterprise.MasterFiles.GUI
{
	public class ComingSoonModel
	{
		public ComingSoonModel(string country)
		{
			this.country = country;
		}

		readonly string country;

		public string ComingSoonString => ResourceStringHelper.GetComingSoonString(country);
	}
}
