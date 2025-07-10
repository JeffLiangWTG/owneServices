using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSOutputBlockControlGenerator : BlockControlGenerator<APLACR, APLZCR>
	{
		public AMSOutputBlockControlGenerator()
			: this(CBPEDIInterchange.ApplicationCodes.AMS)
		{
		}

		public AMSOutputBlockControlGenerator(ZString applicationCode)
			: base(applicationCode)
		{
		}

		protected override MessageBlockDeserialiser MessageBlockDeserialiser
		{
			get { return new OutputMessageBlockDeserialiser(); }
		}

		protected override ZString ApplicationIdentifier
		{
			get { return B.ApplicationIdentifier; }
		}

		protected override void EnsureBBlockCanBeDeserialise(string message)
		{
			if (!message.StartsWith("ACR"))
			{
				throw new InvalidMessageFormatException("message does not start with a 'ACR' block");
			}
		}

		protected override void EnsureYBlockCanBeDeserialise(string yBlock)
		{
			if (!yBlock.StartsWith("ZCR"))
			{
				throw new InvalidMessageFormatException("message does not end with a 'ZCR' block");
			}
		}

		protected override US.Messaging.Business.MessageBuildingBlocks.MessageBlock GetMessageBlock(string applicationIdentifier, string eightyCharacterBlock, string preferredMessageBlockApplicationCode)
		{
			var result = base.GetMessageBlock(applicationIdentifier, eightyCharacterBlock, preferredMessageBlockApplicationCode);
			if (result is OUTR02Combine)
			{
				if (isNextOUTR02AContinuationType)
				{
					result = new OUTR02Continuation();
					result.Deserialise(eightyCharacterBlock);
					isNextOUTR02AContinuationType = false;
				}
				else
				{
					var outr02 = new OUTR02();
					outr02.Deserialise(eightyCharacterBlock);
					result = outr02;
					isNextOUTR02AContinuationType = outr02.LineDelimiter == 1;
				}
			}
			else
			{
				isNextOUTR02AContinuationType = false;
			}
			return result;
		}
		bool isNextOUTR02AContinuationType;
	}
}
