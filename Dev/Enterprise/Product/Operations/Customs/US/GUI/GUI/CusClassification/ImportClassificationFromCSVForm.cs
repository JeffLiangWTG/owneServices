using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportClassificationsFromCSVForm : DataLoaderForm
	{
		public ImportClassificationsFromCSVForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = "Please Note: Only classifications with valid tariff details will be loaded.";
			DialogResult result = Globals.Message.Show(loadingData, "Confirm Lookup Code Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		public override string FormHeading
		{
			get { return "Import Lookup Code Data"; }
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
