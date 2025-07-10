using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(PreAdviceTransportCollection))]
	sealed class PreAdviceTransportCollectionBOTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobShipmentPreplanning parent = Factory.New<JobShipmentPreplanning>();
			return new PreAdviceTransportCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<Transport>();
		}

		#endregion
	}
}
