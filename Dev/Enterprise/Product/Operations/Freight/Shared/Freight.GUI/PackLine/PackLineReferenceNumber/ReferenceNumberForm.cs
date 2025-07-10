using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class ReferenceNumberForm : ZChildForm
	{
		public ReferenceNumberForm(PackLine parent)
			: base(parent)
		{
			InitializeComponent();

			this.ReferenceNumbersGrid.ReadOnly = !(GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsDeveloper);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			bool errors = false;

			foreach (BusinessObject bizo in this.ReferenceNumbersGrid.List)
			{
				bizo.RunPreSaveValidation();
				if (bizo.HasErrors)
				{
					errors = true;
				}
			}

			if (errors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Close();
			}
		}
	}
}
