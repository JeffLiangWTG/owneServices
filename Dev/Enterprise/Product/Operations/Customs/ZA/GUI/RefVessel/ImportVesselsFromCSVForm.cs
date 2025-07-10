using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.GUI
{
	public class ImportVesselsFromCSVForm : MasterFiles.GUI.ImportVesselsFromCSVForm
	{
		public ImportVesselsFromCSVForm()
		{
			this.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("ImportVesselsFromCSVForm|2963E1E9-1A12-4C7E-B38B-8E40034515AD", "Import Vessel Data (ZA special template)");
		}

		protected override MasterFiles.Business.RefVesselDataLoad GetNewVesselDataLoader()
		{
			return new Business.RefVesselDataLoad();
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = "Please Note: Only vessels with valid TransportID, Name and CarrierCode will be loaded.";
			DialogResult result = Globals.Message.Show(loadingData, "Confirm Vessel Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}
	}
}
