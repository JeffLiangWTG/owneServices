using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC556CMessageInterpreter(CusEntryHeader cusEntryHeader)
	: ImpExpMessageInterpreter<ICC556C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC556C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC556);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: $"CC{dataProvider.ExportOperation?.BusinessRejectionType}-{dataProvider.ExportOperation?.RejectionReason}",
			paramValues: new ParamValueCollection {
				{ CommonStrings.OfficeRejectMessage, dataProvider.CustomsOfficeOfExportReferenceNumber },
				{ CommonStrings.LRN, dataProvider.LRN },
				{ CommonStrings.MRN, dataProvider.MRN },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: $"The message {dataProvider.ExportOperation?.BusinessRejectionType} received rejection [CC556C] from Customs",
			paramValues: new ParamValueCollection {
				{ "1. " + CommonStrings.RejectedMessageIdentification, dataProvider.CorrelationIdentifier },
				{ "2. " + CommonStrings.BusinessRejectionType, dataProvider.ExportOperation?.BusinessRejectionType },
				{ "3. " + CommonStrings.RejectionDateAndTime, dataProvider.ExportOperation?.RejectionDateAndTime },
				{ "4. " + CommonStrings.RejectionCode, dataProvider.ExportOperation?.RejectionCode },
				{ "5. " + CommonStrings.RejectionReason, dataProvider.ExportOperation?.RejectionReason },
		});
		htmlWriter.WriteThematicBreak();

		var functionalErrors = dataProvider.FunctionalErrors;
		if (!functionalErrors.IsNullOrEmpty())
		{
			var titleColumns = new ZString[] {
				CommonStrings.Number,
				FunctionalErrorsString.ErrorPointer,
				FunctionalErrorsString.ErrorCode,
				FunctionalErrorsString.ErrorReason,
				FunctionalErrorsString.OriginalAttributeValue,
			};

			htmlWriter.WriteParamValuesTableTranspose(
				caption: CaptionStrings.FunctionalErrors,
				paramValuesList: GetFunctionalErrorsInformation(functionalErrors),
				titleColumns: titleColumns,
				rowIndex: false);
		}
	}

	IEnumerable<ParamValues> GetFunctionalErrorsInformation(IReadOnlyCollection<IFunctionalError> functionalErrors)
	{
		if (functionalErrors.IsNullOrEmpty())
		{
			yield break;
		}

		var index = 0;
		foreach (var functionalError in functionalErrors)
		{
			yield return new ParamValues((++index).ToString(), [
				index.ToString(),
				functionalError.ErrorPointer,
				functionalError.ErrorCode.ToString(),
				functionalError.ErrorReason,
				functionalError.OriginalAttributeValue,
			]);
		}
	}
}
