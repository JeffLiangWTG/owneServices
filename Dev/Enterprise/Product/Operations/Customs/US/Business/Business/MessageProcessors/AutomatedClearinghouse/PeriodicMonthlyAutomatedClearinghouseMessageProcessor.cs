using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse)]
	public class PeriodicMonthlyAutomatedClearinghouseMessageProcessor : AutomatedClearinghouseMessageProcessor
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.ABIMessagesGroup;
		}

		protected override string MessageTypeDescription => "ACH Debit Authorization";
	}
}
