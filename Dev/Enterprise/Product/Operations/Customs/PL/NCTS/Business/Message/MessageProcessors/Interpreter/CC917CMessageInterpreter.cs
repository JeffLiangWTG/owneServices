using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.NCTS.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class CC917CMessageInterpreter(NctsCommonMovementHeader movementHeader) : NctsMessageInterpreterBase<IIE917>(movementHeader)
{
	protected override void InterpretCore(IIE917 dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteParamValueTable(
			caption: MessageTitles.IE917,
			paramValues: GetDeclarationIdentificationNumbers(dataProvider));

		var xmlErrorInformation = GetXmlErrorInformation(dataProvider.XMLErrors);
		if (!xmlErrorInformation.IsNullOrEmpty())
		{
			var titleColumns = new ZString[] {
				XmlError.LineNumber,
				XmlError.ColumnReference,
				XmlError.Pointer,
				XmlError.Code,
				XmlError.AdditionalText,
				XmlError.SubmittedValue,
			};

			htmlWriter.WriteParamValuesTableTranspose(
				caption: XmlError.Caption,
				paramValuesList: xmlErrorInformation,
				titleColumns: titleColumns,
				rowIndex: true);
		}
	}

	ParamValueCollection GetDeclarationIdentificationNumbers(IIE917 dataProvider) => NctsHeader.IsDepartureMovement
		? new ParamValueCollection {
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN, dataProvider.MRN },
		}
		: new ParamValueCollection {
			{ CommonStrings.MRN, dataProvider.MRN },
		};

	List<ParamValues> GetXmlErrorInformation(IReadOnlyCollection<IXMLError> xmlErrors)
	{
		var paramValueCollection = new List<ParamValues>();

		if (xmlErrors.IsNullOrEmpty())
		{
			return paramValueCollection;
		}

		var xmlErrorCodes = Factory.GetCL030XMLErrorCodeListOnlyList();
		var index = 0;
		foreach (var xmlError in xmlErrors)
		{
			var errorProperty = new List<ZString>();
			var errorCode = GetErrorCode(xmlError);
			errorProperty.Add(xmlError.ErrorLineNumber);
			errorProperty.Add(xmlError.ErrorColumnNumber);
			errorProperty.Add(xmlError.ErrorPointer);
			errorProperty.Add(errorCode + "-" + xmlErrorCodes.GetDescriptionFromCode(errorCode));
			errorProperty.Add(xmlError.ErrorText);
			errorProperty.Add(xmlError.OriginalAttributeValue);

			paramValueCollection.Add(new ParamValues(index.ToString(), errorProperty.ToArray()));
			index++;
		}

		return paramValueCollection;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	static string GetErrorCode(IXMLError error)
	{
		const string pattern = @"\d+";
		var errorCode = Regex.Match(error.ErrorCode, pattern, RegexOptions.Singleline).Value;
		return errorCode;
	}
}
