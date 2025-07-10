using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public class ESignatureHelper : IESignatureHelper
	{
		public bool CanSignMessage(MessageSendAcknowledgeAndSign signInData)
		{
			return (ZFormModaliser.ShowDialogAndDispose(new ESignatureForm(signInData)) == DialogResult.OK);
		}
	}
}
