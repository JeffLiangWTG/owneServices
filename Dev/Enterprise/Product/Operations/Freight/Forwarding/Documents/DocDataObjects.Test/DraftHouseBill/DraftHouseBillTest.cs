using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DraftHouseBill))]
	sealed class DraftHouseBillTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var draftHouseBill = new DraftHouseBill(
					"ForwardingConsol",
					"C00001015");

			draftHouseBill.Containers = Array.Empty<DraftHouseBillContainer>();

			return draftHouseBill;
		}
	}
}
