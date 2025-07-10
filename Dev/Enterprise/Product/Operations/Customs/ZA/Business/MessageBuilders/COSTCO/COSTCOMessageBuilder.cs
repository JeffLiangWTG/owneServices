using CargoWise.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using SixteenA = Enterprise.Edifact.D16A.Messages.COSTCO;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class COSTCOMessageBuilder : EDIFACTMessageBuilder<ICOSTCOMessageDataProvider, SixteenA.COSTCOMessage, COSTCOEDIMessage>
	{
		public COSTCOMessageBuilder(ICOSTCOMessageDataProvider source, MessageSubTypes messageSubType)
			: base(source, messageSubType, new ZACharacterSet())
		{
			this.source = Argument.NotNull(source, nameof(source));
		}

		protected override void PopulateEdifactMessage()
		{
			new COSTCOMessageTextBuilder(edifactMessage, source, messageSubType).Create();
		}

		readonly ICOSTCOMessageDataProvider source;
	}
}
