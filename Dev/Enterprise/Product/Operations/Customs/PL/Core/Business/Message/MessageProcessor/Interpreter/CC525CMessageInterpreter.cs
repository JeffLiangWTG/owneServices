using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class CC525CMessageInterpreter(CusEntryHeader cusEntryHeader) : ImpExpMessageInterpreter<ICC525C>(cusEntryHeader)
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "HTML strings")]
	protected override void InterpretCore(ICC525C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteParamValueTable(
			caption: AESMessageCodes.Codes.CC525,
			paramValues: new ParamValueCollection {
				{ "MRN", dataProvider.MRN },
				{ "Declaration has been released for exit on", dataProvider.ExportOperation.ReleaseDate.ToShortDateString() },
				{ "Customs Office Of Exit - Actual", dataProvider.CustomsOfficeOfExitReferenceNumber },
				{ "Storing Flag", dataProvider.ExportOperation.StoringFlag },
				{ "Status is set to", EntryHeader.CH_EntryStatus },
			});
	}
}
