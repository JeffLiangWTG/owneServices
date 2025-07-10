using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(ReceiveErrorTypes))]
	class ReceiveErrorTypesTest : NotificationSubscriberTypeTest<ReceiveErrorTypes>
	{
		protected override ReceiveErrorTypes NewNotificationType(string message) => new ReceiveErrorTypes(message);

		protected override ReceiveErrorTypes NewNotificationType(string name, string message) =>
			new ReceiveErrorTypes(message);
	}
}
