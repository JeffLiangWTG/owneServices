using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class EdocsSwappableControl : ZUserControl
	{
		public EdocsSwappableControl() => InitializeComponent();

		public void UpdateEDocs<T>(T item) where T : IBusiness, IDocManagerSupport
		{
			if (item != null)
			{
				eDocsPlugIn?.Dispose();
				eDocsPlugIn = new eDocsPlugIn(item);

				if (!eDocsInitialized)
				{
					InitializeEDocsPlugin();
					eDocsInitialized = true;
				}

				eDocsUserControl.PlugIn = eDocsPlugIn;

				//In ZForm constructor, the BindingContext gets replaced with ZBindingContext, which replaces its listManagers with BindingContextHashTable.
				//The following was missing for the case that EdocsSwappableControl directly attached to any parent which is not ZForm (e.g module).
				eDocsUserControl.BindingContext = new ZBindingContext();
				eDocsUserControl.SetDataBinding(eDocsPlugIn.BusinessEntity, "");

				eDocsPlugIn.RegisterPlugInAsEditable();
				BindingSource.DataSourceType = typeof(T);
			}

			ShowEdocUserControl(item != null);
		}

		public void ShowEdocUserControl(bool show)
		{
			NoContactSelectedLabel.Visible = !show;

			if (eDocsUserControl != null)
			{
				eDocsUserControl.Visible = show;
			}
		}

		bool eDocsInitialized;
		public bool HasChanges => eDocsPlugIn?.BusinessEntity?.HasChanges ?? false;
	}
}
