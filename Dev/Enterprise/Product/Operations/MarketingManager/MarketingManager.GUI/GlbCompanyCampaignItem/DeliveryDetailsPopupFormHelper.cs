using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class DeliveryDetailsPopupFormHelper : IDeliveryDetailsPopupFormHelper
	{
		public void ShowDeliveryDetailsPopupForm(IEmailDeliveryDetailsProvider provider, Form parentForm)
		{
			var form = new DeliveryDetailsPopupForm();
			form.SetDataBinding(provider, "");
			ZFormModaliser.Show(form, parentForm);
		}
	}
}
