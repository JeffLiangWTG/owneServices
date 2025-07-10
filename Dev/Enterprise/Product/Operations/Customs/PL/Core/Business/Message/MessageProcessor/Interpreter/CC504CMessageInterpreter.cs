using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

public class CC504CMessageInterpreter(CusEntryHeader cusEntryHeader)
	: ImpExpMessageInterpreter<ICC504C>(cusEntryHeader)
{
	protected override void InterpretCore(ICC504C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC504);
		htmlWriter.WriteThematicBreak();

		var exportOperation = dataProvider.ExportOperation;
		var paramValueCollection = new ParamValueCollection {
			{ CommonStrings.CustomsOfficeOfExport, dataProvider.CustomsOfficeOfExportReferenceNumber },
			{ CommonStrings.LRN, dataProvider.LRN },
			{ CommonStrings.MRN, dataProvider.MRN },
			{ CommonStrings.DeclarationAmendmentDate, exportOperation?.AmendmentDateAndTime },
			{ CommonStrings.DeclarationAmendmentAcceptanceDate, exportOperation?.AmendmentAcceptanceDateAndTime },
		};
		htmlWriter.WriteParamValueTable(paramValues: paramValueCollection);
	}
}
