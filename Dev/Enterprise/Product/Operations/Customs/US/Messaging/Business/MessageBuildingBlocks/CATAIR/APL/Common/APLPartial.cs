using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common
{
	partial class APLA : MessageBlock, IABIControlMessageBlockA
	{
		#region IControlMessageBlockA Members

		ZString IControlMessageBlockA.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
		}

		ZString IControlMessageBlockA.FilerID
		{
			get { return ReceiverFilerCode; }
			set { ReceiverFilerCode = value; }
		}

		#endregion

		#region IABIControlMessageBlockA Members

		ZString IABIControlMessageBlockA.ReceiverSiteCode
		{
			get { return ReceiverDistrictPort; }
			set { ReceiverDistrictPort = value; }
		}

		ZString IABIControlMessageBlockA.ReceiverFilerCode
		{
			get { return ReceiverFilerCode; }
			set { ReceiverFilerCode = value; }
		}

		ZDate IABIControlMessageBlockA.TransmissionDate
		{
			get { return CurrentDate; }
			set { CurrentDate = value; }
		}

		ZString IABIControlMessageBlockA.ReceiverOfficeCode
		{
			get { return ReceiverOfficeCode; }
			set { ReceiverOfficeCode = value; }
		}

		ZDate IABIControlMessageBlockA.CurrentDate
		{
			get { return CurrentDate; }
			set { CurrentDate = value; }
		}

		#endregion
	}

	partial class APLB : MessageBlock, IABIControlMessageBlockB
	{
		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
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
			get { return EntryFilerCode; }
			set { EntryFilerCode = value; }
		}

		ZString IABIControlMessageBlockB.ProcessingOfficeCode
		{
			get { return ProcessingOfficeCode; }
			set { ProcessingOfficeCode = value; }
		}

		ZString IABIControlMessageBlockB.PreparerDistrictPort
		{
			get { return PreparerDistrictPort; }
			set { PreparerDistrictPort = value; }
		}

		ZString IABIControlMessageBlockB.UserData
		{
			get { return UserData; }
			set { UserData = value; }
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
			get { return PaymentTypeIndicator.ToString(); }
		}

		ZString IABIControlMessageBlockB.ClientBranchDesignation
		{
			get { return ClientBranchDesignation; }
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
			get { return PreparerIndicator; }
			set { PreparerIndicator = value; }
		}

		ZString IABIControlMessageBlockB.PreparerFilerCode
		{
			get { return PreparerFilerCode; }
			set { PreparerFilerCode = value; }
		}

		ZString IABIControlMessageBlockB.PreparerOfficeCode
		{
			get { return PreparerOfficeCode; }
			set { PreparerOfficeCode = value; }
		}

		#endregion
	}

	partial class APLY : MessageBlock, IABIControlMessageBlockY
	{
		#region IControlMessageBlockY Members

		ZString IABIControlMessageBlockY.ApplicationIdentifier
		{
			get { return ApplicationIdentifier; }
			set { ApplicationIdentifier = value; }
		}

		ZString IABIControlMessageBlockY.ProcessingDistrictPortCode
		{
			get { return ProcessingDistrictPortCode; }
			set { ProcessingDistrictPortCode = value; }
		}

		ZString IABIControlMessageBlockY.EntryFilerCode
		{
			get { return EntryFilerCode; }
			set { EntryFilerCode = value; }
		}

		ZInt IABIControlMessageBlockY.NumberOfTransactionDetailRecordsInTheBlock
		{
			get { return NumberOfTransactionDetailRecordsInTheBlock; }
			set { NumberOfTransactionDetailRecordsInTheBlock = value; }
		}

		#endregion
	}

	partial class APLZ : MessageBlock, IABIControlMessageBlockZ
	{
		#region IControlMessageBlockZ Members

		ZString IABIControlMessageBlockZ.ReceiverSiteCode
		{
			get { return DistrictPortOfABIReceiver; }
			set { DistrictPortOfABIReceiver = value; }
		}

		ZString IABIControlMessageBlockZ.ReceiverFilerCode
		{
			get { return ReceiverFilerCode; }
			set { ReceiverFilerCode = value; }
		}

		ZDate IABIControlMessageBlockZ.TransmissionDate
		{
			get { return CurrentDate; }
			set { CurrentDate = value; }
		}

		ZString IABIControlMessageBlockZ.ReceiverOfficeCode
		{
			get { return ReceiverOfficeCode; }
			set { ReceiverOfficeCode = value; }
		}

		#endregion
	}
}
