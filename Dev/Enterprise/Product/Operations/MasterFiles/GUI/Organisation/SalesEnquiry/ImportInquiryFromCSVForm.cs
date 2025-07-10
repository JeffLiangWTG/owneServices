using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportInquiryFromCSVForm : DataLoaderForm
	{
		public ImportInquiryFromCSVForm()
		{
			InitializeComponent();
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new InquiryDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((InquiryDataLoad)dataLoader).ImportInquiryData(dataToLoad);
		}
	}
}
