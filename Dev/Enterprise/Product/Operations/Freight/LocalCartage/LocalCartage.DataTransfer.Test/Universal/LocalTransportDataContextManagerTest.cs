using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	[TestedType(typeof(LocalTransportDataContextManager))]
	sealed class LocalTransportDataContextManagerTest : ShipmentDataContextManagerTestCase<LocalTransportDataContextManager, CommonCartage>
	{
		public void TestReadFromTestFile_TransportBooking_Loose()
		{
			var blueCo = Helper.CreateOrgHeader("AAABBB", "1 Blue Street");
			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Loose.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'BookingPartyDocumentaryAddress':- Matched to 'AAABBB' by code, address '1 Blue Street' (only address).
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), logNoteText);

			var cartage = AssertCartage(Factory, "T00001000", "TB00000005", "DST", "AIR", "LSE", "HUL", "", "", "", 21, "PCE", 3, 4);
			AssertEquals("Should have 2 loose booked moves.", 2, cartage.LooseBookedMoves.Count);
			AssertEquals("Should have 0 containers.", 0, cartage.Containers.Count());
			var looseMoves = cartage.LooseBookedMoves.OrderBy(m => m.EW_DisplayOrder);
			var move1 = looseMoves.First();
			var move2 = looseMoves.Skip(1).First();
			AssertMoveAddresses(move1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertMoveAddresses(move2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertMove(move1, 1, "BOX", 1m, "KG", 1m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			AssertMove(move2, 20, "PLT", 2m, "KG", 3m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			var legs = move1.CartageLegs;
			var legs2 = move2.CartageLegs;
			AssertEquals(1, legs.Count);
			AssertEquals(1, legs2.Count);
			var leg1 = legs[0];
			var leg2 = legs2[0];
			AssertLegAddresses(leg1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertLegAddresses(leg2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertLegDates(leg1, ZDateTime.Empty, ZDateTime.Empty);
			AssertLegDates(leg2, ZDateTime.Empty, ZDateTime.Empty);
			AssertLeg(leg1, 1, "A", "");
			AssertLeg(leg2, 1, "B", "");
		}

		[TestDate(2014, 1, 1)]
		public void TestReadFromTestFile_TransportBooking_Containerised_MultiInstruction_AddressOverriden()
		{
			// create org
			var blueCo = Helper.CreateOrgHeader("5BELEV", "2 BELEV Road");
			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Containerised - AddressOverride.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Warning because some addresses required creating", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonContainer.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
No matching CommonContainer found, creating new CommonContainer.
Populating CommonContainer...
Successfully loaded matching Container Type.
No matching CommonContainer found, creating new CommonContainer.
Populating CommonContainer...
Successfully loaded matching Container Type.
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CYDSYD; Company Name: CYD; Address Code: 1 STUDIO STREET; Address 1: 1 STUDIO STREET; City: BOTANY]'.
Matching 'LocalCartageExporter':- Matched to '5BELEV' by code, address '2 BELEV Road' (only address).
Matching 'LocalCartageExporter':- Matched to '5BELEV' by code, address '2 BELEV Road' (only address).
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: CTOSYDSYD; Company Name: CTO SYD; Address Code: ALEX; Address 1: ALEX; City: ZANDER]'.
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CYDSYD; Company Name: CYD; Address Code: 1 STUDIO STREET; Address 1: 1 STUDIO STREET; City: BOTANY]'.
Matching 'LocalCartageExporter':- Matched to '5BELEV' by code, address '2 BELEV Road' (only address).
Matching 'LocalCartageExporter':- Matched to '5BELEV' by code, address '2 BELEV Road' (only address).
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: CTOSYDSYD; Company Name: CTO SYD; Address Code: ALEX; Address 1: ALEX; City: ZANDER]'.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonContainer.
".Trim(), logNoteText);

			var cartage = AssertCartage(Factory, "T00001000", "TB00000006", "ORG", "SEA", "CNT", "TRL", "", "", "", 0, "PLT", 0, 0);
			AssertEquals("Should have 3 JobDocAddresses Linked", 3, cartage.DocAddresses.Count);
			AssertEquals(1, cartage.DocAddresses.Cast<JobDocAddress>().Count(a => a.DocAddressType == DocAddressType.LocalCartageYard));
			AssertEquals(1, cartage.DocAddresses.Cast<JobDocAddress>().Count(a => a.DocAddressType == DocAddressType.LocalCartageCTO));
			AssertEquals(1, cartage.DocAddresses.Cast<JobDocAddress>().Count(a => a.DocAddressType == DocAddressType.LocalCartageExporter));
		}

		[TestDate(2014, 1, 1)]
		public void TestReadFromTestFile_TransportBooking_Containerised()
		{
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Containerised.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Warning because some addresses required creating", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonContainer.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
No matching CommonContainer found, creating new CommonContainer.
Populating CommonContainer...
Successfully loaded matching Container Type.
No matching CommonContainer found, creating new CommonContainer.
Populating CommonContainer...
Successfully loaded matching Container Type.
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CYDSYD; Company Name: CYD; Address Code: 1 STUDIO STREET; Address 1: 1 STUDIO STREET; City: BOTANY]'.
Matching 'LocalCartageExporter':- Matched to '4BELEV' by code, address 'DELIVERY' by short code.
Matching 'LocalCartageExporter':- Matched to '4BELEV' by code, address 'DELIVERY' by short code.
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: CTOSYDSYD; Company Name: CTO SYD; Address Code: ALEX; Address 1: ALEX; City: ZANDER]'.
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CYDSYD; Company Name: CYD; Address Code: 1 STUDIO STREET; Address 1: 1 STUDIO STREET; City: BOTANY]'.
Matching 'LocalCartageExporter':- Matched to '4BELEV' by code, address 'DELIVERY' by short code.
Matching 'LocalCartageExporter':- Matched to '4BELEV' by code, address 'DELIVERY' by short code.
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: CTOSYDSYD; Company Name: CTO SYD; Address Code: ALEX; Address 1: ALEX; City: ZANDER]'.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonContainer.
".Trim(), logNoteText);

			var cartage = AssertCartage(Factory, "T00001000", "TB00000006", "ORG", "SEA", "CNT", "TRL", "", "", "", 0, "PLT", 0, 0);
			AssertEquals("Should have 0 booked moves.", 0, cartage.LooseBookedMoves.Count);
			AssertEquals("Should have 2 containers.", 2, cartage.Containers.Count());
			AssertEquals("Only have container therefore must be containerised.", CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			var containers = cartage.Containers.OrderBy(c => c.JC_ContainerNum);
			var container1 = AssertContainer(containers, 0, "CONT1234567", "20GP", ZDateTime.Empty, "", new ZDateTime(2014, 1, 8, 15, 0, 0), "SlotRef456");
			var container2 = AssertContainer(containers, 1, "CONT2345678", "40GP", ZDateTime.Empty, "", new ZDateTime(2014, 1, 8, 14, 0, 0), "SlotRef123");
			var move1 = cartage.GetBookedMoves(container1).First();
			var move2 = cartage.GetBookedMoves(container2).First();
			AssertMoveAddresses(move1, "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", "CTO SYD ALEX ZANDER NSW 2015");
			AssertMoveAddresses(move2, "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", "CTO SYD ALEX ZANDER NSW 2015");
			AssertMove(move1, 0, "PLT", 0m, "KG", 0m, "M3", "TRL", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			AssertMove(move2, 0, "PLT", 0m, "KG", 0m, "M3", "TRL", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			var legs = move1.CartageLegs.OrderBy(l => l.JU_DisplayOrder);
			var legs2 = move2.CartageLegs.OrderBy(l => l.JU_DisplayOrder);
			AssertEquals(2, legs.Count());
			AssertEquals(2, legs2.Count());
			var leg1a = legs.First();
			var leg1b = legs.Skip(1).First();
			var leg2a = legs2.First();
			var leg2b = legs2.Skip(1).First();
			AssertLegAddresses(leg1a, "CYD 1 STUDIO STREET BOTANY NSW 2020", "", "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES");
			AssertLegAddresses(leg1b, "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", "", "CTO SYD ALEX ZANDER NSW 2015");
			AssertLegDates(leg1a, new ZDateTime(2014, 1, 2, 14, 0, 0), new ZDateTime(2014, 1, 4, 14, 0, 0));
			AssertLegDates(leg1b, ZDateTime.Empty, new ZDateTime(2014, 1, 7, 14, 0, 0));
			AssertLeg(leg1a, 1, "C", "MLT Note");
			AssertLeg(leg1b, 2, "D", "Dlv Note");
			AssertLegAddresses(leg2a, "CYD 1 STUDIO STREET BOTANY NSW 2020", "", "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES");
			AssertLegAddresses(leg2b, "4B ELEVATOR COMPONENTS LIMITED ADDRESS 1 UNITED STATES", "", "CTO SYD ALEX ZANDER NSW 2015");
			AssertLegDates(leg2a, new ZDateTime(2014, 1, 2, 14, 0, 0), new ZDateTime(2014, 1, 3, 14, 0, 0));
			AssertLegDates(leg2b, new ZDateTime(2014, 1, 5, 14, 0, 0), new ZDateTime(2014, 1, 7, 14, 0, 0));
			AssertLeg(leg2a, 1, "A", "MLT Note");
			AssertLeg(leg2b, 2, "B", "Dlv Note");
		}

		[TestDate(2014, 1, 1)]
		public void TestReadFromTestFile_TransportBooking_Containerised_BookedMoves()
		{
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Containerised - WithShipment.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonContainer, 2 x CommonBookedCtgMove, 2 x CusEntryNumber.
".Trim(), serviceTaskLog.ToString());

			AssertCartage(Factory, "T00001000", "TB00000678", "DST", "SEA", "CNT", "", "", "TM0", "stuff", 20, "PLT", 30000m, 30m);
		}

		public void TestReadFromTestFile_TransportBooking_Mixed()
		{
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Mixed.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Warning because some addresses required creating", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 1 x CommonContainer, 1 x CommonBookedCtgMove.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
No matching CommonContainer found, creating new CommonContainer.
Populating CommonContainer...
Successfully loaded matching Container Type.
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
Warning - Matching 'LocalCartageYard':- No match found for '[Org. Code: CYDSYD; Company Name: CYD; Address Code: 1 STUDIO STREET; Address 1: 1 STUDIO STREET; City: BOTANY]'.
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'PST: 12 SKYRING TCE' by short code.
Warning - Matching 'LocalCartageCTO':- No match found for '[Org. Code: CTOSYDSYD; Company Name: CTO SYD; Address Code: ALEX; Address 1: ALEX; City: ZANDER]'.
Matching 'LocalCartageExporter':- Matched to '4BELEV' by code, address 'Pick Up Address' by short code.
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'PST: 12 SKYRING TCE' by short code.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 1 x CommonContainer, 1 x CommonBookedCtgMove.
".Trim(), logNoteText);

			var cartage = AssertCartage(Factory, "T00001000", "TB00000007", "ORG", "", "MIX", "WUP", "", "", "", 10, "PLT", 1, 2); // standalone TB, no way to know what the connecting freight mode is
			AssertEquals("Should have 1 booked moves.", 1, cartage.LooseBookedMoves.Count);
			AssertEquals("Should have 1 containers.", 1, cartage.Containers.Count());
			// loose
			var looseMoves = cartage.LooseBookedMoves.OrderBy(m => m.EW_DisplayOrder);
			var looseMove = looseMoves.First();
			AssertMoveAddresses(looseMove, "4B ELEVATOR COMPONENTS LIMITED 729 SARINA DRIVE EAST PEORIA ILLINOIS USA, USA ILLINOIS UNITED STATES", "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE, QLD 4005");
			AssertMove(looseMove, 10, "PLT", 1m, "KG", 2m, "M3", "WUP", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			var looseLegs = looseMove.CartageLegs;
			AssertEquals(1, looseLegs.Count);
			var looseLeg = looseLegs[0];
			AssertLegAddresses(looseLeg, "4B ELEVATOR COMPONENTS LIMITED 729 SARINA DRIVE EAST PEORIA ILLINOIS USA, USA ILLINOIS UNITED STATES", "", "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE, QLD 4005");
			AssertLegDates(looseLeg, new ZDateTime(2014, 1, 6, 12, 0, 0), new ZDateTime(2014, 1, 8, 13, 0, 0));
			AssertLeg(looseLeg, 1, "B", "");
			// containerised
			var containers = cartage.Containers.OrderBy(c => c.JC_ContainerNum);
			var container1 = AssertContainer(containers, 0, "CONT1234", "20GP", ZDateTime.Empty, "", new ZDateTime(2014, 1, 9, 12, 30, 0), "SlotRef135");
			var move1 = cartage.GetBookedMoves(container1).First();
			AssertMoveAddresses(move1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE, QLD 4005", "CTO SYD ALEX ZANDER NSW 2015");
			AssertMove(move1, 0, "PLT", 0m, "KG", 0m, "M3", "WUP", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty);
			var legs = move1.CartageLegs.OrderBy(l => l.JU_DisplayOrder);
			AssertEquals("WUP drop mode, so only 1 leg", 1, legs.Count());
			var leg1a = legs.First();
			AssertLegAddresses(leg1a, "CYD 1 STUDIO STREET BOTANY NSW 2020", "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE, QLD 4005", "CTO SYD ALEX ZANDER NSW 2015");
			AssertLegDates(leg1a, new ZDateTime(2014, 1, 7, 12, 0, 0), new ZDateTime(2014, 1, 9, 12, 0, 0)); // CTO Est Dlv ... CFS is ignored cause it's a wait point leg
			AssertLeg(leg1a, 1, "A", "");
		}

		public void TestReadFromTestFile_TransportBooking_MultiJobConsolidation()
		{
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - MultiJobConsol.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertCartage(Factory, "T00001000", "TB00000001", "", "", "MIX", "", "", "", "", 0, "PLT", 0, 0);
			AssertCartage(Factory, "T00001001", "TB00000002", "", "", "MIX", "", "", "", "", 0, "PLT", 0, 0);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001001 with 1 x CommonCartage.
".Trim(), serviceTaskLog.ToString());
		}

		public void TestReadFromTestFile_TransportBooking_MultiJobConsolidation_NoSubShipments()
		{
			var message = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + "<UniversalShipment xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">" + @"<Shipment>
	<DataContext>
		<DataSourceCollection>
		<DataSource>
			<Type>TransportBookingConsolidation</Type>
			<Key>CB00000001</Key>
		</DataSource>
		</DataSourceCollection>

		<DataTargetCollection>
		<DataTarget>
			<Type>LocalTransport</Type>
		</DataTarget>
		</DataTargetCollection>
	</DataContext>
	</Shipment>
</UniversalShipment>";
			var ediMessage = GetQueuedUniversalShipmentMessage(message);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown(() => manager.Process(ediMessage));
		}

		public void TestReadFromTestFile_TransportBooking_Shipment_Loose()
		{
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Shipment - Loose.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'SendersLocalClient':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
Matching 'SendersLocalClient':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), logNoteText);

			var cartage = AssertCartage(Factory, "T00001000", "TB00000008", "DST", "AIR", "LSE", "HUL", "STD", "HB1234", "Shipment Goods", 21, "PCE", 3, 4);
			AssertSchedule(cartage, "QF123", "DEFRA", "AUPER", new ZDateTime(2014, 1, 11, 8, 45, 0), new ZDateTime(2014, 1, 12, 8, 45, 0));
			AssertEquals("Should have 2 loose booked moves.", 2, cartage.LooseBookedMoves.Count);
			AssertEquals("Should have 0 containers.", 0, cartage.Containers.Count());
			var looseMoves = cartage.LooseBookedMoves.OrderBy(m => m.EW_DisplayOrder);
			var move1 = looseMoves.First();
			var move2 = looseMoves.Skip(1).First();
			AssertMoveAddresses(move1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertMoveAddresses(move2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertMove(move1, 20, "PLT", 2m, "KG", 3m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			AssertMove(move2, 1, "BOX", 1m, "KG", 1m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			var legs = move1.CartageLegs;
			var legs2 = move2.CartageLegs;
			AssertEquals(1, legs.Count);
			AssertEquals(1, legs2.Count);
			var leg1 = legs[0];
			var leg2 = legs2[0];
			AssertLegAddresses(leg1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertLegAddresses(leg2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "BACCARAT CRISTALLERIE RUE DES CRISTALLERIES FR 54120 BACCARAT FRANCE");
			AssertLegDates(leg1, ZDateTime.Empty, ZDateTime.Empty);
			AssertLegDates(leg2, ZDateTime.Empty, ZDateTime.Empty);
			AssertLeg(leg1, 1, "A", "");
			AssertLeg(leg2, 1, "B", "");
			// import again, but with a different delivery.
			// should override the first local transport job
			message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - Shipment - Loose - Delivery Changed.xml"));
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Updated Port Transport T00001000 from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), serviceTaskLog.ToString());
			logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Matching 'SendersLocalClient':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
Successfully loaded matching CommonCartage.
Populating CommonCartage...
Matching 'SendersLocalClient':- Matched to 'BACCRI' by code, address 'Pickup and Delivery Addre' by short code.
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'CABINT' by code, address 'PST: 226 COMMONWEALTH STR' by short code.
Matching 'LocalCartageCFS':- Matched to 'AHARTR' by code, address 'Pickup and Delivery Addre' by short code.
Matching 'LocalCartageImporter':- Matched to 'CABINT' by code, address 'PST: 226 COMMONWEALTH STR' by short code.
Updated Port Transport T00001000 from UniversalShipment.
Successfully saved Port Transport T00001000 with 2 x CommonBookedCtgMove.
".Trim(), logNoteText);

			AssertNull("Should not find a second Port Transport Job", Factory.LoadTop1<CommonCartage>(new ZQuery(JobCartageSchema.JJ_ConsignmentID, "T00001001")));
			cartage = AssertCartage(new UniversalObjectFactory(), "T00001000", "TB00000008", "DST", "AIR", "LSE", "HUL", "STD", "HB1234", "Shipment Goods", 21, "PCE", 3, 4);
			AssertSchedule(cartage, "QF123", "DEFRA", "AUPER", new ZDateTime(2014, 1, 15, 8, 45, 0), new ZDateTime(2014, 1, 16, 8, 45, 0));
			AssertEquals("Should have 2 loose booked moves.", 2, cartage.LooseBookedMoves.Count);
			AssertEquals("Should have 0 containers.", 0, cartage.Containers.Count());
			looseMoves = cartage.LooseBookedMoves.OrderBy(m => m.EW_DisplayOrder);
			move1 = looseMoves.First();
			move2 = looseMoves.Skip(1).First();
			AssertMoveAddresses(move1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "CABLE INTERNATIONAL PTY LTD 226 COMMONWEALTH STREET SURRY HILLS QLD 2010");
			AssertMoveAddresses(move2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "CABLE INTERNATIONAL PTY LTD 226 COMMONWEALTH STREET SURRY HILLS QLD 2010");
			AssertMove(move1, 20, "PLT", 2m, "KG", 3m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			AssertMove(move2, 1, "BOX", 1m, "KG", 1m, "M3", "HUL", new ZDateTime(2014, 1, 6, 7, 0, 0), new ZDateTime(2014, 1, 8, 7, 0, 0), ZDateTime.Empty, ZDateTime.Empty);
			legs = move1.CartageLegs;
			legs2 = move2.CartageLegs;
			AssertEquals(1, legs.Count);
			AssertEquals(1, legs2.Count);
			leg1 = legs[0];
			leg2 = legs2[0];
			AssertLegAddresses(leg1, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "CABLE INTERNATIONAL PTY LTD 226 COMMONWEALTH STREET SURRY HILLS QLD 2010");
			AssertLegAddresses(leg2, "A HARTRODT QLD PTY LTD 12 SKYRING TCE TENERIFFE QLD", "", "CABLE INTERNATIONAL PTY LTD 226 COMMONWEALTH STREET SURRY HILLS QLD 2010");
			AssertLegDates(leg1, ZDateTime.Empty, ZDateTime.Empty);
			AssertLegDates(leg2, ZDateTime.Empty, ZDateTime.Empty);
			AssertLeg(leg1, 1, "A", "");
			AssertLeg(leg2, 1, "B", "");
		}

		public void TestReadFromTestFile_TransportBooking_CustomJobTypeWithAddressOverride()
		{
			var newFactory = new BusinessObjectFactory();
			var orlk = newFactory.New<CommonCartageType>();
			orlk.E3_JobType = "ORLK";
			orlk.E3_GE = GlbDepartment.CurrentDepartment.PK;
			orlk.E3_Description = "Orlk";
			var cnr = orlk.CommonCartageOrganisations.AddNew();
			var cne = orlk.CommonCartageOrganisations.AddNew();
			cnr.E5_OrgType = "CNR";
			cne.E5_OrgType = "CNE";
			var looseMove = orlk.LooseBookedMoveTypes.AddNew();
			looseMove.E4_ContainerMode = "LSE";
			looseMove.E4_E5_FromOrg = cnr.PK;
			looseMove.E4_E5_WaitPointOrg = cne.PK;
			var looseLeg = orlk.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cnr.PK;
			looseLeg.E4_E5_ToOrg = cne.PK;
			newFactory.Save();
			var message = GetQueuedUniversalShipmentMessage(resourceRetriever.Value.GetString("Enterprise.Freight.LocalCartage.DataTransfer.Testing.Universal.TestFiles.TransportBooking - JobType Address Override.xml"));
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertEquals("Warning because some addresses required creating", EDIMessageStatusList.Codes.Warning, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 1 x CommonBookedCtgMove, 1 x StmNote.
".Trim(), serviceTaskLog.ToString());
			var logNoteText = message.GetLogNoteText();
			AssertMultilineASCIIEquals("message.GetLogNoteText()", @"
Warning - Line 17: <Shipment>.<OuterPacksPackageType>.<Code> exceeded its maximum length of 3 characters. 11 characters were found.
Warning - Line 24: <Shipment>.<ServiceLevel>.<Code> exceeded its maximum length of 3 characters. 9 characters were found.
Warning - Line 114: <Shipment>.<PackingLineCollection>.<PackingLine>.<PackType>.<Code> exceeded its maximum length of 3 characters. 11 characters were found.
Warning - Matching 'LocalClient':- No match found for '[Org. Code: BENSGB002]'.
No matching CommonCartage found, creating new CommonCartage.
Populating CommonCartage...
Warning - Matching 'LocalClient':- No match found for '[Org. Code: BENSGB002]'.
No matching CommonBookedCtgMove found, creating new CommonBookedCtgMove.
Populating CommonBookedCtgMove...
Warning - Matching 'LocalCartageExporter':- No match found for '[Company Name: BENSON PRINTERS; Address 1: 15 DENBIGH ROAD LIVERPOOL]'.
Warning - Matching 'LocalCartageImporter':- No match found for '[Company Name: OFFICE GOLD LTD; Address 1: UNIT 27 GLENMORE BUSINESS PK TELFORD RD; Address 2: SALISBURY WILTS]'.
No matching StmNote found, creating new StmNote.
Populating StmNote...
Warning - Matching 'LocalClient':- No match found for '[Org. Code: BENSGB002]'.
Warning - Unknown Address Type [LocalClient] found. Job Document Address not imported.
Added Port Transport  from UniversalShipment.
Successfully saved Port Transport T00001000 with 1 x CommonBookedCtgMove, 1 x StmNote.
".Trim(), logNoteText);

			var cartage = Factory.LoadTop1<CommonCartage>(new ZQuery(JobCartageSchema.JJ_ConsignmentID, "T00001000"));
			AssertEquals("Should re-used empty addresses, so should have 2.", 2, cartage.DocAddresses.Count);
			AssertEquals("Should have 1 booked moves.", 1, cartage.LooseBookedMoves.Count);
			var move = cartage.LooseBookedMoves.First();
			AssertMoveAddresses(move, "BENSON PRINTERS 15 DENBIGH ROAD LIVERPOOL L9 1ED", "OFFICE GOLD LTD UNIT 27 GLENMORE BUSINESS PK TELFORD RD SALISBURY WILTS SP2 7GL");
			AssertEquals("Should have 1 leg.", 1, move.CartageLegs.Count);
			var leg = move.CartageLegs.First();
			AssertLegAddresses(leg, "BENSON PRINTERS 15 DENBIGH ROAD LIVERPOOL L9 1ED", "", "OFFICE GOLD LTD UNIT 27 GLENMORE BUSINESS PK TELFORD RD SALISBURY WILTS SP2 7GL");
		}

		CommonCartage AssertCartage(UniversalObjectFactory factory, ZString jobNumber, ZString clientRef, ZString direction, ZString transportMode, ZString containerMode, ZString dropMode, ZString serviceLevel, ZString wayBill, ZString goodsDescription, ZInt packCount, ZString packType, ZDecimal weight, ZDecimal volume)
		{
			var cartage = factory.LoadTop1<CommonCartage>(new ZQuery(JobCartageSchema.JJ_ConsignmentID, jobNumber));
			AssertNotNull("Created Port Transport should exist.", cartage);
			AssertEquals(clientRef, cartage.JJ_OrderReferenceNumber);
			AssertEquals(containerMode, cartage.JJ_ContainerMode);
			AssertEquals(direction, cartage.JJ_Direction);
			AssertEquals(transportMode, cartage.JJ_ShippingTransportMode);
			AssertEquals("", cartage.JJ_E3_NKJobType);
			AssertEquals(dropMode, cartage.JJ_DropMode);
			AssertEquals("", cartage.JJ_QuoteNumber);
			AssertEquals("v2", cartage.JJ_Status);
			AssertEquals(serviceLevel, cartage.JJ_RS_NKServiceLevel);
			AssertEquals(wayBill, cartage.JJ_WaybillNumber);
			AssertEquals(goodsDescription, cartage.JJ_GoodsDescription);
			AssertEquals(GlbBranch.CurrentBranch.PK, cartage.JJ_GB);
			AssertEquals(ZGuid.Empty, cartage.JJ_ParentID);
			AssertEquals("", cartage.JJ_ParentTableCode);
			AssertEquals(ZDateTime.Empty, cartage.JJ_A_JCL);
			AssertEquals(packCount, cartage.JJ_OuterPacks);
			AssertEquals(packType, cartage.JJ_F3_NKPackType);
			AssertEquals(volume, cartage.JJ_Volume);
			AssertEquals("M3", cartage.JJ_VolumeUQ);
			AssertEquals(weight, cartage.JJ_Weight);
			AssertEquals("KG", cartage.JJ_WeightUQ);
			return cartage;
		}

		void AssertSchedule(CommonCartage cartage, ZString flightNumber, ZString load, ZString disch, ZDateTime etd, ZDateTime eta)
		{
			AssertEquals(flightNumber, cartage.VoyageFlight);
			AssertEquals(load, cartage.PortOfLoading);
			AssertEquals(disch, cartage.PortOfDischarge);
			AssertEquals(etd, cartage.E_DEP);
			AssertEquals(eta, cartage.E_ARV);
		}

		CommonContainer AssertContainer(IEnumerable<CommonContainer> containers, int index, ZString containerNo, ZString containerType, ZDateTime dlvSlotDate, ZString dlvSlotRef, ZDateTime picSlotDate, ZString picSlotRef)
		{
			var container = containers.ElementAtOrDefault(index);
			AssertNotNull("Could not find a container at index" + index, container);
			AssertEquals(containerNo, container.JC_ContainerNum);
			AssertEquals(containerType, container.Container.RC_Code);
			AssertEquals(dlvSlotDate, container.JC_ArrivalSlotDateTime);
			AssertEquals(dlvSlotRef, container.JC_ArrivalSlotReference);
			AssertEquals(picSlotDate, container.JC_DepartureSlotDateTime);
			AssertEquals(picSlotRef, container.JC_DepartureSlotReference);
			return container;
		}

		void AssertMove(CommonBookedCtgMove move, int packCount, ZString packType, ZDecimal weight, ZString weighUQ, ZDecimal volume, ZString volumeUQ, ZString dropMode, ZDateTime reqDlvFrom, ZDateTime reqDlvTo, ZDateTime reqPicFrom, ZDateTime reqPicTo)
		{
			AssertEquals(packCount, move.EW_BookedPackCount);
			AssertEquals(packType, move.EW_F3_NKPackType);
			AssertEquals(weight, move.EW_BookedWeight);
			AssertEquals(weighUQ, move.EW_WeightUQ);
			AssertEquals(volume, move.EW_BookedVolume);
			AssertEquals(volumeUQ, move.EW_VolumeUQ);
			AssertEquals("M", move.EW_DimUnit);
			AssertEquals(dropMode, move.EW_DropMode);
			AssertEquals(reqDlvFrom, move.EW_RequestedDeliveryTimeStart);
			AssertEquals(reqDlvTo, move.EW_RequestedDeliveryTimeEnd);
			AssertEquals(reqPicFrom, move.EW_RequestedPickupTimeStart);
			AssertEquals(reqPicTo, move.EW_RequestedPickupTimeEnd);
		}

		void AssertMoveAddresses(CommonBookedCtgMove move, ZString rateFromAddress, ZString rateToAddress)
		{
			AssertEquals(rateFromAddress, move.PickupFromDocAddress.AddressAsASingleLine); // shouldn't be cyd!!
			AssertEquals(rateToAddress, move.WaitPointDocAddress.AddressAsASingleLine);
			AssertNull(move.DeliverToDocAddress);
		}

		void AssertLegAddresses(CommonCartageLeg leg, ZString pickupAddress, ZString waitPointAddress, ZString deliveryAddress)
		{
			AssertEquals(pickupAddress, leg.PickupFromDocAddress.AddressAsASingleLine);
			if (waitPointAddress.IsEmpty)
			{
				AssertNull(waitPointAddress, leg.WaitPointDocAddress);
			}
			else
			{
				AssertEquals(waitPointAddress, leg.WaitPointDocAddress.AddressAsASingleLine);
			}

			AssertEquals(deliveryAddress, leg.DeliverToDocAddress.AddressAsASingleLine);
		}

		void AssertLegDates(CommonCartageLeg leg, ZDateTime estPic, ZDateTime estDlv)
		{
			AssertEquals(estPic, leg.JU_PlannedPickupTime);
			AssertEquals(estDlv, leg.JU_EstimatedDeliveryTime);
			AssertEquals(ZDateTime.Empty, leg.JU_PlannedPickupTimeEnd);
			AssertEquals(ZDateTime.Empty, leg.JU_EstimatedDeliveryTimeEnd);
			AssertEquals(ZDateTime.Empty, leg.JU_PickupTimeIn);
			AssertEquals(ZDateTime.Empty, leg.JU_PickupTimeOut);
			AssertEquals(ZDateTime.Empty, leg.JU_WaitPointTimeIn);
			AssertEquals(ZDateTime.Empty, leg.JU_WaitPointTimeOut);
			AssertEquals(ZDateTime.Empty, leg.JU_DeliverTimeIn);
			AssertEquals(ZDateTime.Empty, leg.JU_DeliverTimeOut);
			AssertEquals(ZDateTime.Empty, leg.JU_CartagePickupDemurrage);
			AssertEquals(ZDateTime.Empty, leg.JU_CartageWaitPointDemurrage);
			AssertEquals(ZDateTime.Empty, leg.JU_CartageDeliveryDemurrage);
		}

		void AssertLeg(CommonCartageLeg leg, ZInt order, ZString suffix, ZString note)
		{
			AssertEquals(order, leg.JU_DisplayOrder);
			AssertEquals(suffix, leg.JU_SplitDeliverySuffix);
			AssertEquals(note, leg.JU_LegNotes);
			AssertEquals(false, leg.JU_IsEmptyContainer);
			AssertEquals("", leg.JU_AdditionalService);
			AssertEquals("", leg.JU_DeliverySignedFor);
			AssertEquals("", leg.JU_GatePassNumber);
			AssertEquals(0, leg.JU_RunSheetSequence);
			AssertEquals(ZGuid.Empty, leg.JU_EY_RunSheet);
			AssertEquals(ZGuid.Empty, leg.JU_GC);
			AssertEquals("", leg.JU_MessageStatus);
			AssertEquals("", leg.JU_Status);
			AssertEquals(ZGuid.Empty, leg.JU_RQ_ExtraEquip1);
			AssertEquals(ZGuid.Empty, leg.JU_RQ_ExtraEquip2);
			AssertEquals(ZGuid.Empty, leg.JU_RQ_Trailer);
		}

		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LocalTransport, GetManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			AssertEquals("T00001234", GetManager().DataContextKey);
		}

		public void TestManageShipments()
		{
			AssertEquals(true, GetManager().ManagesShipments());
		}

		public void TestManageEvents()
		{
			AssertEquals(true, GetManager().ManagesEvents);
		}

		public void TestDefaultOutputDirectory()
		{
			AssertNull(GetManager().DefaultOutputDirectory);
		}

		public void TestEventContextValues()
		{
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
ConsignmentNoteNumber - T00001234
OrderNumber - Order123
WaybillNumber - W00001234
QuoteNumber - Q00001234
TransportBookingJobID - TB00006543
Other - hello
TRA - transportIDDD
			".Trim(), GetManager().EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment Version=""0.1"" xmlns=""http://www.cargowise.com/Schemas/Universal"">
	<Shipment Action=""MERGE"">
	<DataContext>
		<DataSourceCollection>
		<DataSource>
			<Type>LocalTransport</Type>
		</DataSource>
		</DataSourceCollection>

		<Company>
		<Code>EDI</Code>
		<Name>Eagle Datamation International</Name>
		</Company>
		<EnterpriseID>EDI</EnterpriseID>
		<EventType>
		<Code>ATH</Code>
		<Description>Action Authorised</Description>
		</EventType>
		<ServerID>DAT</ServerID>
		<TriggerDate>2011-03-27T11:13:00</TriggerDate>
		<TriggerDescription>Test Trigger</TriggerDescription>
		<TriggerType>Trigger</TriggerType>
	</DataContext>
	</Shipment>
</UniversalShipment>
";
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return new RecipientRoleType[] { RecipientRoleType.DCA, RecipientRoleType.PCA };
			}
		}

		protected override string GetExpectedDataSourceForUniversalShipmentTopLevelDataObject(IDataContextManager manager)
		{
			return "LocalTransport [T00001234]";
		}

		protected override CommonCartage GetNewBusinessObjectForTesting()
		{
			var result = base.GetNewBusinessObjectForTesting();
			result.JJ_ConsignmentID = "T00001234";
			result.JJ_OrderReferenceNumber = "Order123";
			result.JJ_WaybillNumber = "W00001234";
			result.JJ_QuoteNumber = "Q00001234";
			var etbReference = result.AdditionalReferenceNumbers.AddNew();
			etbReference.CE_EntryType = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			etbReference.CE_EntryNum = "TB00006543";
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(result.AdditionalReferenceNumbers.AddNew(), "OTH", "hello");
			LocalTransportDataObjectWriterTest.SetupAdditionalReference(result.AdditionalReferenceNumbers.AddNew(), "TRA", "transportIDDD");
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		IEventDataContextManager GetManager()
		{
			return GetNewBusinessObjectForTesting().GetUniversalDataContextManager() as IEventDataContextManager;
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory.BOFactory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
