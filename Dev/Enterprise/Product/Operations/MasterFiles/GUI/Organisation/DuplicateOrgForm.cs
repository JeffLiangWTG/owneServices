using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DuplicateOrgForm : ZChildForm
	{
		public DuplicateOrgForm()
		{
			InitializeComponent();
		}

		public DuplicateOrgForm(OrgHeader organisation)
			: base(organisation)
		{
			InitializeComponent();
		}

		#region GUI Setup
		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		void SaveNewOrgButton_Click(object sender, System.EventArgs e)
		{
			if (SaveCore())
			{
				UserDecision = ContinueWithSave.Yes;
				DialogResult = DialogResult.Yes;
			}
		}

		/// <summary>
		/// Executes the core GUI behaviors of saving organizations.
		/// </summary>
		protected virtual bool SaveCore()
		{
			return true;
		}

		void CancelSaveButton_Click(object sender, System.EventArgs e)
		{
			UserDecision = ContinueWithSave.No;
			DialogResult = DialogResult.Cancel;
		}

		public ContinueWithSave UserDecision = ContinueWithSave.No;

		#endregion
	}
}
