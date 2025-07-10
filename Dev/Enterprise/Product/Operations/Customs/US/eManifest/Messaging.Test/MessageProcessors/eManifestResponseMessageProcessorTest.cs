using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class eManifestResponseMessageProcessorTest : ResponseMessageProcessorTest
	{
		public void TestMessageStatusForMultipleCompleteManifestMessages()
		{
			trip.Messages.RemoveAndDeleteAll();
			var message1 = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify, branch1, ZDateTime.Now, eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText);
			message1.EM_ApplicationReference = "12";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			message1.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var message2 = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "13", userToNotify, branch1, ZDateTime.Now, eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText);
			message2.EM_ApplicationReference = "12";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			message2.EM_MessageSubType = MessageActionCodes.Codes.Change;
			message2.EM_Status = EDIMessage.Status.Pending;
			var message3 = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "14", userToNotify, branch1, ZDateTime.Now, eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText);
			message3.EM_ApplicationReference = "12";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			message3.EM_MessageSubType = MessageActionCodes.Codes.Change;
			message3.EM_Status = EDIMessage.Status.Pending;
			var message4 = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "15", userToNotify, branch1, ZDateTime.Now, eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText);
			message4.EM_ApplicationReference = "12";
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			message4.EM_MessageSubType = MessageActionCodes.Codes.Change;
			message4.EM_Status = EDIMessage.Status.Pending;
			trip.Messages.Add(message1);
			trip.Messages.Add(message2);
			trip.Messages.Add(message3);
			trip.Messages.Add(message4);

			processor = new MessageProcessor { Logger = logger };
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			ReloadMessages();
			AssertEquals("Some messages were not processed, so updated to AWC.", MessageStatusList.Codes.AwaitingChange, trip.BH_MessageStatus);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Pending, message3.EM_Status);
			AssertEquals(EDIMessage.Status.Pending, message4.EM_Status);

			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			message.Interchange.EI_InterchangeNum = "123";
			SaveFactoryAndExecuteBatch();
			ReloadMessages();
			AssertEquals("All messages have been processed, so updated to CLC.", MessageStatusList.Codes.ClearChange, trip.BH_MessageStatus);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);
			AssertEquals(EDIMessage.Status.Pending, message4.EM_Status);

			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			message.Interchange.EI_InterchangeNum = "1234";
			SaveFactoryAndExecuteBatch();
			ReloadMessages();
			AssertEquals(EDIMessage.Status.Pending, message4.EM_Status);

			void ReloadMessages()
			{
				var factory = new BusinessObjectFactory();
				message1 = factory.Load<EDIMessage>(message1.PK);
				message2 = factory.Load<EDIMessage>(message2.PK);
				message3 = factory.Load<EDIMessage>(message3.PK);
				message4 = factory.Load<EDIMessage>(message4.PK);
			}
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestHandleNextMessagesWhenProcessingResponseForMultipleMessages_Accepted()
		{
			trip.Messages.RemoveAndDeleteAll();
			var unassociateMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText);
			unassociateMessage.EM_ApplicationReference = "16";
			unassociateMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			unassociateMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var preliminaryTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "20", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText);
			preliminaryTripMessage.EM_Status = EDIMessage.Status.Pending;
			preliminaryTripMessage.EM_ApplicationReference = "16";
			preliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			preliminaryTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			preliminaryTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var crewPassengerMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, "24", userToNotify, branch1, ZDateTime.Now.AddDays(2), CrewPassengersDetailsMessageText);
			crewPassengerMessage.EM_Status = EDIMessage.Status.Pending;
			crewPassengerMessage.EM_ApplicationReference = "16";
			crewPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			crewPassengerMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			crewPassengerMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var completeTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "26", userToNotify, branch1, ZDateTime.Now.AddDays(2), PreliminaryTripConfirmationMessageText);
			completeTripMessage.EM_Status = EDIMessage.Status.Pending;
			completeTripMessage.EM_ApplicationReference = "16";
			completeTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			completeTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			completeTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Confirmation;
			trip.Messages.Add(unassociateMessage);
			trip.Messages.Add(preliminaryTripMessage);
			trip.Messages.Add(crewPassengerMessage);
			trip.Messages.Add(completeTripMessage);
			processor = new MessageProcessor { Logger = logger };
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			var newPreliminaryTripMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == preliminaryTripMessage.PK);
			AssertEquals(EDIMessage.Status.Queued, newPreliminaryTripMessage.EM_Status);
			AssertContains("Message text", "BGM+336:::STANDARD+LOCKMAN0000001+2", newPreliminaryTripMessage.EM_MessageText);
			var newCrewPassengerMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == crewPassengerMessage.PK);
			AssertEquals(EDIMessage.Status.Pending, newCrewPassengerMessage.EM_Status);
			AssertContains("Message text", eManifestMessageManagerHelper.MessagePlaceHolder, newCrewPassengerMessage.EM_MessageText);
			var newCompleteTripMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == completeTripMessage.PK);
			AssertEquals(EDIMessage.Status.Pending, newCompleteTripMessage.EM_Status);
			AssertContains("Message text", eManifestMessageManagerHelper.MessagePlaceHolder, newCompleteTripMessage.EM_MessageText);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestHandleNextMessagesWhenProcessingResponseForMultipleMessages_Rejected()
		{
			trip.Messages.RemoveAndDeleteAll();
			var unassociateMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText);
			unassociateMessage.EM_ApplicationReference = "16";
			unassociateMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			unassociateMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var preliminaryTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "20", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText);
			preliminaryTripMessage.EM_Status = EDIMessage.Status.Pending;
			preliminaryTripMessage.EM_ApplicationReference = "16";
			preliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			preliminaryTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			preliminaryTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			var crewPassengerMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, "24", userToNotify, branch1, ZDateTime.Now.AddDays(2), CrewPassengersDetailsMessageText);
			crewPassengerMessage.EM_Status = EDIMessage.Status.Pending;
			crewPassengerMessage.EM_ApplicationReference = "16";
			crewPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			crewPassengerMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			crewPassengerMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			var completeTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "26", userToNotify, branch1, ZDateTime.Now.AddDays(2), PreliminaryTripConfirmationMessageText);
			completeTripMessage.EM_Status = EDIMessage.Status.Pending;
			completeTripMessage.EM_ApplicationReference = "16";
			completeTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			completeTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Confirmation;
			completeTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			trip.Messages.Add(unassociateMessage);
			trip.Messages.Add(preliminaryTripMessage);
			trip.Messages.Add(crewPassengerMessage);
			trip.Messages.Add(completeTripMessage);
			processor = new MessageProcessor { Logger = logger };
			message = MessagingTestHelper.CreateMessage(Factory, ErrorUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			var newPreliminaryTripMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == preliminaryTripMessage.PK);
			AssertEquals(true, newPreliminaryTripMessage.IsCancelled);
			AssertEquals(EDIMessage.Status.Discarded, newPreliminaryTripMessage.EM_Status);
			var newCrewPassengerMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == crewPassengerMessage.PK);
			AssertEquals(true, newCrewPassengerMessage.IsCancelled);
			AssertEquals(EDIMessage.Status.Discarded, newCrewPassengerMessage.EM_Status);
			var newCompleteTripMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == completeTripMessage.PK);
			AssertEquals(true, newCompleteTripMessage.IsCancelled);
			AssertEquals(EDIMessage.Status.Discarded, newCompleteTripMessage.EM_Status);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestMessageStatusForMultipleMessages()
		{
			string errorCompleteManifestMessage = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+999+CUSRES:D:03B:UN
BGM+132:::STANDARD+AAGCMAN0000001+22
DTM+132:201204130023:203
FTX+AIQ+++MAN26
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
			trip.Messages.RemoveAndDeleteAll();
			var unassociateMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText);
			unassociateMessage.EM_ApplicationReference = "16";
			unassociateMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			var preliminaryTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "20", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText);
			preliminaryTripMessage.EM_Status = EDIMessage.Status.Pending;
			preliminaryTripMessage.EM_ApplicationReference = "16";
			preliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			var crewPassengerMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, "24", userToNotify, branch1, ZDateTime.Now.AddDays(2), CrewPassengersDetailsMessageText);
			crewPassengerMessage.EM_Status = EDIMessage.Status.Pending;
			crewPassengerMessage.EM_ApplicationReference = "16";
			crewPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			var completeTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "26", userToNotify, branch1, ZDateTime.Now.AddDays(2), PreliminaryTripConfirmationMessageText);
			completeTripMessage.EM_Status = EDIMessage.Status.Pending;
			completeTripMessage.EM_ApplicationReference = "16";
			completeTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			trip.Messages.Add(unassociateMessage);
			trip.Messages.Add(preliminaryTripMessage);
			trip.Messages.Add(crewPassengerMessage);
			trip.Messages.Add(completeTripMessage);
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedPreliminary;
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			processor = new MessageProcessor { Logger = logger };
			message = MessagingTestHelper.CreateMessage(Factory, errorCompleteManifestMessage);
			SaveFactoryAndExecuteBatch();
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
		}

		public void TestFindAssociatedBOByOriginalMessage()
		{
			const string invalidJobReferenceMessage = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+96+CUSRES:D:03B:UN
BGM+132:::STANDARD+MIML43055+22
DTM+132:201204210023:203
FTX+AIQ+++PTR22
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+8+96
UNE+1+1956
UNZ+1+1956
";
			message = MessagingTestHelper.CreateMessage(Factory, invalidJobReferenceMessage);
			SaveFactoryAndExecuteBatch();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
		}

		public void TestFindAssociatedBOByTripReferenceAndCarrierCode()
		{
			preliminaryTripConfirmationMessage.EM_LinkTable = "";
			preliminaryTripConfirmationMessage.EM_LinkUniqueID = ZGuid.Empty;
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+96+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+22
DTM+132:201204210023:203
FTX+AIQ+++PTR22
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+8+96
UNE+1+1956
UNZ+1+1956
";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
		}

		public void TestFindAssociatedBOWithMultipleTripReferencesAndCarrierCodes()
		{
			var trip1 = MessagingTestHelper.GetTripAwaitingReply(Factory, "F947360E-B024-43DA-8593-494337EBA71C", "MAN0000001", "LOCK");
			var preliminaryTripConfirmationMessage1 = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "22", userToNotify, branch1, ZDateTime.Now.AddDays(10), PreliminaryTripConfirmationMessageText);
			trip1.Messages.Add(preliminaryTripConfirmationMessage1);
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+96+CUSRES:D:03B:UN
BGM+132:::STANDARD+LOCKMAN0000001+00
DTM+132:201204210023:203
FTX+AIQ+++PTR00
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+8+96
UNE+1+1956
UNZ+1+1956
";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_LinkUniqueID", trip1.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
		}

		public void TestMessageWithNoLinkedObject()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST'UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B'UNH+1956+CUSRES:D:03B:UN'BGM+132:::ST+LOCKMAN0000015'ERP+1'ERC+511'FTX+AAO+++Man Returned to Preliminary'UNT+38+1956'UNE+1+1956'UNZ+1+1956";
			const string errorMessage = "Could not find an associated business object (Job) for document reference = 'LOCKMAN0000015'";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("EM_MessageNum", "1956", message.EM_MessageNum);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
		}

		public void TestInvalidCompleteManifestMessage()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST'UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B'UNH+1956+CUSRES:D:03B:UN'BGM+12:::ST+LOCKMAN0000001'DTM+132:200412301200:203'UNT+38+1956'UNE+1+1956'UNZ+1+1956";
			const string errorMessage = "The message processor was unable to interpret received message.";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertEquals("EM_MessageNum", "1956", message.EM_MessageNum);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", string.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
			AssertEmail("e-Manifest Response Message Processor Error Report", errorMessage, message.EM_MessageText, string.Empty);
		}

		public void TestNoAssociatedTransmitMessageFoundWithHPNStatus()
		{
			trip.Messages[0].EM_Status = EDIMessage.Status.Sent;
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", MessageStatusList.Codes.AcknowledgedOriginal, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertContains("LastLog", "1 message processed", new StringCollectionX(logger.UserLogStrings).ToString());
		}

		public void TestNoAssociatedTransmitMessageFound()
		{
			const string errorMessage = @"No associated transmit message has been found that matches the following details:
Application Code 'MAN', Message Number '12'.";
			trip.Messages[0].Delete();
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch2.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", string.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
			AssertEmail("e-Manifest Response Message Processor Error Report", errorMessage, message.EM_MessageText, string.Empty);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedCompleteManifestMessage()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_MessageOwner", "MAN0000001", message.EM_MessageOwner);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Cancelled, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CompleteManifestAcceptedMessageInterpretation.html").Replace("edient:Command=ShowEditForm&LicenceCode=&ControllerID=eManifest&BusinessEntityPK=84f89ebe-e700-4010-ade4-907fcba389e5&Hash=%2brjICa2iV3chJKHgGg04i1ii0j7YRz%2f9I", "");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Accepted Complete e-Manifest w/ACE ID Response for LOCKMAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, "SecondUserToNotify@blah.com");
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedCompleteManifestWihtSplitShipmentMessage()
		{
			trip.Messages[0].Delete();
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify, branch1, ZDateTime.Now.AddDays(7), eManifestOriginalMessageWrapperTest.CompleteManifestWithSplitShipmentMessageText));
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", new ZDateTime(2006, 11, 30, 23, 00, 00), shipment2.B0_ReleaseStatusDate);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestCancellationAcceptedCompleteManifestMessage()
		{
			const string expectedBody = @"</strong>A 'Message Accepted' response has been received from CBP for a Complete e-Manifest w/ACE ID.<br />
Please see message details below.<br />
";
			trip.Messages[0].EM_MessageSubType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Cancellation;
			trip.BH_MessageStatus = MessageStatusList.Codes.AwaitingDelete;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			trip.Shipments[0].B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.CancellationAcceptedCompleteManifestInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Cancelled, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", EntryStatusList.Codes.Cancelled, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearDelete, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", EntryStatusList.Codes.Cancelled, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", string.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertContains("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation);
			AssertEmail("Cancellation accepted Complete e-Manifest w/ACE ID Response for LOCKMAN0000001", expectedBody, message.EM_MessageText, "SecondUserToNotify@blah.com");
		}

		public void TestStatusUpdateMessage()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.StatusUpdateInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.TripArrived, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "1778", message.EM_MessageNum);
			AssertEquals("EM_GB", branch2.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.TripArrived, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentHold, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.StatusUpdateMessageInterpretation.html").Replace("edient:Command=ShowEditForm&LicenceCode=&ControllerID=eManifest&BusinessEntityPK=84f89ebe-e700-4010-ade4-907fcba389e5&Hash=%2brjICa2iV3chJKHgGg04i1ii0j7YRz%2f9I", "");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText);
			AssertEventLog(trip.Logs, Events.CustomsManifestStatus, "ARV - Trip arrived", "This Trip: MAN0000001");
			AssertEventLog(shipment1.Logs, Events.CustomsEntryStatus, "RNE - Shipment Released (Entered and released)", "This Shipment: KH041203101", releaseDate);
			AssertEventLog(shipment2.Logs, Events.CustomsEntryStatus, "HLD - Shipment Hold", "This Shipment: KH041203102", releaseDate);
			message = MessagingTestHelper.CreateMessage(Factory, StatusUpdateInterchangeText2);
			SaveFactoryAndExecuteBatch();
			var releaseDate2 = new ZDateTime(2006, 12, 02, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate2, shipment2.B0_ReleaseStatusDate);
			AssertEventLog(trip.Logs, Events.CustomsManifestStatus, "ARV - Trip arrived", "This Trip: MAN0000001");
			AssertEventLog(shipment1.Logs, Events.CustomsEntryStatus, "RNE - Shipment Released (Entered and released)", "This Shipment: KH041203101", releaseDate);
			AssertEventLog(shipment2.Logs, Events.CustomsEntryStatus, "RNE - Shipment Released (Entered and released)", "This Shipment: KH041203102", releaseDate2);
		}

		public void TestTripStatusUpdateMessage()
		{
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, TripStatusUpdateInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.TripArrived, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.TripArrived, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ZString.Empty, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ZString.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", "Status Update", message.EM_MessageText);
		}

		public void TestShipmentStatusUpdateMessageWithIdenticalStatuses()
		{
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, ShipmentsStatusUpdateWithIdenticalStatusesInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", ShipmentEntryStatusList.Codes.ShipmentHold, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2012, 04, 24, 10, 30, 0);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentHold, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentHold, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", "Status Update", message.EM_MessageText);
		}

		public void TestShipmentStatusUpdateMessageWithDocSegment()
		{
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, ShipmentsStatusUpdateWithDocSegment);
			SaveFactoryAndExecuteBatch();
			var proccessor = new eManifestResponseMessageProcessor(logger);
			proccessor.ProcessMessage(message);
			AssertEquals(1, Regex.Matches(message.EM_MessageText, "DOC+").Count);
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", ShipmentEntryStatusList.Codes.EntryNotOnFile, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "39", message.EM_MessageNum);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", "Status Update", message.EM_MessageText);
		}

		public void TestHasPreliminaryTripMessageRFFAAMblock()
		{
			trip.Messages.RemoveAndDeleteAll();
			var unassociateMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText);
			unassociateMessage.EM_ApplicationReference = "16";
			unassociateMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			unassociateMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var preliminaryTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "20", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText);
			preliminaryTripMessage.EM_Status = EDIMessage.Status.Pending;
			preliminaryTripMessage.EM_ApplicationReference = "16";
			preliminaryTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			preliminaryTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			preliminaryTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var crewPassengerMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewAndPassenger, "24", userToNotify, branch1, ZDateTime.Now.AddDays(2), CrewPassengersDetailsMessageText);
			crewPassengerMessage.EM_Status = EDIMessage.Status.Pending;
			crewPassengerMessage.EM_ApplicationReference = "16";
			crewPassengerMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			crewPassengerMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			crewPassengerMessage.EM_MessageSubType = MessageActionCodes.Codes.Original;
			var completeTripMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "26", userToNotify, branch1, ZDateTime.Now.AddDays(2), PreliminaryTripConfirmationMessageText);
			completeTripMessage.EM_Status = EDIMessage.Status.Pending;
			completeTripMessage.EM_ApplicationReference = "16";
			completeTripMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USeManifest;
			completeTripMessage.EM_MessageText = eManifestMessageManagerHelper.MessagePlaceHolder;
			completeTripMessage.EM_MessageSubType = MessageActionCodes.Codes.Confirmation;
			trip.Messages.Add(unassociateMessage);
			trip.Messages.Add(preliminaryTripMessage);
			trip.Messages.Add(crewPassengerMessage);
			trip.Messages.Add(completeTripMessage);
			processor = new MessageProcessor { Logger = logger };
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			var newPreliminaryTripMessage = trip.Messages.Cast<EDIMessage>().First(x => x.PK == preliminaryTripMessage.PK);
			AssertEquals(EDIMessage.Status.Queued, newPreliminaryTripMessage.EM_Status);
			AssertContains("RFF+AAM text1", "RFF+AAM:LOCKKH041203101", newPreliminaryTripMessage.EM_MessageText);
			AssertContains("RFF+AAM text2", "RFF+AAM:LOCKKH041203102", newPreliminaryTripMessage.EM_MessageText);
		}

		public void TestShipmentStatusUpdateMessageWithoutDocSegment()
		{
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, ShipmentsStatusUpdateWithoutDocSegment);
			SaveFactoryAndExecuteBatch();
			var proccessor = new eManifestResponseMessageProcessor(logger);
			proccessor.ProcessMessage(message);
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", ShipmentEntryStatusList.Codes.EntryNotOnFile, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "39", message.EM_MessageNum);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", "Status Update", message.EM_MessageText);
		}

		public void TestShipmentStatusUpdateMessageWithDifferentStatuses()
		{
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			message = MessagingTestHelper.CreateMessage(Factory, ShipmentsStatusUpdateWithDifferentStatusesInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.ShipmentsStatusUpdate, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentHold, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", new ZDateTime(2012, 04, 24, 10, 30, 0), shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", new ZDateTime(2012, 04, 24, 10, 30, 0), shipment2.B0_ReleaseStatusDate);
			AssertEmail("Complete e-Manifest w/ACE ID Status Update Message for LOCKMAN0000001", "Status Update", message.EM_MessageText);
		}

		public void TestErrorCompleteManifestMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.ErrorCompleteManifestMessage);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.Error, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.Error, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", EntryStatusList.Codes.Error, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ZString.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
		}

		public void TestAcceptedWithErrorsCompleteManifestMessage()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedWithErrorsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "13", message.EM_MessageNum);
			AssertEquals("EM_GB", branch2.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedPreliminary, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", EntryStatusList.Codes.Error, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", EntryStatusList.Codes.Error, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.ErrorResponseMessageInterpretation.html").Replace("edient:Command=ShowEditForm&LicenceCode=&ControllerID=eManifest&BusinessEntityPK=84f89ebe-e700-4010-ade4-907fcba389e5&Hash=%2brjICa2iV3chJKHgGg04i1ii0j7YRz%2f9I", "");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody.Replace("'", "&#39;"), message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Error Complete e-Manifest w/ACE ID Response for LOCKMAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText);
		}

		public void TestErrorCompleteManifestMessageDoesNotOverrideShipmentsStatuses()
		{
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			shipment1.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased;
			shipment1.B0_ReleaseStatusDate = releaseDate;
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedWithErrorsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedUnassociatedShipmentsMessage()
		{
			const string expectedBody = @"Reference Number : MAN0000001<br />
<br />
</strong>A 'Message Accepted' response has been received from CBP for an Unassociated Shipments.<br />
Please see message details below.<br />
<br />
<br />

<br />
<hr />
<br />
<br />
<!--EndSection Details-->
Regards,<br />
<br />
CargoWise One Administrative Messages Sender<br />
<br />
<br />
<br />";
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "16", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Accepted, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Accepted, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			AssertContains("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation);
			AssertEmail("Accepted Unassociated Shipments Response for MAN0000001", expectedBody, message.EM_MessageText);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedUnassociatedShipmentsChange()
		{
			trip.Messages[2].Delete();
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), UnassociatedShipmentsChangeMessageText));
			var releaseDate = new ZDateTime(2006, 11, 29, 23, 00, 00);
			trip.Shipments[0].B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			trip.Shipments[0].B0_ReleaseStatusDate = releaseDate;
			trip.Shipments[1].B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			trip.Shipments[1].B0_ReleaseStatusDate = releaseDate;
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "16", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Accepted Unassociated Shipments Response for MAN0000001", "Message Accepted", message.EM_MessageText);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestErrorUnassociatedShipmentsMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, ErrorUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Error, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "16", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Accepted, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", new ZDateTime(2006, 11, 30, 23, 00, 00), shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", EntryStatusList.Codes.Error, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Error Unassociated Shipments Response for MAN0000001", "Error", message.EM_MessageText);
		}

		public void TestInvalidUnassociatedShipmentsErrorResponseDoesNotMarkShipmentsAsAccepted()
		{
			message = MessagingTestHelper.CreateMessage(Factory, InvalidErrorUnassociatedShipmentsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Error, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "16", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ZString.Empty, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ZString.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Error Unassociated Shipments Response for MAN0000001", "Error", message.EM_MessageText);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedCompleteTripMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedCompleteTripInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "18", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Accepted Complete Trip Details Response for AAGCMAN0000001", "Message Accepted", message.EM_MessageText);
		}

		[TestDate(2006, 11, 30, 23, 00, 00)]
		public void TestAcceptedPreliminaryTripMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedPreliminaryTripInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "20", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedPreliminary, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			var releaseDate = new ZDateTime(2006, 11, 30, 23, 00, 00);
			AssertEquals("shipment1.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Linked, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", releaseDate, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ShipmentEntryStatusList.Codes.Accepted, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", releaseDate, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Accepted Preliminary Trip Details Response for AAGCMAN0000001", "Message Accepted", message.EM_MessageText);
		}

		public void TestAcceptedPreliminaryConfirmationMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedPreliminaryTripConfirmationInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedComplete, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "22", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedComplete, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ZString.Empty, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ZString.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Accepted Preliminary Trip Details Response for AAGCMAN0000001", "Message Accepted", message.EM_MessageText);
		}

		public void TestAcceptedCrewPassengersDetailsMessage()
		{
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedCrewPassengersDetailsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "24", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", ZString.Empty, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertEquals("shipment1.B0_ReleaseStatus", ZString.Empty, shipment1.B0_ReleaseStatus);
			AssertEquals("shipment1.B0_ReleaseStatusDate", ZDateTime.Empty, shipment1.B0_ReleaseStatusDate);
			AssertEquals("shipment2.B0_ReleaseStatus", ZString.Empty, shipment2.B0_ReleaseStatus);
			AssertEquals("shipment2.B0_ReleaseStatusDate", ZDateTime.Empty, shipment2.B0_ReleaseStatusDate);
			AssertEmail("Accepted Crew/Passengers Details Response for AAGCMAN0000001", "Message Accepted", message.EM_MessageText);
		}

		public void TestMessageStatusIsNotAwaitingReply()
		{
			const string errorMessage = "An ERROR response message has been received from CBP for a Complete e-Manifest";
			trip.BH_MessageStatus = MessageStatusList.Codes.ClearOriginal;
			message = MessagingTestHelper.CreateMessage(Factory, eManifestResponseMessageWrapperTest.AcceptedWithErrorsInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", TripEntryStatusList.Codes.AcceptedPreliminary, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "13", message.EM_MessageNum);
			AssertEquals("EM_GB", branch2.PK, message.EM_GB);
			AssertEquals("BH_ReleaseStatus", TripEntryStatusList.Codes.AcceptedPreliminary, trip.BH_ReleaseStatus);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertEmail("Error Complete e-Manifest w/ACE ID Response for LOCKMAN0000001", errorMessage, message.EM_MessageText);
		}

		protected override IProcessorForTest GetProcessorForTest()
		{
			var processor = new ProcessForTest(logger);
			message = MessagingTestHelper.CreateMessage(Factory, AcceptedCompleteTripInterchangeText);
			Factory.Save();
			processor.ProcessMessage(message);
			return processor;
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = MessagingTestHelper.GetTripAwaitingReply(Factory, "84f89ebe-e700-4010-ade4-907fcba389e5", "MAN0000001", "LOCK");
			shipment1 = trip.Shipments.AddNew();
			shipment1.B0_MasterBillNumber = "KH041203101";
			shipment2 = trip.Shipments.AddNew();
			shipment2.B0_MasterBillNumber = "KH041203102";
			var userToNotify2 = Factory.NewWithValidTestData<GlbStaff>();
			userToNotify2.GS_FullName = "SecondUserToNotify";
			userToNotify2.GS_EmailAddress = "SecondUserToNotify@blah.com";
			userToNotify2.GS_Code = "NU2";
			branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch2 = Factory.NewWithValidTestData<GlbBranch>();
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "12", userToNotify2, branch1, ZDateTime.Now, CompleteManifestMessageText));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.eManifest, "13", userToNotify, branch2, ZDateTime.Now.AddDays(1)));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.UnassociatedShipments, "16", userToNotify, branch1, ZDateTime.Now.AddDays(2), eManifestOriginalMessageWrapperTest.UnassociatedShipmentsMessageText));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CompleteTrip, "18", userToNotify, branch1, ZDateTime.Now.AddDays(3), CompleteTripMessageText));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "20", userToNotify, branch1, ZDateTime.Now.AddDays(4), eManifestOriginalMessageWrapperTest.PreliminaryTripMessageText));
			preliminaryTripConfirmationMessage = MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "22", userToNotify, branch1, ZDateTime.Now.AddDays(5), PreliminaryTripConfirmationMessageText);
			trip.Messages.Add(preliminaryTripConfirmationMessage);
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.PreliminaryTrip, "24", userToNotify, branch1, ZDateTime.Now.AddDays(6), CrewPassengersDetailsMessageText));
			processor = new MessageProcessor { Logger = logger };
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		Trip trip;
		MessageProcessor processor;
		EDIMessage message;
		EDIMessage preliminaryTripConfirmationMessage;
		GlbBranch branch1;
		GlbBranch branch2;
		Shipment shipment1;
		Shipment shipment2;
		EmbeddedResourceRetriever resourceRetriever;

		void AssertEventLog(Logs logs, Event type, string reference, string source, ZDateTime? time = null)
		{
			var clearedEvent = logs.MostRecentLogByEventTime(type, reference);
			AssertNotNull("Event should exist", clearedEvent);
			AssertEquals("SL_Reference", reference, clearedEvent.SL_Reference);
			AssertEquals("SL_TableFriendlyName", source, clearedEvent.SL_TableFriendlyName);
			var eventTime = time.GetValueOrDefault();
			if (!eventTime.IsEmpty)
			{
				AssertEquals("SL_EventTime", eventTime, clearedEvent.SL_EventTime);
			}
		}

		void SaveFactoryAndExecuteBatch()
		{
			Factory.Save();
			processor.ExecuteBatch();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			trip = factory.Load<Trip>(trip.PK);
			shipment1 = factory.Load<Shipment>(shipment1.PK);
			shipment2 = factory.Load<Shipment>(shipment2.PK);
		}

		const string UnassociatedShipmentsChangeMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:03B:UN
