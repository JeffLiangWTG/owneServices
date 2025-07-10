using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCCountry)]
	public class USCCountryCollection : BusinessObjectCollection<USCCountry>
	{
		public USCCountryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public USCCountryCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
