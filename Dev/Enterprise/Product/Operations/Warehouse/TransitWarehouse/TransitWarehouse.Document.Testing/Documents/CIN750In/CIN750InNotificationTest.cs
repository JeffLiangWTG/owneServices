using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750InNotification))]
	sealed class CIN750InNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bo = new CIN750InNotification(
				nameof(WhsItemReceiveConsignment),
				"RC0000001");

			return bo;
		}
	}
}
