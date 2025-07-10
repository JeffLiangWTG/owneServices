using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	[TestedType(typeof(PackUnpackShipmentDependentCollection))]
	public class PackUnpackShipmentDependentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TallyContainer parent = Factory.New<TallyContainer>();
			return new PackUnpackShipmentDependentCollection(Factory, parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PackUnpackShipment>();
		}
	}
}
