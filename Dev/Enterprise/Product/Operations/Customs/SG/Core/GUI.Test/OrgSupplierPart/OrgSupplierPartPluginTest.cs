using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	sealed class OrgSupplierPartPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (OrgSupplierPartPlugin plugin = (OrgSupplierPartPlugin)GetNewPlugIn(Part))
			{
				AssertType<OrgSupplierPartControl>(plugin.UserControl);
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart() => Factory.New<OrgSupplierPart>();

		protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part) => new OrgSupplierPartPlugin((OrgSupplierPart)part);
	}
}
