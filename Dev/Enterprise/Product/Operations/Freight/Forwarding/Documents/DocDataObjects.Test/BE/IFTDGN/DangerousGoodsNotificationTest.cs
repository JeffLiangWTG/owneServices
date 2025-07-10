using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.BE.Testing
{
	[TestedType(typeof(DangerousGoodsNotification))]
	sealed class DangerousGoodsNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dangerousGoodsNotification = new DangerousGoodsNotification(
					"ForwardingConsol",
					"C00001015");

			dangerousGoodsNotification.PackingLines = System.Array.Empty<DGNPackingLine>();
			dangerousGoodsNotification.Containers = System.Array.Empty<DGNContainer>();

			return dangerousGoodsNotification;
		}
	}
}
