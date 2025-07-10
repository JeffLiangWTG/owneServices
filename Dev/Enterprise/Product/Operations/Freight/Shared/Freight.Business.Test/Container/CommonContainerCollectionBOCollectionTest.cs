using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommonContainerCollection))]
	public class CommonContainerCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CommonContainerCollection(GetNewConsol(), Factory);
		}

		protected virtual CommonConsol GetNewConsol()
		{
			return Factory.New<CommonConsol>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CommonContainer>();
		}
	}
}
