using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750OutNotification))]
	sealed class CIN750OutNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizO = new CIN750OutNotification(
				nameof(WhsItemDispatchConsignment),
				"DC0000001");

			return bizO;
		}
	}
}
