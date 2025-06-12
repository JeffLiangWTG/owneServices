using Microsoft.BizTalk.Message.Interop;
using System.Net;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public interface IWebRequestFactory
	{
        WebRequest CreateRequest(IBaseMessage message, WSHttpExProperties config);
        WebRequest PreAuthenticateRequest(IBaseMessage message, WSHttpExProperties config);
	}
}
