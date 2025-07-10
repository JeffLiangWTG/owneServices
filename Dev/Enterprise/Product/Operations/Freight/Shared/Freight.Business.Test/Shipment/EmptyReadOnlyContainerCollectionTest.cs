using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Freight.Business.CommonShipment;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(EmptyReadOnlyContainerCollection))]
	sealed class EmptyReadOnlyContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EmptyReadOnlyContainerCollection(Factory);
		}
	}
}
