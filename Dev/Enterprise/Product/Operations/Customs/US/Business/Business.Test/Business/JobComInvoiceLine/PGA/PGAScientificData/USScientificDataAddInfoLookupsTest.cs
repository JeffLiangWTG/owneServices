using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USScientificDataAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCountryList()
		{
			var data = Factory.New<ScientificData>();
			AssertEquals(typeof(USCCountryCollection), data.AddInfoLookups.USCountryList.GetType());
		}
	}
}
