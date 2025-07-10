using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Universal.GUI
{
	public class ImportTariffFromCSVForm : DataLoaderForm
	{
		public ImportTariffFromCSVForm()
		{
			CaptionResourceString = Res.GetData("52A81E2A-3401-4587-93A3-10133BFF9E09", "Import Tariff Data");
		}

		public override bool ConfirmLoadData()
		{
			var loadingData = Res.GetString("AD607574-FDAA-4D4B-B27D-718F40E13E6E", "Please Note: Only tariffs with Country Code and Version and Tariff Type and Tariff Code will be loaded. If it is new Tariff to import, Description and UOM1 is necessary too.");
			var result = Globals.Message.Show(loadingData, Res.GetString("BB79E351-8704-49A5-BEFF-F98C56199F89", "Confirm Tariff Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return result == DialogResult.OK;
		}

		protected override DataLoad GetNewDataLoader() => new TariffViewDataLoad();

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad) => ((TariffViewDataLoad)dataLoader).ImportTariffData(dataToLoad);
	}
}
