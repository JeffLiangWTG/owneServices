using CargoWise.IO;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WaitDownloadingForm : ZChildForm, IProcessStatus
	{
		public WaitDownloadingForm()
		{
			InitializeComponent();

			this.label.Text = Res.GetString("DDE051C9-E8A6-4D90-B64E-C837D867F48D", "Please wait while the installer is being downloaded ...");
		}

		public void UpdateStatus(string status, int progressValue)
		{
		}
	}
}
