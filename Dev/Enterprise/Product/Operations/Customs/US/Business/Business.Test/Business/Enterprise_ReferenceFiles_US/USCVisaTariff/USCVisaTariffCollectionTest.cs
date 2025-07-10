using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCVisaTariffCollection))]
	sealed class USCVisaTariffCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetOrCreateFor()
		{
			USCVisaTariffCollection coll = new USCVisaTariffCollection(Visa);

			AssertNull(coll.Find("0000"));

			USCVisaTariff tariff1 = coll.GetOrCreateFor("0000");
			AssertNotNull(tariff1);
			AssertEquals("one element is added", 1, coll.Count);

			AssertEquals(tariff1, coll.Find("0000"));

			USCVisaTariff tariff2 = coll.GetOrCreateFor("0000");
			AssertEquals(tariff1, tariff2);
			AssertEquals("still one element", 1, coll.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new USCVisaTariffCollection(Visa);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			USCVisaTariff result = Factory.New<USCVisaTariff>();
			result.UK_UO = Visa.PK;
			return result;
		}

		USCVisa Visa
		{
			get { return fVisa ?? (fVisa = Factory.New<USCVisa>()); }
		}
		USCVisa fVisa;
	}
}
