using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.ExitControl.Business.ExitControlConstants.InterpretationStrings;

namespace Enterprise.Customs.PL.ExitControl.Business;

sealed class CC557CMessageInterpreter(CusExitReport attachedObject) : MessageHtmlInterpreterBase<CusExitReport, ICC557C>(attachedObject)
{
	protected override void InterpretCore(ICC557C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC557);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: $"CC{dataProvider.ExportOperation?.BusinessRejectionType}-{dataProvider.ExportOperation?.RejectionReason}",
			paramValues: new ParamValueCollection {
				{ CommonStrings.CustomsOfficeOfExit, dataProvider.CustomsOfficeOfExitActual?.ReferenceNumber },
				{ CommonStrings.LRN, dataProvider.LRN },
				{ CommonStrings.MRN, dataProvider.MRN },
		});
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(
			caption: $"The message {dataProvider.ExportOperation?.BusinessRejectionType} received rejection [CC557C] from Customs",
			paramValues: new ParamValueCollection {
				{ $"1. {CommonStrings.RejectedMessageIdentification}", dataProvider.CorrelationIdentifier },
				{ $"2. {CommonStrings.BusinessRejectionType}", dataProvider.ExportOperation?.BusinessRejectionType },
				{ $"3. {CommonStrings.RejectionDateAndTime}", dataProvider.ExportOperation?.RejectionDateAndTime },
				{ $"4. {CommonStrings.RejectionCode}", dataProvider.ExportOperation?.RejectionCode },
				{ $"5. {CommonStrings.RejectionReason}", dataProvider.ExportOperation?.RejectionReason },
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

	static IEnumerable<ParamValues> GetFunctionalErrorsInformation(IReadOnlyCollection<IFunctionalError> functionalErrors) =>
		functionalErrors.Select((error, index) => new ParamValues(
			Name: $"{index + 1}",
			StringValues:
			[
				$"{index + 1}",
				error.ErrorPointer,
				error.ErrorCode.ToString(),
				error.ErrorReason,
				error.OriginalAttributeValue,
			]));
}
