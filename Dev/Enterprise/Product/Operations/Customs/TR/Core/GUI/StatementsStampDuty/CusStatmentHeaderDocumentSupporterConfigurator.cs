using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	sealed class CusStatmentHeaderDocumentSupporterConfigurator : ITRCusStatmentHeaderDocumentSupporterConfigurator
	{
		public DocumentSupporterDataState PrintStampDutyLedgerConfig(CusStatementHeader statementHeader, DocumentSupporterDataState dataState)
		{
			var printDate = statementHeader.B2_PrintDate;
			var printMessage = printDate.IsEmpty ? Res.GetString("6323B168-8362-49FA-87EF-5966DB71C5EB", "Do you want to print The Stamp Duty Ledger?") : Res.GetString("D3419D7B-84E1-4B46-B209-5D533D3C7D7C", "Do you want to print The Stamp Duty Ledger? It was printed on {0}", printDate.ToShortDateString());
			var dialogResult = Globals.Message.Show(printMessage, Res.GetString("23FAE0F7-19EA-4304-80AD-37B6B496AFCD", "Print Stamp Duty Ledger Confirmation"),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Information, ZDialogResult.No);

			var errorMessage = Res.GetString("41C670C5-3366-4685-AAC6-EBDCB54FEB62", "Print process for Stamp Duty Ledger is Canceled");

			if (dialogResult == ZDialogResult.Yes)
			{
				using (var form = new StampDutyLedgerDocumentEntryForm(statementHeader))
				{
					ZFormModaliser.ShowDialogAndDispose(form);

					if (form.DialogResult != DialogResult.OK)
					{
						dataState = new DocumentSupporterDataState(false, errorMessage);
					}
				}
			}
			else
			{
				dataState = new DocumentSupporterDataState(false, errorMessage);
			}

			return dataState;
		}
	}
}
