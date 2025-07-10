using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Customs.TR.NCTS.Business.MessagingProcess;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.TR.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class TRNctsDepartureMovementMessagingMenuProvider : NctsDepartureMovementMessagingMenuProvider
	{
		public TRNctsDepartureMovementMessagingMenuProvider(EU.NCTS.Business.NctsHeader header, NctsMovementForm nctsMovementForm)
		: base(header)
		{
			ParentForm = nctsMovementForm;
		}
		NctsHeader TRNctsHeader => (NctsHeader)Header;

		public TRNctsDepartureMovementMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		protected override void SendDepartureMessasgeClickCore(object sender)
		{
			var menu = sender as ZMenuItem;
			if (SaveAndContinue(menu))
			{
				var providerFactory = new TRCustomsMessagingProviderFactory(NctsCustomsMessagingProvider.New, string.Empty);
				_ = TRCustomsMessagingGui.SendMessages(TRNctsHeader, providerFactory, ParentForm);
			}
		}
	}
}
