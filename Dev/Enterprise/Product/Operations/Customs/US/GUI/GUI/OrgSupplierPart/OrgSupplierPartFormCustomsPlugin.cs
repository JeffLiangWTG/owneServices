using System.Windows.Forms;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	public class OrgSupplierPartFormCustomsPlugin : Customs.GUI.OrgSupplierPartFormCustomsPlugin
	{
		public OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part)
			: base(part)
		{
		}

		protected override Control GetNewUserControl() => new OrgSupplierPartFormCustomsControl();
	}
}
