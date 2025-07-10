using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	public partial class ASESE90 : ICargoReleaseStatus, ITariffNumberStatusAndErrors
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.LineNumber
		{
			get { return LineNumber; }
		}
		public ZString LineNumber;

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessageText; }
		}

		ZString IStatusesAndErrors.Code
		{
			get
			{
				return MessageTypeCode == SimplifiedEntryMessageTypeCodesList.Codes.RecordRejected ||
				MessageTypeCode == SimplifiedEntryMessageTypeCodesList.Codes.RecordAcceptedWithWarning ? MessageIdentifierCode : MessageTypeCode;
			}
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICargoReleaseStatus

		ZString ICargoReleaseStatus.Status
		{
			get { return MessageTypeCode; }
		}

		ZString ICargoReleaseStatus.NarrativeMessage
		{
			get { return NarrativeMessageText; }
		}

		ZString ICargoReleaseStatus.ErrorIdentifierCode
		{
			get { return MessageIdentifierCode; }
		}

		#endregion

		#region ITariffNumberStatusAndErrors

		ZString ITariffNumberStatusAndErrors.TariffNumber
		{
			get { return TariffNumber; }
		}
		public ZString TariffNumber;

		ZString ITariffNumberStatusAndErrors.PGAAgencyCode
		{
			get { return PGAAgencyCode; }
		}
		public ZString PGAAgencyCode;

		ZString ITariffNumberStatusAndErrors.PGALineNo
		{
			get { return PGALineNo; }
		}
		public ZString PGALineNo;

		#endregion
	}
}
