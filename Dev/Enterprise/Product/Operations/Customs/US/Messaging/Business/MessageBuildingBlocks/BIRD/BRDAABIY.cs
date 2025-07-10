using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("Y")]
	[OutputBlock("Y")]
	public sealed class BRDAABIY : MessageBlock, IABIControlMessageBlockY
	{
		public BRDAABIY()
			: base("Y")
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

		#region IABIControlMessageBlockY Members

		ZString IABIControlMessageBlockY.ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.ACEBIRDTransaction; }
			set
			{
			}
		}

		ZString IABIControlMessageBlockY.ProcessingDistrictPortCode
		{
			get;
			set;
		}

		ZString IABIControlMessageBlockY.EntryFilerCode
		{
			get;
			set;
		}

		ZInt IABIControlMessageBlockY.NumberOfTransactionDetailRecordsInTheBlock
		{
			get { return ZInt.Zero; }
			set { }
		}

		#endregion
	}
}
