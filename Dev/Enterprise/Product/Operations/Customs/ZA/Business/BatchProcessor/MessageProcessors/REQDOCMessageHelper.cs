using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.REQDOC;
using Enterprise.Edifact.D99B.Segments;
using D99BMessageFactory = Enterprise.Edifact.D99B.EdifactD99BMessageFactory;

namespace Enterprise.Customs.ZA.Business.MessageProcessor
{
	public class REQDOCMessageHelper : NonPersistentBusinessObject
	{
		REQDOCMessageHelper(REQDOCMessage message, BusinessObjectFactory factory) : base(factory)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		public static REQDOCMessageHelper New(REQDOCEDIMessage zaMessage)
		{
			REQDOCMessageHelper result = null;
			if (zaMessage != null)
			{
				var d99BMessageFactory = new D99BMessageFactory();
				var zaCharSet = new ZACharacterSetNoCasing();
				if (zaMessage.GetAutoEdifactMessageUsingNamedFactory(d99BMessageFactory, zaCharSet) is REQDOCMessage reqdocMessage)
				{
					result = new REQDOCMessageHelper(reqdocMessage, zaMessage.Factory);
				}
			}
			return result;
		}

		public ZString MessageSender
		{
			get
			{
				var messageSender = (from sg2 in message.Group2.Cast<SegmentGroup2>()
									 from nad in sg2.NAD.Cast<NADSegment>()
									 where nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.DocumentMessageIssuerSender
									 select nad.PartyIdentificationDetails.PartyIdentifier).FirstOrDefault();

				return messageSender ?? ZString.Empty;
			}
		}

		public ZString LocalReferenceNumber => MessageType != DocumentNameCodeList.StatementOfAccountMessage ? DocumentNumber : ZString.Empty;

		ZString MessageType => BGMSegment.DocumentMessageName.DocumentNameCode.ToString();

		ZString DocumentNumber => BGMSegment.DocumentMessageIdentification.DocumentMessageNumber;

		BGMSegment BGMSegment => bgmSegment ?? (bgmSegment = message.BGM[0]);
		BGMSegment bgmSegment;

		readonly REQDOCMessage message;
	}
}
