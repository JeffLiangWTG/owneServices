using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(OrderErrorTypes))]
	class OrderErrorTypesTest : NotificationSubscriberTypeTest<OrderErrorTypes>
	{
		protected override OrderErrorTypes NewNotificationType(string message) => new OrderErrorTypes(message);

		protected override OrderErrorTypes NewNotificationType(string name, string message) =>
			new OrderErrorTypes(message);
	}
}