BGM+87:::STANDARD+SYSTEM+2
RFF+ABO:SHP<<MSGNO PLACEHOLDER>>
CNI+1+:24
RFF+AAM:LOCKKH041203101
LOC+9+01535:78
GEI+7+135
NAD+OS+++WEBCOM+3480 PHARMACY AVENUE+SCARBOROUGH+ON:163+12345+CA
NAD+CN+++WIDGETS OF LOS ANGELES+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456+US
GID+1
PAC+500++BOX
FTX+AAA+++COMPUTERS
MEA+AAI++K:5000
CNI+2+:24
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

		const string CompleteTripMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+AAGCMAN0000001+22
DTM+132:201204270023:203
RFF+ABO:CTR<<MSGNO PLACEHOLDER>>
LOC+60+0901:77
DOC+700+:23
RFF+AAM:LOCKKH041203101
DOC+700+:23
RFF+AAM:LOCKKH041203102
NAD+CA+AAGC:172
NAD+VW+14133:109+++1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456
TDT+11++03+:::BT++I++:8::1234567890
TDT+11++03+:::BT++I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT++I++:274::12345678
TDT+11++03+:::BT++I++:215::EQU123:US
LOC+89+VA:163
UNT+13+<<MSGNO PLACEHOLDER>>
";

		const string PreliminaryTripConfirmationMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSREP:D:03B:UN
