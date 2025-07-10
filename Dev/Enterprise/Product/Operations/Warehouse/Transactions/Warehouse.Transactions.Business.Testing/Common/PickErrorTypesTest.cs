using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(PickErrorTypes))]
	class PickErrorTypesTest : NotificationSubscriberTypeTest<PickErrorTypes>
	{
		protected override PickErrorTypes NewNotificationType(string message) => new PickErrorTypes(message);

		protected override PickErrorTypes NewNotificationType(string name, string message) =>
			new PickErrorTypes(message);
	}
}
