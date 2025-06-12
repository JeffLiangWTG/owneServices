using Microsoft.BizTalk.Message.Interop;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public interface ITransferrerMessageFactory
	{
		IBaseMessage CreateMessage(IBaseMessageFactory baseFactory, string fileName, string location, string transportLocation, string transportType, Stream fs);
	}
}
