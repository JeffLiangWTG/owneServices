using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transit.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects.Testing
{
	[TestedType(typeof(CIN750DeconsNotification))]
	public sealed class CIN750DeconsNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = new CIN750DeconsNotification(nameof(WhsItemDispatchConsignment), "DC00000001");

			return bizo;
		}
	}
}
