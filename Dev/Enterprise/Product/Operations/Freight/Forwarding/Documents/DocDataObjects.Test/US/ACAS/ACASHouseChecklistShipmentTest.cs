using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	[TestedType(typeof(ACASHouseChecklistShipment))]
	sealed class ACASHouseChecklistShipmentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var acasShipment = new ACASHouseChecklistShipment("zzz", "zzz", "ACAS House Checklist");
			return acasShipment;
		}
	}
}
