using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbReleaseNoteForm : ZChildForm
	{
		public GlbReleaseNoteForm()
			: base()
		{
			InitializeComponent();
		}

		public GlbReleaseNoteForm(GlbReleaseNoteManager businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton);
			SortGridByDateDescending();
		}

		#region Implementation

		public new GlbReleaseNoteManager BusinessEntity
		{
			get { return (GlbReleaseNoteManager)base.BusinessEntity; }
		}

		void SortGridByDateDescending()
		{
			BusinessEntity.ReleaseNotes.ApplySort(GlbReleaseNoteSchema.GF_ReleaseNoteDate.Name, ListSortDirection.Descending);
			ReleaseNotesGrid.RefreshTableStyles();
		}

		#endregion

		#region View Note

		void ViewButton_Click(object sender, System.EventArgs e)
		{
			ViewNoteViaButton();
		}

		void ReleaseNotesGrid_DoubleClick(object sender, System.EventArgs e)
		{
			ViewNoteViaDoubleClick();
		}

		void ViewNoteViaButton()
		{
			if (ReleaseNotesGrid.ListManager.Position >= 0)
			{
				if (ReleaseNotesGrid.SelectedRowCount < 2)
				{
					GlbReleaseNote selectedNote = (GlbReleaseNote)ReleaseNotesGrid.ListManager.GetCurrent();
					OpenNoteInWebBrowser(selectedNote);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("1707dd69-d59a-4579-be7d-d74e031e1103", "Please select only one update note to view at a time."), Res.GetString("9ccc5c33-45c7-423f-bafd-0ed5b1b606a2", "Select a Note"));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("6ffffc20-2b3d-4b87-8f6c-f05a61e15ce9", "Please select an update note to view."), Res.GetString("e0276273-c0d2-482b-8b7a-22a887ab8c82", "Select a Note"));
			}
		}

		void ViewNoteViaDoubleClick()
		{
			if (HasRowAtMouseCursorPosition)
			{
				OpenNoteInWebBrowser(((GlbReleaseNote)ReleaseNotesGrid.SelectedElements[0]));
			}
		}

#if DEBUG
		protected virtual
#endif
		async void OpenNoteInWebBrowser(GlbReleaseNote note)
		{
			if (note.IsWiseTechGlobalItemViaTrustedMessaging)
			{
				if (!await ObjectFactory.Get<ISystemUserAccountCollectionTermChecker>().CheckTermAcknowledged())
				{
					return;
				}
			}

			WebUrlLauncher.Launch(note.GetDownloadURL());
			note.IsCurrentlyRead = true;
		}

#if DEBUG
		protected virtual
#endif
		bool HasRowAtMouseCursorPosition
		{
			get
			{
				int row = ReleaseNotesGrid.SelectedElements.Length > 0 ? ReleaseNotesGrid.HitTest(ReleaseNotesGrid.PointToClient(Cursor.Position)).Row : -1;
				return (row >= 0 && row < ReleaseNotesGrid.List.Count && ReleaseNotesGrid.List[row] != null);
			}
		}

		#endregion
	}
}