BGM+336:::STANDARD+AAGCMAN0000001+6
DTM+132:201204210023:203
RFF+ABO:PTR<<MSGNO PLACEHOLDER>>
UNT+5+<<MSGNO PLACEHOLDER>>
";

		const string CrewPassengersDetailsMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+PAXLST:D:03B:UN
BGM+10:::STANDARD+AAGCMAN0000001+2
RFF+ABO:CRW<<MSGNO PLACEHOLDER>>
TDT+11++03++AAGC:172
DTM+132:20120421:102
NAD+VW+14133:109++RICHARDSON:BRIAN:MICHAEL:::1+1234 SUNSETT BVD+LOS ANGELESE+CA:163+123456
ATT+2++M
DTM+329:19461224:102
EMP+4+++1:::YES
NAT+2+CA::5
DOC+39+FA246801
LOC+91+CA:162
DOC+5K+R568M2356217
LOC+91+AB:163
LOC+91+CA:162
UNT+16+<<MSGNO PLACEHOLDER>>
";

		const string StatusUpdateInterchangeText2 = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+177++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+177+UN+D:03B
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
DTM+163:200611301300:203
ERP+2
ERC+SN050
FTX+AAO+++Shipment Hold
DOC+929:::63+654984654
PAC+100
RFF+AAM:LOCKKH041203102
RFF+BM:1234654968486
LOC+5+1234:77
LOC+188+4567:77
LOC+219+9874:276
DTM+163:200612022300:203
ERP+2
ERC+SN053
FTX+AAO+++Shipment Released (Entered and released)
UNT+10+1778
UNE+1+1778
UNZ+1+1778
";

		const string TripStatusUpdateInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+999999999+CUSRES:D:03B:UN
