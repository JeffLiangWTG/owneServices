using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750ConsNotification))]
	public sealed class CIN750ConsNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = new CIN750ConsNotification(nameof(WhsItemDispatchConsignment), "DC00000001");

			return bizo;
		}
	}
}
