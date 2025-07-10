using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business
{
	[TestedType(typeof(DeliveryAgentToSelectFromForPrintingCollection))]
	sealed class DeliveryAgentToSelectFromForPrintingCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeliveryAgentToSelectFromForPrintingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DeliveryAgentToSelectFromForPrinting>();
		}
	}
}
