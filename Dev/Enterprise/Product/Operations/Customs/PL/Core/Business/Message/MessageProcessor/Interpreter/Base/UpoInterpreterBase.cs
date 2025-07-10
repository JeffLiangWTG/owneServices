using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Polish output")]
public class UPOInterpreterBase<TLinkedObject>(TLinkedObject linkedObject)
	: MessageHtmlInterpreterBase<TLinkedObject, IUpo>(linkedObject)
	where TLinkedObject : EnterpriseBusinessObject
{
	protected override void InterpretCore(IUpo dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(GetMessageTitle(dataProvider) + GetTransmitDocumentTypeForTitle(dataProvider));
		htmlWriter.WriteThematicBreak();

		var paramsValues = new ParamValueCollection
		{
			{ CommonStrings.TransmitDocumentName, dataProvider.TransmitDocumentAbbreviated },
			{ CommonStrings.TransmitDocumentNumber, dataProvider.CorrelationIdentifier },
			{ CommonStrings.ExternalSystemID, dataProvider.ExternalSystemID },
			{ CommonStrings.IdentyfikatorECIPSEAP, dataProvider.IdentifierEcipSeap },
			{ CommonStrings.Applicant, dataProvider.NameOfTheApplicant },
			{ CommonStrings.IssuingSystem, dataProvider.NameOfIssuingSystem },
			{ CommonStrings.DateOfCreation, dataProvider.DateOfCreation },
			{ CommonStrings.DateOfCompletion, dataProvider.DateOfCompletion },
		};
		paramsValues.AddRange(AdditionalParams);
		htmlWriter.WriteParamValueTable(paramsValues);
		htmlWriter.WriteThematicBreak();

		switch (dataProvider.Errors?.Count)
		{
			case null:
			case 0: break;
			case 1:
				htmlWriter.WriteHeader(level: HtmlTextWriterTag.H3,
					caption: CaptionStrings.Error,
					styles: [(HtmlTextWriterStyle.MarginBottom, (NoResString)"5px")]);
				WriteRecord(htmlWriter, dataProvider.Errors.First());
				break;

			default:
				htmlWriter.WriteParamValueTable(
					caption: CaptionStrings.Errors,
					dataProvider.Errors.Select((record, index)
						=> new HtmlBlockWriterParam<IUpoError>(
							name: $"No {(index + 1)}",
							dataSource: record,
							writeValueCallback: WriteRecord)));
				break;
		}
	}

	public virtual string GetMessageTitle(IUpo dataProvider) => MessageTitles.UPO;

	protected virtual IEnumerable<IParamValue> AdditionalParams => [];

	public static string GetTransmitDocumentTypeForTitle(IUpo dataProvider)
		=> !string.IsNullOrWhiteSpace(dataProvider.TransmitDocumentType)
			? $" ({dataProvider.TransmitDocumentType})"
			: string.Empty;

	static void WriteRecord(HtmlBlockWriter htmlWriter, IUpoError record)
	{
		if (record.ErrorTexts.Count == 0 && record.XPathPointers.Count == 0)
		{
			return;
		}

		if (record.XPathPointers.Count > 0)
		{
			htmlWriter.WriteHeader(level: HtmlTextWriterTag.H4,
				caption: record.XPathPointers.Count > 1 ? CaptionStrings.Locations : CaptionStrings.Location,
				styles: [
					(HtmlTextWriterStyle.MarginTop, (NoResString)"8px"),
					(HtmlTextWriterStyle.MarginBottom, (NoResString)"5px"),
				]);
			var index = 1;
			foreach (var xPathPointer in record.XPathPointers)
			{
				htmlWriter.WriteText(xPathPointer);
				if (index++ < record.XPathPointers.Count)
				{
					htmlWriter.WriteLineBreak();
				}
			}
		}

		if (record.ErrorTexts.Count > 0)
		{
			htmlWriter.WriteHeader(level: HtmlTextWriterTag.H4,
				caption: CaptionStrings.Problem,
				styles: [
					(HtmlTextWriterStyle.MarginTop, (NoResString)"10px"),
					(HtmlTextWriterStyle.MarginBottom, (NoResString)"3px"),
				]);
			htmlWriter.WriteMultipleTextWithCaption(record.ErrorTexts.Select(x => new ParamValue(x.Language, x.Text)));
		}
	}
}
