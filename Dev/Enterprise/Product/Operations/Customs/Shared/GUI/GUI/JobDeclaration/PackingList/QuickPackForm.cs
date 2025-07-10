using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class QuickPackForm : ZChildForm
	{
		public QuickPackForm(QuickPack quickPack)
			: base(quickPack)
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (passedByCheck())
			{
				DialogResult = DialogResult.OK;
			}
		}

		bool passedByCheck()
		{
			var result = true;
			var quickPack = (QuickPack)BusinessEntity;
			if (quickPack.QuickPackItems.Cast<QuickPackItem>().All(c => c.Pack.IsEmpty))
			{
				result = false;
				Globals.Message.ShowInformation(Res.GetString("D1AB0583-FBD4-4EC8-B6C2-DC8AAA5E64EF", "Please select at least one pack."));
			}
			return result;
		}
	}
}
