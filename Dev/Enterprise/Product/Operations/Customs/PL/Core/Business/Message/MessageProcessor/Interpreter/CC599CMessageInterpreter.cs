using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CC599CMessageInterpreter(CusEntryHeader entryHeader) : ImpExpMessageInterpreter<ICC599C>(entryHeader)
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
	protected override void InterpretCore(ICC599C dataProvider, HtmlBlockWriter htmlWriter)
	{
		htmlWriter.WriteParamValueTable(
			caption: AESMessageCodes.Codes.CC599,
			paramValues: new ParamValueCollection {
				{ "LRN", dataProvider.LRN },
				{ "MRN", dataProvider.MRN },
			});

		if (dataProvider.ExitControlResult is IExitControlResult exitControlResult)
		{
			AppendExitResult();
		}

		htmlWriter.WriteParagraph("Status is set to " + EntryHeader.CH_EntryStatus);
		return ;

		void AppendExitResult()
		{
			var exitStoppedDate = exitControlResult.ExitStoppedDate;

			htmlWriter.WriteParagraph(exitStoppedDate is null
				? $"Declaration has exited the EU on {exitControlResult.ExitDate} according to customs office {dataProvider.CustomsOfficeOfExitActualReferenceNumber}."
				: $"Declaration has not exited the EU. The exit was stopped on {exitStoppedDate.Value} according to customs office {dataProvider.CustomsOfficeOfExitActualReferenceNumber}.");

			var exitControlResultCode = exitControlResult.Code;
			htmlWriter.WriteParagraph($"Control result code is {exitControlResultCode} {GetExitControlResultCodeDescription(exitControlResultCode)}");

			AppendStateOfSeals(exitControlResult.StateOfSeals);
		}

		void AppendStateOfSeals(int? stateOfSeals)
		{
			var stateOfSealsAsDescription = stateOfSeals is 1 ? "Ok" : "Not Ok";
			htmlWriter.WriteParagraph($"State of seals is stateOfSeals {stateOfSealsAsDescription}");
		}

		string GetExitControlResultCodeDescription(string code) => code switch
		{
			AESExitControlResultCodes.Codes.A1 => AESExitControlResultCodes.Descriptions.A1,
			AESExitControlResultCodes.Codes.A2 => AESExitControlResultCodes.Descriptions.A2,
			AESExitControlResultCodes.Codes.A4 => AESExitControlResultCodes.Descriptions.A4,
			AESExitControlResultCodes.Codes.B1 => AESExitControlResultCodes.Descriptions.B1,
			_ => string.Empty
		};
	}
}
