using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGDataItemForm : ZChildForm
	{
		public UNDGDataItemForm(IUNDGDataItemProvider uNDGDataItemProvider)
			: base((BusinessObject)uNDGDataItemProvider)
		{
			InitializeComponent();
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			bool errors = false;

			foreach (BusinessObject bizo in this.zGrid1.List)
			{
				bizo.RunPreSaveValidation();
				if (bizo.HasErrors())
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

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
