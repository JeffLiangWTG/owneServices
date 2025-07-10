using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingResponseLineCollection))]
	public class RoutingResponseLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RoutingResponseLineCollection>
	{
		public void TestAllowNewRemove()
		{
			RoutingResponseLineCollection result = new RoutingResponseLineCollection(Factory);
			AssertEquals(false, result.AllowNew);
			AssertEquals(false, result.AllowRemove);
		}

		#region Implementation

		protected override RoutingResponseLineCollection GetCollectionToTest()
		{
			return new RoutingResponseLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RoutingResponseLine("", Factory);
		}

		#endregion
	}
}
