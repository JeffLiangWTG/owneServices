using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(CBPEDIMessageCollectionForTesting))]
	sealed class CBPEDIMessageCollectionTest : Enterprise.Messaging.Business.EDIMessageCollectionTest
	{
		public void TestSettingEM_SendWithMessageErrorsForNewChild()
		{
			Factory.SetValue(() => new FactoryValues() { Value = true });
			var collection = new CBPEDIMessageCollectionForTesting(Factory);
			collection.Master.AddRowMessageError("BLAH");
			var message1 = collection.AddNew(typeof(CBPMessageForTesting));
			AssertEquals(true, message1.EM_SendWithMessageErrors);
			Factory.SetValue(() => new FactoryValues() { Value = false });
			var message2 = collection.AddNew(typeof(CBPMessageForTesting));
			AssertEquals(false, message2.EM_SendWithMessageErrors);
		}

		public void TestGetMatchedErrorBlocks()
		{
			var collection = new CBPEDIMessageCollectionForTesting(Factory);
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;
			message.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			collection.Add(message);

			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message1.EM_MessageText = new ZZZB2() { StringB = "B1" }.Serialise() + new ZZZD() { StringD = "ACCEPTED - RECORDS REQUIRED", DecimalD = 10m }.Serialise() + new ZZZY2() { StringY = "Y1" }.Serialise();
			collection.Add(message1);

			AssertEquals("ACCEPTED - RECORDS REQUIRED", ((IStatusesAndErrors)collection.GetMatchedErrorBlocks(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting2)[0]).NarrativeMessage);
		}

		public void TestGetLastMessageWithSpecificMessageBlock()
		{
			var coll = (CBPEDIMessageCollection)GetCollectionToTest();
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzc2 = new ZZZC() { StringC = "C2", DateC = ZDate.BrettsBirthday.AddDays(6), DecimalC = 6m, IntC = 6, ShortC = 7 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };

			var message1 = Factory.New<CBPMessageForTesting>();
			message1.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message1.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting1;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			message1.EM_MessageText = zzb1.Serialise() + zzc1.Serialise() + zzy1.Serialise();
			coll.Add(message1);

			var message2 = Factory.New<CBPMessageForTesting>();
			message2.EM_ReceiveTransmit = CBPEDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.DummyForTesting2;
			message1.EM_SystemCreateTimeUtc = ZDateTime.Today;
			message2.EM_MessageText = zzb1.Serialise() + zzc2.Serialise() + zzy1.Serialise();
			coll.Add(message2);

			AssertEquals(message2, coll.GetLastMessageWithSpecificMessageBlock<CBPMessageForTesting>(CBPEDIInterchange.ApplicationCodeForTesting, ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIMessage.Direction.Receive, typeof(ZZZC)));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CBPEDIMessageCollectionForTesting(Factory);
	}

	sealed class CBPEDIMessageCollectionForTesting : CBPEDIMessageCollection
	{
		public CBPEDIMessageCollectionForTesting(BusinessObjectFactory factory)
			: base(factory.New<DummyBusinessObject>(), new ZQuery())
		{
		}

		internal new CBPMessageForTesting AddNew(Type bizOType) => (CBPMessageForTesting)base.AddNew(bizOType);

		public override Type GetTypeOfElementsFromPK(ZGuid pK) => typeof(CBPMessageForTesting);
	}
}
