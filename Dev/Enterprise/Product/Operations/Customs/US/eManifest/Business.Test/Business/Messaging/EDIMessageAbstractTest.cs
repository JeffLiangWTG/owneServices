using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	abstract class EDIMessageAbstractTest<T> : EnterpriseBusinessObjectTestCase where T : EDIMessage
	{
		public void TestMessageSubTypeDescription()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("EM_MessageSubTypeDescription", MessageSubTypeCodes.Descriptions.Original, message.EM_MessageSubTypeDescription);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = MessageTypes.Codes.eManifest;
			AssertEquals("EM_MessageSubTypeDescription", MessageTypes.Descriptions.eManifest, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = EntryStatusList.Codes.Error;
			AssertEquals("EM_MessageSubTypeDescription", EntryStatusList.Descriptions.Error, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = TripEntryStatusList.Codes.HoldTrip;
			AssertEquals("EM_MessageSubTypeDescription", TripEntryStatusList.Descriptions.HoldTrip, message.EM_MessageSubTypeDescription);

			message.EM_MessageSubType = ShipmentEntryStatusList.Codes.ShipmentHold;
			AssertEquals("EM_MessageSubTypeDescription", ShipmentEntryStatusList.Descriptions.ShipmentHold, message.EM_MessageSubTypeDescription);
		}

		public void TestMessageNumberFilledIn()
		{
			Db.Connection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.EDIFACTNumberFountain("M", EDIMessage.ApplicationCodes.USeManifest, EDIMessage.ApplicationCodes.USCustoms).SetNext(Factory, 12348);
			}
			finally
			{
				Db.Connection.CommitTransaction(); // Updating next number fountain value for the test
			}
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = 12348", message.EM_MessageText);
		}

		public void TestOriginalMessage()
		{
			const string messageNum1 = "1";
			const string messageNum2 = "2";
			message = GetOriginalMessage(messageNum1);
			message.EM_IsActive = false;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;

			var original1 = GetOriginalMessage(messageNum1);
			var original2 = GetOriginalMessage(messageNum2);
			var received = GetReceivedMessage(messageNum1);

			AssertEquals(original1.PK, received.OriginalMessage.PK);
			AssertNull(original1.OriginalMessage);

			received.EM_MessageNum = messageNum2;
			AssertEquals(original2.PK, received.OriginalMessage.PK);
			AssertNull(original2.OriginalMessage);
		}

		public override void TestCloneAuditProperties()
		{
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_SystemCreateUser = User.ServiceUserCode;

			var clone = (T)message.Clone();
			AssertEquals("EM_SystemCreateTimeUtc should be copied when clonned by message processor", message.EM_SystemCreateTimeUtc, clone.EM_SystemCreateTimeUtc);
			AssertEquals("EM_SystemCreateUser don't need to be copied as far as will be set to ~BP anyway", ZString.Empty, clone.EM_SystemCreateUser);
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected T GetReceivedMessage(string messageNum, string messageText = "")
		{
			var received = (T)GetNewBusinessObject();
			received.EM_MessageNum = messageNum;
			received.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			received.EM_Status = EDIMessage.Status.Received;
			received.EM_MessageText = messageText;
			return received;
		}

		protected T GetOriginalMessage(string messageNum, string messageText = "")
		{
			var original = (T)GetNewBusinessObject();
			original.EM_MessageNum = messageNum;
			original.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			original.EM_Status = EDIMessage.Status.Sent;
			original.EM_MessageText = messageText;
			return original;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (T)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<T>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = (T)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		protected T message;
	}
}
