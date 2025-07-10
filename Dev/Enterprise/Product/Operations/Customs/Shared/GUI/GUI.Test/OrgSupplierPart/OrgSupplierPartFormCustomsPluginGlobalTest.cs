using System.Windows.Forms;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsPluginGlobalTest : OrgSupplierPartFormCustomsPluginTest
	{
		public override void TestGetNewUserControl()
		{
			using (OrgSupplierPartFormCustomsPlugin plugin = GetNewPlugIn(Part))
			{
				OrgSupplierPartFormCustomsPluginGlobalForTest globalPlugin = (OrgSupplierPartFormCustomsPluginGlobalForTest)plugin;
				using (Control control = globalPlugin.GetNewUserControl())
				{
					AssertEquals(typeof(OrgSupplierPartFormCustomsControlGlobal), control.GetType());
				}
			}
		}

		protected override MasterFiles.Business.OrgSupplierPart GetNewPart() => Factory.New<OrgSupplierPart>();

		protected override OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part) => new OrgSupplierPartFormCustomsPluginGlobalForTest(part);

		sealed class OrgSupplierPartFormCustomsPluginGlobalForTest : OrgSupplierPartFormCustomsPluginGlobal
		{
			public OrgSupplierPartFormCustomsPluginGlobalForTest(MasterFiles.Business.OrgSupplierPart part) : base(part)
			{
			}

			public new Control GetNewUserControl() => base.GetNewUserControl();
		}
	}
}
