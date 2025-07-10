using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class BookingRequestReplyCodeMappingTest : ConsolIncomingMessageCodeMappingTest
	{
		public void TestUnlocoCodeMapping_AgentConsol()
		{
			var bookingRequestReply = BookingMessagesForTest.CreateBookingRequestReply();
			var consol = BookingMessagesForTest.CreateConsolMatchingDataTarget(Factory.BOFactory, bookingRequestReply);

			var carrier = consol.ShippingLine;

			CreateOCMUnlocoMappings(carrier, new Dictionary<string, string>
			{
				["SEZZZ"] = "SEGOT",
				["DEZZZ"] = "DEWVN"
			});

			Factory.SaveForTesting();

			AssertUnlocoCodeMapping(bookingRequestReply, consol);
		}

		public void TestUnlocoCodeMapping_CoLoadConsol()
		{
			var bookingRequestReply = BookingMessagesForTest.CreateBookingRequestReply();
			var consol = BookingMessagesForTest.CreateCoLoadConsolMatchingDataTarget(Factory.BOFactory, bookingRequestReply);

			var creditor = consol.Creditor;

			CreateOCMUnlocoMappings(creditor, new Dictionary<string, string>
			{
				["SEZZZ"] = "SEGOT",
				["DEZZZ"] = "DEWVN"
			});

			Factory.SaveForTesting();

			AssertUnlocoCodeMapping(bookingRequestReply, consol);
		}

		void AssertUnlocoCodeMapping(UniversalShipment bookingRequestReply, ForwardingConsol consol)
		{
			var transportLegs = bookingRequestReply
				.TransportLegCollection
				.OrderBy(t => t.LegOrder)
				.ToArray();

			transportLegs[0].PortOfDischarge.Code = "DEZZZ";

			transportLegs[1].PortOfLoading.Code = "DEZZZ";
			transportLegs[1].PortOfDischarge.Code = "SEZZZ";

			var consolDataContextManagerForTest = new ForwardingConsolDataContextManagerProxyForTest();
			var contextManagers = new Hashtable
			{
				[nameof(DataContextType.ForwardingConsol)] = new TestObjectHandle(consolDataContextManagerForTest)
			};

			using (ObjectFactory.Substitute("UniversalDataContextManagers", contextManagers))
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				var res = CreateAndProcessUniversalShipment(bookingRequestReply);

				AssertMultilineASCIIEquals("import log",
@"Information|Updated Consol C09140756 (Master Bill='HLCUSZX2209BHYC4') from UniversalShipment.
Information|Successfully saved Consol C09140756 (Master Bill='HLCUSZX2209BHYC4') with 1 x ForwardingConsolStmNote, 2 x Transport.",
					string.Join("\r\n", res.Logs.Select(l => $"{l.Type}|{l.Message}")));

				transportLegs = consolDataContextManagerForTest
					.UniversalShipmentToRead
					.TransportLegCollection
					.OrderBy(t => t.LegOrder)
					.ToArray();

				AssertEquals("DEZZZ was mapped to DEWVN on TransportLeg.PortOfDischarge[LegOrder=0]",
					"DEWVN", transportLegs[0].PortOfDischarge.Code);
				AssertEquals("DEZZZ was mapped to DEWVN on TransportLeg.PortOfLoading[LegOrder=1]",
					"DEWVN", transportLegs[1].PortOfLoading.Code);
				AssertEquals("SEZZZ was mapped to SEGOT on TransportLeg.PortOfDischarge[LegOrder=1]",
					"SEGOT", transportLegs[1].PortOfDischarge.Code);
			}
		}
	}
}
