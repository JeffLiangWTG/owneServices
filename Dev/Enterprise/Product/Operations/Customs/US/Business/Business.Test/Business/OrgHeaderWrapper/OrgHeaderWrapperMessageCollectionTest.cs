using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapperMessageCollection))]
	public class OrgHeaderWrapperMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestCollectionIncludesMessagesFromAddress()
		{
			var org1 = Factory.New<OrgHeader>();
			var address1 = org1.Addresses.AddNew();
			var address2 = org1.Addresses.AddNew();

			var org1Message = Factory.New<MQEDIMessage>();
			org1Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			org1Message.EM_LinkedObject = org1;

			var address1Message = Factory.New<MQEDIMessage>();
			address1Message.EM_LinkedObject = address1;
			address1Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;

			var org2 = Factory.New<OrgHeader>();
			var address3 = org2.Addresses.AddNew();
			var org2Message = Factory.New<EDIMessage>();
			org2Message.EM_LinkedObject = org2;
			org2Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			var address3Message = Factory.New<EDIMessage>();
			address3Message.EM_LinkedObject = address3;
			address3Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;

			var wrapper = OrgHeaderWrapper.New(org1);

			var collection = wrapper.Messages;
			AssertEquals(2, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);

			var address2Message = Factory.New<MQEDIMessage>();
			address2Message.EM_LinkedObject = address2;
			address2Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;

			AssertEquals(2, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);

			collection.Load();
			AssertEquals(3, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);
			AssertCollectionContains(address2Message, collection);

			var address4Message = Factory.New<EDIMessage>();
			address4Message.EM_LinkedObject = address1;
			address4Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			AssertEquals(3, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);
			AssertCollectionContains(address2Message, collection);

			collection.Add(address4Message);
			AssertEquals(4, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);
			AssertCollectionContains(address2Message, collection);
			AssertCollectionContains(address4Message, collection);
			AssertEquals(address1, address4Message.EM_LinkedObject);

			collection.Add(org2Message);
			AssertEquals(5, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);
			AssertCollectionContains(address2Message, collection);
			AssertCollectionContains(address4Message, collection);
			AssertCollectionContains(org2Message, collection);
			AssertEquals(org1, org2Message.EM_LinkedObject);

			collection.Add(address3Message);
			AssertEquals(6, collection.Count);
			AssertCollectionContains(org1Message, collection);
			AssertCollectionContains(address1Message, collection);
			AssertCollectionContains(address2Message, collection);
			AssertCollectionContains(address4Message, collection);
			AssertCollectionContains(org2Message, collection);
			AssertCollectionContains(address3Message, collection);
			AssertEquals(org1, address3Message.EM_LinkedObject);

			var address5Message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			address5Message.EM_LinkedObject = address1;
			address5Message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NativeDataMessaging;

			collection.Load();
			Assert("Should only contain USI messages", !collection.Contains(address5Message));
		}

		public void TestTypedSingleParameterAddNew()
		{
			var collection = (OrgHeaderWrapperMessageCollection)GetCollectionToTest();

			var collectionType = collection.GetType();
			var method = collectionType.GetMethod("AddNew", new Type[] { typeof(Type) });

			var bizO2 = (BusinessObject)method.Invoke(collection, new object[] { typeof(MQEDIMessage) });
			AssertNotNull("AddNew of Type " + method.ReturnType.FullName + " not null", bizO2);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgHeaderWrapperMessageCollection(Organisation);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<EDIMessage>();
		}

		OrgHeader Organisation
		{
			get { return org ?? (org = Factory.New<OrgHeader>()); }
		}
		OrgHeader org;
	}
}
