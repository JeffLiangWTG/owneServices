using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class CrewOrEquipmentRegistrationMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateCrewRegistrationOriginalMessage()
		{
			var builder = new CrewOrEquipmentRegistrationMessageBuilder(GetData(), MessageSubTypes.Create, false);
			var builderResults = builder.PopulateMessages().GetBuilderResults().ToArray();
			AssertEquals("Messages Size", 2, builderResults.Length);
			var message = builderResults[0].Message;
			AssertEquals("EM_MessageType", MessageTypes.Codes.CrewOrEquipmentRegistration, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", CrewRegistrationOriginal, message.EM_FormattedMessageText);
			var expectedInterpretation = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewRegistrationMessageInterpretation.html");
			AssertMultilineASCIIEquals("Crew Registration Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
			message = builderResults.Last().Message;
			AssertEquals("EM_MessageType", MessageTypes.Codes.CrewOrEquipmentRegistration, message.EM_MessageType);
			AssertMultilineASCIIEquals("Message text", CrewRegistrationOriginal2, message.EM_FormattedMessageText);
			expectedInterpretation = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewRegistrationMessageInterpretation2.html");
			AssertMultilineASCIIEquals("Crew Registration Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation.Replace("<tr><td style=", "\r\n<tr><td style="));
		}

		IDisposable userContextChange;
		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			if (userContextChange != null)
			{
				userContextChange.Dispose();
			}

			base.TearDown();
		}

		ICrewOrEquipmentRegistration GetData()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Billy Bob";
			staff.GS_PublishWorkPhone = true;
			staff.GS_WorkPhone = "1234567890";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = Env.CurrentCompany.PK;
			branch.GB_Phone = "0987654321";
			staff.GS_GB_HomeBranch = branch.PK;
			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartment.PK);
			var trip = Factory.New<Trip>();
			trip.BH_CarrierSCAC = "LOCK";
			AddCrew(trip);
			return new eManifestMessageWrapper(trip, MessageTypes.Codes.CrewOrEquipmentRegistration);
		}

		internal static void AddCrew(Trip trip)
		{
			var contact = trip.Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Brian  Michael  Jordan  Richardson ";
			contact.OC_Birthday = new ZDate(1946, 12, 24);
			contact.OC_Gender = Constants.Genders.Man;
			contact.OC_RN_NKNationality = Constants.CountryCodes.Canada;
			contact.Person.UpdateFromContact(contact);
			eManifestMessageWrapperTest.AddDocOrNumber(contact.Certificates, TravelDocumentTypes.Codes.CommercialDriversLicense, "R568M2356217", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia);
			eManifestMessageWrapperTest.AddDocOrNumber(contact.Certificates, TravelDocumentTypes.Codes.Passport, "FA246801", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates);
			eManifestMessageWrapperTest.AddDocOrNumber(contact.Certificates, TravelDocumentTypes.Codes.OtherTravelDocument, "13465");
			eManifestMessageWrapperTest.AddDocOrNumber(contact.Certificates, TravelDocumentTypes.Codes.HazmatEndorsement, "8456");
			var crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			crew.CP_OC_Contact = contact.PK;
			var crew2 = trip.CrewMembers.AddNew();
			crew2.CP_Type = CrewTypes.Codes.CrewMember;
			crew2.CP_FullName = "Henry Williams";
			crew2.CP_DateOfBirth = new ZDate(1952, 07, 12);
			crew2.CP_Gender = Constants.Genders.Man;
			crew2.CP_RN_NKNationality = Constants.CountryCodes.Canada;
			crew2.CP_HasHazmatEndorsment = true;
			eManifestMessageWrapperTest.AddDocOrNumber(crew2.Certificates, TravelDocumentTypes.Codes.Passport, "MD784392", ZDateTime.Empty, Constants.CountryCodes.UnitedStates);
			eManifestMessageWrapperTest.AddDocOrNumber(crew2.Certificates, TravelDocumentTypes.Codes.CommercialDriversLicense, "W146L1752853", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates, USStatesList.Codes.Virginia);
			eManifestMessageWrapperTest.AddDocOrNumber(crew2.Certificates, TravelDocumentTypes.Codes.PermanentResidentCard2, "1234567890", new ZDateTime(2014, 03, 27), Constants.CountryCodes.UnitedStates);
			var crew3 = trip.CrewMembers.AddNew();
			crew3.CP_Type = CrewTypes.Codes.CrewMember;
			crew3.CP_FullName = "Marianne Jane Martin";
			crew3.CP_DateOfBirth = new ZDate(1949, 11, 5);
			crew3.CP_Gender = Constants.Genders.Woman;
			crew3.CP_RN_NKNationality = Constants.CountryCodes.Canada;
			eManifestMessageWrapperTest.AddDocOrNumber(crew3.Certificates, CrewACEIdTypes.Codes.Id, "14135");
		}

		internal const string CrewRegistrationOriginal = @"UNH+<<MSGNO PLACEHOLDER>>+MEDPID:D:02A:UN
BGM+++9
PNA+FY+++++7:BILLY BOB, 1234567890
GIS+23:::1
PNA+FM+++++2:BRIAN+7:MICHAEL JORDAN+1:RICHARDSON
RFF+AAZ:LOCK
RFF+ZZZ:R568M2356217
RFF+OTD:13465
RFF+AIG:USFA246801
RFF+ALH:YES
DTM+329:24121946:4
NAT+2+CA
LOC+ZZZ+++:162::US
LOC+ZZZ+++:84::VA
PDI+M
UNT+16+<<MSGNO PLACEHOLDER>>
";
		internal const string CrewRegistrationOriginal2 = @"UNH+<<MSGNO PLACEHOLDER>>+MEDPID:D:02A:UN
BGM+++9
PNA+FY+++++7:BILLY BOB, 1234567890
GIS+23:::1
PNA+FM+++++2:HENRY+7+1:WILLIAMS
RFF+AAZ:LOCK
RFF+AIG:USMD784392
RFF+ZZZ:W146L1752853
RFF+ET:1234567890
RFF+ALH:YES
DTM+329:12071952:4
NAT+2+CA
LOC+ZZZ+++:162::US
LOC+ZZZ+++:84::VA
LOC+ZZZ+:163::US
PDI+M
UNT+17+<<MSGNO PLACEHOLDER>>
";
		internal const string CrewRegistrationOriginal3 = @"UNH+<<MSGNO PLACEHOLDER>>+MEDPID:D:02A:UN
BGM+++9
PNA+FY+++++7:BILLY BOB, 1234567890
GIS+23:::1
PNA+FM+++++2:ABBEY+7+1:BARTON
RFF+AQW:85441223364
RFF+AAZ:AAGC
RFF+ALH:NO
RFF+AIG:CASR926874
DTM+329:02121969:4
NAT+2+CA
LOC+ZZZ+++:162::CA
LOC+ZZZ+++:84::AB
PDI+M
UNT+17+<<MSGNO PLACEHOLDER>>
";
	}
}
