using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public interface IHttpExTransferrerFactory
	{
		IHttpExTransferrer CreateTransferrer(IBaseMessage message, string propertyNamespace, IBaseMessageFactory messageFactory);
	}
}
