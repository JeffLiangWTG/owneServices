using System.Windows.Forms;
using CargoWise.BrandManager;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportOrganisationsFromCSVForm : DataLoaderForm
	{
		public ImportOrganisationsFromCSVForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = Res.GetString("6e748965-9143-45b3-a8bc-b66c84e3f84c", "Please Note: Only Organizations not already found in {0} will be loaded.", BrandingFactory.Instance.ProductName);
			DialogResult result = Globals.Message.Show(loadingData, Res.GetString("db2dd5b8-aa49-474e-94f8-06c9402c817c", "Confirm Organization Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			return (result == DialogResult.OK);
		}

		protected override DataLoad GetNewDataLoader()
		{
			return GetOrgDataLoader();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((OrgDataLoad)dataLoader).ImportOrganisationData(dataToLoad, NewCodesRadioButton.Checked);
		}

		protected virtual OrgDataLoad GetOrgDataLoader()
		{
			return new OrgDataLoad();
		}
	}
}
