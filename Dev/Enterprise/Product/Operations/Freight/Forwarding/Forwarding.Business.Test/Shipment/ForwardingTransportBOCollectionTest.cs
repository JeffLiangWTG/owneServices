using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingTransportCollection))]
	sealed class ForwardingTransportBOCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingTransportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = typeof(CommonConsol);
			return transport;
		}

		#endregion
	}
}
