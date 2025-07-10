using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassInnerPackLineCollection))]
	public class GatePassInnerPackLineCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GatePassShipment parent = Factory.New<GatePassShipment>();
			return new GatePassInnerPackLineCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GatePassPackLine>();
		}
	}
}
