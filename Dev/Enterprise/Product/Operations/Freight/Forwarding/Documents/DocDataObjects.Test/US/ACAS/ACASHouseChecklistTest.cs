using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	[TestedType(typeof(ACASHouseChecklist))]
	sealed class ACASHouseChecklistTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var acas = new ACASHouseChecklist("zzz", "zzz", "ACAS House Checklist");
			acas.Shipments = new List<ACASHouseChecklistShipment>();

			return acas;
		}
	}
}
