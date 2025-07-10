namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO60")]
	public class ACEQWO60 : ASESSO60Base
	{
		public ACEQWO60()
			: base("WO60")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO60")]
	public class ASESSO60 : ASESSO60Base
	{
		public ASESSO60()
			: base("SO60")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeStatusNotification)]
	[OutputBlock("PO60")]
	public class SESNPO60 : ASESSO60Base
	{
		public SESNPO60()
			: base("PO60")
		{
		}
	}

	public abstract partial class ASESSO60Base : MessageBlock
	{
		public ASESSO60Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the disposition action date.
		/// </summary>
		[MessageBlockDate(5, "M", "MMddyy")]
		public ZDate DispositionActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the disposition action.
		/// </summary>
		[MessageBlockString(4, 11, "M")]
		public ZString DispositionActionTime;

		/// <summary>
		/// A code representing the disposition action.
		/// </summary>
		[MessageBlockString(2, 15, "M")]
		public ZString DispositionActionCode;

		/// <summary>
		/// The narrative message associated with the disposition code.
		/// </summary>
		[MessageBlockString(40, 17, "M")]
		public ZString NarrativeMessage;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the release date. This data element is only returned if the Disposition Action Code is 22 or 98.
		/// </summary>
		[MessageBlockDate(57, "C", "MMddyy")]
		public ZDate ReleaseDate;

		/// <summary>
		/// A code representing the action or date ACE has used to determine the current release date. This data element is only returned if the Disposition Action Code is 22 or 98.
		/// </summary>
		[MessageBlockString(2, 63, "C")]
		public ZString ReleaseOrigin;

		/// <summary>
		/// A code representing a required document.
		/// </summary>
		[MessageBlockString(6, 65, "C")]
		public ZString DocumentType;
	}
}
