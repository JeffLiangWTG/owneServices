using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressUserControlForTest : AddressUserControl
	{
		public AddressUserControlForTest(AdministrationPanelManager manager) : base(manager)
		{
		}

		protected override void Dispose(bool disposing)
		{
			DesignerActionExtenderProvider.Dispose();
			base.Dispose(disposing);
		}
	}
}
