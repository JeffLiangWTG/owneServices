using Microsoft.BizTalk.Message.Interop;
using System.IO;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class TransferrerMessageFactory : ITransferrerMessageFactory
	{
		public IBaseMessage CreateMessage(IBaseMessageFactory baseFactory, string fileName, string location, string transportLocation, string transportType, Stream fs)
		{
			var part = baseFactory.CreateMessagePart();
			part.Data = fs;
			var message = baseFactory.CreateMessage();
			message.AddPart("body", part, true);

			var context = new SystemMessageContext(message.Context);
			context.InboundTransportLocation = transportLocation;
			context.InboundTransportType = transportType;

			message.Context.Write("Uri", "http://cargowise.com/ehub/adapters", location);
			message.Context.Write("FileName", "http://cargowise.com/ehub/adapters", fileName);
			message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/ftp-properties", fileName);
			return message;
		}
	}
}
