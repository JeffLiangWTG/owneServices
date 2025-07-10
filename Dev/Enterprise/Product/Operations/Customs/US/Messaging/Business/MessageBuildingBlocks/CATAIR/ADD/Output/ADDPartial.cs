using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.Add5106toImporterFileProcessingResults)]
	public partial class ADDT345 : MessageBlock, IStatusesAndErrors
	{
		#region IStatusesAndErrors Members

		ZString IStatusesAndErrors.Code
		{
			get { return RecordType.ToString(); }
		}

		ZString IStatusesAndErrors.LineNumber
		{
			get { return ZString.Empty; }
		}

		ZString IStatusesAndErrors.NarrativeMessage
		{
			get { return NarrativeMessage; }
		}

		ZString IStatusesAndErrors.ReferenceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
