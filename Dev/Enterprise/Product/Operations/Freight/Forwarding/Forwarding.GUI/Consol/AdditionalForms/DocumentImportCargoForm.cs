using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentImportCargoForm : ZChildForm
	{
		public DocumentImportCargoForm(DocumentImportCargoLabel businessObject)
			: base(businessObject)
		{
			InitializeComponent();
		}

		#region Button Clicks

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			if (BusinessEntity.Notifications.HasErrors())
			{
				Globals.Message.ShowError(Res.GetString("c9294973-6615-4ccb-9061-8a307c075768", "All errors must be fixed before you proceed.") + "\r\n\r\n" + ((DocumentImportCargoLabel)BusinessEntity).Notifications.GetErrors().ToUniqueMessageListString());
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		#endregion
	}
}
