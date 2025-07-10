using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public class UpdateUENReferencesForm : DataLoaderForm
	{
		public override string FileDialogFilter
		{
			get
			{
				{ return (NoResString)"Excel files (*.csv)|*.csv|All files (*.*)|*.*"; }
			}
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new UpdateUENReferences();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((UpdateUENReferences)dataLoader).UpdateUENReferenceData(dataToLoad);
		}
	}
}
