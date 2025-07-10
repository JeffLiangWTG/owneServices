using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business;
using static Enterprise.Customs.PL.ExitControl.Business.ExitControlConstants;

namespace Enterprise.Customs.PL.ExitControl.Business;

sealed class CC521CMessageInterpreter(CusExitReport attachedObject) : MessageHtmlInterpreterBase<CusExitReport, ICC521C>(attachedObject)
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "HTML strings")]
	protected override void InterpretCore(ICC521C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteHeader(caption: InterpretationStrings.MessageTitles.CC521C);
		htmlWriter.WriteThematicBreak();

		htmlWriter.WriteParamValueTable(paramValues: new ParamValueCollection {
			{ InterpretationStrings.CommonStrings.CustomsOfficeOfExit, dataProvider.CustomsOfficeOfExitReferenceNumber },
			{ InterpretationStrings.CommonStrings.MRN, dataProvider.MRN },
			{ InterpretationStrings.DiversionRejection.Date, dataProvider.PreparationDateAndTime.ToShortDateString() },
			{ InterpretationStrings.DiversionRejection.ReasonCode, GetDiversionRejectionReasonCodeWithDescription(dataProvider.ExportOperation.DiversionRejectionReasonCode) },
			{ InterpretationStrings.DiversionRejection.Details, dataProvider.ExportOperation.DiversionRejectionText },
		});

		string GetDiversionRejectionReasonCodeWithDescription(string reasonCode) => !string.IsNullOrEmpty(reasonCode)
			? LinkedObject.Factory.GetCodeWithDescription(RefCusCodeListType.CL046, reasonCode)
			: string.Empty;
	}
}
