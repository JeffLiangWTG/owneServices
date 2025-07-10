using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDEDIMessageCollection))]
	public class CMDEDIMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadToEnsureThatRelationshipFilterIsCorrect()
		{
			AssertMessageCollection(Collection, "Pre-condition, should not have any elements");
			ZString code = EDIMessage.ApplicationCodes.SingaporeCMD;
			CreateMessage(code, JobShipmentSchema.Constants.TableName, Shipment.PK, code, "Testing123");
			AssertMessageCollection(Collection, "Message Testing123 should be added to the collection", "Testing123");
			CreateMessage(EDIMessage.ApplicationCodes.SeaCargo, JobShipmentSchema.Constants.TableName, Shipment.PK, code, "1234");
			AssertMessageCollection(Collection, "Application Code SeaCargo is wrong, should not be added to the collection", "Testing123");
			CreateMessage(code, JobConsolSchema.Constants.TableName, Shipment.PK, code, "1234");
			AssertMessageCollection(Collection, "LinkTable JobConsol is wrong, should not be added to the collection", "Testing123");
			CreateMessage(code, JobShipmentSchema.Constants.TableName, ZGuid.NewZGuid(), code, "1234");
			AssertMessageCollection(Collection, "LinkUniqueID is not the Shipment's PK, should not be added to the collection", "Testing123");
			CreateMessage(code, JobShipmentSchema.Constants.TableName, Shipment.PK, "FNA", "1234");
			AssertMessageCollection(Collection, "MessageType is not CMD, should not be added to the collection", "Testing123");
			CreateMessage(code, JobShipmentSchema.Constants.TableName, Shipment.PK, code, "1234");
			AssertMessageCollection(Collection, "Message 1234 should be added to the collection", "Testing123", "1234");
		}

		public void TestNewChild()
		{
			CMDEDIMessage message = Collection.AddNew();
			AssertEquals(Shipment.PK, message.EM_LinkUniqueID);
		}

		public void TestIsManagedForDataRefresh()
		{
			Assert("Should be set to true in the constructor", Collection.IsManagedForDataRefresh);
		}

		public void TestCollectionIsSetToReadOnlyOnConstruction()
		{
			Assert("Should be set to ReadOnly in the constructor", Collection.ReadOnly);
			CMDEDIMessage newMessage = Collection.AddNew();
			Assert("Children should be ReadOnly as well", newMessage.ReadOnly);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CMDShipmentWrapper shipmentStatus = new CMDShipmentWrapper(shipment);
			return new CMDEDIMessageCollection(shipmentStatus);
		}

		CMDEDIMessage CreateMessage(ZString applicationCode, ZString linkTable, ZGuid linkUniqueID, ZString messageType, ZString messageText)
		{
			CMDEDIMessage message = Factory.New<CMDEDIMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_LinkTable = linkTable;
			message.EM_LinkUniqueID = linkUniqueID;
			message.EM_MessageType = messageType;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNumber.ToString();
			messageNumber++;
			return message;
		}

		void AssertMessageCollection(CMDEDIMessageCollection collection, ZString assertionMessage, params ZString[] expectedMessagesText)
		{
			collection.Load();
			collection.Sort(EDIMessageSchema.Constants.EM_MessageNum, ListSortDirection.Ascending);
			AssertEquals(assertionMessage, expectedMessagesText.Length, collection.Count);
			for (int i = 0; i < expectedMessagesText.Length; i++)
			{
				AssertEquals("MessageText is not as expected", expectedMessagesText[i], collection[i].EM_MessageText);
			}
		}

		ForwardingShipment Shipment
		{
			get
			{
				return Collection.Parent.Shipment;
			}
		}

		new CMDEDIMessageCollection Collection
		{
			get
			{
				return (CMDEDIMessageCollection)base.Collection;
			}
		}

		int messageNumber = 1;
		#endregion
	}
}
