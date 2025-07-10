using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefAirlineSpecialHandlingCodeCollection))]
	public class RefAirlineSpecialHandlingCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefAirlineSpecialHandlingCodeCollection(Factory.NewWithValidTestData<RefAirline>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
		}
	}
}
