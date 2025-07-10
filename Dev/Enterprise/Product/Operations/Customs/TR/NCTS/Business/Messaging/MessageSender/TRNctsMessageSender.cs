using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.NCTS.Business.Messaging
{
	public class TRNctsMessageSender : IMessageSender
	{
		public TRNctsMessageSender(NctsHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		NctsHeader Header { get; }

		public BusinessObject Parent => Header;

		public IBusinessObjectCollection Messages => Header.Messages;

		public ZString JobReference => Header.BH_JobReference;
	}
}
