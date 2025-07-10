using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.GUI.Testing;

[TestedType(typeof(Phase5MessagingMenuProvider))]
sealed class Phase5MessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestGetProvider()
	{
		var nctsHeader = Factory.NewMoq<NctsHeader>().Object;
		AssertType<Phase5MessagingMenuProvider>("Provider Type", EU.NCTS.GUI.Phase5MessagingMenuProvider.GetProvider(nctsHeader));
	}

	public void TestMessageSendingForm()
	{
		var nctsHeader = Factory.NewMoq<NctsHeader>().Object;
		var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);

		var menuProvider = new Phase5MenuProviderForTest(nctsHeader);
		using var messageSendingForm = menuProvider.GetMessageSendingFormExposed(sendingObjectParent);
		AssertType<MessageSendingForm>("Message Sending Form Type", messageSendingForm);
	}

	class Phase5MenuProviderForTest : Phase5MessagingMenuProvider
	{
		public Phase5MenuProviderForTest(NctsHeader header) : base(header)
		{
		}

		public EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormExposed(NctsHeaderMessageSendingObjectParent sendingObjectParent)
			=> GetMessageSendingFormCore(sendingObjectParent);
	}
}
