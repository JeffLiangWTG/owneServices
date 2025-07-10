using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D08A.Messages.MEDPID;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class CrewOrEquipmentRegistrationMessageWrapperTest : TestCaseWithFactory
	{
		public void TestCrewAcceptedResponseMessage()
		{
			var wrapper = GetWrapper(CrewAcceptedInterchangeText);
			AssertEquals("Equipment.Count", 0, wrapper.Equipment.Count());
			AssertEquals("CrewMembers.Count", 1, wrapper.CrewMembers.Count());
			var crew = wrapper.CrewMembers.First();
			AssertEquals("IsAccepted", true, crew.IsAccepted);
			AssertEquals("TransmissionReferenceNumber", "BRIANMICHAELJORDANRICHARDSONM24-Dec-46CAR568M2356217", wrapper.TransmissionReferenceNumber);
			AssertEquals("ACEId", "14133", crew.ACEId);
			AssertEquals("CarrierCode", "LOCK", crew.CarrierCode);
			AssertEquals("FirstName", "BRIAN", crew.FirstName);
			AssertEquals("MiddleName", "MICHAEL JORDAN", crew.MiddleName);
			AssertEquals("LastName", "RICHARDSON", crew.LastName);
			AssertEquals("DateOfBirth", new ZDateTime(1946, 12, 24), crew.DateOfBirth);
			AssertEquals("Gender", "M", crew.Gender);
			AssertEquals("Citizenship", "CA", crew.Citizenship);
			AssertEquals("HazmatEndorsement", "YES", crew.HazmatEndorsement);
			AssertEquals("TravelDocuments.Count", 3, crew.TravelDocuments.Count());
			AssertTravelDocument(crew.TravelDocuments.ElementAt(0), "ZZZ - Commercial driver's license", "R568M2356217", "US", "VA");
			AssertTravelDocument(crew.TravelDocuments.ElementAt(1), "AIG - Passport", "FA246801", "US");
			AssertTravelDocument(crew.TravelDocuments.ElementAt(2), "OTD - Other travel document", "13465");
			AssertEquals("Notifications.Count", 0, crew.Notifications.Count());
			wrapper = GetWrapper(CrewAcceptedInterchangeText2);
			AssertEquals("Equipment.Count", 0, wrapper.Equipment.Count());
			AssertEquals("CrewMembers.Count", 1, wrapper.CrewMembers.Count());
			crew = wrapper.CrewMembers.First();
			AssertEquals("IsAccepted", true, crew.IsAccepted);
			AssertEquals("TransmissionReferenceNumber", "HENRYWILLIAMSM12-Jul-52CAW146L1752853", wrapper.TransmissionReferenceNumber);
			AssertEquals("ACEId", "14134", crew.ACEId);
			AssertEquals("CarrierCode", "LOCK", crew.CarrierCode);
			AssertEquals("FirstName", "HENRY", crew.FirstName);
			AssertEquals("MiddleName", ZString.Empty, crew.MiddleName);
			AssertEquals("LastName", "WILLIAMS", crew.LastName);
			AssertEquals("DateOfBirth", new ZDate(1952, 07, 12), crew.DateOfBirth);
			AssertEquals("Gender", "M", crew.Gender);
			AssertEquals("Citizenship", "CA", crew.Citizenship);
			AssertEquals("HazmatEndorsement", "YES", crew.HazmatEndorsement);
			AssertEquals("TravelDocuments.Count", 3, crew.TravelDocuments.Count());
			AssertTravelDocument(crew.TravelDocuments.ElementAt(0), "AIG - Passport", "MD784392", "US");
			AssertTravelDocument(crew.TravelDocuments.ElementAt(1), "ZZZ - Commercial driver's license", "W146L1752853", "US", "VA");
			AssertTravelDocument(crew.TravelDocuments.ElementAt(2), "ET - Permanent resident card C2", "1234567890", "US");
			AssertEquals("Notifications.Count", 0, crew.Notifications.Count());
		}

		public void TestCrewErrorResponseMessage()
		{
			var wrapper = GetWrapper(CrewErrorInterchangeText);
			AssertEquals("Equipment.Count", 0, wrapper.Equipment.Count());
			AssertEquals("CrewMembers.Count", 1, wrapper.CrewMembers.Count());
			var crew = wrapper.CrewMembers.First();
			AssertEquals("IsAccepted", false, crew.IsAccepted);
			AssertEquals("ACEId", ZString.Empty, crew.ACEId);
			AssertEquals("CarrierCode", "AAGC", crew.CarrierCode);
			AssertEquals("FirstName", "ABBEY", crew.FirstName);
			AssertEquals("MiddleName", ZString.Empty, crew.MiddleName);
			AssertEquals("LastName", "BARTON", crew.LastName);
			AssertEquals("DateOfBirth", new ZDateTime(1969, 12, 02), crew.DateOfBirth);
			AssertEquals("Gender", "M", crew.Gender);
			AssertEquals("Citizenship", "CA", crew.Citizenship);
			AssertEquals("HazmatEndorsement", "NO", crew.HazmatEndorsement);
			AssertEquals("TravelDocuments.Count", 2, crew.TravelDocuments.Count());
			AssertTravelDocument(crew.TravelDocuments.First(), "AQW - Driving license (national)", "85441223364", "CA", "AB");
			AssertTravelDocument(crew.TravelDocuments.Last(), "AIG - Passport", "SR926874", "CA");
			AssertEquals("Notifications.Count", 1, crew.Notifications.Count());
			var error = (ITableInterpretation)crew.Notifications.First();
			AssertEquals("Error.Code", "003", error.Values.First());
			AssertEquals("Error.Description", "Invalid Driver CDL 85441223364 supplied", error.Values.Last());
		}

		public void TestEquipmentAcceptedResponseMessage()
		{
			var wrapper = GetWrapper(EquipmentAcceptedInterchangeText);
			AssertEquals("Equipment.Count", 1, wrapper.Equipment.Count());
			AssertEquals("CrewMembers.Count", 0, wrapper.CrewMembers.Count());
			AssertEquals("TransmissionReferenceNumber", "TF-Trailerdryfreight1234567890BA12YY", wrapper.TransmissionReferenceNumber);
			var equipment = wrapper.Equipment.First();
			AssertEquals("IsAccepted", true, equipment.IsAccepted);
			AssertEquals("ACEId", "14134", equipment.ACEId);
			AssertEquals("CarrierCode", "LOCK", equipment.CarrierCode);
			AssertEquals("EquipmentType", "TF - Trailer dry freight", equipment.EquipmentType);
			AssertEquals("EquipmentId", "1234567890", equipment.EquipmentId);
			AssertEquals("ConveyanceId", ZString.Empty, equipment.ConveyanceId);
			AssertEquals("TransponderId", ZString.Empty, equipment.TransponderId);
			AssertEquals("VehicleIdentificationNumber", ZString.Empty, equipment.VehicleIdentificationNumber);
			AssertEquals("LicensePlateNumber", "BA12YY", equipment.LicensePlateNumber);
			AssertEquals("CountryOfLicensePlateRegistration", "US", equipment.CountryOfLicensePlateRegistration);
			AssertEquals("StateOrProvinceOfLicensePlateRegistration", "IL", equipment.StateOrProvinceOfLicensePlateRegistration);
			AssertEquals("Notifications.Count", 0, equipment.Notifications.Count());
		}

		public void TestConveyanceErrorResponseMessage()
		{
			var wrapper = GetWrapper(ConveyanceErrorInterchangeText);
			AssertEquals("Equipment.Count", 1, wrapper.Equipment.Count());
			AssertEquals("CrewMembers.Count", 0, wrapper.CrewMembers.Count());
			var equipment = wrapper.Equipment.First();
			AssertEquals("IsAccepted", false, equipment.IsAccepted);
			AssertEquals("ACEId", ZString.Empty, equipment.ACEId);
			AssertEquals("CarrierCode", "LOCK", equipment.CarrierCode);
			AssertEquals("EquipmentType", "PU - Pickup truck", equipment.EquipmentType);
			AssertEquals("EquipmentId", ZString.Empty, equipment.EquipmentId);
			AssertEquals("ConveyanceId", "46765464", equipment.ConveyanceId);
			AssertEquals("TransponderId", "789543218", equipment.TransponderId);
			AssertEquals("VehicleIdentificationNumber", "1234567890", equipment.VehicleIdentificationNumber);
			AssertEquals("LicensePlateNumber", "BBDD11", equipment.LicensePlateNumber);
			AssertEquals("CountryOfLicensePlateRegistration", "US", equipment.CountryOfLicensePlateRegistration);
			AssertEquals("StateOrProvinceOfLicensePlateRegistration", "IL", equipment.StateOrProvinceOfLicensePlateRegistration);
			AssertEquals("Notifications.Count", 1, equipment.Notifications.Count());
			var error = (ITableInterpretation)equipment.Notifications.First();
			AssertEquals("Error.Code", "003", error.Values.First());
			AssertEquals("Error.Description", "Invalid VIN", error.Values.Last());
		}

		static void AssertTravelDocument(CrewOrEquipmentRegistrationMessageWrapper.CrewMemberWrapper.TravelDocumentWrapper doc, string type, string number, string country = "", string state = "")
		{
			AssertEquals("TravelDocumentType", type, doc.TravelDocumentType);
			AssertEquals("TravelDocumentNumber", number, doc.TravelDocumentNumber);
			AssertEquals("CountryOfIssuance", country, doc.CountryOfIssuance);
			AssertEquals("StateOrProvinceOfIssuance", state, doc.StateOrProvinceOfIssuance);
		}

		CrewOrEquipmentRegistrationMessageWrapper GetWrapper(string interchangeText)
		{
			var message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			var medpid = (MEDPIDMessage)message.GetAutoEdifactMessageUsingNamedFactory(new eManifestMessageFactory(), new UNOACharacterSet());
			return new CrewOrEquipmentRegistrationMessageWrapper(Factory, medpid);
		}

		internal const string CrewErrorInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+253++ACE
UNH+255+MEDPID:D:02A:UN
BGM+++23
GIS+36:::1
PNA+FM+++++2:ABBEY+7+1:BARTON
RFF+AQW:85441223364
RFF+AAZ:AAGC
RFF+ALH:NO
RFF+AIG:CASR926874
DTM+329:02121969:4
NAT+2+CA
FTX+AAO++AA002+003Invalid Driver CDL 85441223364 supplied
LOC+ZZZ+++:162::CA
LOC+ZZZ+++:84::AB
PDI+M
UNT+16+255
UNZ+1+253
";
		internal const string CrewAcceptedInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+253++ACE
UNH+255+MEDPID:D:02A:UN
BGM+++44
GIS+23:::1
PNA+FM+++++2:BRIAN+7: MICHAEL  JORDAN +1:RICHARDSON
RFF+AAZ:LOCK
RFF+ZZZ:R568M2356217
RFF+AIG:USFA246801
RFF+OTD:13465
RFF+ALH:YES
DTM+329:24121946:4
NAT+2+CA
FTX+AAI++AA001+502Create Successful for Driver - ACEID 14133
LOC+ZZZ+++:162::US
LOC+ZZZ+++:84::VA
PDI+M
UNT+16+255
UNZ+1+253
";
		internal const string CrewAcceptedInterchangeText2 = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+254++ACE
UNH+255+MEDPID:D:02A:UN
BGM+++44
GIS+23:::1
PNA+FM+++++2:HENRY+7+1:WILLIAMS
RFF+AAZ:LOCK
RFF+AIG:USMD784392
RFF+ZZZ:W146L1752853
RFF+ET:1234567890
RFF+ALH:YES
DTM+329:12071952:4
NAT+2+CA
FTX+AAI++AA001+502Create Successful for Driver - ACEID 14134
LOC+ZZZ+++:162::US
LOC+ZZZ+++:84::VA
LOC+ZZZ+:163::US
PDI+M
UNT+16+255
UNZ+1+253
";
		const string EquipmentAcceptedInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+253++ACE
UNH+255+MEDPID:D:02A:UN
BGM+++23
GIS+23:::2
RFF+AAZ:LOCK
RFF+EQ:1234567890
RFF+ABZ:BA12YY
IHC+1+:::TF
FTX+AAI++AA001+14134
LOC+ZZZ+:162::US+:229::IL
UNT+16+255
UNZ+1+253
";
		const string ConveyanceErrorInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+253++ACE
UNH+255+MEDPID:D:02A:UN
BGM+++44
GIS+36:::3
RFF+AAZ:LOCK
RFF+CRN:46765464
RFF+AKG:1234567890
RFF+TN:789543218
RFF+ABZ:BBDD11
IHC+1+:::PU
FTX+AAO++AA002+003Invalid VIN
LOC+ZZZ+:162::US+:229::IL
UNT+16+255
UNZ+1+253
";
	}
}
