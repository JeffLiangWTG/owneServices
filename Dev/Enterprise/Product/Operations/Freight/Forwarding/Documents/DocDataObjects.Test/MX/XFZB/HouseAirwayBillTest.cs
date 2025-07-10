using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.MX.Testing
{
	[TestedType(typeof(HouseAirwayBill))]
	sealed class HouseAirwayBillTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var hawb = new HouseAirwayBill("ForwardingShipment", "S20210301");

			hawb.Shipper = new Address(Factory);
			hawb.Consignee = new Address(Factory);
			hawb.ExportAgent = new Address(Factory);
			hawb.RateLines = Array.Empty<HouseAirwayBillRateLine>();
			hawb.SpecialHandling = Array.Empty<HouseAirwayBillSpecialHandling>();

			return hawb;
		}
	}
}
