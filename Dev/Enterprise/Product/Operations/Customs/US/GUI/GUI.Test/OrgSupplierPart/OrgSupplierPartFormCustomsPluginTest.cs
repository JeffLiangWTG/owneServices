using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	sealed class OrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (var plugin = (OrgSupplierPartFormCustomsPlugin)GetNewPlugIn(Part))
			using (var control = plugin.UserControl)
			{
				AssertEquals(typeof(OrgSupplierPartFormCustomsControl), control.GetType());
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart() => Factory.New<OrgSupplierPart>();

		protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part) => new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)part);
	}
}
