using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomFieldsCommunicationUserControl : ZUserControl
	{
		public CustomFieldsCommunicationUserControl()
		{
			InitializeComponent();
			CustomFieldsCollectionDetailsControl.SetNothingSetupMessageLabelText(Res.GetString("32B2194A-4C21-4F1A-8F76-2C89A885C16E", "To make use of this tab, please setup communication manager custom fields in Workflow Manager."));
		}
	}
}
