using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class ImportEDICodeMappingFromCSVForm : DataLoaderForm
	{
		public ImportEDICodeMappingFromCSVForm(OrgHeader orgProxy)
		{
			this.orgProxy = orgProxy;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			base.OutputListBox.HorizontalScrollbar = true;
			InitializeComponent();
		}

		public override bool ConfirmLoadData()
		{
			string loadingData = Res.GetString("e39aa559-b860-42a8-a3ea-52944626a598", "Please Note: Only valid data mapping will be loaded.");
			DialogResult result = Globals.Message.Show(loadingData, Res.GetString("9a366409-4b02-4956-a199-2b7863d0c7d5", "Confirm Data Mapping Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new EDICodeMappingDataLoader(orgProxy);
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			try
			{
				((EDICodeMappingDataLoader)dataLoader).ImportData(dataToLoad, Res.GetString("{9222A862-6D41-48c3-B804-33789F9C1232}", "EDI Code Mapping"));
			}
			catch (System.NotImplementedException)
			{
				Globals.Message.Show(Res.GetString("7e7f34c9-f4ab-455a-967c-e4d2c5df6a7d", "Product data import from CSV file has not been implemented for your country/region."));
			}
		}
		readonly OrgHeader orgProxy;
	}
}
