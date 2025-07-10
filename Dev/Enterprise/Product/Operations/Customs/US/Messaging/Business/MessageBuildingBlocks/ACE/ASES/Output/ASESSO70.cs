namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	/// <summary>
	/// Fallback Version enable the system to still work in case Customs add a new version that we haven't handle yet.
	/// Whenever we add a new version, the latest fallback version should be updated.
	/// </summary>
	public static partial class FallbackVersionConstants
	{
		public const string ACEQWO70LatestVersion = "03";
		public const string ASESSO70LatestVersion = "03";
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO70")]
	public class ACEQWO70 : ASESSO70Base
	{
		public ACEQWO70()
			: base("WO70")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO70")]
	public class ASESSO70 : ASESSO70Base
	{
		public ASESSO70()
			: base("SO70")
		{
		}
	}

	public abstract partial class ASESSO70Base : MessageBlock
	{
		protected ASESSO70Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A code indicating the Partner Government Agency. Valid codes can be found in Appendix V (Government Agency Codes) in the ACE ABI CATAIR Appendices.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString GovernmentAgencyCode;

		/// <summary>
		/// A code indicating an agency's program for which the PGA data set is related. Refer to Appendix PGA for valid codes.
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString GovernmentAgencyProgramCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the status action date.
		/// </summary>
		[MessageBlockString(6, 11, "C")]
		public ZString StatusActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the status action.
		/// </summary>
		[MessageBlockString(4, 17, "C")]
		public ZString StatusActionTime;

		/// <summary>
		/// A code representing the status of a PGAs review, at the Entry Level.
		/// </summary>
		[MessageBlockString(2, 21, "C")]
		public ZString EntryLevelStatusCode;

		/// <summary>
		/// The narrative message associated with the Entry Review Status code
		/// </summary>
		[MessageBlockString(28, 23, "C")]
		public ZString EntryLevelStatusMessage;

		/// <summary>
		/// A code representing the status of a PGAs review at the Entry Line level.
		/// </summary>
		[MessageBlockString(2, 51, "C")]
		public ZString EntryLineLevelStatusCode;

		/// <summary>
		/// A code representing the status of a PGAs review at the PGA Line level.
		/// </summary>
		[MessageBlockString(2, 53, "C")]
		public ZString PGALineLevelStatusCode;

		/// <summary>
		/// A code representing the reason for a PGAs status review
		/// </summary>
		[MessageBlockString(2, 55, "C")]
		public ZString StatusReasonCode;

		/// <summary>
		/// The beginning CBP line number
		/// </summary>
		[MessageBlockString(3, 57, "C")]
		public ZString BeginningCBPLine;

		/// <summary>
		/// The beginning PGA line number
		/// </summary>
		[MessageBlockString(3, 60, "C")]
		public ZString BeginningPGALine;

		/// <summary>
		/// If there is a range, THRU is in this field
		/// </summary>
		[MessageBlockString(4, 63, "C")]
		public ZString RangeIndicator;

		/// <summary>
		/// The ending CBP line item
		/// </summary>
		[MessageBlockString(3, 67, "C")]
		public ZString EndingCBPLine;

		/// <summary>
		/// The ending PGA line number
		/// </summary>
		[MessageBlockString(3, 70, "C")]
		public ZString EndingPGALine;

		/// <summary>
		/// A code representing a required document.
		/// </summary>
		[MessageBlockString(3, 73, "C")]
		public ZString DocumentTypeCode;

		/// <summary>
		/// A code indicating the type of PGA hold on the entry
		/// </summary>
		[MessageBlockString(1, 76, "C")]
		public ZString PGAEntryHoldType;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO70", "01")]
	public class ACEQWO70_01 : ASESSO70_01Base
	{
		public ACEQWO70_01()
			: base("WO70")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO70", "01")]
	public class ASESSO70_01 : ASESSO70_01Base
	{
		public ASESSO70_01()
			: base("SO70")
		{
		}
	}

	public abstract partial class ASESSO70_01Base : MessageBlock
	{
		protected ASESSO70_01Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A code indicating the Partner Government Agency. Valid codes can be found in Appendix V (Government Agency Codes) in the ACE ABI CATAIR Appendices.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString GovernmentAgencyCode;

		/// <summary>
		/// A code indicating an agency's program for which the PGA data set is related. Refer to Appendix PGA for valid codes.
		/// </summary>
		[MessageBlockString(3, 8, "C")]
		public ZString GovernmentAgencyProgramCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the status action date.
		/// </summary>
		[MessageBlockString(6, 11, "C")]
		public ZString StatusActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the status action.
		/// </summary>
		[MessageBlockString(4, 17, "C")]
		public ZString StatusActionTime;

		/// <summary>
		/// A code representing the status of a PGAs review, at the Entry Level.
		/// </summary>
		[MessageBlockString(2, 21, "C")]
		public ZString PGAEntryLevelStatusCode;

		/// <summary>
		/// The narrative message associated with the Entry Review Status code
		/// </summary>
		[MessageBlockString(28, 23, "C")]
		public ZString PGAEntryLevelStatusMessage;

		/// <summary>
		/// FUTURE USE
		/// </summary>
		[MessageBlockString(2, 51, "C")]
		public ZString EntryLineLevelStatusCode;

		/// <summary>
		/// A code representing the status of a PGAs review at the PGA Line level.
		/// </summary>
		[MessageBlockString(2, 53, "C")]
		public ZString PGALineLevelStatusCode;

		/// <summary>
		/// A code representing the reason for a PGAs status review
		/// </summary>
		[MessageBlockString(2, 55, "C")]
		public ZString StatusReasonCode;

		/// <summary>
		/// The beginning CBP line number
		/// </summary>
		[MessageBlockString(4, 57, "C")]
		public ZString BeginningCBPLine;

		/// <summary>
		/// The associated tariff position.
		/// </summary>
		[MessageBlockString(1, 61, "C")]
		public ZString BeginningTariffPosition;

		/// <summary>
		/// The beginning PGA line number.
		/// </summary>
		[MessageBlockString(3, 62, "C")]
		public ZString BeginningPGALine;

		/// <summary>
		/// The ending CBP line number
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString EndingCBPLine;

		/// <summary>
		/// The associated tariff position.
		/// </summary>
		[MessageBlockString(1, 69, "C")]
		public ZString EndingTariffPosition;

		/// <summary>
		/// The ending PGA line number
		/// </summary>
		[MessageBlockString(3, 70, "C")]
		public ZString EndingPGALine;

		/// <summary>
		/// A code representing a required document.
		/// </summary>
		[MessageBlockString(5, 73, "C")]
		public ZString DocumentTypeCode;

		/// <summary>
		/// A code indicating the type of PGA hold on the entry
		/// </summary>
		[MessageBlockString(1, 78, "C")]
		public ZString PGAEntryHoldType;

		/// <summary>
		/// A code indicating which version of the SO70, SO71, SO72 grouping is being sent out to trade.
		/// </summary>
		[MessageBlockString(2, 79, "C")]
		public ZString PGAProcessingGroupVersion;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO70", "02")]
	public class ACEQWO70_02 : ASESSO70_02Base
	{
		public ACEQWO70_02()
			: base("WO70")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO70", "02")]
	public class ASESSO70_02 : ASESSO70_02Base
	{
		public ASESSO70_02()
			: base("SO70")
		{
		}
	}

	public abstract partial class ASESSO70_02Base : MessageBlock
	{
		protected ASESSO70_02Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// A code indicating the Partner Government Agency.Valid codes can be found in Appendix V (Government Agency Codes) in the ACE ABI CATAIR Appendices.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString GovernmentAgencyCode;

		/// <summary>
		/// A code indicating an agency's program for which the PGA data set is related. Refer to Appendix PGA for valid codes.
		/// </summary>
		[MessageBlockString(3, 8, "C")]
		public ZString GovernmentAgencyProgramCode;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the status action date.
		/// </summary>
		[MessageBlockString(6, 11, "C")]
		public ZString StatusActionDate;

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the status action.
		/// </summary>
		[MessageBlockString(4, 17, "C")]
		public ZString StatusActionTime;

		/// <summary>
		/// A code representing the status of a PGAs review, at the Entry Level.
		/// </summary>
		[MessageBlockString(2, 21, "C")]
		public ZString PGAEntryLevelStatusCode;

		/// <summary>
		/// The narrative message associated with the Entry Review Status code
		/// </summary>
		[MessageBlockString(28, 23, "C")]
		public ZString PGAEntryLevelStatusMessage;

		/// <summary>
		/// FUTURE USE
		/// </summary>
		[MessageBlockString(2, 51, "C")]
		public ZString EntryLineLevelStatusCode;

		/// <summary>
		/// A code representing the status of a PGAs review at the PGA Line level.
		/// </summary>
		[MessageBlockString(2, 53, "C")]
		public ZString PGALineLevelStatusCode;

		/// <summary>
		/// A code representing the reason for a PGAs status review
		/// </summary>
		[MessageBlockString(2, 55, "C")]
		public ZString StatusReasonCode;

		/// <summary>
		/// The beginning CBP line number
		/// </summary>
		[MessageBlockString(4, 57, "C")]
		public ZString BeginningCBPLine;

		/// <summary>
		/// The associated tariff position.
		/// </summary>
		[MessageBlockString(1, 61, "C")]
		public ZString BeginningTariffPosition;

		/// <summary>
		/// The beginning PGA line number.
		/// </summary>
		[MessageBlockString(3, 62, "C")]
		public ZString BeginningPGALine;

		/// <summary>
		/// The ending CBP line number
		/// </summary>
		[MessageBlockString(4, 65, "C")]
		public ZString EndingCBPLine;

		/// <summary>
		/// The associated tariff position.
		/// </summary>
		[MessageBlockString(1, 69, "C")]
		public ZString EndingTariffPosition;

		/// <summary>
		/// The ending PGA line number
		/// </summary>
		[MessageBlockString(3, 70, "C")]
		public ZString EndingPGALine;

		/// <summary>
		/// A code representing a required document.
		/// </summary>
		[MessageBlockString(5, 73, "C")]
		public ZString DocumentTypeCode;

		/// <summary>
		/// A code indicating the type of PGA hold on the entry
		/// </summary>
		[MessageBlockString(1, 78, "C")]
		public ZString PGAEntryHoldType;

		/// <summary>
		/// A code indicating which version of the SO70, SO71, SO72 grouping is being sent out to trade.
		/// </summary>
		[MessageBlockString(2, 79, "C")]
		public ZString PGAProcessingGroupVersion;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO70", "03")]
	public class ACEQWO70_03 : ASESSO70_03Base
	{
		public ACEQWO70_03()
			: base("WO70")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	[OutputBlock("SO70", "03")]
	public class ASESSO70_03 : ASESSO70_03Base
	{
		public ASESSO70_03()
			: base("SO70")
		{
		}
	}

	public abstract partial class ASESSO70_03Base : ASESSO70_02Base
	{
		protected ASESSO70_03Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}
	}
}
