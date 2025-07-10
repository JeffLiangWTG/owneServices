using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR16Populator
	{
		public static RECR16 Populate(IReconciliation reconciliationData)
		{
			RECR16 r16 = null;
			ZString textComment = reconciliationData.TextComment;
			if (textComment.Length > 75)
			{
				r16 = new RECR16();
				r16.TextComment = textComment.SubstringSafe(75, 75);
			}
			return r16;
		}
	}
}
