using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class DeliveryNotificationsUserControl : ZUserControl
	{
		public DeliveryNotificationsUserControl()
		{
			InitializeComponent();
		}

		protected JobDeclaration JobDeclaration => (JobDeclaration)CurrentDataItem;

		public void HandleDeclarationControlVisibilityChanged()
		{
			bool isECIWriteOff = JobDeclaration.IsECIWriteoff;
			bool isTSWDeclaration = JobDeclaration.IsTSWDeclaration;
			DeliveryNotificationPartyGroupBox.Visible = isTSWDeclaration || !isECIWriteOff;
			DeliveryNotificationPartyGroupBox.Text = isTSWDeclaration ?
				Enterprise.Customs.NZ.GUI.Res.GetString("9a1d9cef-6506-4285-95a6-6ac933be454f", "Delivery Notification Party") :
				Enterprise.Customs.NZ.GUI.Res.GetString("e53bc50d-6e59-4b64-995b-ff6fc0dcf75a", "Customs Delivery Authority");
		}
	}
}
