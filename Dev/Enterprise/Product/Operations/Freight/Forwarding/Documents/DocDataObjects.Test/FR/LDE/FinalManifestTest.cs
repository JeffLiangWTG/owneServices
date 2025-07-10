using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(FinalManifest))]
	sealed class FinalManifestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dataObject = new FinalManifest(ZString.Empty, ZString.Empty);
			dataObject.Containers = new List<BookingContainer>();
			dataObject.PackingLines = new List<BookingPackingLine>();
			dataObject.GoodsDetails = new List<GoodsDetail>();
			return dataObject;
		}
	}
}
