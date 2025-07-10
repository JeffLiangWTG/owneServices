using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(TransferErrorTypes))]
	class TransferErrorTypesTest : NotificationSubscriberTypeTest<TransferErrorTypes>
	{
		protected override TransferErrorTypes NewNotificationType(string message) => new TransferErrorTypes(message);

		protected override TransferErrorTypes NewNotificationType(string name, string message) =>
			new TransferErrorTypes(message);
	}
}
