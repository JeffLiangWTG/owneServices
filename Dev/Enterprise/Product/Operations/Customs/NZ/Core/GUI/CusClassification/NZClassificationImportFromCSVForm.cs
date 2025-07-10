using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI
{
	public class NZClassificationImportFromCSVForm : DataLoaderForm
	{
		public override bool ConfirmLoadData()
		{
			string loadingData = "Please Note: Only classifications with valid tariff details will be loaded.";
			DialogResult result = Globals.Message.Show(loadingData, "Confirm Classification Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		public override string FormHeading
		{
			get
			{
				return Enterprise.Customs.NZ.GUI.Res.GetString("635de059-fd4e-4fa4-98a2-0b5a4a5f2724", "Import Classification Data");
			}
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((ClassificationDataLoad)dataLoader).ImportClassificationData(dataToLoad);
		}
	}
}
