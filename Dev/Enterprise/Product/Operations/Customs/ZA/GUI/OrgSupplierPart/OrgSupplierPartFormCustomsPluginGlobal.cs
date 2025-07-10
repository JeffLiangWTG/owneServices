using System.Windows.Forms;

namespace Enterprise.Customs.ZA.GUI
{
	public class OrgSupplierPartFormCustomsPluginGlobal : Customs.GUI.OrgSupplierPartFormCustomsPluginGlobal
	{
		public OrgSupplierPartFormCustomsPluginGlobal(MasterFiles.Business.OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return userControl = new OrgSupplierPartFormCustomsControlGlobal();
		}
	}
}
