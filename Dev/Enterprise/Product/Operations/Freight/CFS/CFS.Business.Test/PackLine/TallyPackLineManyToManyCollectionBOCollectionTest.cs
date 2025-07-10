using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyPackLineManyToManyCollection))]
	public class TallyPackLineManyToManyCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TallyContainer parent = Factory.New<TallyContainer>();
			return new TallyPackLineManyToManyCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PackUnpackShipment>().OuterPackLines.AddNew();
		}
	}
}
