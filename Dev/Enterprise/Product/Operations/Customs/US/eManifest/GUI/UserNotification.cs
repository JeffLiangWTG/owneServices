using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public class UserNotification : Customs.GUI.UserNotification, IUserNotification
	{
		#region Implementation of IUserNotification

		public bool ShowShipmentsActionsDialog(IShipmentActionsProvider provider, ZString messageDescription)
		{
			return ZFormModaliser.ShowDialogAndDispose(new SendingActionForm(provider, messageDescription)) == DialogResult.OK;
		}

		#endregion
	}
}
