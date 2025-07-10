using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.GUI.Common
{
	class LabelsDocumentEventHandler : IDocumentEventsHandler
	{
		public bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return menuItem.SU_MenuName == LabelsName.ImportLabel || menuItem.SU_MenuName == LabelsName.OnForwardingLabel || menuItem.SU_MenuName == LabelsName.TranshipmentLabel;
		}

		public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			var documentSupporter = DocumentSupporter as ILabelDocumentSupporter;
			if (documentSupporter != null)
			{
				if (e.MenuItem.SU_MenuName == LabelsName.ImportLabel && !documentSupporter.SupportImportLabels)
				{
					e.Cancel = true;
					Globals.Message.ShowInformation(documentSupporter.ImportLabelErrorMessage, Res.GetString("94f58da4-de02-42c1-ba73-094c6254c20a", "Import Labels"));
				}
				else if (e.MenuItem.SU_MenuName == LabelsName.OnForwardingLabel && !documentSupporter.SupportOnForwardingLabels)
				{
					e.Cancel = true;
					Globals.Message.ShowInformation(documentSupporter.OnForwardingErrorMessage, Res.GetString("d1ea084e-1aec-4dc7-90a4-3252ea83c552", "On Forwarding Labels"));
				}
				else if (e.MenuItem.SU_MenuName == LabelsName.TranshipmentLabel && !documentSupporter.SupportTranshipmentLabels)
				{
					e.Cancel = true;
					Globals.Message.ShowInformation(documentSupporter.TranshipmentErrorMessage, Res.GetString("2a2f3b0f-b7be-4324-987d-74e0e14baff7", "Transhipment Labels"));
				}
			}
		}

		public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		public DocumentSupporter DocumentSupporter { get; set; }
	}
}
