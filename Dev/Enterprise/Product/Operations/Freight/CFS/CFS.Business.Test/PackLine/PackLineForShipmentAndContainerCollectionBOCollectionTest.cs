using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackLineForShipmentAndContainerCollection))]
	public class PackLineForShipmentAndContainerCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			PackUnpackShipment parent = Factory.New<PackUnpackShipment>();
			return new PackLineForShipmentAndContainerCollection(parent, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TallyPackLine>();
		}
	}
}
