using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class BookingConfirmationCodeMappingTest : ConsolIncomingMessageCodeMappingTest
	{
		public void TestUnlocoCodeMapping()
		{
			var bookingConfirmation = BookingMessagesForTest.CreateBookingConfirmation();
			var consol = BookingMessagesForTest.CreateConsolMatchingDataTarget(Factory.BOFactory, bookingConfirmation);
			AssertNotNull("consol with carrier has been created", consol.ShippingLine);

			var carrier = consol.ShippingLine;

			CreateOCMUnlocoMappings(carrier, new Dictionary<string, string>
			{
				["SEZZZ"] = "SEGOT",
				["DEZZZ"] = "DEWVN"
			});

			Factory.SaveForTesting();

			bookingConfirmation.PortOfDischarge.Code = "SEZZZ";

			var transportLegs = bookingConfirmation
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
				var res = CreateAndProcessUniversalShipment(bookingConfirmation);

				AssertMultilineASCIIEquals("import log",
@"Information|Universal Shipment data was linked to Consol C09140756.
Information|Successfully saved, but nothing was reported as being updated.",
					string.Join("\r\n", res.Logs.Select(l => $"{l.Type}|{l.Message}")));

				AssertEquals("SEZZZ was mapped to SEGOT on top level Shipment",
					"SEGOT", consolDataContextManagerForTest.UniversalShipmentToRead.PortOfDischarge.Code);

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
