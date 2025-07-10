using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("32")]
	public partial class ENS32 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("57")]
	public partial class ENS57 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("90")]
	public partial class ENS90 : MessageBlock, IENS90, IENSDeferredTaxIndicator
	{
		ZDecimal IENS90.GetTotal(bool deferred)
		{
			return (deferred ? ZDecimal.Zero : GrandTotalEstimatedTax) +
				TotalEstimatedDuty +
				GrandTotalFeeAmount +
				TotalAntidumpingDutyAmount +
				TotalCountervailingDutyAmount;
		}

		ZString IENSDeferredTaxIndicator.DeferredTaxIndicator
		{
			get { return DeferredTaxIndicator; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction)]
	public partial class ENSH : MessageBlock, IStatementUpdateInputHBlock
	{
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
	}
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("10")]
	public partial class ENS10 : MessageBlock, IENS10
	{
		ZString IENS10.EntryType
		{
			get { return EntryType; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("20")]
	public partial class ENS20 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("21")]
	public partial class ENS21 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("22")]
	public partial class ENS22 : MessageBlock, IBlock22BillDetails
	{
		ZString IBlock22BillDetails.ITNo { get { return InBondNumber; } }
		ZDate IBlock22BillDetails.ITDate { get { return ITDate; } }
		ZString IBlock22BillDetails.MasterBillNumber { get { return MasterBillNumber; } }
		ZString IBlock22BillDetails.HouseBillNumber { get { return HouseBillNumber; } }
		ZString IBlock22BillDetails.SubHouseBillNumber { get { return SubHouseBillNumber; } }
		ZInt IBlock22BillDetails.Quantity { get { return Quantity; } }
		ZString IBlock22BillDetails.Unit { get { return Unit; } }
		ZString IBlock22BillDetails.IssuerCodeOfMasterBillNumber { get { return IssuerCodeOfMasterBillNumber; } }
		ZString IBlock22BillDetails.IssuerCodeOfHouseBillNumber { get { return IssuerCodeOfHouseBillNumber; } }
		ZString IBlock22BillDetails.IssuerCodeOfSubHouseBillNumber { get { return ZString.Empty; } }
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("30")]
	public partial class ENS30 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("34")]
	public partial class ENS34 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("35")]
	public partial class ENS35 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("40")]
	public partial class ENS40 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("42")]
	public partial class ENS42 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("43")]
	public partial class ENS43 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("50")]
	public partial class ENS50 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("51")]
	public partial class ENS51 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("52")]
	public partial class ENS52 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("60")]
	public partial class ENS60 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("62")]
	public partial class ENS62 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("70")]
	public partial class ENS70 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("80")]
	public partial class ENS80 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("81")]
	public partial class ENS81 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("89")]
	public partial class ENS89 : MessageBlock { }
}