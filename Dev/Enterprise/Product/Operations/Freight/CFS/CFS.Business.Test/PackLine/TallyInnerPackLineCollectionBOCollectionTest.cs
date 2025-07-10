using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyInnerPackLineCollection))]
	public class TallyInnerPackLineCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			PackUnpackShipment parent = Factory.New<PackUnpackShipment>();
			return new TallyInnerPackLineCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TallyPackLine>();
		}
	}
}
