using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCityTownCollectionForTest : RefCityTownCollection
	{
		public RefCityTownCollectionForTest(BusinessObjectFactory factory, string countryCode) : base(factory, new ZQuery(), countryCode, "") { }

		public IFindBoxListProvider Provider => FindBoxListProvider;
	}
}
