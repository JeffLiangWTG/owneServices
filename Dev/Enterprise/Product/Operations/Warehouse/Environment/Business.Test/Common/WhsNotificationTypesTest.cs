using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsErrorTypes))]
	class WhsNotificationsTypeTest : NotificationSubscriberTypeTest<WhsErrorTypes>
	{
		protected override WhsErrorTypes NewNotificationType(string message) => new WhsErrorTypes(message);

		protected override WhsErrorTypes NewNotificationType(string name, string message) => new WhsErrorTypes(name, message);
	}
}
