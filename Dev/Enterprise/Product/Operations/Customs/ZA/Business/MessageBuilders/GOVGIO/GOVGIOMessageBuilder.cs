using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Edifact.D16A.Messages.GOVCBR;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	class GOVGIOMessageBuilder : EDIFACTMessageBuilder<GOVGIOMessageHeader, GOVCBRMessage, GOVGIOEDIMessage>
	{
		public GOVGIOMessageBuilder(GOVGIOMessageHeader source, MessageSubTypes messagesubType)
			: base(source, messagesubType, new ZACharacterSet())
		{
			this.source = source;
		}

		protected override void PopulateEdifactMessage()
		{
			var builder = new GOVGIOMessageTextBuilder(edifactMessage, source.ManifestHeader, messageSubType);
			builder.Create();
		}

		readonly GOVGIOMessageHeader source;
	}
}
