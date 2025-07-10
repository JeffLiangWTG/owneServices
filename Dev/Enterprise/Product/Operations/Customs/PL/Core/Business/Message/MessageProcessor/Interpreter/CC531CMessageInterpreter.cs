using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC531CMessageInterpreter(CusEntryHeader cusEntryHeader) : ImpExpMessageInterpreter<ICC531C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC531C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC531);
		htmlWriter.WriteThematicBreak();
		htmlWriter.WriteParamValueTable(
			paramValues: new ParamValueCollection {
				{ CommonStrings.CustomsOfficeOfExportReferenceNumber, dataProvider.CustomsOfficeOfExportReferenceNumber },
				{ CommonStrings.MRN, dataProvider.MRN },
				{ CommonStrings.LodgementOfSupplementaryDeclarationStartDate, dataProvider.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationStartDate.ToShortDateString() },
				{ CommonStrings.LodgementOfSupplementaryDeclarationExpiryDate, dataProvider.TimerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationExpiryDate.ToShortDateString() },
				{ CommonStrings.TimerExpiryInformation, dataProvider.TimerExpiryForSupplementaryDeclaration?.TimerExpiryInformation },
			});
	}
}
