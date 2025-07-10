using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummary)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSE0 : MessageBlock, IStatusesAndErrors, I7501Status
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return CargoWise.Types.ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		ZString I7501Status.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString I7501Status.NarrativeMessage
		{
			get { return NarrativeMessage; }
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
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSEC : MessageBlock, IStatusesAndErrors, I7501Errors
	{
		public ZString LineNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSEXX : MessageBlock, IStatusesAndErrors, I7501Errors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return LineNumber > 0 ? LineNumber.ToString() : ""; }
		}

		ZString I7501Errors.LineNumber
		{
			get { return LineNumber.ToString(); }
		}

		public ZString Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse)]
	partial class ENSH1 : MessageBlock, IStatusesAndErrors, I7501Status, IStatementUpdateOutputH1Block
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region I7501Status

		ZString I7501Status.Code
		{
			get { return ErrorMessageIdentifier; }
		}

		ZString I7501Status.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		bool I7501Status.IsMessageStatus
		{
			get { return true; }
		}

		ZDateTime I7501Status.StatusDate
		{
			get { return ZDateTime.Empty; }
		}

		#endregion

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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse)]
	partial class ENSH2 : MessageBlock, IStatementUpdateAdditionalOutput
	{
		#region IStatementUpdateOutputH2Block

		ZString IStatementUpdateAdditionalOutput.StatementNumber
		{
			get { return StatementNumber; }
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

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSU1 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return Narrative; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSU3 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return LineNumber.ToString(); }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return RejectComments; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.EntrySummaryResponse)]
	partial class ENSU4 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return RejectComments; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	//[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	partial class ENSEA : MessageBlock, IStatusesAndErrors, I7501Errors, IInBondErrors
	{
		public ZString LineNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Code
		{
			get { return ZString.Empty; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IInBondErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}

	partial class ENSEB : MessageBlock, IStatusesAndErrors, I7501Errors, IInBondErrors
	{
		public ZString LineNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Code
		{
			get { return ZString.Empty; }
		}

		ZString IInBondErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}

	//[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	partial class ENSEY : MessageBlock, IStatusesAndErrors, I7501Errors, IInBondErrors
	{
		public ZString LineNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Code
		{
			get { return ZString.Empty; }
		}

		ZString IInBondErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}

	//[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicRejectRequestNotification)]
	partial class ENSEZ : MessageBlock, IStatusesAndErrors, I7501Errors, IInBondErrors
	{
		public ZString LineNumber
		{
			get { return ZString.Empty; }
		}

		public ZString Code
		{
			get { return ZString.Empty; }
		}

		ZString I7501Errors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IInBondErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		bool I7501Errors.IsError
		{
			get { return true; }
		}
	}
}
