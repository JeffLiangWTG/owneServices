using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIInputA : MessageBlock, IABIControlMessageBlockA
	{
		#region IControlMessageBlockA Members

		ZString IControlMessageBlockA.ApplicationIdentifier
		{
			get { return ApplicationIdentifierCode; }
			set { ApplicationIdentifierCode = value; }
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
	partial class AABIInputB : MessageBlock, IABIControlMessageBlockB
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

		ZString IABIControlMessageBlockB.PreparerDistrictPort
		{
			get { return RemotePreparerDistrictPortCode; }
			set { RemotePreparerDistrictPortCode = value; }
		}

		ZString IABIControlMessageBlockB.UserData
		{
			get { return FilerPreparersUserDataText; }
			set { FilerPreparersUserDataText = value; }
		}

		ZString IABIControlMessageBlockB.StatementNumber
		{
			get { return ZString.Empty; }
		}

		ZDate IABIControlMessageBlockB.PreliminaryStatementPrintDate
		{
			get { return ZDate.Empty; }
		}

		ZString IABIControlMessageBlockB.PaymentTypeIndicator
		{
			get { return ZString.Empty; }
		}

		ZString IABIControlMessageBlockB.ClientBranchDesignation
		{
			get { return ZString.Empty; }
		}

		ZString IABIControlMessageBlockB.ImporterOfRecordNumber
		{
			get { return ZString.Empty; }
		}

		ZString IABIControlMessageBlockB.StatementStatus
		{
			get { return ZString.Empty; }
		}

		ZString IABIControlMessageBlockB.PreparerIndicator
		{
			get { return RemotelyFiledIndicator; }
			set { RemotelyFiledIndicator = value; }
		}

		ZString IABIControlMessageBlockB.PreparerFilerCode
		{
			get { return RemotePreparerFilerCode; }
			set { RemotePreparerFilerCode = value; }
		}

		ZString IABIControlMessageBlockB.PreparerOfficeCode
		{
			get { return RemotePreparerOfficeCode; }
			set { RemotePreparerOfficeCode = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIInputY : MessageBlock, IABIControlMessageBlockY
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
			get { return ZInt.Zero; }
			set { }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	partial class AABIInputZ : MessageBlock, IABIControlMessageBlockZ
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
}
