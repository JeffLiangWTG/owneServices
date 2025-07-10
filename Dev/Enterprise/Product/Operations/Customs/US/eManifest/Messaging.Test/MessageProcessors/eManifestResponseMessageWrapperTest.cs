using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.CUSRES;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class eManifestResponseMessageWrapperTest : TestCaseWithFactory
	{
		public void TestTrip()
		{
			AssertEquals("Trip.Notifications.Count", 1, errorWrapper.Notifications.Count());
			AssertEquals("ProcessingDate", ZDateTime.Empty, errorWrapper.ProcessingDate);
			AssertEquals("TransmissionReferenceNumber", "MAN13", errorWrapper.TransmissionReferenceNumber);
			AssertEquals("TripReference", "LOCKMAN0000001", errorWrapper.TripReference);
			AssertEquals("MethodOfTransportation", "03 - Road", errorWrapper.MethodOfTransportation);
			AssertEquals("CarrierCode", "LOCK", errorWrapper.CarrierCode);
			AssertEquals("FirstExpectedPortOfArrival", "0152", errorWrapper.FirstExpectedPortOfArrival);
			AssertEquals("EstimatedDateOfArrival", new ZDateTime(2004, 12, 30, 12, 0, 0), errorWrapper.EstimatedDateOfArrival);
			AssertEquals("TransitDirectionCode", "I", errorWrapper.TransitDirectionCode);
			AssertEquals("AmendmentReasonCode", "03", errorWrapper.AmendmentReasonCode);
			AssertEquals("CarrierACEId", "456734654", statusWrapper.CarrierACEId);
			AssertEquals("DistrictPortOfEntry", "3004", statusWrapper.DistrictPortOfEntry);
		}

		public void TestConveyance()
		{
			var conveyance = errorWrapper.Conveyance;
			AssertEquals("ConveyanceType", "BT - Box truck", conveyance.ConveyanceType);
			AssertEquals("ConveyanceACEId", "10000324", conveyance.ConveyanceACEId);
			AssertEquals("ConveyanceId", "46765464", conveyance.ConveyanceId);
			AssertEquals("VehicleIdentificationNumber", "1234567890", conveyance.VehicleIdentificationNumber);
			AssertEquals("DepartmentOfTransportationNumber", "46456487", conveyance.DepartmentOfTransportationNumber);
			AssertEquals("TransponderId", "789543218", conveyance.TransponderId);
			AssertEquals("InsuranceDetails", "STATE FARM INSURANCE COMPANY\r\nQO123456789TF\r\n2004\r\n100000", conveyance.InsuranceDetails);
			AssertEquals("SealNumbers", "12345; 45613", conveyance.SealNumbers);
			AssertEquals("IITEntityIndicators", "EC; MC", conveyance.IITEntityIndicators);
			var licensePlate = conveyance.LicensePlates.First();
			AssertEquals("LicensePlateNumber", "BBDD11", licensePlate.LicensePlateNumber);
			AssertEquals("CountryOfRegistration", "US", licensePlate.CountryOfRegistration);
			AssertEquals("StateOrProvinceOfRegistration", "IL", licensePlate.StateOrProvinceOfRegistration);
		}

		public void TestEquipment()
		{
			AssertEquals("Equipment.Count", 2, errorWrapper.Equipment.Count());
			var equipment = errorWrapper.Equipment.First();
			AssertEquals("EquipmentType", "CN", equipment.EquipmentType);
			AssertEquals("EquipmentACEId", ZString.Empty, equipment.EquipmentACEId);
			AssertEquals("EquipmentId", "68465464", equipment.EquipmentId);
			AssertEquals("SealNumbers", "78945; 98765", equipment.SealNumbers);
			AssertEquals("IITEntityIndicators", "EI; MI", equipment.IITEntityIndicators);
			var licensePlate = equipment.LicensePlates.First();
			AssertEquals("LicensePlateNumber", "BA12YY", licensePlate.LicensePlateNumber);
			AssertEquals("CountryOfRegistration", "US", licensePlate.CountryOfRegistration);
			AssertEquals("StateOrProvinceOfRegistration", "CA", licensePlate.StateOrProvinceOfRegistration);
			equipment = errorWrapper.Equipment.Last();
			AssertEquals("EquipmentACEId", "9876541", equipment.EquipmentACEId);
		}

		public void TestCrewMember()
		{
			AssertEquals("CrewMembers.Count", 1, errorWrapper.CrewMembers.Count());
			var crewMember = errorWrapper.CrewMembers.First();
			AssertEquals("CrewType", "VW - Responsible party", crewMember.CrewType);
			AssertEquals("CrewId", "4321", crewMember.CrewId);
			AssertEquals("IdType", "109 - ACE id", crewMember.IdType);
			AssertEquals("FirstName", "BILL", crewMember.FirstName);
			AssertEquals("MiddleName", "BOOTSTRAP", crewMember.MiddleName);
			AssertEquals("LastName", "TURNER", crewMember.LastName);
			AssertEquals("DateOfBirth", new ZDate(1970, 12, 30), crewMember.DateOfBirth);
			AssertEquals("Gender", "M", crewMember.Gender);
			AssertEquals("Citizenship", "US", crewMember.Citizenship);
			AssertEquals("HazmatEndorsement", "ZZN1354651", crewMember.HazmatEndorsement);
			var travelDocument = crewMember.TravelDocument;
			AssertEquals("TravelDocumentType", "5K - Commercial driver's license", travelDocument.TravelDocumentType);
			AssertEquals("TravelDocumentNumber", "ABA4321465", travelDocument.TravelDocumentNumber);
			AssertEquals("CountryOfIssuance", "US", travelDocument.CountryOfIssuance);
			AssertEquals("StateOrProvinceOfIssuance", "IL", travelDocument.StateOrProvinceOfIssuance);
			var address = crewMember.USAddress;
			AssertEquals("Address", "21 LINCOLN STREET", address.Address);
			AssertEquals("City", "CHICAGO", address.City);
			AssertEquals("StateOrProvince", "IL", address.StateOrProvince);
			AssertEquals("Country", "US", address.Country);
			AssertEquals("Postcode", "2009", address.Postcode);
		}

		public void TestShipment()
		{
			AssertEquals("Shipments.Count", 2, errorWrapper.Shipments.Count());
			var shipment = errorWrapper.Shipments.First();
			AssertEquals("Shipment.Notifications.Count", 2, shipment.Notifications.Count());
			AssertEquals("ShipmentReleaseType", "63 - Immediate exportation", shipment.ShipmentReleaseType);
			AssertEquals("ShipmentControlNumber", "LOCKKH041203101", shipment.ShipmentControlNumber);
			AssertEquals("ShipmentIdentifier", "684864684", shipment.ShipmentIdentifier);
			AssertEquals("PortOrPointOfLoading", "12345", shipment.PortOrPointOfLoading);
			AssertEquals("PlaceOfReceipt", "TAHSIS", shipment.PlaceOfReceipt);
			AssertEquals("TransferDestinationFIRMSCode", "9874", shipment.TransferDestinationFIRMSCode);
			AssertEquals("BoardedQuantity", 10, shipment.BoardedQuantity);
			AssertEquals("FDAFreightIndicator", "135 - No food products included in shipment", shipment.FDAFreightIndicator);
			AssertEquals("IITEntityIndicator", "MC", shipment.IITEntityIndicator);
			AssertEquals("ShipmentAmendmentReasonCode", "03", shipment.ShipmentAmendmentReasonCode);
			shipment = errorWrapper.Shipments.Last();
			AssertEquals("Shipment.Notifications.Count", 4, shipment.Notifications.Count());
			AssertEquals("ShipmentControlNumber", "LOCKKH041203102", shipment.ShipmentControlNumber);
			AssertEquals("FDAFreightIndicator", "135 - No food products included in shipment", shipment.FDAFreightIndicator);
			AssertEquals("Parties.Count", 3, shipment.Parties.Count());
			shipment = statusWrapper.Shipments.First();
			AssertEquals("StatusNotificationCode", "SN053", shipment.StatusNotificationCode);
			AssertEquals("StatusNotificationDate", new ZDateTime(2006, 11, 30, 23, 0, 0), shipment.StatusNotificationDate);
			AssertEquals("DistrictPortOfEntry", "4567", shipment.DistrictPortOfEntry);
			AssertEquals("ArrivalFIRMSCode", "9874", shipment.ArrivalFIRMSCode);
			AssertEquals("InbondPortOfUSDestination", "1234", shipment.InbondPortOfUSDestination);
		}

		public void TestParty()
		{
			var shipment = errorWrapper.Shipments.First();
			AssertEquals("Parties.Count", 1, shipment.Parties.Count());
			var party = shipment.Parties.First();
			AssertEquals("PartyType", "CN - Consignee", party.PartyType);
			AssertEquals("PartyId", "0000065427", party.PartyId);
			AssertEquals("PartyIdType", "109 - ACE Assigned Number", party.PartyIdType);
			AssertEquals("PartyName", "KATHY SMITH", party.PartyName);
			AssertEquals("ABIRoutingCode", "ABI12346", party.ABIRoutingCode);
			AssertEquals("Address", "DOODY STREET", party.Address);
			AssertEquals("City", "BELTSVILLE", party.City);
			AssertEquals("StateOrProvince", "MD", party.StateOrProvince);
			AssertEquals("Country", "US", party.Country);
			AssertEquals("Postcode", "20708", party.Postcode);
			AssertEquals("Phone", "8005551212", party.Phone);
			AssertEquals("Email", "PARTY@TEST.COM", party.Email);
		}

		public void TestCommodity()
		{
			var commodity = errorWrapper.Shipments.First().Commodity;
			AssertEquals("NumberOfPackages", 100, commodity.NumberOfPackages);
			AssertEquals("TypeOfPackages", "BOX", commodity.TypeOfPackages);
			AssertEquals("CargoGrossWeight", 2650m, commodity.CargoGrossWeight);
			AssertEquals("WeightUnitOfMeasure", "K - Kilograms", commodity.WeightUnitOfMeasure);
			AssertEquals("DescriptionOfCargo", "FRENCH DARK CHOCOLATE", commodity.DescriptionOfCargo);
			AssertEquals("ShippingMarks", "MARKS LINE 1MARKS LINE 2MARKS LINE 3", commodity.ShippingMarks);
			AssertEquals("CustomsValue", 2500, commodity.CustomsValue);
			AssertEquals("CountryOfOrigin", "CA", commodity.CountryOfOrigin);
			AssertEquals("HarmonizedNumbers", "6601100000; 6602001000; 6602001001", commodity.HarmonizedNumbers);
			AssertEquals("HazardousMaterialsDetails", "UNDG132\r\nNAMEBIG BOSS\r\nTELE31264846516\r\n\r\nUNDG456\r\nNAMEBIG BOSS\r\nTELE31264846516", commodity.HazardousMaterialsDetails);
			AssertEquals("VehicleIdentificationNumbers", "64987465456; 4654879874", commodity.VehicleIdentificationNumbers);
			AssertEquals("C4Codes", "13213213; 21646544", commodity.C4Codes);
		}

		public void TestInBond()
		{
			var inBond = errorWrapper.Shipments.First().InBond;
			AssertEquals("InbondDestination", "1234", inBond.InbondDestination);
			AssertEquals("OnwardCarrier", "4567", inBond.OnwardCarrier);
			AssertEquals("BondedCarrier", "456789134654", inBond.BondedCarrier);
			AssertEquals("Inbond7512Number", string.Empty, inBond.Inbond7512Number);
			AssertEquals("TransferCarrier", "321465987456", inBond.TransferCarrier);
			AssertEquals("ForeignPortOfDestination", "79845", inBond.ForeignPortOfDestination);
			AssertEquals("EstimatedDateOfUSExit", new ZDate(2004, 12, 30), inBond.EstimatedDateOfUSExit);
			AssertEquals("MexicanPedimentoNumber", "98765413265478", inBond.MexicanPedimentoNumber);
			inBond = errorWrapper.Shipments.Last().InBond;
			AssertEquals("Inbond7512Number", "554002525", inBond.Inbond7512Number);
			AssertEquals("EstimatedDateOfUSExit", new ZDate(2004, 12, 13), inBond.EstimatedDateOfUSExit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var message = MessagingTestHelper.CreateMessage(Factory, AcceptedWithErrorsInterchangeText);
			var cusres = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet());
			errorWrapper = new eManifestResponseMessageWrapper(Factory, cusres);
			message = MessagingTestHelper.CreateMessage(Factory, StatusUpdateInterchangeText);
			cusres = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet());
			statusWrapper = new eManifestResponseMessageWrapper(Factory, cusres);
		}

		eManifestResponseMessageWrapper errorWrapper;
		eManifestResponseMessageWrapper statusWrapper;

		internal const string AcceptedCompleteManifestInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+46++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+46+UN+D:03B
