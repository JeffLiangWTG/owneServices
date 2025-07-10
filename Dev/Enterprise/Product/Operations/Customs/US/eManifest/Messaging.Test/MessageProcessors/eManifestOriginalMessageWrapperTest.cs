using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class eManifestOriginalMessageWrapperTest : EDIFACTMessageProcessorTest
	{
		public void TestUnassociatedShipmentsWrapper()
		{
			#region Message Text
			const string messageText = @"UNH+19+CUSCAR:D:03B:UN
BGM+87:::STANDARD+SYSTEM+2
RFF+ABO:SHP19
CNI+1+:23
RFF+AAM:XXXT1239TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+500++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:5000
CNI+2+:22
RFF+AAM:XXXT1210TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+200++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:2000
CNI+3+:24
DOC+714:::34+BBBA4520TEST::AAGC
RFF+AAM:XXXT1211TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+200++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:2000
UNT+24+19
";
			#endregion
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, messageText);
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", false, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", false, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", true, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", true, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", false, wrapper.ContainsEquipmentInfo);
			var shipments = wrapper.Shipments.ToArray();
			AssertEquals("Shipments.Count", 3, shipments.Length);
			AssertShipment(shipments[0], "XXXT1239TEST", ShipmentEntryStatusList.Codes.Accepted);
			AssertShipment(shipments[1], "XXXT1210TEST", EntryStatusList.Codes.Cancelled);
			AssertShipment(shipments[2], "XXXT1211TEST", string.Empty);
		}

		public void TestUnassociatedShipmentsWhenTripIsLodgedWrapper()
		{
			#region Message Text
			const string messageText = @"UNH+19+CUSCAR:D:03B:UN
BGM+87:::STANDARD+LOCKMAN0000001+2
RFF+ABO:SHP19
CNI+1+:23
RFF+AAM:XXXT1239TEST
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+500++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:5000
UNT+24+19
";
			#endregion
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, messageText);
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", false, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", false, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", true, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", true, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", false, wrapper.ContainsEquipmentInfo);
			var shipments = wrapper.Shipments.ToArray();
			AssertEquals("Shipments.Count", 1, shipments.Length);
			AssertShipment(shipments[0], "XXXT1239TEST", ShipmentEntryStatusList.Codes.Linked);
		}

		public void TestCompleteManifestCancellationWrapper()
		{
			#region Message Text
			const string messageText = @"UNH+1+CUSCAR:D:03B:UN
BGM+85:::STANDARD+LOCKMAN0000001+3
DTM+132:201108260423:203
RFF+ABO:MAN1
UNT+5+1
";
			#endregion
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, messageText);
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Cancellation;
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", true, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", false, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", false, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", false, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", false, wrapper.ContainsEquipmentInfo);
		}

		public void TestCompleteManifestWithSplitShipmentWrapper()
		{
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, CompleteManifestWithSplitShipmentMessageText);
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", false, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", false, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", false, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", false, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", true, wrapper.ContainsEquipmentInfo);
			var shipments = wrapper.Shipments.ToArray();
			AssertEquals("Shipments.Count", 1, shipments.Length);
			AssertShipment(shipments[0], "LOCKKH041203102", ShipmentEntryStatusList.Codes.Linked);
		}

		public void TestPreliminaryTripWrapper()
		{
			#region Message Text
			const string messageText = @"UNH+65+CUSREP:D:03B:UN
BGM+336:::STANDARD+AAGCMAN0000015+2
DTM+132:201204210023:203
RFF+ABO:PTR65
LOC+60+0901:77
DOC+700+:23
RFF+AAM:AAGCXXXT1250TEST
DOC+700+:23
RFF+AAM:AAGCXXXT1251TEST
DOC+714+BBBA4520TEST:22:AAGC
RFF+AAM:AAGCXXXT1252TEST
DOC+714+BBBA4521TEST:22:AAGC
RFF+AAM:AAGCXXXT1253TEST
NAD+CA+AAGC:172
TDT+11++03+:::TR++I++:146::DUMMY
EQD+CH
SEL+1234567890
RFF+ABZ:AAGC123456
LOC+89+VA:163
LOC+89+US:162
UNT+16+65
";
			#endregion
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, messageText);
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", false, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", false, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", true, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", false, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", true, wrapper.ContainsEquipmentInfo);
			var shipments = wrapper.Shipments.ToArray();
			AssertEquals("Shipments.Count", 4, shipments.Length);
			AssertShipment(shipments[0], "AAGCXXXT1250TEST", ShipmentEntryStatusList.Codes.Linked);
			AssertShipment(shipments[1], "AAGCXXXT1251TEST", ShipmentEntryStatusList.Codes.Linked);
			AssertShipment(shipments[2], "AAGCXXXT1252TEST", ShipmentEntryStatusList.Codes.Accepted);
			AssertShipment(shipments[3], "AAGCXXXT1253TEST", ShipmentEntryStatusList.Codes.Accepted);
		}

		public void TestPreliminaryTripConfirmationWrapper()
		{
			#region Message Text
			const string messageText = @"UNH+56+CUSREP:D:03B:UN
BGM+336:::STANDARD+AAGCMAN0000013+6
DTM+132:201204210023:203
RFF+ABO:PTR56
UNT+5+56
";
			#endregion
			var message = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "12", userToNotify, GlbBranch.CurrentBranch, ZDateTime.Now, messageText);
			var wrapper = new eManifestOriginalMessageWrapper(message);
			AssertEquals("IsCancelation", false, wrapper.IsCancelation);
			AssertEquals("IsConfirmation", true, wrapper.IsConfirmation);
			AssertEquals("IsPreliminary", true, wrapper.IsPreliminary);
			AssertEquals("IsUnassociatedShipments", false, wrapper.IsUnassociatedShipments);
			AssertEquals("ContainsEquipmentInfo", false, wrapper.ContainsEquipmentInfo);
		}

		static void AssertShipment(eManifestOriginalMessageWrapper.ShipmentWrapper shipment, string scn, string status)
		{
			AssertEquals("ShipmentControlNumber", scn, shipment.ShipmentControlNumber);
			AssertEquals("RequestedShipmentStatus", status, shipment.RequestedShipmentStatus);
		}

		internal const string CompleteManifestWithSplitShipmentMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+85:::STANDARD+AAGCMAN0000047+22
