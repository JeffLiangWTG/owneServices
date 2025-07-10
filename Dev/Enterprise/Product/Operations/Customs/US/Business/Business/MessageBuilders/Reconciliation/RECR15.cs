using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR15Populator
	{
		public static RECR15 Populate(IReconciliation reconciliationData)
		{
			RECR15 r15 = new RECR15();
			r15.ImportEntrySource = reconciliationData.ImportEntrySource;
			r15.TextComment = reconciliationData.TextComment.Left(75);
			return r15;
		}
	}
}
