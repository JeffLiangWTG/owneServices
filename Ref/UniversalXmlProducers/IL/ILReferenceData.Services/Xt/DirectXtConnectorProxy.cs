using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;
using ILogger = CargoWise.xTMessaging.Integration.ILogger;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public class DirectXtConnectorProxy : DirectxTConnector
	{
		public DirectXtConnectorProxy(IMsgClientProvider msgClientProvider, IDirectxTMessagingConfig xTMessagingConfig, ILogger logger, ISubmitMsgAttributeModifier msgAttributeModifier, IReceiveHandler receiveHandler)
			: base(msgClientProvider, xTMessagingConfig, logger, msgAttributeModifier, receiveHandler)
		{
		}

		public async Task<(bool, long, string)> SendAsync(IXtMessageInfo xtMessageInfo)
		{
			Argument.NotNull(xtMessageInfo, nameof(xtMessageInfo));
			xtMessageInfo.XTMessageAttributes.TryGetValue(CargoWise.xTMessaging.Integration.Constants.CustomMsgAttributes.MessageSubType, out var messageSubType);
			var sw = Stopwatch.StartNew();
			InitializeIfNeeded();
			var (sentSuccessful, msgId, errorNote) = await SendInterchange(xtMessageInfo, CancellationToken.None);

			sw.Stop();

			if (!sentSuccessful)
			{
				return (false, 0, $"Sending Direct Xt Message {messageSubType} failed:{errorNote}");
			}

			return (true, msgId, $"A message of type {messageSubType}, with XtReference {msgId}, has been successfully generated. Total time taken: {sw.Elapsed.TotalSeconds} second(s).");
		}

		public (bool, string) Receive()
		{
			var sw = Stopwatch.StartNew();

			InitializeIfNeeded();
			base.Receive(CancellationToken.None);

			sw.Stop();

			return (true, string.Empty);
		}
	}
}
