using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZZ")]
	[OutputBlock("ZZ")]
	public sealed class BRDZZ : MessageBlock, IABIControlMessageBlockY
	{
		public BRDZZ()
			: base("ZZ")
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

		/// <summary>
		/// Number of records between AA and ZZ records
		/// </summary>
		[MessageBlockInt(9, 7, "M")]
		public ZInt RecordCount;

		#region IControlMessageBlockY Members

		ZString IABIControlMessageBlockY.ApplicationIdentifier
		{
			get { return BIRDApplicationCodeList.GetABIApplicationCode(ApplicationCode); }
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
			get { return RecordCount; }
			set { RecordCount = value; }
		}

		#endregion
	}
}
