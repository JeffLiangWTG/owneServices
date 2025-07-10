using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIOutputA : MessageBlock, IABIControlMessageBlockA
	{
		#region IControlMessageBlockA Members

		ZString IControlMessageBlockA.ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BIRDTransaction; }
			set { }
		}

		ZString IControlMessageBlockA.FilerID
		{
			get { return SenderReceiverIDCode; }
			set { SenderReceiverIDCode = value; }
		}

		#endregion

		#region IABIControlMessageBlockA Members

		ZString IABIControlMessageBlockA.ReceiverSiteCode
		{
			get { return SenderReceiverSiteCode; }
			set { SenderReceiverSiteCode = value; }
		}

		ZString IABIControlMessageBlockA.ReceiverFilerCode
		{
			get { return SenderReceiverIDCode; }
			set { SenderReceiverIDCode = value; }
		}

		ZDate IABIControlMessageBlockA.TransmissionDate
		{
			get { return TransmissionDate; }
			set { TransmissionDate = value; }
		}

		ZString IABIControlMessageBlockA.ReceiverOfficeCode
		{
			get { return SenderReceiverOfficeCode; }
			set { SenderReceiverOfficeCode = value; }
		}

		ZDate IABIControlMessageBlockA.CurrentDate
		{
			get { return ZDate.Today; }
			set { }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIOutputB : MessageBlock, IABIControlMessageBlockB
	{
		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return ApplicationIdentifierCode; }
			set { ApplicationIdentifierCode = value; }
		}

		#endregion

		#region IABIControlMessageBlockB Members

		ZString IABIControlMessageBlockB.ProcessingDistrictPortCode
		{
			get { return ProcessingDistrictPortCode; }
			set { ProcessingDistrictPortCode = value; }
		}

		ZString IABIControlMessageBlockB.PreparerDistrictPort
		{
			get { return RemotePreparerDistrictPortCode; }
			set { RemotePreparerDistrictPortCode = value; }
		}

		ZString IABIControlMessageBlockB.EntryFilerCode
		{
			get { return FilerCode; }
			set { FilerCode = value; }
		}

		ZString IABIControlMessageBlockB.ProcessingOfficeCode
		{
			get { return ProcessingFilerOfficeCode; }
			set { ProcessingFilerOfficeCode = value; }
		}

		ZString IABIControlMessageBlockB.UserData
		{
			get { return FilerPreparersUserDataText; }
			set { FilerPreparersUserDataText = value; }
		}

		ZString IABIControlMessageBlockB.StatementNumber
		{
			get { return StatementNumber; }
		}

		ZDate IABIControlMessageBlockB.PreliminaryStatementPrintDate
		{
			get { return PreliminaryStatementPrintDate; }
		}

		ZString IABIControlMessageBlockB.PaymentTypeIndicator
		{
			get { return PaymentTypeCode; }
		}

		ZString IABIControlMessageBlockB.ClientBranchDesignation
		{
			get { return StatementClientBranchIdentifier; }
		}

		ZString IABIControlMessageBlockB.ImporterOfRecordNumber
		{
			get { return ImporterOfRecordNumber; }
		}

		ZString IABIControlMessageBlockB.StatementStatus
		{
			get { return StatementStatus; }
		}

		ZString IABIControlMessageBlockB.PreparerIndicator
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IABIControlMessageBlockB.PreparerFilerCode
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IABIControlMessageBlockB.PreparerOfficeCode
		{
			get { return ZString.Empty; }
			set { }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIOutputY : MessageBlock, IABIControlMessageBlockY
	{
		#region IABIControlMessageBlockY Members

		ZString IABIControlMessageBlockY.ApplicationIdentifier
		{
			get { return ApplicationIdentifierCode; }
			set { ApplicationIdentifierCode = value; }
		}

		ZString IABIControlMessageBlockY.ProcessingDistrictPortCode
		{
			get { return ProcessingDistrictPortCode; }
			set { ProcessingDistrictPortCode = value; }
		}

		ZString IABIControlMessageBlockY.EntryFilerCode
		{
			get { return FilerCode; }
			set { FilerCode = value; }
		}

		ZInt IABIControlMessageBlockY.NumberOfTransactionDetailRecordsInTheBlock
		{
			get { return OutputTransactionImageCount; }
			set { OutputTransactionImageCount = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIOutputZ : MessageBlock, IABIControlMessageBlockZ
	{
		#region IABIControlMessageBlockZ Members

		ZString IABIControlMessageBlockZ.ReceiverSiteCode
		{
			get { return SenderReceiverSiteCode; }
			set { SenderReceiverSiteCode = value; }
		}

		ZString IABIControlMessageBlockZ.ReceiverFilerCode
		{
			get { return SenderReceiverIDCode; }
			set { SenderReceiverIDCode = value; }
		}

		ZDate IABIControlMessageBlockZ.TransmissionDate
		{
			get { return TransmissionDate; }
			set { TransmissionDate = value; }
		}

		ZString IABIControlMessageBlockZ.ReceiverOfficeCode
		{
			get { return SenderReceiverOfficeCode; }
			set { SenderReceiverOfficeCode = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	public partial class AABIOutputX1 : IStatusesAndErrors
	{
		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return this.NarrativeText; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return this.ConditionCode; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}
	}
}