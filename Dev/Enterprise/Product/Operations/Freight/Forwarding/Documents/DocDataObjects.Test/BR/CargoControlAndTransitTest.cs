using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	[TestedType(typeof(CargoControlAndTransit))]
	sealed class CargoControlAndTransitTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cct = new CargoControlAndTransit("zzz", "zzz");
			cct.Shipper = new Address(Factory);
			cct.Consignee = new Address(Factory);
			cct.ImportAgent = new Address(Factory);
			cct.RateLines = Array.Empty<CargoControlAndTransitRateLine>();
			cct.SpecialHandling = Array.Empty<CargoControlAndTransitSpecialHandling>();

			return cct;
		}
	}
}
