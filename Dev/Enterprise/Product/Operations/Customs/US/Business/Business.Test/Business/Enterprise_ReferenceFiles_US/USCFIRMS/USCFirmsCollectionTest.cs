using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCFirmsCollection))]
	sealed class USCFirmsCollectionTest : ActiveBusinessObjectCollectionTestCase<USCFirmsCollection>
	{
		protected override USCFirmsCollection GetCollectionToTest()
		{
			USCFirmsCollection result = new USCFirmsCollection(Factory);
			result.AdditionalFilter = new ZQuery(USCFIRMSSchema.US_City, "ABERDEEN");
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			USCFIRMS firms = Factory.New<USCFIRMS>();
			firms.US_City = "ABERDEEN";
			return firms;
		}

		public override void TestAddNew()
		{
			int initialCount = Collection.Count;

			USCFIRMS bizO1 = Collection.AddNew();
			bizO1.US_City = "ABERDEEN";

			USCFIRMS bizO2 = Collection.AddNew();
			bizO2.US_City = "ABERDEEN";

			AssertEquals("Collection count", initialCount + 2, Collection.Count);
			Assert("Contains new elements", Collection.Contains(bizO1));
			Assert("Contains new elements", Collection.Contains(bizO2));
		}
	}
}
