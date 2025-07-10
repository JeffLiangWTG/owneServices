using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using static Enterprise.Integration.Customs.US.InBond;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class InBondWPMQEDIMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenenerateMessage()
		{
			var dummy = Factory.New<DummyIInBondWP>();
			var builder = new InBondWPMQEDIMessageBuilder();
			var message = builder.Generate(dummy, InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);
			AssertEquals("Should have one message", 1, dummy.Messages.Count);
			AssertEquals(message, dummy.Messages[0]);
			AssertEquals(dummy, dummy.Messages[0].EM_LinkedObject);
			AssertEquals("Status", true, new ImportMessageStatusList().IsWaitingForResponse(dummy.MessageStatus));
			AssertEquals("Message Owner", Constants.ACE, message.EM_MessageOwner);

			message = builder.Generate(dummy, InBondWPActionCodeList.Codes.DiversionRequest);
			AssertEquals("Message SubType", EM_MessageSubTypeList.Codes.InBondDiversionRequest, message.EM_MessageSubType);

			var iBill = Factory.New<ICusInBondBill>();
			var iMessageBill = Factory.New<DummyIInBondWP>();
			message = builder.Generate(dummy, iBill, iMessageBill, InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination);
			AssertEquals(1, iMessageBill.Messages.Count);
			AssertEquals(message, iMessageBill.Messages[0]);
			AssertEquals(iMessageBill, iMessageBill.Messages[0].EM_LinkedObject);
			AssertEquals(true, new ImportMessageStatusList().IsWaitingForResponse(iMessageBill.MessageStatus));

			var iContainer = Factory.New<ICusInBondContainer>();
			var iMessageContainer = Factory.New<DummyIInBondWP>();
			message = builder.Generate(dummy, iBill, iMessageBill, iContainer, iMessageContainer, InBondWPActionCodeList.Codes.ArriveContainerAtDestination);
			AssertEquals(1, iMessageContainer.Messages.Count);
			AssertEquals(message, iMessageContainer.Messages[0]);
			AssertEquals(iMessageContainer, iMessageContainer.Messages[0].EM_LinkedObject);
			AssertEquals(true, new ImportMessageStatusList().IsWaitingForResponse(iMessageContainer.MessageStatus));
		}
	}
}
