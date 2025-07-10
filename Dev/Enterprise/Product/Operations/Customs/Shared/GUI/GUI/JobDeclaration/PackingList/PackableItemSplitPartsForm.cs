using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class PackableItemSplitPartsForm : ZChildForm
	{
		public PackableItemSplitPartsForm(PackableItemsSplitter packableItemsSplitter)
			: base(packableItemsSplitter)
		{
			InitializeComponent();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				var packableItemsSplitter = (PackableItemsSplitter)BusinessEntity;
				if (packableItemsSplitter.PackableItemParts.Count > 1)
				{
					DialogResult = System.Windows.Forms.DialogResult.OK;
					Close();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("97b069da-d7a3-4fd2-94b3-0b188bb430a9", "Please enter at least two items."));
				}
			}
		}
	}
}