BGM+34+LOCKMAN0000001
DTM+132:201204270023:203
DTM+163:201204241030:203
FTX+AIQ+++PTR22
TDT+11++03+:::BT+8CWS+I++:172
TDT+11+++++++:146::1M8GDM9AXKP042788
TDT+11+++++++:274::12345678
TDT+11+++++++:8::1234567890
LOC+24+0901:77
NAD+CA+0000000504:109
ERP+1
ERC+SN030
FTX+AAH+++Trip arrived
UNT+15+999999999
UNE+1+1778
UNZ+1+1778
";

		const string ShipmentsStatusUpdateWithIdenticalStatusesInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+999999999+CUSRES:D:03B:UN
BGM+34+LOCKMAN0000001
DTM+132:201204270023:203
FTX+AIQ+++PTR22
TDT+11++03+:::BT+8CWS+I++:172
TDT+11+++++++:146::1M8GDM9AXKP042788
TDT+11+++++++:274::12345678
TDT+11+++++++:8::1234567890
LOC+24+0901:77
NAD+CA+0000000504:109
DOC+929:::61+123456782
PAC+10
RFF+AAM:LOCKKH041203101
LOC+219+0901++::276
DTM+163:201204241030:203
ERP+2
ERC+SN050
FTX+AAH+++Shipment Hold
DOC+929:::63+456789012
PAC+20
RFF+AAM:LOCKKH041203102
LOC+219+0901++::276
DTM+163:201204241030:203
ERP+2
ERC+SN050
FTX+AAH+++Shipment Hold
UNT+51+999999999
UNE+1+1778
UNZ+1+1778
";

		const string ShipmentsStatusUpdateWithDifferentStatusesInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+999999999+CUSRES:D:03B:UN
