using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class DocumentNumbersUserControl : ZUserControl
	{
		public DocumentNumbersUserControl()
		{
			InitializeComponent();
		}

		void DocumentNumbersEditButton_Click(object sender, System.EventArgs e)
		{
			var item = CurrentDataItem as CusEntryInstruction;
			if (item != null)
			{
				DocumentNumberCollectionForm.ShowDialog(item.DocumentNumbers);
			}
		}
	}
}
