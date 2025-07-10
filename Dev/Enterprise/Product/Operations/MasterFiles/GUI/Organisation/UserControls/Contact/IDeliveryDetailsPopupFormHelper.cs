using System.Windows.Forms;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI
{
	public interface IDeliveryDetailsPopupFormHelper
	{
		void ShowDeliveryDetailsPopupForm(IEmailDeliveryDetailsProvider provider, Form parentForm);
	}
}
