using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public class OrgSupplierPartPlugin : Customs.GUI.OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl()
		{
			return new OrgSupplierPartControl();
		}
	}
}
