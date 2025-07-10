using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation)]
	public class ACEPeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor : PeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor<ACEABIProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentation)]
	public class ACSPeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor : PeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor<ACSABIProcessor, APLA, APLB, APLY>
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse)]
	[TopLevel(typeof(AABIX0), typeof(AABIOutputX1))]
	public class PeriodicDailyStatementACHDebitAuthorizationEntrySummaryFailureMessageProcessor : PeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor<ACSABIProcessor, APLA, APLB, APLY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class PeriodicMonthlyAutomatedClearinghouseFailureMessageProcessor<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : AutomatedClearinghouseFailureMessageProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where T : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected override Integration.IRegistryItem GetEmailGroupRegistryItem()
		{
			return USCustomsDataRegistry.Instance.ABIMessagesGroup;
		}

		protected override string MessageTypeDescription => "ACH Debit Authorization";
	}
}
