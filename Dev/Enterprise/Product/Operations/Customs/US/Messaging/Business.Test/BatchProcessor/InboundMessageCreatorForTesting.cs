using System;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class InboundMessageCreatorForTesting : InboundMessageCreator<APLA, APLB, APLY, APLZ, CBPMessageForTesting>
	{
		protected override ZString GetMessageNum(APLA msgBlockA, APLB msgBlockB, Stream messageTextStream, APLY msgBlockY, APLZ msgBlockZ)
		{
			messageTextStream.Position = 0;
			var reader = new StreamReader(messageTextStream, ASCIIEncoding.ASCII);
			var buffer = new char[80];
			int readCount = reader.Read(buffer, 0, 80);
			if (readCount > 0)
			{
				Array.Resize(ref buffer, readCount);
				if (new string(buffer).Contains(ThrowInvalidMessageFormatExceptionTrigger))
				{
					throw new InvalidMessageFormatException("Message text contain invalid format");
				}
			}
			return "0";
		}

		public const string ThrowInvalidMessageFormatExceptionTrigger = "INVALIDMESSAGEFORMATEXCEPTION";

		protected override ZString GetApplicationIdentifier(APLA msgBlockA, APLB msgBlockB) => msgBlockB.ApplicationIdentifier;
	}
}
