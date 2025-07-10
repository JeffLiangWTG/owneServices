using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	[TestedType(typeof(CargoControlAndTransitHouseManifest))]
	sealed class CargoControlAndTransitHouseManifestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cctHouseManifest = new CargoControlAndTransitHouseManifest("zzz", "zzz");
			cctHouseManifest.SendingParty = new Address(Factory);
			cctHouseManifest.ReceivingAgent = new Address(Factory);
			cctHouseManifest.Shipments = Array.Empty<CargoControlAndTransitDetail>();

			return cctHouseManifest;
		}
	}
}
