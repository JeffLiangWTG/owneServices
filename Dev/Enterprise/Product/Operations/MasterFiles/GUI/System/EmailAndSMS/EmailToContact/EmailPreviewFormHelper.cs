using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	static class EmailPreviewFormHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Debug")]
		public static void ShowPreviewForm(EmailWithAttachment bizO)
		{
			IDisposable previewFileDisposable;
			using (new ZWaitCursorChanger())
			{
				previewFileDisposable = bizO.SetupPreviewFile();
			}

			using (previewFileDisposable)
			{
				if (!string.IsNullOrEmpty(bizO.PreviewFilePath))
				{
					ZFormModaliser.ShowDialogAndDispose(RichTextEmailDisplayZForm.FromFile(bizO.Body, bizO.PreviewFilePath, bizO.Subject));
#if DEBUG
					if (Globals.IsTest)
					{
						Application.DoEvents();
					}
#endif
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("2ffa49d2-5232-400e-a620-9d0ed0a1837d", "Cannot show email preview"));
				}
			}
		}
	}
}
