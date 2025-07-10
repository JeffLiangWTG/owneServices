using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AdministrationPanelFormForTest : AdministrationPanelForm
	{
		public AdministrationPanelFormForTest(BusinessObjectFactory factory) : this(new AdministrationPanelManager(factory))
		{
		}

		public AdministrationPanelFormForTest(AdministrationPanelManager manager) : base(manager)
		{
		}

		public void SelectTab(string tabName)
		{
			var mainTabControl = Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
			mainTabControl?.SelectTab(tabName);
		}

		public IButton ApplyButton => fApplyButton;
		public IButton PostButton => fPostButton;
		public IButton CloseButton => fCancelButton;
		public ZTabControl DeDupTabControl => DuplicatesSubTabControl;
		public AddressUserControl UserControl => AddressUserControl;

		protected override void Dispose(bool disposing)
		{
			DesignerActionExtenderProvider.Dispose();
			base.Dispose(disposing);
		}
	}
}
