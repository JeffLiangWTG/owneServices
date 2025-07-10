using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Providers
{
	public class DummyObsoletePropertiesLookup : IObsoletePropertiesLookup
	{
		public string LookupObsoleteName(string propertyName)
		{
			return string.Empty;
		}
	}
}
