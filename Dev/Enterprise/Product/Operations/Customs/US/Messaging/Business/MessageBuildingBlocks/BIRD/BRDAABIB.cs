using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("B")]
	[OutputBlock("B")]
	public sealed class BRDAABIB : MessageBlock, IABIControlMessageBlockB
	{
		public BRDAABIB()
			: base("B")
		{
		}

		/// <summary>
		/// The code for the U.S. port where the enclosed transaction(s) are to be 'processed'.
		/// </summary>
		[MessageBlockString(4, 4, "M", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ProcessingDistrictPortCode;

		/// <summary>
		/// Filer's identification code (as assigned by CBP).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString FilerCode;

		/// <summary>
		/// A code that identifies the type of transaction data within the block.
		/// </summary>
		[MessageBlockString(2, 11, "M")]
		public ZString ApplicationIdentifierCode;

		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction; }
			set { }
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
			get;
			set;
		}

		ZString IABIControlMessageBlockB.PreparerDistrictPort
		{
			get;
			set;
		}

		ZString IABIControlMessageBlockB.StatementNumber
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZDate IABIControlMessageBlockB.PreliminaryStatementPrintDate
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZString IABIControlMessageBlockB.PaymentTypeIndicator
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZString IABIControlMessageBlockB.ClientBranchDesignation
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZString IABIControlMessageBlockB.ImporterOfRecordNumber
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZString IABIControlMessageBlockB.StatementStatus
		{
			get { throw new System.InvalidOperationException(); }
		}

		ZString IABIControlMessageBlockB.UserData
		{
			get;
			set;
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
}
