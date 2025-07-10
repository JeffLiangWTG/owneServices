using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.NCTS.Business.Messaging
{
	public class NctsMessageSender : INctsMessageSender
	{
		public bool CreateMessage(EU.NCTS.Business.NctsHeader nctsHeader, ISendsMessagesToCustoms sendMessagesToCustoms, NctsMessageFunctionSet messageFunction)
		{
			if (!TRCustomsDataRegistry.Instance.EnableTRNCTS.Value)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("7BD851EE-BAFA-44D8-B835-429C0B6F6B88", "NCTS has not been implemented for the country of departure or destination selected.\r\nPlease select a departure or destination office in another country."));
				return false;
			}

			if (nctsHeader != null)
			{
				var transmissionGenerator = new TRNctsTransmissionMessageGenerator(messageFunction);
				var messageManager = new NctsMessageManager(nctsHeader, transmissionGenerator);
				messageManager.SendNctsMessage(sendMessagesToCustoms);
			}
			return true;
		}
	}
}
