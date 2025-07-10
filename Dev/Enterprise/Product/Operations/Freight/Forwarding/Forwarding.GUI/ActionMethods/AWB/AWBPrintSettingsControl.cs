using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class AWBPrintSettingsControl : ZUserControl
	{
		public AWBPrintSettingsControl(bool includeMessageErrors)
		{
			InitializeComponent();
			if (includeMessageErrors)
			{
				lblOnErrorMessageError.Text = Res.GetString("33eee891-652f-4c35-8b65-ace5525b8925", "On Error / Message Error:");
			}
		}
	}
}
