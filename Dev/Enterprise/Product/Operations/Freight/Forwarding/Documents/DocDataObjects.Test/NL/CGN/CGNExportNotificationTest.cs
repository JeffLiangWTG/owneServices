using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	[TestedType(typeof(CGNExportNotification))]
	sealed class CGNExportNotificationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new CGNExportNotification("ForwardingConsol", "C00001015");
	}
}
