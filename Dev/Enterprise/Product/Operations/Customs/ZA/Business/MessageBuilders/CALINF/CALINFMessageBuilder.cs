using CargoWise.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageBuilders.CALINF;
using SixteenA = Enterprise.Edifact.D16A.Messages.CALINF;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class CALINFMessageBuilder : EDIFACTMessageBuilder<ICALINFMessageDataProvider, SixteenA.CALINFMessage, CALINFEDIMessage>
	{
		public CALINFMessageBuilder(ICALINFMessageDataProvider source, MessageSubTypes messageSubType)
			: base(source, messageSubType, new ZACharacterSet())
		{
			this.source = Argument.NotNull(source, nameof(source));
		}

		protected override void PopulateEdifactMessage()
		{
			new CALINFMessageTextBuilder(edifactMessage, source, messageSubType).Create();
		}

		readonly ICALINFMessageDataProvider source;
	}
}
