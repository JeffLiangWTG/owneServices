using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class MessageEditForm : ZChildForm
	{
		public MessageEditForm()
		{
			InitializeComponent();
		}

		public ZString EditMessage(ZString messageText)
		{
			ZString result = messageText;
			zTextBoxMessage.Text = messageText;
			if (ZFormModaliser.ShowDialogWithoutDispose(this) == DialogResult.OK)
			{
				result = zTextBoxMessage.Text;
			}
			return result;
		}
	}
}
