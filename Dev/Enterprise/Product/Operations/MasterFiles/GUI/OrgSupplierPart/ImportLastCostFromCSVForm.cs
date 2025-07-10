using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportLastCostFromCSVForm : DataLoaderForm
	{
		public ImportLastCostFromCSVForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new OrgSupplierPartLastCostDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((OrgSupplierPartLastCostDataLoad)dataLoader).ImportPartLastCostData(dataToLoad, LegacyCodesYesRadioButton.Checked);
		}
	}
}
