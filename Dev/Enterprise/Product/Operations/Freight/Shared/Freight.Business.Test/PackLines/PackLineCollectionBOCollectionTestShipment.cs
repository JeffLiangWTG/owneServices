using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLineCollection))]
	sealed class PackLineCollectionBOCollectionTestShipment : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonShipment parent = CommonShipment.New(Factory);
			return new PackLineCollection(parent, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PackLine>();
		}
	}
}
