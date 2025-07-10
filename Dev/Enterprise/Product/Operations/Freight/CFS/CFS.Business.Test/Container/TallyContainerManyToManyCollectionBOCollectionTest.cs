using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyContainerManyToManyCollection))]
	public class TallyContainerManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TallyPackLine parent = Factory.New<TallyPackLine>();
			return new TallyContainerManyToManyCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TallyContainer>();
		}
	}
}
