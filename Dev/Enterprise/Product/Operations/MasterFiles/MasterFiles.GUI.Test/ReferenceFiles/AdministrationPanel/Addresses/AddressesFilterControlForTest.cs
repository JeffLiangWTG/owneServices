using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressesFilterControlForTest : AddressesFilterControl
	{
		public AddressesFilterControlForTest(MDMAdminPanelAddressCollection collection, AddressesFilterBusinessObject bizo) : base(collection, bizo)
		{
		}

		public ZLabel ToolStripRecordsFoundLabelForTest => ToolStripRecordsFoundLabel;
	}
}
