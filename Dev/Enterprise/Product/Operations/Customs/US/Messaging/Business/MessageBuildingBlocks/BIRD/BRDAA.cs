using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("AA")]
	[OutputBlock("AA")]
	public sealed partial class BRDAA : MessageBlock, IABIControlMessageBlockB
	{
		public BRDAA()
			: base("AA")
		{
		}

		/// <summary>
		/// 7501 - Entry Summary Records
		/// STAT - Status Records
		/// MSGS - Messages
		/// JI - Entry Summary Query Input
		/// JR - Entry Summary Query Output
		/// NR - Courtesy Notice of Liquidation
		/// 3461 - Entry Release Records
		/// </summary>
		[MessageBlockString(4, 3, "M")]
		public ZString ApplicationCode;

		[MessageBlockString(3, 7, "M")]
		public ZString SendersFilerCode;

		[MessageBlockString(4, 10, "M")]
		public ZString SendersDDPP;

		[MessageBlockString(20, 14, "M")]
		public ZString OriginatingBrokerRef;

		[MessageBlockDate(41, "M", "yyyyMMdd")]
		public ZDate CreationDate;

		/// <summary>
		/// in the format of HHMMSS (HH: Hours, MM: Minutes, SS: Seconds)
		/// </summary>
		[MessageBlockString(6, 49, "O")]
		public ZString CreationTime;

		/// <summary>
		/// Always 0100
		/// </summary>
		[MessageBlockString(4, 55, "M")]
		public ZString Version;

		[MessageBlockString(20, 61, "O")]
		public ZString UserArea;

		#region IControlMessageBlockB Members

		ZString IControlMessageBlockB.ApplicationIdentifier
		{
			get { return BIRDApplicationCodeList.GetABIApplicationCode(ApplicationCode); }
			set { }
		}

		#endregion

		#region IABIControlMessageBlockB Members

		ZString IABIControlMessageBlockB.ProcessingDistrictPortCode
		{
			get { return SendersDDPP; }
			set { SendersDDPP = value; }
		}

		ZString IABIControlMessageBlockB.EntryFilerCode
		{
			get { return SendersFilerCode; }
			set { SendersFilerCode = value; }
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
