using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D16A;
using Enterprise.Edifact.D16A.Messages.CALINF;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class CALINFMessageHelper : NonPersistentBusinessObject
	{
		CALINFMessageHelper(CALINFMessage message, BusinessObjectFactory factory) : base(factory)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public static CALINFMessageHelper New(CALINFEDIMessage zaMessage)
		{
			CALINFMessageHelper result = null;
			if (zaMessage != null)
			{
				var d16aMessageFactory = new D16AMessageFactory();
				var zaCharSet = new ZACharacterSet();
				if (zaMessage.GetAutoEdifactMessageUsingNamedFactory(d16aMessageFactory, zaCharSet) is CALINFMessage calinfMessage)
				{
					result = new CALINFMessageHelper(calinfMessage, zaMessage.Factory);
				}
			}
			return result;
		}

		public ZString DocumentNumber => BGMSegment.DocumentMessageIdentification.DocumentIdentifier;

		BGMSegment BGMSegment => bgmSemgent ?? (bgmSemgent = message.BGM[0]);
		BGMSegment bgmSemgent;

		readonly CALINFMessage message;
	}
}
