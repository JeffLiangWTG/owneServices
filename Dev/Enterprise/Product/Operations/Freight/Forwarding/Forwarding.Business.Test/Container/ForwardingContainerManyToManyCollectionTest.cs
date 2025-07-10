using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingContainerManyToManyCollection))]
	sealed class ForwardingContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingPackLine packLine = Factory.New<ForwardingShipment>().OuterPackLines.AddNew();
			return packLine.Containers;
		}
	}
}
