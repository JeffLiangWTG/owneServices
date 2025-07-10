using System;
using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Documents
{
	public partial class MAFCoverSheetForm : ZChildForm
	{
		public MAFCoverSheetForm()
		{
		}

		public MAFCoverSheetForm(NZDocsMAFCoverSheet bO)
			: base(bO)
		{
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			PrintMAFCoverSheet();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void PrintMAFCoverSheet()
		{
			// Dialog Result is Set on this control. No Code Required, but having this mathod stops the Basher Complaining.
		}
	}
}
