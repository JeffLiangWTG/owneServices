
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.StatementUpdate)]
	public partial class ASTUH : MessageBlock, IStatementUpdateInputHBlock
	{
		#region IStatementUpdate

		ZString IStatementUpdateInputHBlock.DistrictPortOfEntrySummary
		{
			get { return DistrictPortOfEntrySummary; }
			set { DistrictPortOfEntrySummary = value; }
		}

		ZString IStatementUpdateInputHBlock.EntryFilerCode
		{
			get { return EntryFilerCode; }
			set { EntryFilerCode = value; }
		}

		ZString IStatementUpdateInputHBlock.EntryNumber
		{
			get { return EntryNumber; }
			set { EntryNumber = value; }
		}

		ZString IStatementUpdateInputHBlock.PaymentTypeIndicator
		{
			get { return PaymentTypeIndicator; }
			set { PaymentTypeIndicator = value; }
		}

		ZDate IStatementUpdateInputHBlock.PreliminaryStatementPrintDate
		{
			get { return PreliminaryStatementPrintDate; }
			set { PreliminaryStatementPrintDate = value; }
		}

		ZString IStatementUpdateInputHBlock.ClientBranchDesignation
		{
			get { return ClientBranchDesignation; }
			set { ClientBranchDesignation = value; }
		}

		ZString IStatementUpdateInputHBlock.PeriodicStatementMonth
		{
			get { return PeriodicStatementMonth; }
			set { PeriodicStatementMonth = value; }
		}

		#endregion
	}
}
