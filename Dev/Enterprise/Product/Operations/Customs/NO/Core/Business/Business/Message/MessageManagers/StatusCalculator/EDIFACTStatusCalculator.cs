using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.NO.Business
{
	public class EDIFACTStatusCalculator : EDIFACTMessageStatusCalculator
	{
		public EDIFACTStatusCalculator(ZString messageType)
		{
			this.messageType = messageType;
		}

		public override ZString MessageTypeDescription => messageType;

		public override ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject)
		{
			//TODO UNDEFINED 
			return ZString.Empty;
		}

		readonly ZString messageType;
	}
}
