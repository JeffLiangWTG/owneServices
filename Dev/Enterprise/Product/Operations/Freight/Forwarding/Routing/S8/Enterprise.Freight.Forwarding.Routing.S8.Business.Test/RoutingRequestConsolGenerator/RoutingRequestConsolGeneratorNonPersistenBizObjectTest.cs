using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	[TestedType(typeof(RoutingRequestConsolGenerator))]
	class RoutingRequestConsolGeneratorNonPersistenBizObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RoutingRequestConsolGenerator(CreateMultiDaysSelection());
		}

		RoutingMultiDaysSelection CreateMultiDaysSelection()
		{
			var requestDate = new ZDateTime(2018, 7, 6);
			var testMessageLine1 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};

			return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
		}

		#endregion
	}
}
