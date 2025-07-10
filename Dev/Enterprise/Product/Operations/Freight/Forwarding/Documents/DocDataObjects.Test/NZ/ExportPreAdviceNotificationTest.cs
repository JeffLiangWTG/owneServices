using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NZ.Testing
{
	[TestedType(typeof(ExportPreAdviceNotification))]
	sealed class ExportPreAdviceNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var exportPreAdviceNotification = new ExportPreAdviceNotification(
					"ForwardingConsol",
					"C00001015");

			return exportPreAdviceNotification;
		}
	}
}