BGM+34+LOCKMAN0000001
DTM+132:201204270023:203
FTX+AIQ+++PTR22
TDT+11++03+:::BT+8CWS+I++:172
TDT+11+++++++146:::1M8GDM9AXKP042788
TDT+11+++++++274:::12345678
TDT+11+++++++8:::1234567890
LOC+24+0901:77
NAD+CA+0000000504:109
DOC+929:::61+123456782
PAC+10
RFF+AAM:LOCKKH041203101
LOC+219+0901++::276
DTM+163:201204241030:203
ERP+2
ERC+SN050
FTX+AAH+++Shipment Hold
DOC+929:::63+456789012
PAC+20
RFF+AAM:LOCKKH041203102
LOC+219+0901++::276
DTM+163:201204241030:203
ERP+2
ERC+SN053
FTX+AAO+++Shipment Released (Entered and released)
UNT+51+999999999
UNE+1+1778
UNZ+1+1778
";

		const string ShipmentsStatusUpdateWithoutDocSegment = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+1+CUSRES:D:03B:UN
BGM+34+LOCKMAN0000001
DTM+132:201808021739:203
FTX+AIQ+++MAN39
TDT+11++03+:::TR+8CWS+I++:172
TDT+11+++++++146:::1GBJG312361240314
TDT+11+++++++8:::044415BAC12CA003
TDT+11+++++++109:::10660665
NAD+CA+0011639582:109
PAC+1
RFF+AAM:CIUC1212121212
DTM+163:201808021741:203
ERP+2
ERC+SN105
FTX+AAH+++Entry not on file
UNT+16+1
UNE+1+1778
UNZ+1+1778
";

		const string ShipmentsStatusUpdateWithDocSegment = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20061108:0939+1778++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20061108:0939+1778+UN+D:03B
