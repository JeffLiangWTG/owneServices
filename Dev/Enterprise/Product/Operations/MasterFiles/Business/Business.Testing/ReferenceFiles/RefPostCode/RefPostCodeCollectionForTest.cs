using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPostCodeCollectionForTest : RefPostCodeCollection
	{
		public RefPostCodeCollectionForTest(BusinessObjectFactory factory, string countryCode) : base(factory, new ZQuery(), countryCode, "", "") { }

		public IFindBoxListProvider Provider => FindBoxListProvider;
	}
}
