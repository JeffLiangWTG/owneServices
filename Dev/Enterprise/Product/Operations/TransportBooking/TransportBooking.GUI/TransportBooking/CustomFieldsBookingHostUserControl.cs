using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class CustomFieldsBookingHostUserControl : ZUserControl
	{
		public CustomFieldsBookingHostUserControl()
		{
			InitializeComponent();
			CustomFieldsHostUserControl.SetNothingSetupMessageLabelText(Res.GetString("4EDF11A6-99F8-43B6-A5EC-F97A426EA323", "To make use of this tab, please setup Transport Booking custom fields in Workflow Manager."));
		}
	}
}
