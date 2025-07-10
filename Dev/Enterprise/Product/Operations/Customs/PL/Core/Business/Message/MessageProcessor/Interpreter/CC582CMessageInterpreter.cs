using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC582CMessageInterpreter(CusEntryHeader cusEntryHeader) : ImpExpMessageInterpreter<ICC582C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC582C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC582);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(paramValues: new ParamValueCollection {
			{ CommonStrings.CustomsOfficeOfExport, dataProvider.CustomsOfficeOfExportReferenceNumber },
			{ CommonStrings.MRN, dataProvider.MRN },
			{ CommonStrings.RequestOnNonExitedExportDate, dataProvider.ExportOperation.RequestOnNonExitedExportDate },
			{ CommonStrings.LimitForResponseDate, dataProvider.ExportOperation.LimitForResponseDate },
		});
	}
}
