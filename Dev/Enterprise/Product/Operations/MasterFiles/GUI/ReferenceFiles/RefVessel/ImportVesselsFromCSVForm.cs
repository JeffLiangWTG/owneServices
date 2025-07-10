using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public class ImportVesselsFromCSVForm : DataLoaderForm
	{
		public ImportVesselsFromCSVForm()
		{
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ImportVesselsFromCSVForm|fd056f7b-fd8d-4a94-9fec-07a8c776fd49", "Import Vessel Data");
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = Res.GetString("1f0f93f9-9de6-4694-b742-82000035e82d", "Please Note: Only vessels with names and Lloyds numbers will be loaded.");
			DialogResult result = Globals.Message.Show(loadingData, Res.GetString("3f39b0ba-23ee-4d8b-8626-0a8cfa1886f3", "Confirm Vessel Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		protected override DataLoad GetNewDataLoader()
		{
			return GetNewVesselDataLoader();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((RefVesselDataLoad)dataLoader).ImportVesselData(dataToLoad);
		}

		protected virtual RefVesselDataLoad GetNewVesselDataLoader()
		{
			return new RefVesselDataLoad();
		}
	}
}
