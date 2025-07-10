using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(ImportManifest))]
	sealed class ImportManifestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var importManifest = new ImportManifest(
				"ForwardingShipment",
				"S0001000");

			importManifest.Containers = new List<BookingContainer>();
			importManifest.GoodsDetails = new List<GoodsDetail>();

			return importManifest;
		}
	}
}
