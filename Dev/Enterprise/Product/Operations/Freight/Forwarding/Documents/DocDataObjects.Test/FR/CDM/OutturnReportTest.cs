using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(OutturnReport))]
	class OutturnReportTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var outturnReport = new OutturnReport(
				"ForwardingShipment",
				"S0001000");

			outturnReport.Containers = new List<BookingContainer>();
			outturnReport.GoodsDetails = new List<GoodsDetail>();

			return outturnReport;
		}
	}
}
