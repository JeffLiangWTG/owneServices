using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	[TestedType(typeof(PortbaseContainer))]
	sealed class PortbaseContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var portbaseContainer = new PortbaseContainer
			{
				Shipments = new List<PortbaseShipment>()
			};

			return portbaseContainer;
		}
	}
}
