using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC529CMessageInterpreter(CusEntryHeader cusEntryHeader)
	: ImpExpMessageInterpreter<ICC529C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC529C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC529);
		htmlWriter.WriteThematicBreak();

		var paramValueCollection = new ParamValueCollection {
			{ CommonStrings.LinkedByMessageIdentification, dataProvider.CorrelationIdentifier },
			{ CommonStrings.MessageSentOn, dataProvider.PreparationDateAndTime },
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN , dataProvider.MRN },
			{ CommonStrings.DeclarationIsReleasedOn, dataProvider.ExportOperation?.ReleaseDate },
			{ CommonStrings.CustomsOfficeOfExport, dataProvider.CustomsOfficeOfExport?.ReferenceNumber },
			{ CommonStrings.ControlResult, dataProvider.ControlResultDate },
		};
		if (EntryHeader.CusEntryNumber == null)
		{
			paramValueCollection.Add(CommonStrings.DeclarationIsAcceptedOn, dataProvider.ExportOperation?.DeclarationAcceptanceDate);
		}
		htmlWriter.WriteParamValueTable(paramValues: paramValueCollection);
	}
}
