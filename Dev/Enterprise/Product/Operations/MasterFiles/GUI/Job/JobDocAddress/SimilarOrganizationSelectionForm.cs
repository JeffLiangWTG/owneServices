using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Represents a form for selecting an organization among similar ones.
	/// </summary>
	public partial class SimilarOrganizationSelectionForm : DuplicateOrgForm
	{
		#region Constructors
		public SimilarOrganizationSelectionForm()
		{
			InitializeComponent();
		}

		void SimilarOrgsDisplayGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			SaveNewOrgButton.PerformClick();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="organization">The organization whose similar ones are displayed.</param>
		public SimilarOrganizationSelectionForm(OrgHeader organization) : base(organization)
		{
			InitializeComponent();
			FirstSelectedOrganization = null;
			newOrganizationButton.DialogResult = DialogResult.Cancel;
			SimilarOrgsDisplayGrid.MouseDoubleClick += SimilarOrgsDisplayGrid_MouseDoubleClick;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the current selected organizations.
		/// </summary>
		public OrgHeader FirstSelectedOrganization { get; private set; }

		/// <summary>
		/// Gets whether to create a new organization.
		/// </summary>
		public bool ToCreateNew { get; private set; }
		#endregion

		#region Private/Protected Members
		void NewOrganizationButton_Click(object sender, EventArgs e)
		{
			UserDecision = ContinueWithSave.No;
			DialogResult = DialogResult.Cancel;
			ToCreateNew = true;
		}

		/// <summary>
		/// Executes the core GUI behaviors of saving organizations.
		/// </summary>
		protected override bool SaveCore()
		{
			base.SaveCore();
			if (SimilarOrgsDisplayGrid.SelectedRowCount < 1)
			{
				Globals.Message.ShowError(Res.GetString("27C68C9E-229D-49AC-9EAD-23FA08AB1F15", "Please select one organization."));
				return false;
			}
			if (SimilarOrgsDisplayGrid.SelectedRowCount > 1)
			{
				Globals.Message.ShowError(Res.GetString("79C32080-A74D-4565-A107-98D9519F7CC9", "Please select only one organization."));
				return false;
			}
			FirstSelectedOrganization = ((OrgPatternMatch)SimilarOrgsDisplayGrid.GetFirstSelectedRow()).Header;
			return true;
		}
		#endregion
	}
}
