using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	public class CandidateManagementZChildForm : ZChildForm
	{
		public CandidateManagementZChildForm(CandidateModuleBusinessObject businessEntity) : base(businessEntity)
			=> CaptionRenderingEnabled = true;

		protected override ContinueWithSave ValidateAndSave()
			=> (Controls.Find("CandidateDetailsControl", true).First() as CandidateDetailsControl).ValidateAndSave();

		protected override void OnClosing(CancelEventArgs e)
		{
			var candidateManagementControl = (CandidateManagementControl)Controls.Find("CandidateManagementControl", true).First();
			candidateManagementControl.RemoveHandlers();

			base.OnClosing(e);

			if (e.Cancel)
			{
				candidateManagementControl.AddCandidateChangedHandler();
			}
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			var eDocsUserControl = this.FindSingleOrDefault<eDocsUserControl>("eDocsUserControl");
			if (eDocsUserControl == null)
			{
				return;
			}

			var storageDocsGrid = eDocsUserControl.FindSingleOrDefault<DocumentsZGrid>("StorageDocsGrid");
			if (storageDocsGrid == null)
			{
				return;
			}

			storageDocsGrid.OnDragDropForRemote(drgevent);
		}
	}
}
