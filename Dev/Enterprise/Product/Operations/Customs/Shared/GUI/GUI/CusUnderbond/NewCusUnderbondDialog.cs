using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	/// <summary>
	/// Summary description for NewCusUnderbondDialog.
	/// </summary>
	public partial class NewCusUnderbondDialog : ZArchitecture.GUI.ZChildForm
	{
		public NewCusUnderbondDialog()
		{
			InitializeComponent();
		}

		public NewCusUnderbondDialog(NewCusUnderbondNonPersistent bizo) : base(bizo)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			this.InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		internal void OKButton_Click(object sender, System.EventArgs e)
		{
			fOKPressed = true;
			this.Close();
		}

		bool fOKPressed;
		public bool OKPressed
		{
			get
			{
				return fOKPressed;
			}
		}

		#region Windows Form Designer generated code
		#endregion

	}
}
