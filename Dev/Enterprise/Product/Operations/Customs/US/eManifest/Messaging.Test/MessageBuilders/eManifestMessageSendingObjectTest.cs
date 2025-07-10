using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(eManifestMessageSendingObject))]
	sealed class eManifestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEndToEnd()
		{
			var trip = eManifestMessageWrapperTest.GetTrip(Factory, MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, testSaveAndLoad: false, isFinalized: true, headerOnly: false, addCustomsBroker: true);
			var sendObject = new eManifestMessageSendingObject(new eManifestMessageSendingObjectParent(trip), MessageTypes.Codes.UnassociatedShipments, MessageSubTypes.Create, 1);
			var builder = eManifestMessageManagerHelper.GetMessageBuilder(sendObject, sendObject.MessageType, sendObject.ActionCode);
			var messageResult = builder.PopulateMessages().GetBuilderResults().FirstOrDefault();
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
UNT+45+<<MSGNO PLACEHOLDER>>
";
			AssertMultilineASCIIEquals("Message text", expectedResult, messageResult.Message.EM_FormattedMessageText);
		}

		public void TestMessageDescription()
		{
			var trip = Factory.New<Trip>();
			var objectParent = new eManifestMessageSendingObjectParent(trip);
			var sendObject = new eManifestMessageSendingObject(objectParent, MessageTypes.Codes.Acknowledgement, MessageSubTypes.Confirmation, 1);
			AssertEquals("Confirm Acknowledgement", sendObject.MessageDescription);
			sendObject = new eManifestMessageSendingObject(objectParent, MessageTypes.Codes.CompleteTrip, MessageSubTypes.Create, 1);
			AssertEquals("Complete Trip Details", sendObject.MessageDescription);
		}

		public void TestConveyanceEquipmentID()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var conveyance = Factory.NewWithValidTestData<Equipment>();
			trip.Equipment.DeleteAll();
			trip.Equipment.Add(conveyance);
			conveyance.BJ_IsConveyance = true;
			conveyance.BJ_VIN = "12345678901234567";
			var sendingObject = new eManifestMessageSendingObject(new eManifestMessageSendingObjectParent(trip), MessageTypes.Codes.CompleteTrip, MessageSubTypes.Create, 1);
			AssertEquals("12345678901234567", sendingObject.Conveyance.EquipmentId);
			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_VIN = "10203040506070809000";
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertEquals("10203040506070809", sendingObject.Conveyance.EquipmentId);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var trip = Factory.New<Trip>();
			return new eManifestMessageSendingObject(new eManifestMessageSendingObjectParent(trip), MessageTypes.Codes.Acknowledgement, MessageSubTypes.Confirmation, 1);
		}
	}
}
