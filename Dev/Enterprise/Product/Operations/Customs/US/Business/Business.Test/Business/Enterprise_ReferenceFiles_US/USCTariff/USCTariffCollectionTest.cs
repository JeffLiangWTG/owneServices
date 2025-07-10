using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCTariffCollection))]
	public class USCTariffCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USCTariffCollection(Factory);
		}

		public void TestFilterBusinessObjectDefaults()
		{
			USCTariffCollection collection = new USCTariffCollection(Factory);

			string dateKey = USCTariff.FilterSchema.Date + FilterBusinessObjectDefault.FilterPropertyDelimiter + USCTariff.FilterSchema.DatePropertyNameToDefault;
			Assert("Date filter should be defaulted", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(dateKey));
			AssertEquals("Default date should be Today", USCTariffCollection.DateRangeSearchTexts.Today, collection.FilterBusinessObjectDefaults[dateKey].Value.ToString());
		}
	}
}