UNH+46+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+20
DTM+132:201204130023:203
FTX+AIQ+++MAN12
TDT+11++03+:::BT+LOCK+I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT+LOCK+I++:274::12345678
TDT+11++03+:::BT+LOCK+I++:8::1234567890
LOC+60+0901
RFF+RFA:03
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+13+46
UNE+1+46
UNZ+1+46
";

		internal const string CancellationAcceptedCompleteManifestInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+46++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+46+UN+D:03B
UNH+46+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+20
DTM+132:201204130023:203
FTX+AIQ+++MAN12
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+13+46
UNE+1+46
UNZ+1+46
";

		internal const string StatusUpdateInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+1778+CUSRES:D:03B:UN
BGM+34:::ST+LOCKMAN0000001
DTM+163:200611302300:203
TDT+11++03+:::TR+LOCK+I++:146::16ABB43764376
TDT+11+++++++:274::11223344
LOC+24+3004:77
NAD+CA+456734654:109
RFF+ACD:BBDD11
LOC+89+IL:163
LOC+89+US:162
RFF+EQ:68465464
RFF+ZZZ:CN
RFF+ABZ:BA12YY
LOC+89+CA:163
LOC+89+US:162
ERP+1
ERC+SN030
FTX+AAO+++Trip arrived
DOC+:ZZZ
DTM+329:19701230:102
NAD+VW+4321:109++TURNER:BILL:BOOTSTRAP
DOC+929:::63+654984654
PAC+100
RFF+AAM:LOCKKH041203101
RFF+BM:1234654968486
LOC+5+1234:77
LOC+188+4567:77
LOC+219+9874:276
DTM+163:200611302300:203
ERP+2
ERC+SN053
FTX+AAO+++Shipment Released (Entered and released)
DOC+929:::63+654984654
PAC+100
RFF+AAM:LOCKKH041203102
RFF+BM:1234654968486
LOC+5+1234:77
LOC+188+4567:77
LOC+219+9874:276
DTM+163:200611302300:203
ERP+2
ERC+SN050
FTX+AAO+++Shipment Hold
UNT+10+1778
UNE+1+1778
UNZ+1+1778
";

		internal const string AcceptedWithErrorsInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+1956+CUSRES:D:03B:UN
