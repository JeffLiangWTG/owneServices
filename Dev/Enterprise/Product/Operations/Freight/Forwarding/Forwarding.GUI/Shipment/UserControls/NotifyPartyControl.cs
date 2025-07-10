using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class NotifyPartyControl : ZUserControl
	{
		public NotifyPartyControl()
		{
			InitializeComponent();
			InitializeAdditionalCaptions();
		}

		void InitializeAdditionalCaptions()
		{
			NotifyPartyDocumentaryDocAddressControl.Text = Res.GetString("Forwarding|ModeAndPartyControl|NotifyParty", "Notify Party");
			MissingResourceStringChecker.ExcludeFromTest(NotifyPartyDocumentaryDocAddressControl);
		}
	}
}
