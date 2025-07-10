using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.GUI.MasterFiles
{
	public class OrgSupplierPartFormCustomsPluginGlobal : Customs.GUI.OrgSupplierPartFormCustomsPluginGlobal
	{
		public OrgSupplierPartFormCustomsPluginGlobal(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return userControl = new OrgSupplierPartFormCustomsControlGlobal();
		}
	}
}

#region TestCase
#if DEDUG

namespace Enterprise.Customs.NZ.GUI.MasterFiles.Testing
{
	public class TestOrgSupplierPartFormCustomsPluginForSingleLookup : Enterprise.Customs.GUI.OrgSupplierPartFormCustomsPluginForSingleLookup.OrgSupplierPartFormCustomsPluginForSingleLookupTest
	{
	}
}
#endif
#endregion
