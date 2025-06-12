using Microsoft.BizTalk.Message.Interop;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public interface IHttpExTransferrer
	{
		IBaseMessage SendRequest(IBaseMessage message);
	}
}
