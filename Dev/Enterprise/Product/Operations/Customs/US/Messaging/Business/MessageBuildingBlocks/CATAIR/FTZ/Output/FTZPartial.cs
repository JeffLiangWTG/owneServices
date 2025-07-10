using System.Collections.ObjectModel;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD10 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD10_01 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD11 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD12 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD20 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD30 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD40 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD40_01 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD41 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD42 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD43 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD50 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD51 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD60 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator)]
	public partial class FTZZD61 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF90 : MessageBlock, IFTZNF90
	{
		ZString IFTZNF90.AdmissionType => AdmissionType;

		ZString IFTZNF90.ZoneID => ZoneID;

		ZInt IFTZNF90.CalendarYear => CalendarYear;

		ZString IFTZNF90.ControlNumber => ControlNumber;

		ZString IFTZNF90.PortCode => PortCode;

		ZString IFTZNF90.DirectDeliveryIndicator => DirectDeliveryIndicator;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF90_01 : MessageBlock, IFTZNF90
	{
		ZString IFTZNF90.AdmissionType => AdmissionType;

		ZString IFTZNF90.ZoneID => ZoneID;

		ZInt IFTZNF90.CalendarYear => CalendarYear;

		ZString IFTZNF90.ControlNumber => ControlNumber;

		ZString IFTZNF90.PortCode => PortCode;

		ZString IFTZNF90.DirectDeliveryIndicator => DirectDeliveryIndicator;
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF96 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF91 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return DispositionCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return new DispositionList().GetDescriptionFromCode(DispositionCode); }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF92 : MessageBlock { }

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	public partial class FTZNF95 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.Code
		{
			get { return ErrorCode; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return Remarks; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	partial class FTZNF10 : ISerialiserSupporter, IFTZNF10
	{
		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)
			{
				var block = new FTZFT10();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)
			{
				var block = new FTZFZ10();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else
			{
				return this.Serialise();
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	partial class FTZNF10_01 : ISerialiserSupporter, IFTZNF10
	{
		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)
			{
				var block = new FTZFT10_01();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)
			{
				var block = new FTZFZ10();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else
			{
				return this.Serialise();
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	partial class FTZNF11 : ISerialiserSupporter
	{
		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)
			{
				var block = new FTZFT11();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)
			{
				var block = new FTZFZ11();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else
			{
				return this.Serialise();
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	partial class FTZNF12 : ISerialiserSupporter
	{
		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)
			{
				var block = new FTZFT12();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)
			{
				var block = new FTZFZ12();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else
			{
				return this.Serialise();
			}
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone)]
	partial class FTZNF20 : ISerialiserSupporter
	{
		string ISerialiserSupporter.Serialise(bool humanFriendly, string outgoingApplicationIdentifier, ReadOnlyCollection<MessageBlock> messageBlocks)
		{
			if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData)
			{
				var block = new FTZFT20();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else if (outgoingApplicationIdentifier == ACEApplicationIdentifierCodeList.Codes.FTZEventReporting)
			{
				var block = new FTZFZ20();
				block.Deserialise(this.Serialise());
				return block.Serialise(humanFriendly);
			}
			else
			{
				return this.Serialise();
			}
		}
	}
}
