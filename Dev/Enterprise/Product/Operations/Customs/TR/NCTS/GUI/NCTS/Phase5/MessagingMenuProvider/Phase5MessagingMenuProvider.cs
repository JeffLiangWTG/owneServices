using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
	{
		public Phase5MessagingMenuProvider(NctsHeader header) : base(header) { }

		public Phase5MessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper) { }

		public new NctsHeader Header => (NctsHeader)base.Header;

		protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
		{
			foreach (var menuItem in base.CreateMenuItemsCore())
			{
				yield return menuItem;
			}
		}

		protected override void SendToCustomsCore(ZMenuItem menuItem)
		{
			if (SaveAndContinue(menuItem))
			{
				var providerFactory = new TRCustomsMessagingProviderFactory(Phase5NctsCustomsTR5MessagingProvider.New, string.Empty);
				_ = TRCustomsMessagingGui.SendMessages(Header, providerFactory, ParentForm);
			}
		}

		protected override MessageSendingForm GetMessageSendingFormCore(EU.NCTS.Business.NctsHeaderMessageSendingObjectParent messageSendingobjectParent) => new MessageSendingForm(messageSendingobjectParent);
	}
}
