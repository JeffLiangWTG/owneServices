using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTransitTimeDetailCollection))]
	public class RefTransitTimeDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefTransitTimeDetailCollection(Factory.NewWithValidTestData<RefTransitTime>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<RefTransitTimeDetail>();
		}
	}
}
