using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	public class OGADispositionDataCollection : DependentCusAddInfoCollection<OGADispositionData, BusinessObject>
	{
		public OGADispositionDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USOGADisposition)
		{
		}

		public OGADispositionData AddNewIfNotExist(IPGADispositionProvider dispProvider, string envelopeNumber = "")
		{
			OGADispositionData result = null;

			foreach (OGADispositionData disposition in this)
			{
				if (disposition.US_Code == dispProvider.PGALineDispositionCode &&
					disposition.US_OGAIdentifier == dispProvider.OtherAgencyQuotaIdentifier &&
					disposition.US_ProgramCode == dispProvider.GovernmentAgencyProgramCode &&
					disposition.US_DispositionDate == dispProvider.DispositionDateTime &&
					disposition.US_OGADispositionBeginningCBPLine == dispProvider.BeginningCBPLineNo &&
					disposition.US_OGADispositionBeginningOGALine == dispProvider.BeginningOGALineNo &&
					disposition.US_EnvelopeNumber == envelopeNumber
					)
				{
					result = disposition;
					break;
				}
			}

			if (result == null)
			{
				result = AddNew();
				result.US_Code = dispProvider.PGALineDispositionCode;
				result.US_DispositionDate = dispProvider.DispositionDateTime;
				result.US_OGAIdentifier = dispProvider.OtherAgencyQuotaIdentifier;
				result.US_ProgramCode = dispProvider.GovernmentAgencyProgramCode;
				result.US_DispositionDate = dispProvider.DispositionDateTime;
				result.US_OGADispositionBeginningCBPLine = dispProvider.BeginningCBPLineNo;
				result.US_OGADispositionBeginningOGALine = dispProvider.BeginningOGALineNo;
				result.US_OGADispositionStatusCode = dispProvider.EntryDispositionCode;
				result.US_OGADispositionStatusMessage = dispProvider.NarrativeMessage.Left(USOGADispositionDataAddInfo.Schema.US_OGADispositionStatusMessageMaxLength);
				result.US_OGADispositionBeginningCBPLine = dispProvider.BeginningCBPLineNo;
				result.US_BeginningTariffPosition = dispProvider.BeginningTariffPosition;
				result.US_OGADispositionRangeIndicator = dispProvider.RangeIndicator;
				result.US_OGADispositionEndCBPLine = dispProvider.EndingCBPLineNo;
				result.US_EndingTariffPosition = dispProvider.EndingTariffPosition;
				result.US_OGADispositionEndOGALine = dispProvider.EndingOGALineNo;
				result.US_OGADispositionStatusCodeEntryLine = dispProvider.EntryLineDispositionCode;
				result.US_ReviewReasonCode = dispProvider.ReviewReasonCode;
				result.US_DocumentType = dispProvider.DocumentTypeCode;
				result.US_Order = (ZShort)Count;
				result.US_EnvelopeNumber = envelopeNumber;
			}
			return result;
		}
	}
}
