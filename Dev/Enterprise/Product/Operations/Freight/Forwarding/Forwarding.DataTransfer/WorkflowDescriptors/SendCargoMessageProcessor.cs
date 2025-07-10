using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class SendCargoMessageProcessor : IProcessor
	{
		public SendCargoMessageProcessor(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
		}

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token)
		{
			new ConsolCustomsCargoMessageHelper(consol, notifications).SendConsolCargoMessage(false);
		}

		#endregion

		readonly ForwardingConsol consol;
	}
}

