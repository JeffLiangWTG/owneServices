using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MDMAdminPanelAddressViewForTest : MDMAdminPanelAddressView
	{
		public MDMAdminPanelAddressViewForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsValidationEnabledCoreForTest => base.IsValidationEnabledCore(null);
	}
}
