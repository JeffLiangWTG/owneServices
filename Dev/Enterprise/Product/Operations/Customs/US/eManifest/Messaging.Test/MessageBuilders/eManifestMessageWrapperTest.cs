using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class eManifestMessageWrapperTest : TestCaseWithFactory
	{
		public void TestPopulateCompleteManifestOriginal()
		{
			var data = GetData(Factory, MessageTypes.Codes.eManifest, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: true, headerOnly: false, addCustomsBroker: true);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP000001+22
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
FTX+INS+++NAMEHAZMAT SHIPMENT INSURANCE:PLCY23494564:AMNT2000000:YEAR2011
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
CNI+1+:23
DOC+630:::SI+684864684
DOC+950:::63+1234569876543211
DOC+929:::MO+98765413265478
RFF+AAM:LOCK1234654894
CNT+58:6
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
GEI+7+134
TDT+11
DTM+133:20110831:102
RFF+AWM
TSR+41
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CB++0901SV9AA+CUSTOMS BROKER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG1321:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG4561:NAMEBIG BOSS:TELE31264846516
MEA+AAI++K:2650
MOA+40:2500
SGP+8987964:109
DGS+++1321
DGS+++4561
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3
CST++6601100000:122+6602001000:122+6602001001:122
LOC+27+FR:162
UNT+79+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCompleteManifestOriginalWithoutEquipment()
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_JobReference = "MAN0001001";
			trip.BH_VoyageNumber = "TRIP001001";
			trip.BH_ETA = new ZDateTime(2011, 09, 23, 11, 25, 0);
			trip.BH_PortUnladingDCode = "2704";
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			var equipment = trip.Equipment.AddNew();
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("T1", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			equipment.BJ_RC_RoadContainerType = refContainer.PK;
			var data = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP001001+5
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
NAD+CA+LOCK:172
EQD+T1
UNT+8+<<MSGNO PLACEHOLDER>>
";
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCompleteManifestChangeTestingSaveAndLoad()
		{
			var data = GetData(Factory, MessageTypes.Codes.eManifest, MessageSubTypes.Change, testSaveAndLoad: true, isFinalized: true);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP000001+5
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+RFA:03
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
FTX+INS+++NAMEHAZMAT SHIPMENT INSURANCE:PLCY23494564:AMNT2000000:YEAR2011
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
CNI+1+:23
DOC+630:::SI+684864684
DOC+950:::63+1234569876543211
DOC+929:::MO+98765413265478
RFF+AAM:LOCK1234654894
CNT+58:6
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
GEI+7+134
TDT+11
DTM+133:20110831:102
RFF+AWM
TSR+41
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG1321:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG4561:NAMEBIG BOSS:TELE31264846516
MEA+AAI++K:2650
MOA+40:2500
SGP+8987964:109
DGS+++1321
DGS+++4561
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3
CST++6601100000:122+6602001000:122+6602001001:122
UNT+76+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCompleteManifestReturnedToPreliminaryChange()
		{
			var data = GetData(Factory, MessageTypes.Codes.eManifest, MessageSubTypes.Change, testSaveAndLoad: false, isFinalized: false);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP000001+5
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
FTX+INS+++NAMEHAZMAT SHIPMENT INSURANCE:PLCY23494564:AMNT2000000:YEAR2011
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
CNI+1+:23
DOC+630:::SI+684864684
DOC+950:::63+1234569876543211
DOC+929:::MO+98765413265478
RFF+AAM:LOCK1234654894
CNT+58:6
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
GEI+7+134
TDT+11
DTM+133:20110831:102
RFF+AWM
TSR+41
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG1321:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG4561:NAMEBIG BOSS:TELE31264846516
MEA+AAI++K:2650
MOA+40:2500
SGP+8987964:109
DGS+++1321
DGS+++4561
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3
CST++6601100000:122+6602001000:122+6602001001:122
LOC+27+FR:162
UNT+76+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCompleteManifestSplitShipment()
		{
			var data = GetSplitShipmentsData(MessageTypes.Codes.eManifest);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTEST012301+22
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
FTX+INS+++NAMEHAZMAT SHIPMENT INSURANCE:PLCY23494564:AMNT2000000:YEAR2011
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
CNI+1+:24
RFF+AAM:LOCK1234654894
CNT+58:6
GEI+7+135
TDT+11
RFF+RFA:03
NAD+OS+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
NAD+CN+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
GID+1
FTX+AAA+++DUMMY
CNI+2+:24
RFF+AAM:LOCK1234654895
CNT+58:7
GEI+7+135
TDT+11
RFF+RFA:03
NAD+OS+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
NAD+CN+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
GID+1
FTX+AAA+++DUMMY
UNT+41+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCrewPassengersOriginal()
		{
			var data = GetData(Factory, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: false);
			var builder = new CrewAndPassengersMessageBuilder(data, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+PAXLST:D:03B:UN
BGM+10:::STANDARD+LOCKTRIP000001+2
RFF+ABO:CRW<<MSGNO PLACEHOLDER>>
TDT+11++03++LOCK:172
DTM+132:20110923:102
NAD+VW+0000041153:109++AOMAD:CHRIS::::1+11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
ATT+2++M
DTM+329:19350919:102
EMP+4+++1:::8456
NAT+2+US::5
DOC+5K+P100971204141
LOC+91+VA:163
LOC+91+US:162
DOC+39+15504141
LOC+91+US:162
DOC+OTD+13465
NAD+FL+1234:8++TURNER:BILL:BOOTSTRAP:::1
ATT+2++M
DTM+329:19701230:102
NAT+2+US::5
DOC+39+EA12343
LOC+91+US:162
UNT+23+<<MSGNO PLACEHOLDER>>";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.CrewAndPassenger, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateCrewPassengersChangeTestingSaveAndLoad()
		{
			var data = GetData(Factory, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Change, testSaveAndLoad: true, isFinalized: true);
			var builder = new CrewAndPassengersMessageBuilder(data, MessageTypes.Codes.CrewAndPassenger, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+PAXLST:D:03B:UN
BGM+10:::STANDARD+LOCKTRIP000001+4
RFF+ABO:CRW<<MSGNO PLACEHOLDER>>
RFF+RFA:03
TDT+11++03++LOCK:172
DTM+132:20110923:102
NAD+VW+0000041153:109++AOMAD:CHRIS::::1+11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
ATT+2++M
DTM+329:19350919:102
EMP+4+++1:::8456
NAT+2+US::5
DOC+5K+P100971204141
LOC+91+VA:163
LOC+91+US:162
DOC+39+15504141
LOC+91+US:162
DOC+OTD+13465
NAD+FL+1234:8++TURNER:BILL:BOOTSTRAP:::1
ATT+2++M
DTM+329:19701230:102
NAT+2+US::5
DOC+39+EA12343
LOC+91+US:162
UNT+24+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.CrewAndPassenger, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestUnassociatedShipmentsOriginal()
		{
			var data = GetData(Factory, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: false);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+87:::STANDARD+SYSTEM+2
RFF+ABO:SHP<<MSGNO PLACEHOLDER>>
CNI+1+:23
DOC+630:::SI+684864684
DOC+950:::63+1234569876543211
DOC+929:::MO+98765413265478
RFF+AAM:LOCK1234654894
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
GEI+7+134
TDT+11
DTM+133:20110831:102
RFF+AWM
TSR+41
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG1321:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG4561:NAMEBIG BOSS:TELE31264846516
MEA+AAI++K:2650
MOA+40:2500
SGP+8987964:109
DGS+++1321
DGS+++4561
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3
CST++6601100000:122+6602001000:122+6602001001:122
LOC+27+FR:162
UNT+42+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.UnassociatedShipments, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestUnassociatedShipmentsChangeTestingSaveAndLoad()
		{
			var data = GetData(Factory, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Change, testSaveAndLoad: true, isFinalized: false);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+87:::STANDARD+LOCKTRIP000001+4
DTM+132:201109231125:203
RFF+ABO:SHP<<MSGNO PLACEHOLDER>>
CNI+1+:23
DOC+630:::SI+684864684
DOC+950:::63+1234569876543211
DOC+929:::MO+98765413265478
RFF+AAM:LOCK1234654894
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
GEI+7+134
TDT+11
DTM+133:20110831:102
RFF+AWM
TSR+41
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG1321:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG4561:NAMEBIG BOSS:TELE31264846516
MEA+AAI++K:2650
MOA+40:2500
SGP+8987964:109
DGS+++1321
DGS+++4561
PCI++MARKS LINE 1:MARKS LINE 2:MARKS LINE 3
CST++6601100000:122+6602001000:122+6602001001:122
UNT+42+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.UnassociatedShipments, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestAllTypesOfUnassociatedShipments()
		{
			var data = GetAllTypesOfShipmentsData(MessageTypes.Codes.UnassociatedShipments);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+87:::STANDARD+SYSTEM+2
RFF+ABO:SHP<<MSGNO PLACEHOLDER>>
CNI+1+:23
RFF+AAM:LOCKXXXT1410TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+2+:23
RFF+AAM:LOCKXXXT1411TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+3+:23
DOC+929:::83
RFF+AAM:LOCKXXXT1413TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+4+:23
DOC+929:::18
RFF+AAM:LOCKXXXT1414TEST
LOC+9+01535:78
GEI+7+136
TDT+11
DTM+133:20120427:102
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+5+:23
DOC+929:::84
RFF+AAM:LOCKXXXT1416TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+6+:23
DOC+929:::85
RFF+AAM:LOCKXXXT1417TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CNI+7+:23
DOC+929:::13
RFF+AAM:LOCKXXXT1418TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
MOA+40:200
LOC+27+CA:162
CNI+8+:23
DOC+929:::35
RFF+AAM:LOCKXXXT1419TEST
LOC+9+01535:78
GEI+7+134
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
MOA+40:200
LOC+27+CA:162
CNI+9+:23
RFF+AAM:LOCKXXXT1420TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
CST++TS00TM00A0AP00:117+TS00TM00A0AP01:117
CNI+10+:23
DOC+950:::63
RFF+AAM:LOCKXXXT1422TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
MOA+40:200
CNI+11+:23
DOC+950:::62
RFF+AAM:LOCKXXXT1423TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
MOA+40:200
CNI+12+:23
DOC+950:::63
RFF+AAM:LOCKXXXT1424TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++SHIPPER+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
NAD+CN+++CONSIGNEE+12 PARTY STREET+CHICAGO+IL:163+2009+US
CTA+IC
COM+31264846516:TE
GID+1
PAC+100++BOX
FTX+AAA+++FRENCH DARK CHOCOLATE
MEA+AAI++K:100
MOA+40:200
UNT+191+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.UnassociatedShipments, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPreliminaryTripDetailsOriginal()
		{
			var data = GetData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: false);
			var builder = new TripReportMessageBuilder(data, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKTRIP000001+2
DTM+132:201109231125:203
RFF+ABO:PTR<<MSGNO PLACEHOLDER>>
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700
RFF+AAM:LOCK1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
UNT+42+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.PreliminaryTrip, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPreliminaryTripDetailsOriginalHeaderOnly()
		{
			var data = GetData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: false, headerOnly: true);
			var builder = new TripReportMessageBuilder(data, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKTRIP000001+2
DTM+132:201109231125:203
RFF+ABO:PTR<<MSGNO PLACEHOLDER>>
LOC+60+2704:77
NAD+CA+LOCK:172
TDT+11++03+:::TR++I++:146::DUMMY
UNT+8+<<MSGNO PLACEHOLDER>>
";
			AssertEquals("EM_MessageType", MessageTypes.Codes.PreliminaryTrip, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPreliminaryTripDetailsChange()
		{
			var data = GetData(Factory, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Change, testSaveAndLoad: false, isFinalized: false);
			var builder = new TripReportMessageBuilder(data, MessageTypes.Codes.PreliminaryTrip, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKTRIP000001+4
DTM+132:201109231125:203
RFF+ABO:PTR<<MSGNO PLACEHOLDER>>
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700
RFF+AAM:LOCK1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
UNT+42+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.PreliminaryTrip, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestCompleteTripDetailsChangeTestingSaveAndLoad()
		{
			var data = GetData(Factory, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Change, testSaveAndLoad: true, isFinalized: true);
			var builder = new TripReportMessageBuilder(data, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Change);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+LOCKTRIP000001+4
DTM+132:201109231125:203
RFF+ABO:CTR<<MSGNO PLACEHOLDER>>
RFF+RFA:03
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
LOC+60+2704:77
DOC+700
RFF+AAM:LOCK1234654894
QTY+11:6
TAX+10
MOA+67:2000000
FII+SY++::::::HAZMAT SHIPMENT INSURANCE
RFF+ICO:23494564
DTM+429:2011:602
NAD+CA+LOCK:172
NAD+VW+0000041153:109+++11107 SUNSET HILLS ROAD+RESTON+VA:163+20190
NAD+FL+1234:8
TDT+11++03+:::PU++I++:109::64894654
TDT+11++03+:::PU++I++:172::46765464
TDT+11++03+:::PU++I++:8::789543218
TDT+11++03+:::PU++I++:146::1234567890
TDT+11++03+:::PU++I++:274::46456487
TDT+11++03+:::PU++I++:215::BBDD11:US
LOC+89+IL:163
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
UNT+45+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.CompleteTrip, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateEquipmentDetailsWithoutEquipmentId()
		{
			var data = GetEquipmentData(MessageTypes.Codes.eManifest, true);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP112233+22
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:32132
RFF+SN:31321
RFF+IIT:EC
RFF+IIT:EI
NAD+CA+LOCK:172
TDT+11++03+:::T1++I++:109::5498168
TDT+11++03+:::T1++I++:215::BA12YY:US
LOC+89+IL:163
EQD+CN
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
UNT+22+<<MSGNO PLACEHOLDER>>";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestPopulateEquipmentDetails()
		{
			var data = GetEquipmentData(MessageTypes.Codes.eManifest, false);
			var builder = new CompleteManifestMessageBuilder(data, MessageTypes.Codes.eManifest, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			#region Expected Message Text
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKTRIP112233+22
DTM+132:201109231125:203
LOC+60+2704:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
NAD+CA+LOCK:172
EQD+CN+8987964:109
SEL+56484
SEL+64845
RFF+IIT:MC
RFF+IIT:MI
RFF+ABZ:68465464
LOC+89+AL:163
LOC+89+US:162
EQD+T1+5498168:109
SEL+32132
SEL+31321
RFF+IIT:EC
RFF+IIT:EI
RFF+ABZ:BA12YY
LOC+89+IL:163
LOC+89+US:162
UNT+23+<<MSGNO PLACEHOLDER>>";
			#endregion
			AssertEquals("EM_MessageType", MessageTypes.Codes.eManifest, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
		}

		public void TestConveyanceEquipmentID()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var conveyance = Factory.NewWithValidTestData<Equipment>();
			trip.Equipment.DeleteAll();
			trip.Equipment.Add(conveyance);
			conveyance.BJ_IsConveyance = true;
			conveyance.BJ_VIN = "12345678901234567";
			var wrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			AssertEquals("12345678901234567", wrapper.Conveyance.EquipmentId);
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_VIN = "10203040506070809000";
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertEquals("10203040506070809", wrapper.Conveyance.EquipmentId);
		}

		public void TestConveyanceEquipmentType()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var conveyance = Factory.NewWithValidTestData<Equipment>();
			trip.Equipment.DeleteAll();
			trip.Equipment.Add(conveyance);
			conveyance.BJ_IsConveyance = true;
			var wrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			AssertEquals("TR", wrapper.Conveyance.EquipmentType);
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.PickupTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			conveyance.BJ_RC_RoadContainerType = refContainer.PK;
			wrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			AssertEquals("PU", wrapper.Conveyance.EquipmentType);
		}

		public void TestEmptyJobDoesNotBlowUpAndConditionalFieldsAreNotPopulated()
		{
			var trip = Factory.New<Trip>();
			trip.BH_ETA = ZDateTime.Empty;
			AssertNoExceptionsThrown(trip);
			trip.CrewMembers.AddNew();
			var equipment = trip.Equipment.AddNew();
			var shipment = trip.Shipments.AddNew();
			AssertNoExceptionsThrown(trip);
			equipment.SealNumbers.AddNew();
			shipment.Parties.AddNew();
			var commodity = shipment.Commodities.AddNew();
			AssertNoExceptionsThrown(trip);
			commodity.HarmonizedNumbers.AddNew();
			commodity.VehicleIdentificationNumbers.AddNew();
			commodity.C4Codes.AddNew();
			commodity.UNDGs.AddNew();
			#region Expected Result
			const string expectedResult = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD++22
DTM+132::203
LOC+60+:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
NAD+CA+:172
EQD
SEL
CNI+1+:23
RFF+AAM
LOC+9+:78
GEI+7+135
NAD+OS
NAD+CN
NAD
GID+1
PAC+0
FTX+PRD
FTX+AAC+++UNDG:NAME:TELE
MEA+AAI++K:0
DGS
CST
UNT+23+<<MSGNO PLACEHOLDER>>
";
			#endregion
			AssertNoExceptionsThrown(trip, expectedResult);
		}

		internal static Trip GetTrip(BusinessObjectFactory factory, string messageType, MessageSubTypes action, bool testSaveAndLoad, bool isFinalized, bool headerOnly = false, bool addCustomsBroker = false)
		{
			var trip = factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_JobReference = "MAN0000001";
			trip.BH_VoyageNumber = "TRIP000001";
			trip.BH_ETA = new ZDateTime(2011, 09, 23, 11, 25, 0);
			trip.BH_PortUnladingDCode = "2704";
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			if (!headerOnly)
			{
				AddCrewMembers(trip);
				AddConveyance(factory, trip);
				AddEquipment(factory, trip);
				AddShipment(factory, trip, addCustomsBroker);
			}

			if (testSaveAndLoad)
			{
				factory.Save(); //Test that everything saves and loads fine
				trip = new BusinessObjectFactory().Load<Trip>(trip.PK);
			}

			if (messageType == MessageTypes.Codes.PreliminaryTrip || messageType == MessageTypes.Codes.CompleteTrip)
			{
				if (action == MessageSubTypes.Create)
				{
					if (!headerOnly)
					{
						trip.ShipmentsActions[0].B0_ActionCode = MessageActionCodes.Codes.Link;
					}
				}
				else if (action == MessageSubTypes.Change)
				{
					if (isFinalized)
					{
						trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
					}
					else
					{
						trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
					}

					if (!headerOnly)
					{
						trip.ShipmentsActions[0].B0_ActionCode = MessageActionCodes.Codes.DeLink;
					}
				}
			}
			else if (action == MessageSubTypes.Change)
			{
				if (isFinalized || messageType == MessageTypes.Codes.UnassociatedShipments)
				{
					trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
					if (!headerOnly)
					{
						trip.ShipmentsActions[0].B0_ActionCode = MessageActionCodes.Codes.Change;
						trip.ShipmentsActions[0].B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01;
					}
				}
				else
				{
					trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
				}
			}

			return trip;
		}

		ICompleteManifest GetData(BusinessObjectFactory factory, string messageType, MessageSubTypes action, bool testSaveAndLoad, bool isFinalized, bool headerOnly = false, bool addCustomsBroker = false)
		{
			var trip = GetTrip(factory, messageType, action, testSaveAndLoad, isFinalized, headerOnly, addCustomsBroker);
			var data = new eManifestMessageWrapper(trip, messageType);
			if (messageType == MessageTypes.Codes.PreliminaryTrip || messageType == MessageTypes.Codes.CompleteTrip)
			{
				if (action == MessageSubTypes.Change)
				{
					if (isFinalized)
					{
						data.AmendmentReasonCode = AmendmentReasonCodes.Codes.C03;
					}
				}
			}
			else if (action == MessageSubTypes.Change)
			{
				if (isFinalized || messageType == MessageTypes.Codes.UnassociatedShipments)
				{
					data.AmendmentReasonCode = AmendmentReasonCodes.Codes.C03;
				}
			}

			ForceCollectionOrder(trip);
			return data;
		}

		ICompleteManifest GetSplitShipmentsData(string messageType)
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_JobReference = "MAN0000001";
			trip.BH_VoyageNumber = "TEST012301";
			trip.BH_ETA = new ZDateTime(2011, 09, 23, 11, 25, 0);
			trip.BH_PortUnladingDCode = "2704";
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			AddCrewMembers(trip);
			AddConveyance(Factory, trip);
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.B0_MasterBillNumber = "1234654894";
			shipment.B0_BoardedQuantity = 6;
			shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.B0_MasterBillNumber = "1234654895";
			shipment.B0_BoardedQuantity = 7;
			return new eManifestMessageWrapper(trip, messageType);
		}

		ICompleteManifest GetAllTypesOfShipmentsData(string messageType)
		{
			var trip = Factory.New<Trip>();
			AddShipment(Factory, trip, ShipmentTypes.Codes.PAPS, "LOCKXXXT1410TEST");
			AddShipment(Factory, trip, ShipmentTypes.Codes.PAPS, "LOCKXXXT1411TEST");
			AddShipment(Factory, trip, ShipmentTypes.Codes.FreeOfDuty, "LOCKXXXT1413TEST");
			var shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.GoodsAstray, "LOCKXXXT1414TEST");
			shipment.B0_DateOfExport = new ZDateTime(2012, 04, 27);
			shipment.B0_WasOutOfUSFor45DaysOrLess = true;
			AddShipment(Factory, trip, ShipmentTypes.Codes.ReturnedGoods, "LOCKXXXT1416TEST");
			AddShipment(Factory, trip, ShipmentTypes.Codes.UnaccompaniedArticles, "LOCKXXXT1417TEST");
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.LowValue, "LOCKXXXT1418TEST");
			var commodity = shipment.Commodities[0];
			commodity.BY_MonetaryValue = 200;
			commodity.BY_RN_NKCountryOfOrigin = Constants.CountryCodes.Canada;
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.LowValue, "LOCKXXXT1419TEST");
			shipment.B0_IsFDAFreight = true;
			commodity = shipment.Commodities[0];
			commodity.BY_MonetaryValue = 200;
			commodity.BY_RN_NKCountryOfOrigin = Constants.CountryCodes.Canada;
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.BRASS, "LOCKXXXT1420TEST");
			commodity = shipment.Commodities[0];
			commodity.BY_C4Codes = "TS00TM00A0AP00, TS00TM00A0AP01";
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.Inbond, "LOCKXXXT1422TEST");
			shipment.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			commodity = shipment.Commodities[0];
			commodity.BY_MonetaryValue = 200;
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.Inbond, "LOCKXXXT1423TEST");
			shipment.InBond.BM_InBondEntryType = InbondTypes.Codes.TransportationAndExportation;
			commodity = shipment.Commodities[0];
			commodity.BY_MonetaryValue = 200;
			shipment = AddShipment(Factory, trip, ShipmentTypes.Codes.Inbond, "LOCKXXXT1424TEST");
			shipment.InBond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			commodity = shipment.Commodities[0];
			commodity.BY_MonetaryValue = 200;
			return new eManifestMessageWrapper(trip, messageType);
		}

		ICompleteManifest GetEquipmentData(string messageType, bool detailsOnly)
		{
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			trip.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip.BH_JobReference = "MAN0009999";
			trip.BH_VoyageNumber = "TRIP112233";
			trip.BH_ETA = new ZDateTime(2011, 09, 23, 11, 25, 0);
			trip.BH_PortUnladingDCode = "2704";
			trip.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			if (detailsOnly)
			{
				AddEquipmentDetailsOnly(trip);
			}
			else
			{
				AddEquipment(Factory, trip);
			}

			ForceCollectionOrder(trip);
			return new eManifestMessageWrapper(trip, messageType);
		}

		static void ForceCollectionOrder(BusinessObject b)
		{
			var q = new Queue<BusinessObject>(new[] { b });
			var visited = new HashSet<BusinessObject>(new[] { b });
			while (q.Count > 0)
			{
				var next = q.Dequeue();
				var type = next.GetType();
				foreach (var p in type.GetProperties())
				{
					if (typeof(IActiveBusinessObjectCollection).IsAssignableFrom(p.PropertyType))
					{
						var collection = (IActiveBusinessObjectCollection)p.GetValue(next);
						if (collection.SortComparer == null)
						{
							collection.ApplySort(new InstantiationTimeComparer());
						}

						foreach (BusinessObject bizo in collection)
						{
							if (visited.Add(bizo))
							{
								q.Enqueue(bizo);
							}
						}
					}
				}
			}
		}

		static Shipment AddShipment(BusinessObjectFactory factory, Trip trip, string type, string scn)
		{
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = type;
			shipment.B0_MasterBillNumber = scn;
			shipment.B0_RL_NKPortOfLading = "CATOR";
			AddParty(factory, shipment, PartyTypes.Codes.Consignee, "Consignee");
			AddParty(factory, shipment, PartyTypes.Codes.Shipper, "Shipper");
			var commodity = shipment.Commodities.AddNew();
			commodity.BY_GrossWeight = 100;
			commodity.BY_GrossWeightUnit = Constants.Weight.Kilograms;
			commodity.BY_Description = "FRENCH DARK CHOCOLATE";
			commodity.BY_PieceCount = 100;
			commodity.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			return shipment;
		}

		static void AddCrewMembers(Trip trip)
		{
			var crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.ResponsibleParty;
			crew.CP_FullName = "CHRIS AOMAD";
			crew.CP_DateOfBirth = new ZDate(1935, 09, 19);
			crew.CP_Gender = Constants.Genders.Man;
			crew.CP_RN_NKNationality = Constants.CountryCodes.UnitedStates;
			AddDocOrNumber(crew.Certificates, CrewACEIdTypes.Codes.Id, "0000041153");
			AddDocOrNumber(crew.Certificates, TravelDocumentTypes.Codes.HazmatEndorsement, "8456");
			AddDocOrNumber(crew.Certificates, TravelDocumentTypes.Codes.CommercialDriversLicense, "P100971204141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia);
			AddDocOrNumber(crew.Certificates, TravelDocumentTypes.Codes.Passport, "15504141", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates);
			AddDocOrNumber(crew.Certificates, TravelDocumentTypes.Codes.OtherTravelDocument, "13465");
			var address = crew.USAddress;
			address.E2_AddressOverride = true;
			address.E2_Address1 = "11107 SUNSET HILLS ROAD";
			address.E2_City = "RESTON";
			address.E2_State = USStatesList.Codes.Virginia;
			address.E2_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			address.E2_Postcode = "20190";
			var passenger = trip.CrewMembers.AddNew();
			passenger.CP_Type = CrewTypes.Codes.Passenger;
			passenger.CP_FullName = "BILL BOOTSTRAP TURNER";
			passenger.CP_DateOfBirth = new ZDate(1970, 12, 30);
			passenger.CP_Gender = Constants.Genders.Man;
			passenger.CP_RN_NKNationality = Constants.CountryCodes.UnitedStates;
			AddDocOrNumber(passenger.Certificates, CrewACEIdTypes.Codes.ProximityCardId, "1234");
			AddDocOrNumber(passenger.Certificates, TravelDocumentTypes.Codes.Passport, "EA12343", ZDateTime.Empty, Constants.CountryCodes.UnitedStates);
		}

		static void AddConveyance(BusinessObjectFactory factory, Trip trip)
		{
			var refContainer = factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.PickupTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var refEquipment = factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			refEquipment.RQ_VIN = "1234567890";
			refEquipment.RQ_Registration = "BBDD11";
			refEquipment.RQ_RN_NKRegistrationCountry = GetUSBranch(factory).Country.Code;
			refEquipment.RQ_RegState = USStatesList.Codes.Illinois;
			refEquipment.RQ_GateTransponder1 = "789543218";
			AddDocOrNumber(refEquipment.Certificates, ConveyanceReferences.Codes.ACEId, "64894654");
			AddDocOrNumber(refEquipment.Certificates, ConveyanceReferences.Codes.CarrierId, "46765464");
			trip.BH_OH_Carrier = factory.NewWithValidTestData<OrgHeader>().PK;
			trip.Carrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.DOTDepartmentOfTransportation, "46456487", "US");
			var conveyance = trip.Conveyance;
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			conveyance.SealNumbers.AddNew().CY_Data = "12345";
			conveyance.SealNumbers.AddNew().CY_Data = "45613";
			conveyance.BJ_EmptyIITsCoveredByCarrier = true;
			conveyance.BJ_MerchandiseAndIITsCoveredByCarrier = true;
			conveyance.BJ_InsuranceName = "Hazmat Shipment Insurance";
			conveyance.BJ_InsurancePolicyNumber = "23494564";
			conveyance.BJ_InsuranceYearPolicyIssue = 2011;
			conveyance.BJ_InsuranceAmount = 2000000;
		}

		static void AddEquipment(BusinessObjectFactory factory, Trip trip)
		{
			var refContainer = factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("CN", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var refEquipment = factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = false;
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			refEquipment.RQ_Registration = "68465464";
			refEquipment.RQ_RN_NKRegistrationCountry = GetUSBranch(factory).Country.Code;
			refEquipment.RQ_RegState = USStatesList.Codes.Alabama;
			AddDocOrNumber(refEquipment.Certificates, ConveyanceReferences.Codes.ACEId, "8987964");
			var equipment = trip.Equipment.AddNew();
			equipment.SealNumbers.AddNew().CY_Data = "56484";
			equipment.SealNumbers.AddNew().CY_Data = "64845";
			equipment.BJ_MerchandiseAndIITsCoveredByCarrier = true;
			equipment.BJ_MerchandiseAndIITsCoveredByImporter = true;
			equipment.BJ_RQ_Equipment = refEquipment.PK;
			refContainer = factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("T1", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refEquipment = factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = true;
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			refEquipment.RQ_Registration = "BA12YY";
			refEquipment.RQ_RN_NKRegistrationCountry = GetUSBranch(factory).Country.Code;
			refEquipment.RQ_RegState = USStatesList.Codes.Illinois;
			AddDocOrNumber(refEquipment.Certificates, ConveyanceReferences.Codes.ACEId, "5498168");
			equipment = trip.Equipment.AddNew();
			equipment.SealNumbers.AddNew().CY_Data = "32132";
			equipment.SealNumbers.AddNew().CY_Data = "31321";
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			equipment.BJ_EmptyIITsCoveredByImporter = true;
			equipment.BJ_RQ_Equipment = refEquipment.PK;
		}

		void AddEquipmentDetailsOnly(Trip trip)
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("CN", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			var equipment = trip.Equipment.AddNew();
			equipment.BJ_IsConveyance = false;
			equipment.BJ_RC_RoadContainerType = refContainer.PK;
			equipment.BJ_RegistrationNumber = "68465464";
			equipment.BJ_RN_NKRegistrationCountry = GetUSBranch(Factory).Country.Code;
			equipment.BJ_RW_NKRegistrationState = USStatesList.Codes.Alabama;
			equipment.SealNumbers.AddNew().CY_Data = "56484";
			equipment.SealNumbers.AddNew().CY_Data = "64845";
			equipment.BJ_MerchandiseAndIITsCoveredByCarrier = true;
			equipment.BJ_MerchandiseAndIITsCoveredByImporter = true;
			refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode("T1", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			equipment = trip.Equipment.AddNew();
			equipment.BJ_IsConveyance = true;
			equipment.BJ_RC_RoadContainerType = refContainer.PK;
			equipment.BJ_RegistrationNumber = "BA12YY";
			equipment.BJ_RN_NKRegistrationCountry = GetUSBranch(Factory).Country.Code;
			equipment.BJ_RW_NKRegistrationState = USStatesList.Codes.Illinois;
			equipment.SealNumbers.AddNew().CY_Data = "32132";
			equipment.SealNumbers.AddNew().CY_Data = "31321";
			equipment.BJ_EmptyIITsCoveredByCarrier = true;
			equipment.BJ_EmptyIITsCoveredByImporter = true;
			equipment.BJ_ACEID = "5498168";
		}

		internal static void AddDocOrNumber(GenRegCertAccredMaintListCollection certificates, string type, string number, ZDateTime? expiry = null, string country = null, string state = null)
		{
			var cert = certificates.AddNew();
			cert.XZ_Type = type;
			cert.XZ_RefNumber = number;
			cert.XZ_ExpiryOrDueDate = expiry.GetValueOrDefault();
			cert.XZ_RN_NKCountryOfIssuance = country;
			cert.XZ_StateOrProvinceOfIssuance = state;
		}

		internal static GlbBranch GetUSBranch(BusinessObjectFactory factory)
		{
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "USLAX";
			branch.Company.GC_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;
			return branch;
		}

		static void AddShipment(BusinessObjectFactory factory, Trip trip, bool addCustomsBroker)
		{
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment.B0_MasterBillNumber = "1234654894";
			shipment.B0_ReferenceID = "684864684";
			shipment.B0_PortOfLadingKCode = "12345";
			shipment.B0_PlaceOfReceipt = "Tahsis";
			shipment.B0_ServiceType = ServiceTypes.Codes.CollectOnDelivery;
			shipment.B0_Firms = "9874";
			shipment.B0_BoardedQuantity = 6;
			shipment.B0_IsFDAFreight = true;
			shipment.B0_WasOutOfUSFor45DaysOrLess = true;
			AddInBond(shipment);
			AddCommodities(factory, shipment);
			AddParty(factory, shipment, PartyTypes.Codes.Consignee, "Consignee");
			AddParty(factory, shipment, PartyTypes.Codes.Shipper, "Shipper");
			if (addCustomsBroker)
			{
				AddParty(factory, shipment, PartyTypes.Codes.CustomsBroker, "Customs Broker");
			}
		}

		static void AddInBond(Shipment shipment)
		{
			var inbond = shipment.InBond;
			inbond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			inbond.BM_DestinationPortCode = "1234";
			inbond.BM_OnwardCarrier = "4567";
			inbond.BM_InBondCarrierID = "456789134654";
			inbond.InBondNumber = "1234569876543211";
			inbond.BM_TransferCarrier = "321465987456";
			inbond.BM_ForeignDestPortKCode = "79845";
			inbond.BM_ExportDate = new ZDate(2011, 08, 31);
			inbond.BM_PedimentoNumber = "98765413265478";
		}

		static void AddCommodities(BusinessObjectFactory factory, Shipment shipment)
		{
			var commodity = shipment.Commodities.AddNew();
			commodity.BY_GrossWeight = 2.65;
			commodity.BY_GrossWeightUnit = Constants.Weight.Tonnes;
			commodity.BY_Description = "FRENCH DARK CHOCOLATE";
			commodity.BY_PieceCount = 100;
			commodity.BY_ManifestUnitCode = Constants.PkgUnit.Box;
			commodity.BY_MarksAndNumbers = "MARKS LINE 1\r\nMARKS LINE 2\r\nMARKS LINE 3";
			commodity.HarmonizedNumbers.AddNew().CY_Data = "6601100000";
			commodity.HarmonizedNumbers.AddNew().CY_Data = "6602001000";
			commodity.HarmonizedNumbers.AddNew().CY_Data = "6602001001";
			commodity.VehicleIdentificationNumbers.AddNew().CY_Data = "64987465456";
			commodity.VehicleIdentificationNumbers.AddNew().CY_Data = "4654879874";
			commodity.C4Codes.AddNew().CY_Data = "13213213";
			commodity.C4Codes.AddNew().CY_Data = "21646544";
			commodity.BY_MonetaryValue = 2500;
			commodity.BY_RN_NKCountryOfOrigin = Constants.CountryCodes.France;
			commodity.BY_BJ_Equipment = ((Equipment)commodity.Lookups.Equipment[1]).PK;
			var org = factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Big Boss";
			contact.OC_Phone = "+3 (126) 4846516";
			var subs = factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1321";
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs2 = factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "4561";
			subs2.DG_Variant = "";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var hazmat = commodity.UNDGs.AddNew();
			hazmat.DI_DG = subs.PK;
			hazmat.LinkDefault(subs);
			hazmat.DI_OC_DGContact = contact.PK;
			hazmat = commodity.UNDGs.AddNew();
			hazmat.DI_DG = subs2.PK;
			hazmat.LinkDefault(subs2);
			hazmat.DI_OC_DGContact = contact.PK;
		}

		static void AddParty(BusinessObjectFactory factory, Shipment shipment, string type, string name)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = name;
			var address = org.MainAddress;
			address.OA_RL_NKRelatedPortCode = "USLAX";
			address.OA_Address1 = "12 Party Street";
			address.OA_City = "Chicago";
			address.OA_State = USStatesList.Codes.Illinois;
			address.OA_PostCode = "2009";
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.ACE, "54654", Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.FilerCode, "1234", Constants.CountryCodes.UnitedStates);
			org.CustomsCodes.AddNew(PartyIdTypes.Codes.ABIRoutingCode, "0901SV9AA", Constants.CountryCodes.UnitedStates);
			var orgContact = org.Contacts.AddNew();
			orgContact.OC_ContactName = name + "Contact";
			orgContact.OC_Phone = "+3 (126) 4846516";
			orgContact.OC_Email = "party@test.net";
			var party = shipment.Parties.AddNew();
			party.E2_AddressType = type;
			party.OrganisationPK = org.PK;
			party.E2_Contact = orgContact.OC_ContactName;
		}

		static void AssertNoExceptionsThrown(Trip trip, string expectedResult = null)
		{
			var wrapper = new eManifestMessageWrapper(trip, MessageTypes.Codes.eManifest);
			var builder = new CompleteManifestMessageBuilder(wrapper, MessageTypes.Codes.eManifest, MessageSubTypes.Create);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			AssertNotNull("Message should be created", message);
			if (!string.IsNullOrEmpty(expectedResult))
			{
				AssertMultilineASCIIEquals("Message text", expectedResult, message.EM_FormattedMessageText);
			}
		}
	}
}
