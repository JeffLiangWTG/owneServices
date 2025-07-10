using System.Threading;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class T1InterchangeUnpacker : IUniversalCustomsInterchangeUnpacker, IDataCreator<IUniversalCustomsInterchangeUnpacker>
	{
		public T1InterchangeUnpacker(ZInt ucuDelayTimeInMilliseconds)
		{
			this.ucuDelayTimeInMilliseconds = ucuDelayTimeInMilliseconds;
		}
		readonly ZInt ucuDelayTimeInMilliseconds;

		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
		{
			if (ucuDelayTimeInMilliseconds != 0)
			{
				Thread.Sleep(ucuDelayTimeInMilliseconds);
			}
			var message = interchange.Factory.New<EDIMessage>();
			message.EM_ApplicationCode = interchange.EI_ApplicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageType = interchange.EI_InterchangeType;
			message.EM_MessageText = interchange.EI_BodyText;
			message.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessageSchema.EM_MessageNum.MaxLength);
			message.EM_GB = interchange.EI_GB;
			interchange.ContainedMessages.Add(message);

			return new EDIInterchangeUnpackerResult(new[] { message });
		}

		IUniversalCustomsInterchangeUnpacker IDataCreator<IUniversalCustomsInterchangeUnpacker>.Create() => this;
	}
}