UNH+1+CUSRES:D:03B:UN
BGM+34+LOCKMAN0000001
DTM+132:201808021739:203
FTX+AIQ+++MAN39
TDT+11++03+:::TR+8CWS+I++:172
TDT+11+++++++146:::1GBJG312361240314
TDT+11+++++++8:::044415BAC12CA003
TDT+11+++++++109:::10660665
NAD+CA+0011639582:109
DOC+ZZZ
PAC+1
RFF+AAM:CIUC1212121212
DTM+163:201808021741:203
ERP+2
ERC+SN105
FTX+AAH+++Entry not on file
UNT+16+1
UNE+1+1778
UNZ+1+1778
";

		const string AcceptedUnassociatedShipmentsInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+51+CUSRES:D:03B:UN
BGM+132:::STANDARD+SYSTEM
FTX+AIQ+++SHP16
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+7+51
UNE+1+1956
UNZ+1+1956
";

		const string ErrorUnassociatedShipmentsInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+97+CUSRES:D:03B:UN
BGM+132:::STANDARD+SYSTEM
FTX+AIQ+++SHP16
DOC+:ZZZ
RFF+AAM:LOCKKH041203102
LOC+9+01535
GEI+7+135
GEI+5
NAD+OS
NAD+CN
ERP+2
ERC+017
FTX+AAO+++Missing or Invalid Bill
ERP+2
ERC+060
FTX+AAO+++XXXX Bill Rejected XXXX
UNT+48+97
UNE+1+1956
UNZ+1+1956
";

		const string InvalidErrorUnassociatedShipmentsInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+110+CUSRES:D:03B:UN
