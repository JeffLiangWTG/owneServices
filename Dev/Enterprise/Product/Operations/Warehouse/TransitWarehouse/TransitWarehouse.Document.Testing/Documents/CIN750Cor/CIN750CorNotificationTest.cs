using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750CorNotification))]
	sealed class CIN750CorNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = new CIN750CorNotification(nameof(WhsItemReceiveConsignment), "RC0000001");

			return bizo;
		}
	}
}
