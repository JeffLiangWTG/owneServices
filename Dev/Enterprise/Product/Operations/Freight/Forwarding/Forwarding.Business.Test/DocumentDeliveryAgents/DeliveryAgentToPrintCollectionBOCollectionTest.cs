using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business
{
	[TestedType(typeof(DeliveryAgentToPrintCollection))]
	sealed class DeliveryAgentToPrintCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeliveryAgentToPrintCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DeliveryAgentOrgHeader>();
		}
	}
}
