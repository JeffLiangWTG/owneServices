using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RoutingCollection))]
	sealed class RoutingCollectionBOTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = typeof(CommonConsol);
			return transport;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.Transports.ParentDeleting();
			return new RoutingCollection(consol);
		}

		#endregion
	}
}
