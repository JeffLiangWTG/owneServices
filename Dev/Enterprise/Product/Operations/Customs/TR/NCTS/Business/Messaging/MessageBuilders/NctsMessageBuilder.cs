using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.TR.NCTS.Business.Messaging
{
	public class NctsMessageBuilder : INctsNativeBuilder
	{
		public ZString NativeMessage<T>(T nctsHeader, NctsMessageFunctionSet messageFunction, ErrorCollector errorCollector)
		{
			var trNctsHeader = nctsHeader as NctsHeader;
			var messageText = ZString.Empty;
			if (trNctsHeader == null)
			{
				return messageText;
			}

			if (trNctsHeader.IsDepartureMovement)
			{
			}
			else
			{
			}

			return messageText;
		}
	}
}
