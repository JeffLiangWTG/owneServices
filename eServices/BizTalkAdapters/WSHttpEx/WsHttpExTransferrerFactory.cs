using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public class WsHttpExTransferrerFactory : IHttpExTransferrerFactory
	{
		IWebRequestFactory requestFactory = new WSHttpExRequestFactory();

		public IHttpExTransferrer CreateTransferrer(IBaseMessage message, string propertyNamespace, IBaseMessageFactory messageFactory)
		{
			return new WSHttpExTransferrer(message, propertyNamespace, messageFactory, requestFactory);
		}
	}
}
