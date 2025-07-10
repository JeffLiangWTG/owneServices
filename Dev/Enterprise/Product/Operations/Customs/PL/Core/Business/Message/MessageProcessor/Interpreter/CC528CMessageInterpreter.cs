using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC528CMessageInterpreter(CusEntryHeader cusEntryHeader) : ImpExpMessageInterpreter<ICC528C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC528C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC528);
		htmlWriter.WriteThematicBreak();

		var paramValueCollection = new ParamValueCollection {
			{ CommonStrings.LinkedByMessageIdentification, dataProvider.CorrelationIdentifier },
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN , dataProvider.MRN },
			{ CommonStrings.DeclarationIsAcceptedOn, dataProvider.DeclarationAcceptanceDate?.ToShortDateString() },
			{ CommonStrings.CustomsOfficeOfExport, dataProvider.CustomsOfficeOfExport?.ReferenceNumber },
		};
		htmlWriter.WriteParamValueTable(paramValues: paramValueCollection);
	}
}
