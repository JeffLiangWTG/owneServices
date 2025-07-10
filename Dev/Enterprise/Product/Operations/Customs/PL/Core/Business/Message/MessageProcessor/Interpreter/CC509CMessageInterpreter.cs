using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;

namespace Enterprise.Customs.PL.Business;

sealed class CC509CMessageInterpreter(CusEntryHeader cusEntryHeader)
	: ImpExpMessageInterpreter<ICC509C>(cusEntryHeader)
{
	protected override bool UseExtendedGlobalStyle => true;

	protected override void InterpretCore(ICC509C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: MessageTitles.CC509);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(@class: (NoResString)"no-border bold-font",
			paramValues: new ParamValueCollection
			{
				{ CommonStrings.LRN, ": " + dataProvider.LRN },
				{ CommonStrings.MRN , ": " + dataProvider.MRN },
				{ CommonStrings.DeclarationIsInvalidated, ": " + dataProvider.ExportOperation?.InvalidationDecisionDateAndTime.ToShortDateString() },
				{ CommonStrings.CustomsOfficeOfExport, ": " + dataProvider.CustomsOfficeOfExportReferenceNumber },
				{ GetInvalidatedByCustomsDescription(dataProvider.ExportOperation?.InvalidationInitiatedByCustoms ?? false), string.Empty },
				{ CommonStrings.InvalidationJustification, ": " + dataProvider.ExportOperation?.InvalidationJustification },
			});
		return;

		string GetInvalidatedByCustomsDescription(bool invalidatedByCustoms) =>
			invalidatedByCustoms
				? InvalidationInitiatedBy.InitiatedByCustoms
				: InvalidationInitiatedBy.InitiatedByDeclarant;
	}
}
