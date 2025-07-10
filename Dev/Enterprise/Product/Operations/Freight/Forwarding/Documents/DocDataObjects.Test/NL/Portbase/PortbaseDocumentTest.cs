using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	[TestedType(typeof(PortbaseDocument))]
	sealed class PortbaseDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var portbaseDocument = new PortbaseDocument("zzz")
			{
				Containers = new List<PortbaseContainer>()
			};

			return portbaseDocument;
		}
	}
}
