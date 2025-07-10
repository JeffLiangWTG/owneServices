using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	partial class AESSE1 : MessageBlock, I7501Status
	{
		ZString I7501Status.Code
		{
			get { return DispositionTypeCode; }
		}

		ZString I7501Status.NarrativeMessage
		{
			get { return GetNarrativeMessageFromDispositionCode(); }
		}

		bool I7501Status.IsMessageStatus
		{
			get { return true; }
		}

		ZDateTime I7501Status.StatusDate
		{
			get { return DateOfAction; }
		}

		ZString GetNarrativeMessageFromDispositionCode()
		{
			switch (DispositionTypeCode)
			{
				case "1":
					return "Request for electronic invoice data only";
				case "2":
					return "Request for the entry summary package";
				case "3":
					return "Request for specific documents";
				case "4":
					return "Entry summary rejected/PSC rejected";
				case "5":
					return "ACE Entry Summary Inactivated";
				case "6":
					return "Entry Summary Canceled";
				case "7":
					return "No longer used";
				case "8":
					return "A Post Summary Correction (PSC) has been presented by another filer";
				case "C":
					return "Detention Cancelled";
				case "D":
					return "Detained";
				case "E":
					return "TIB extension denied";
				case "P":
					return "PGA processing status information";
				case "Q":
					return "Quota status";
				case "R":
					return "A Post Summary Correction (PSC) Filed was rejected";
				default:
					return DispositionTypeCode;
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	partial class AESSE2 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	partial class AESSE3 : MessageBlock
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification)]
	partial class AESSE4 : MessageBlock
	{
	}
}
