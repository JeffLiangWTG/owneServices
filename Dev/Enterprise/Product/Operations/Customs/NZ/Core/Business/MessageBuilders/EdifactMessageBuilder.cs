using Enterprise.Edifact;
using Enterprise.Edifact.Auto;

namespace Enterprise.Customs.NZ.Business.MessageBuilders
{
	/// <summary>
	/// CUSMOD Edifact message generation.
	/// </summary>
	public abstract class EdifactMessageBuilder : MessageBuilder
	{
		public EdifactMessageBuilder()
		{
		}

		protected abstract SegmentGroup GetNewEDIFACTMessage();
		protected abstract void GenerateGroup0();

		protected override void GenerateIfNotAlreadyGenerated()
		{
			if (!generated)
			{
				GenerateGroup0();
				generated = true;
			}
		}

		public override string GetMessageText()
		{
			GenerateIfNotAlreadyGenerated();
			return EDIFACTMessage.ToString(new UNOACharacterSet());
		}

		protected SegmentGroup EDIFACTMessage
		{
			get
			{
				if (fEDIFACTMessage == null)
				{
					fEDIFACTMessage = GetNewEDIFACTMessage();
				}
				return fEDIFACTMessage;
			}
		}
		SegmentGroup fEDIFACTMessage;
	}
}
