using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSContainerManyToManyCollection))]
	public class CFSContainerManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSPackLine parent = Factory.New<CFSPackLine>();
			return new CFSContainerManyToManyCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CFSContainer>();
		}
	}
}
