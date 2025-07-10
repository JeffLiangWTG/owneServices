using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseCollection))]
	sealed class USCACCaseCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseCollection>
	{
		public void TestDefaultFilters()
		{
			var coll = new USCACCaseCollection(Factory);
			coll.DefaultFilters("", "", "", "0000");

			AssertEquals("0000", coll.FilterBusinessObjectDefaults["Tariff Number:Property"].Value);
			AssertEquals("", coll.FilterBusinessObjectDefaults["Country Code:Property"].Value);
			AssertEquals(ZBool.True, coll.FilterBusinessObjectDefaults["Case Status:Property0"].Value);

			coll = new USCACCaseCollection(Factory);
			coll.DefaultFilters("", "A78945987", "", "");
			AssertEquals("A78945987", coll.FilterBusinessObjectDefaults["Case Number:Property"].Value);
		}

		protected override USCACCaseCollection GetCollectionToTest()
		{
			return new USCACCaseCollection(Factory);
		}
	}
}
