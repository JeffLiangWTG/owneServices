using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCollectionNotesControl : ZUserControl
	{
		public OrgCollectionNotesControl()
		{
			InitializeComponent();
		}
		ZGroupBox CallDetailsGroupBox;
		public ZCodeFindBox CallingStaffZCodeFindBox;
		ZGuidFindBox ContactGuidFindBox;
		ZTextBox CallDetailTextBox;
		ZGuidDropEdit ContactDropEdit;
		ZDropEdit StatusDropEdit;
		ZDropEdit DispositionDropEdit;
		ZDateEdit FollowUpDateEdit;
		ZTextBox PhoneNumberTextBox;
		ZDateEdit CallDateDateEdit;
		ZButton SendEmailButton;
		ZCodeFindBox CallStatusCodeFindBox;

		#region OrgHeader

		public OrgHeader OrgHeader
		{
			get { return (OrgHeader)CurrentDataItem; }
		}

		#endregion

		#region Filtering

		public virtual void FindButton_Click(object sender, EventArgs e)
		{
			OrgHeader.LoadCollectionNotesWithFiltering();
		}

		public virtual void ClearButton_Click(object sender, EventArgs e)
		{
			OrgHeader.ClearCollectionNotesFilterValues();
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				//if (fOrgHeader != null)
				//{
				//Org.FilterValidationError -= new EventHandler(SalesCallReportingControl_FilterValidationError);
				//}

				//if(components != null)
				//{
				//    components.Dispose();
				//}
			}

			base.Dispose(disposing);
		}

		#endregion

		void SendEmailButton_Click(object sender, EventArgs e)
		{
			OrgCollectionNote note = OrgCollectionCallBoundGrid.ListManager.GetCurrent() as OrgCollectionNote;
			if (note != null)
			{
				CollectionNoteEmailCreator newEmail = new CollectionNoteEmailCreator(note);
				EmailContactForm form = new EmailContactForm(newEmail);
				form.Show();
			}
		}

		void OrgCollectionCallBoundGrid_AfterBind(object sender, EventArgs e)
		{
			OrgCollectionCallBoundGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
			ListManager_CurrentChanged(null, null);
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			SendEmailButton.Enabled = OrgCollectionCallBoundGrid.ListManager != null && OrgCollectionCallBoundGrid.ListManager.Count > 0 && OrgCollectionCallBoundGrid.ListManager.Position != -1;
		}
	}
}
