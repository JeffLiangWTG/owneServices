using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D16A;
using Enterprise.Edifact.D16A.Messages.COSTCO;
using Enterprise.Edifact.D16A.Segments;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class COSTCOMessageHelper : NonPersistentBusinessObject
	{
		COSTCOMessageHelper(COSTCOMessage message, BusinessObjectFactory factory) : base(factory)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public static COSTCOMessageHelper New(COSTCOEDIMessage zaMessage)
		{
			COSTCOMessageHelper result = null;
			if (zaMessage != null)
			{
				var d96bMessageFactory = new D16AMessageFactory();
				var zaCharSet = new ZACharacterSet();
				if (zaMessage.GetAutoEdifactMessageUsingNamedFactory(d96bMessageFactory, zaCharSet) is COSTCOMessage cusdecMessage)
				{
					result = new COSTCOMessageHelper(cusdecMessage, zaMessage.Factory);
				}
			}
			return result;
		}

		public ZString DocumentNumber => BGMSegment.DocumentMessageIdentification.DocumentIdentifier;

		BGMSegment BGMSegment => bgmSemgent ?? (bgmSemgent = message.BGM[0]);
		BGMSegment bgmSemgent;

		readonly COSTCOMessage message;
	}
}
