namespace Enterprise.Customs.US.Business
{
	public class USScientificDataAddInfoLookups : AutoUSScientificDataAddInfoLookups
	{
		public USScientificDataAddInfoLookups(AutoUSScientificDataAddInfo parent) : base(parent)
		{
		}

		public USCCountryCollection USCountryList
		{
			get { return new USCCountryCollection(Factory); }
		}
	}
}
