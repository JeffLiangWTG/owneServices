using System.Windows.Forms;
using Enterprise.Customs.NO.Business;

namespace Enterprise.Customs.NO.GUI;

public class OrgSupplierPartFormCustomsPlugin(OrgSupplierPart part) : Customs.GUI.OrgSupplierPartFormCustomsPlugin(part)
{
	protected override Control GetNewUserControl() => new OrgSupplierPartFormCustomsControl();
}
