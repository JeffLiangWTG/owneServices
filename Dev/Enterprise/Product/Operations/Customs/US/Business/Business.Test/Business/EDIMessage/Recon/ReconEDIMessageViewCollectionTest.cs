using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconEDIMessageViewCollection))]
	sealed class ReconEDIMessageViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestProperties()
		{
			var collection = new ReconEDIMessageViewCollection(Factory);
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
			AssertEquals(0, collection.Count);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declMsg = declaration.Messages.AddNew(typeof(MQEDIMessage));
			declMsg.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			collection.Add(declMsg);
			var loadedMsg = collection[0];
			AssertEquals(declaration.PK, loadedMsg.EM_LinkUniqueID);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryMsg = entry.Messages.AddNew(typeof(MQEDIMessage));
			entryMsg.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			collection.Add(entryMsg);
			loadedMsg = collection[1];
			AssertEquals(entry.PK, loadedMsg.EM_LinkUniqueID);
			AssertExceptionThrown(typeof(NotSupportedException), () => collection.AddNew());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ReconEDIMessageViewCollection(Factory);
	}
}