BGM+132:::ST+LOCKMAN0000001
DTM+132:200412301200:203
FTX+INS+++STATE FARM INSURANCE COMPANY:QO123456789TF:2004:100000
FTX+AIQ+++MAN13
TDT+11++03+:::BT+LOCK+I++:109::10000324
TDT+11++03+:::BT+LOCK+I++:172::46765464
TDT+11++03+:::BT+LOCK+I++:8::789543218
TDT+11++03+:::BT+LOCK+I++:146::1234567890
TDT+11++03+:::BT+LOCK+I++:274::46456487
TDT+11++03+:::BT+LOCK+I++:215::BBDD11:US
LOC+60+0152
RFF+RFA:03
RFF+SN:12345
RFF+SN:45613
RFF+IIT:EC
RFF+IIT:MC
RFF+ACD:BBDD11
LOC+89+IL:163
LOC+89+US:162
RFF+EQ:68465464
RFF+ZZZ:CN
RFF+SN:78945
RFF+SN:98765
RFF+IIT:EI
RFF+IIT:MI
RFF+ABZ:BA12YY
LOC+89+CA:163
LOC+89+US:162
RFF+AAQ:9876541
ERP+1
ERC+511
FTX+AAO+++Man Returned to Preliminary
DOC+5K:+ABA4321465
FTX+ZZZ+++M:ZZN1354651:US
LOC+91+IL:163
LOC+91+US:162
DTM+329:19701230:102
NAD+VW+4321:109++TURNER:BILL:BOOTSTRAP+21 LINCOLN STREET+CHICAGO+IL:163+2009+US
DOC+929:::63+654984654
PAC+100++BOX
RFF+AAM:LOCKKH041203101
RFF+RFA:03
RFF+IIT:MC
RFF+BM:1234654968486
RFF+SRN:684864684
RFF+AAE:98765413265478
PCI++MARKS LINE 1
PCI++MARKS LINE 2
PCI++MARKS LINE 3
LOC+9+12345:78
LOC+4+TAHSIS:ZZZ
LOC+103+9874:276
LOC+8+79845:78
LOC+45+1234:77
LOC+27+CA:162
DTM+133:20041230:102
GEI+7+135
MEA+AAI++K:2650
NAD+GC+456789134654:167
NAD+OCG+4567:172
NAD+CH+321465987456:167
NAD+CN+0000065427:109+ABI12346+KATHY SMITH+DOODY STREET+BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
COM+PARTY@TEST.COM:EM
MOA+40:2500
CST++13213213:117
CST++21646544:117
CST++6601100000:122
CST++6602001000:122
CST++6602001001:122
FTX+AAA+++FRENCH DARK CHOCOLATE
FTX+PRD+++64987465456:4654879874
FTX+AAC+++UNDG132:NAMEBIG BOSS:TELE31264846516
FTX+AAC+++UNDG456:NAMEBIG BOSS:TELE31264846516
FTX+ZZZ+++10
ERP+2
ERC+060
FTX+AAO+++XXXX Bill Rejected XXXX
ERP+2
ERC+033
FTX+AAO+++Invalid DDPP
DOC+950:ZZZ+554002525
RFF+AAM:LOCKKH041203102
DTM+133:20041213:102
GEI+7+135
NAD+CN+0000065427:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
NAD+IM+0000001714:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
NAD+SH+0000065422:109++KATHY SMITH++BELTSVILLE+MD+20708+US
CTA+IC
COM+8005551212:TE
CST+1
FTX+ZZZ+++158
ERP+2
ERC+060
FTX+AAO+++XXXX Bill Rejected XXXX
ERP+2
ERC+033
FTX+AAO+++Invalid DDPP
ERP+2
ERC+108
FTX+AAO+++Bonded Carrier ID Required
ERP+2
ERC+491
FTX+AAO+++Ship data mxd with rel typs
UNT+38+1956
UNE+1+1956
UNZ+1+1956
";

		internal const string ErrorCompleteManifestMessage = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+43+CUSRES:D:03B:UN
