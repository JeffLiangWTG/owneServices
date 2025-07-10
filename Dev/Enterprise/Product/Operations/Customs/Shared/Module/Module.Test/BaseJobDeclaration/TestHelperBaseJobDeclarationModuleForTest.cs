using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module.Testing
{
	internal class TestHelperBaseJobDeclarationModule : JobDeclarationModule
	{
		public TestHelperBaseJobDeclarationModule() : base()
		{
			CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public bool HasExWarehouseMenuExposed;

		public bool HasExWarehouseMenu_Exposed => HasExWarehouseMenu;

		public void CreateDeclarationOnExistingShipment_Click_Exposed(object sender, EventArgs e)
		{
			CreateDeclarationOnExistingShipment_Click(sender, e);
		}

		protected override bool HasExWarehouseMenu => base.HasExWarehouseMenu || HasExWarehouseMenuExposed;

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobDeclarationFilterBusinessObject();
		}
	}
}
