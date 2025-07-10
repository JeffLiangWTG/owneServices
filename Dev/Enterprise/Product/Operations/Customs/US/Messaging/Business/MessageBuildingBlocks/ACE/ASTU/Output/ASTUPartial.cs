
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse)]
	public partial class ASTUH1 : MessageBlock, IStatementUpdateOutputH1Block
	{
		#region IStatementUpdateOutputH1Block

		ZString IStatementUpdateOutputH1Block.EntryFilerCode
		{
			get { return EntryFilerCode; }
		}

		ZString IStatementUpdateOutputH1Block.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IStatementUpdateOutputH1Block.PaymentTypeIndicator
		{
			get { return PaymentTypeIndicator; }
		}

		ZDate IStatementUpdateOutputH1Block.PreliminaryStatementPrintDate
		{
			get { return PreliminaryStatementPrintDate; }
		}

		ZString IStatementUpdateOutputH1Block.ClientBranchDesignation
		{
			get { return ClientBranchDesignation; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse)]
	public partial class ASTUH2 : MessageBlock, I7501Status
	{
		public ZString Code
		{
			get { return ConditionCode; }
		}

		public ZString NarrativeMessage
		{
			get { return NarrativeText; }
		}

		bool I7501Status.IsMessageStatus
		{
			get { return true; }
		}

		ZDateTime I7501Status.StatusDate
		{
			get { return ZDateTime.Empty; }
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse)]
	public partial class ASTUH3 : MessageBlock, IStatementUpdateAdditionalOutput
	{
		#region IStatementUpdateOutputH2Block

		ZString IStatementUpdateAdditionalOutput.StatementNumber
		{
			get { return DailyOrPeriodicDailyStatementNumber; }
		}

		ZDecimal IStatementUpdateAdditionalOutput.TotalAmountDue
		{
			get { return TotalAmountDue; }
		}

		ZString IStatementUpdateAdditionalOutput.PeriodicMonthlyStatementNumber
		{
			get { return PeriodicMonthlyStatementNumber; }
		}

		ZDecimal IStatementUpdateAdditionalOutput.PeriodicMonthlyStatementTotalAmountDue
		{
			get { return PeriodicMonthlyStatementTotalAmountDue; }
		}

		#endregion
	}
}
