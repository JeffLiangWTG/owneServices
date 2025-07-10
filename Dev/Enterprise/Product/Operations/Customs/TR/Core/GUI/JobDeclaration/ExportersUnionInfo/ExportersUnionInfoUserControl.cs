using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ExportersUnionInfoUserControl : ZUserControl
	{
		public ExportersUnionInfoUserControl()
		{
			InitializeComponent();
		}

		void ExportUnionPaymentLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			var exportUnionPaymentLinkURL = ExportUnionFtpAdressHelper.GeneratePaymentLink();

			if (!string.IsNullOrEmpty(exportUnionPaymentLinkURL))
			{
				WebUrlLauncher.Launch(exportUnionPaymentLinkURL);
			}
			else
			{
				Globals.Message.Show(Res.GetString("A6FF7694-5B50-4751-A667-030F9F050BB2", "There are missing data to populate this link. Please refer to the registry item to provide them."));
			}
		}
	}
}
