using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors;
using Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	sealed class eManifestShipmentLicenceLogTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2006, 11, 20, 23, 00, 00)]
		public void TestShipmentLicenseLogged()
		{
			var logQuery = new ZQuery(StmActivityLogSchema.S7_FormCaption, Enterprise.Environment.Env.Licence.USeManifest.Name);
			var count = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			#region AcceptedCompleteManifest One
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			message.EM_MessageNum = "17";
			SaveFactoryAndExecuteBatch();
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Cancelled, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment3.B0_ReleaseStatus", string.Empty, shipment3.B0_ReleaseStatus);
			count = count + 1;
			AssertEquals("shipment1 license to be logged", count, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			#endregion
			#region StatusUpdate
			count = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, StatusUpdateInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentHold, shipment2.B0_ReleaseStatus);
			AssertEquals("No shipment license to be logged", count, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			#endregion
			#region CancellationAcceptedCompleteManifest
			count = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			trip.Messages[0].EM_MessageSubType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Cancellation;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingDelete;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			requestMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			message = MessagingTestHelper.CreateMessage(Factory, CancellationAcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Cancelled, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Cancelled, shipment2.B0_ReleaseStatus);
			AssertEquals("No shipment license to be logged", count, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			#endregion
			#region AcceptedCompleteManifest All
			count = Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery);
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = MessageTypes.Codes.SyntaxError;
			requestMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Change;
			trip.Factory.Save();
			MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "46", userToNotify2, branch1, ZDateTime.Now, CompleteManifestMessageText);
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedCompleteManifestInterchangeText2);
			SaveFactoryAndExecuteBatch();
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment2.B0_ReleaseStatus);
			count += 1;
			AssertEquals("Only shipment2 should be logged Because shipment1 is already logged", count, Factory.GetDatabaseCount(typeof(StmActivityLog), logQuery));
			#endregion
		}

		protected override void SetUp()
		{
			trip = MessagingTestHelper.GetTripAwaitingReply(Factory, "F6F4F35F-49EF-42D2-91D4-0F5757F76FDD", "MAN0000001", "LOCK");
			shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "KH041203101";
			shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "KH041203102";
			shipment3 = trip.Shipments.AddNew();
			shipment3.B0_MasterBillNumber = "KH041203103";
			userToNotify2 = Factory.NewWithValidTestData<GlbStaff>();
			userToNotify2.GS_FullName = "SecondUserToNotify";
			userToNotify2.GS_EmailAddress = "SecondUserToNotify@blah.com";
			userToNotify2.GS_Code = "NU2";
			branch1 = Factory.NewWithValidTestData<GlbBranch>();
			trip.Messages.Add(requestMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify2, branch1, ZDateTime.Now.AddDays(-1), CompleteManifestMessageText2));
		}

		Trip trip;
		Shipment shipment1;
		Shipment shipment2;
		Shipment shipment3;
		EDIMessage message;
		GlbBranch branch1;
		GlbStaff userToNotify2;
		EDIMessage requestMessage;

		void SaveFactoryAndExecuteBatch()
		{
			Factory.Save();
			var processor = new MessageProcessor();
			processor.ExecuteBatch();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			trip = factory.Load<Trip>(trip.PK);
			shipment1 = factory.Load<Shipment>(shipment1.PK);
			shipment2 = factory.Load<Shipment>(shipment2.PK);
			shipment3 = factory.Load<Shipment>(shipment3.PK);
		}

		const string CompleteManifestMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+AAGCMAN0000001+22
DTM+132:201204130023:203
LOC+60+0901:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:1234567890
NAD+CA+AAGC:172
NAD+VW+14133:109+++1313 MOCKINGBIRD LANE+ORLANDO+FL:163+32837
NAD+FM+14135:109+++1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456
TDT+11++03+:::BT++I++:8::1234567890
TDT+11++03+:::BT++I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT++I++:274::12345678
TDT+11++03+:::BT++I++:215::EQU123:US
LOC+89+VA:163
EQD+CH
SEL+1234567890
RFF+ABZ:AAGC123456
LOC+89+VA:163
LOC+89+US:162
CNI+1+:23
RFF+AAM:LOCKKH041203101
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+10++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:100.0000
SGP+EQU123:215
CNI+2+:23
RFF+AAM:LOCKKH041203102
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+200++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:2000
SGP+EQU123:215
UNT+31+<<MSGNO PLACEHOLDER>>
";

		const string CompleteManifestMessageText2 = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+AAGCMAN0000001+22
DTM+132:201204130023:203
LOC+60+0901:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:1234567890
NAD+CA+AAGC:172
NAD+VW+14133:109+++1313 MOCKINGBIRD LANE+ORLANDO+FL:163+32837
NAD+FM+14135:109+++1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456
TDT+11++03+:::BT++I++:8::1234567890
TDT+11++03+:::BT++I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT++I++:274::12345678
TDT+11++03+:::BT++I++:215::EQU123:US
LOC+89+VA:163
EQD+CH
SEL+1234567890
RFF+ABZ:AAGC123456
LOC+89+VA:163
LOC+89+US:162
CNI+1+:23
RFF+AAM:LOCKKH041203101
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+10++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:100.0000
SGP+EQU123:215
CNI+2+:22
RFF+AAM:LOCKKH041203102
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+200++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:2000
SGP+EQU123:215
UNT+31+<<MSGNO PLACEHOLDER>>
";

		const string AcceptedCompleteManifestInterchangeText2 = @"UNB+UNOA:4+CBP-KNZ-TEST:ZZ+LOCK:02+20061108:0939+46++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+46+UN+D:03B
UNH+46+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+20
DTM+163:200611252300:203
FTX+AIQ+++MAN46
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
		const string StatusUpdateInterchangeText = @"UNB+UNOA:4+CBP-ACE-T000:ZZ+LOCK:02+20061108:0939+1778++ACETEST
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
DTM+163:200611222300:203
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
DTM+163:200611222300:203
ERP+2
ERC+SN050
FTX+AAO+++Shipment Hold
UNT+10+1778
UNE+1+1778
UNZ+1+1778
";

		const string CancellationAcceptedCompleteManifestInterchangeText = @"UNB+UNOA:4+CBP-ACE-T001:ZZ+LOCK:02+20061108:0939+46++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+46+UN+D:03B
UNH+46+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+20
DTM+163:200611232300:203
FTX+AIQ+++MAN12
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+13+46
UNE+1+46
UNZ+1+46
";
	}
}
