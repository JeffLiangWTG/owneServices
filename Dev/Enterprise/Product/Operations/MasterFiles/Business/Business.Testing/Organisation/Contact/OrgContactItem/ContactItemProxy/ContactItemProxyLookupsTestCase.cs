using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal abstract class ContactItemProxyLookupsTestCase<T> : BusinessObjectLookupsTestCase
			where T : ContactItemProxyLookups
	{
		public void TestDescriptionList()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.DescriptionList);
		}

		public void TestSelectableDescriptionList()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.SelectableDescriptionList);
		}

		public void TestInverseDescriptionList()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.InverseDescriptionList);
		}

		protected abstract T GetNewLookups();
	}
}
