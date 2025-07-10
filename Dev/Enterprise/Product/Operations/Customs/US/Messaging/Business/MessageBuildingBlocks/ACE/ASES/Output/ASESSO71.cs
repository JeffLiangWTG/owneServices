namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO71")]
	public class ACEQWO71 : ASESSO71Base
	{
		public ACEQWO71()
			: base("WO71")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO71")]
	public class ASESSO71 : ASESSO71Base
	{
		public ASESSO71()
			: base("SO71")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	[OutputBlock("PO71")]
	public class SESNPO71 : ASESSO71Base
	{
		public SESNPO71()
			: base("PO71")
		{
		}
	}

	public abstract partial class ASESSO71Base : MessageBlock
	{
		protected ASESSO71Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A code representing the type of number being provided from the PGA, in response to data submitted by trade
		/// </summary>
		[MessageBlockString(2, 5, "C")]
		public ZString PGAReferenceIdentificationNumberQualifier;

		/// <summary>
		/// The number being provided by the PGA, in response to data submitted by trade
		/// </summary>
		[MessageBlockString(12, 7, "C")]
		public ZString PGAReferenceIdentificationNumber;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date that the PGA confirmation was received.
		/// </summary>
		[MessageBlockDate(19, "C", "MMddyy")]
		public ZDate PGAReferenceIdentificationNumberReceiptDate;

		/// <summary>
		/// The military time in HHMMSS (hour, minute, second) format representing the time that the PGA confirmation was received.
		/// </summary>
		[MessageBlockString(6, 25, "C")]
		public ZString PGAReferenceIdentificationNumberReceiptTime;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 31, "C")]
		public ZString PGALineSubReasonCode;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 34, "C")]
		public ZString PGALineSubReasonCode1;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 37, "C")]
		public ZString PGALineSubReasonCode2;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 40, "C")]
		public ZString PGALineSubReasonCode3;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 43, "C")]
		public ZString PGALineSubReasonCode4;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 46, "C")]
		public ZString PGALineSubReasonCode5;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 49, "C")]
		public ZString PGALineSubReasonCode6;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 52, "C")]
		public ZString PGALineSubReasonCode7;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 55, "C")]
		public ZString PGALineSubReasonCode8;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(3, 58, "C")]
		public ZString PGALineSubReasonCode9;

		/// <summary>
		/// A code representing the type of number being provided from the PGA, in response to data submitted by trade.
		/// </summary>
		[MessageBlockString(2, 61, "C")]
		public ZString PGAReferenceIdentificationNumberQualifier1;

		/// <summary>
		/// Code identifying PGA review reason.
		/// </summary>
		[MessageBlockString(18, 63, "C")]
		public ZString PGAReferenceIdentificationNumber1;
	}
}
