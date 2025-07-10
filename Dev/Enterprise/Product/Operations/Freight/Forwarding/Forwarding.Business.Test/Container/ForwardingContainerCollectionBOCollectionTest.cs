using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingContainerCollection))]
	sealed class ForwardingContainerCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingConsol forwardingConsol = Factory.New<ForwardingConsol>();
			return forwardingConsol.Containers;
		}
	}
}
