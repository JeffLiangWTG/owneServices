using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Universal.GUI
{
	public class ImportRateFromCSVForm : DataLoaderForm
	{
		public ImportRateFromCSVForm()
		{
			CaptionResourceString = Res.GetData("A55652E0-B056-4877-AB19-4805507825B4", "Import Rate Data");
		}

		public override bool ConfirmLoadData()
		{
			var loadingData = Res.GetString("FD9423A5-92B8-462A-B5FC-FAA934F0B672", "Please Note: Only rates with Version, Tariff Type, Tariff Code, Rate Code, Preference and Trade Group will be loaded. If it is new rate to import, one of Rate Formula or Ad-valorem Rate or Specific Rate and Specific Rate UOM is necessary too.");
			var result = Globals.Message.Show(loadingData, Res.GetString("C60975C6-6D58-4BB1-9F45-6B3CFD8AF622", "Confirm Rate Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return result == DialogResult.OK;
		}

		protected override DataLoad GetNewDataLoader() => new RateViewDataLoad();

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad) => ((RateViewDataLoad)dataLoader).ImportRateData(dataToLoad);
	}
}
