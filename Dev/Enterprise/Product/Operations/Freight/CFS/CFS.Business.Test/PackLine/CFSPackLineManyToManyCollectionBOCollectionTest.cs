using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSPackLineManyToManyCollection))]
	public class CFSPackLineManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSContainer parent = Factory.New<CFSContainer>();
			return new CFSPackLineManyToManyCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CFSPackLine>();
		}
	}
}
