using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UNDGCountryReferencePivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStorageInstructions()
		{
			AssertEquals("TBC, ACD, GAS, BAS, OXS, MSC, WAT, FLL, LIT, EXP, RAD, ORG, FLS, TOX", new UNDGCountryReferencePivotLookups(null).StorageInstructions.CodesAsString);
		}
	}
}
