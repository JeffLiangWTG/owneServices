using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Module.Testing
{
	sealed class EntryNumberModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEntryType()
		{
			EntryNumberModuleFilter filter = new EntryNumberModuleFilter("Test", delegate
			{ return null; });
			AssertNoError("EntryTypeInfo should has NO error", filter.EntryTypeInfo, "Enter a valid selection.");

			filter.EntryType = "XXX";
			AssertHasError("EntryTypeInfo should has error", filter.EntryTypeInfo, "Enter a valid selection.");

			filter.EntryType = "CCN";
			AssertNoError("EntryTypeInfo should has NO error", filter.EntryTypeInfo, "Enter a valid selection.");
		}
	}
}
