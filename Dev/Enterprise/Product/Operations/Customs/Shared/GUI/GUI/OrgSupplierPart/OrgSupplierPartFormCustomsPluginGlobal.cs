using System.Windows.Forms;

namespace Enterprise.Customs.GUI
{
	public class OrgSupplierPartFormCustomsPluginGlobal : OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPluginGlobal(MasterFiles.Business.OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return userControl = new OrgSupplierPartFormCustomsControlGlobal();
		}

		protected OrgSupplierPartFormCustomsControlGlobal userControl;
	}
}