BGM+132:::STANDARD+AAGCMAN0000001+22
DTM+132:201204130023:203
FTX+AIQ+++MAN12
TDT+11++03+:::BT+AAGC+I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT+AAGC+I++:274::1234567890
TDT+11++03+:::BT+AAGC+I++:8::1234567890
LOC+60+0901
RFF+ACD:EQU123
LOC+89+VA:163
LOC+89+US:162
ERP+1
ERC+502
FTX+AAO+++Inv DOT number
ERP+1
ERC+509
FTX+AAO+++Manifest Rejected
ERP+1
ERC+511
FTX+AAO+++Man Returned to Preliminary
DOC+:ZZZ
NAD+VW+14133:109+++1313 MOCKINGBIRD LANE+ORLANDO+FL+32837
DOC+:ZZZ
NAD+FM+14135:109+++1234 SUNSETT BVD+LOS ANGELESE+CA+123456
DOC+:ZZZ
PAC+10++BOX
RFF+AAM:LOCKKH041203101
LOC+9+01535
GEI+7+135
GEI+5
MEA+AAI++K:100.0000
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA+123456+US
CST+1
FTX+AAA+++COMPUTERS
ERP+2
ERC+017
FTX+AAO+++Missing or Invalid Bill
ERP+2
ERC+060
FTX+AAO+++XXXX Bill Rejected XXXX
UNT+42+43
UNE+1+1956
UNZ+1+1956
";
	}
}
