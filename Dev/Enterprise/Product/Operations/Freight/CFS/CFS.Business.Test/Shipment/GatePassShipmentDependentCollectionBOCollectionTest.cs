using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	[TestedType(typeof(GatePassShipmentDependentCollection))]
	public class GatePassShipmentDependentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GatePassContainer parent = Factory.New<GatePassContainer>();
			return new GatePassShipmentDependentCollection(Factory, parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<GatePassShipment>();
		}
	}
}
