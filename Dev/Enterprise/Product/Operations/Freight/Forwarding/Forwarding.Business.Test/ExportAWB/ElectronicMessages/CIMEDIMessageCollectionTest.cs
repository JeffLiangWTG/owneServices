using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(CIMEDIMessageCollection))]
	sealed class CIMEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHVLVConsignmentFHLMessageIncluded()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Assert("Precondition: CIMEDIMessages is empty", !consol.CIMEDIMessages.Any());

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var fhlMessage = consol.CIMEDIMessages.AddNew();
			fhlMessage.EM_LinkTable = HVLVConsignmentSchema.Constants.TableName;
			fhlMessage.EM_LinkUniqueID = consignment.PK;
			fhlMessage.EM_MessageType = "FHL";
			fhlMessage.EM_ApplicationCode = "CIM";

			Factory.Save();

			var collection = new CIMEDIMessageCollection(consol);
			collection.Load(new ZQuery());

			CombineAssertions("HVLV consignment FHL message included", () =>
			{
				Assert("CIMEDIMessages is not empty", collection.Any());
				Assert("CIMEDIMessages include HVLV consignment FHL message", collection.Cast<EDIMessage>().Any(x => x.EM_LinkUniqueID == consignment.PK));
			});
		}

		public void TestReadonly()
		{
			Messages.AddNew();
			Messages.AddNew();
			foreach (CIMEDIMessage message in Messages)
			{
				AssertEquals(ZBool.True, message.ReadOnly);
			}
		}

		public void TestIndexor()
		{
			CIMEDIMessage message = Messages.AddNew();
			message.EM_ApplicationReference = "1";

			message = Messages.AddNew();
			message.EM_ApplicationReference = "2";

			message = Messages.AddNew();
			message.EM_ApplicationReference = "3";

			AssertEquals("1", Messages[0].EM_ApplicationReference);
			AssertEquals("2", Messages[1].EM_ApplicationReference);
			AssertEquals("3", Messages[2].EM_ApplicationReference);
		}

		public void TestCIMEDIMessageFKCorrectlySet()
		{
			Messages.AddNew();
			AssertEquals(Consol.PK, Messages[0].EM_LinkUniqueID);
			AssertEquals(ForwardingConsol.Schema.TableName, Messages[0].EM_LinkTable);
		}

		public void TestGetLatestTransmittedMessage()
		{
			CIMEDIMessage message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "0";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 1);
			Factory.Save();

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "2";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 2);
			Factory.Save();

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = "1";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 3);
			Factory.Save();

			AssertEquals("1", Messages.GetLatestTransmittedMessage().EM_ApplicationReference);
		}

		public void TestCurrentStatus()
		{
			AssertEquals("No messages have been sent", Messages.CurrentStatus);

			CIMEDIMessage message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 1);
			Factory.Save();
			AssertEquals("Message Queued for Sending", Messages.CurrentStatus);

			message.EM_Status = EDIMessage.Status.Sent;
			AssertEquals("Message Sent", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FMA;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 2);
			Factory.Save();
			AssertEquals("Message Receipt Acknowledged", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FNA;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 3);
			Factory.Save();
			AssertEquals("Message Error Received", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FMA;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 4);
			Factory.Save();
			AssertEquals("Message Error Received", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 5);
			Factory.Save();
			AssertEquals("Message Queued for Sending", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FSA;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 6);
			Factory.Save();
			AssertEquals("Message Receipt Acknowledged", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FSU;
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 7);
			Factory.Save();
			AssertEquals("Message Receipt Acknowledged", Messages.CurrentStatus);

			message = Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FNA;
			message.EM_MessageText =
				@"
FNA
QF
FNA
ACK/AWB REJECTED
/DUPLICATE AWB
FWB/9
232-67350253ADLBUD/T1K95";
			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 1, 1, 0, 0, 8);
			Factory.Save();
			AssertEquals("Message Receipt Acknowledged", Messages.CurrentStatus);
		}

		public void TestLoad_NoRefreshBindingWhenCollectionIsLoading()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals("Pre-condition", 0, consol.CIMEDIMessages.Count);

			var refreshBindingCount = 0;
			consol.AWBCurrentStatusInfo.ValueChanged += (obj, eventArgs) =>
			{
				refreshBindingCount += 1;
			};

			var message1 = Factory.New<CIMEDIMessage>();
			message1.EM_LinkTable = JobConsolSchema.Constants.TableName;
			message1.EM_LinkUniqueID = consol.PK;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_MessageType = CargoIMPMessageTypeList.Codes.FHL;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CIM;
			message1.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			var message2 = Factory.New<CIMEDIMessage>();
			message2.EM_LinkTable = JobConsolSchema.Constants.TableName;
			message2.EM_LinkUniqueID = consol.PK;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageType = CargoIMPMessageTypeList.Codes.FHL;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CIM;
			message2.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;

			consol.CIMEDIMessages.Load();

			CombineAssertions("Load CIMEDIMessages collection should only call AWBCurrentStatusInfo.RefreshBinding() once", () =>
			{
				AssertEquals("2 messages are loaded into collection", 2, consol.CIMEDIMessages.Count);
				AssertEquals("RefreshBinding() is only called once", 1, refreshBindingCount);
			});
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2);
			Factory.Save();

			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);
		}

		#region Implementation

		#region Consol

		ForwardingConsol Consol
		{
			get { return fConsol ?? (fConsol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol fConsol;

		#endregion

		#region Messages

		CIMEDIMessageCollection Messages
		{
			get { return fMessages ?? (fMessages = new CIMEDIMessageCollection(Consol)); }
		}
		CIMEDIMessageCollection fMessages;

		#endregion

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Messages;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CIMEDIMessage>();
		}

		#endregion
	}
}