BGM+132:::STANDARD+SYSTEM
FTX+AIQ+++SHP16
UNT+4+110
UNE+1+1956
UNZ+1+1956
";

		const string AcceptedCompleteTripInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+46+CUSRES:D:03B:UN
BGM+132:::STANDARD+AAGCMAN0000001+20
DTM+132:201204130023:203
FTX+AIQ+++CTR18
TDT+11++03+:::BT+AAGC+I++:146::1M8GDM9AXKP042788
TDT+11++03+:::BT+AAGC+I++:274::12345678
TDT+11++03+:::BT+AAGC+I++:8::1234567890
LOC+60+0901
RFF+RFA:03
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+13+46
UNE+1+1956
UNZ+1+1956
";

		const string AcceptedPreliminaryTripInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+92+CUSRES:D:03B:UN
BGM+132:::STANDARD+AAGCMAN0000001+2
DTM+132:201204210023:203
FTX+AIQ+++PTR20
TDT+11++03+:::TR+AAGC+I++:146::DUMMY
LOC+60+0901
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+10+92
UNE+1+1956
UNZ+1+1956
";

		const string AcceptedPreliminaryTripConfirmationInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+96+CUSRES:D:03B:UN
BGM+132:::STANDARD+AAGCMAN0000001+22
DTM+132:201204210023:203
FTX+AIQ+++PTR22
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+8+96
UNE+1+1956
UNZ+1+1956
";

		const string AcceptedCrewPassengersDetailsInterchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+LOCK:02+20041203:1623+1956++ACETEST
UNG+CUSRES+CBP-ACE-TEST:ZZ+LOCK:ZZ+20041203:1623+1956+UN+D:03B
UNH+95+CUSRES:D:03B:UN
BGM+132+AAGCMAN0000001+2
DTM+132:201204210000:203
FTX+AIQ+++CRW24
ERP+1
ERC+081
FTX+AAO+++Manifest Transmittal
UNT+8+95
UNE+1+1956
UNZ+1+1956
";

		sealed class ProcessForTest : eManifestResponseMessageProcessor, IProcessorForTest
		{
			public ProcessForTest(LoggingInformation logger) : base(logger)
			{
			}

			public new string AcknowledgementEmailMode => base.AcknowledgementEmailMode;
			public new string ImpedimentEmailMode => base.ImpedimentEmailMode;
			public new string ErrorEmailMode => base.ErrorEmailMode;
			public new EDIMessage originalMessage => base.originalMessage;
		}
	}
}