DTM+132:201205170023:203
LOC+60+0901:77
RFF+ABO:MAN<<MSGNO PLACEHOLDER>>
RFF+SN:1234567890
NAD+CA+AAGC:172
NAD+VW+14135:109+++1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456
TDT+11++03+:::BT++I++:109::10000016
TDT+11++03+:::BT++I++:146::1M8GDM9AXKP043446
TDT+11++03+:::BT++I++:274::45678912
TDT+11++03+:::BT++I++:215::EQU456:US
LOC+89+AL:163
EQD+CH
SEL+1234567890
RFF+ABZ:AAGC123456
LOC+89+VA:163
LOC+89+US:162
CNI+1+:24
RFF+AAM:LOCKKH041203102
CNT+58:50
GEI+7+135
TDT+11
RFF+RFA:03
NAD+OS+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
NAD+CN+++DUMMY+DUMMY+DUMMY+DUM:163+DUMMY+DU
GID+1
FTX+AAA+++DUMMY
UNT+24+<<MSGNO PLACEHOLDER>>
";

		internal const string UnassociatedShipmentsMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+87:::STANDARD+SYSTEM+2
RFF+ABO:SHP<<MSGNO PLACEHOLDER>>
CNI+1+:23
RFF+AAM:LOCKKH041203101
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+500++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:5000
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
UNT+24+<<MSGNO PLACEHOLDER>>
";

		internal const string PreliminaryTripMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+AAGCMAN0000001+2
DTM+132:201204210023:203
RFF+ABO:PTR<<MSGNO PLACEHOLDER>>
LOC+60+0901:77
DOC+700+:23
RFF+AAM:LOCKKH041203101
DOC+700+:22
RFF+AAM:LOCKKH041203102
NAD+CA+AAGC:172
TDT+11++03+:::TR++I++:146::DUMMY
EQD+CH
SEL+1234567890
RFF+ABZ:AAGC123456
LOC+89+VA:163
LOC+89+US:162
UNT+10+<<MSGNO PLACEHOLDER>>
";
	}
}
