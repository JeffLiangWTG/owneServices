using System.Windows.Forms;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(OrgSupplierPartFormCustomsPlugin))]
sealed class OrgSupplierPartFormCustomsPluginTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsPluginTest
{
	public override void TestGetNewUserControl()
	{
		using var plugin = (OrgSupplierPartFormCustomsPluginForTesting)GetNewPlugIn(Part);
		using var control = plugin.GetNewUserControl_Exposed();
		AssertEquals(typeof(OrgSupplierPartFormCustomsControl), control.GetType());
	}

	protected override MasterFiles.Business.OrgSupplierPart GetNewPart()
	{
		return (OrgSupplierPart)OrgSupplierPart.New(Factory);
	}

	protected override Customs.GUI.OrgSupplierPartFormCustomsPlugin GetNewPlugIn(MasterFiles.Business.OrgSupplierPart part)
	{
		return new OrgSupplierPartFormCustomsPluginForTesting((OrgSupplierPart)part);
	}

	class OrgSupplierPartFormCustomsPluginForTesting(OrgSupplierPart part) : OrgSupplierPartFormCustomsPlugin(part)
	{
		public Control GetNewUserControl_Exposed()
		{
			return base.GetNewUserControl();
		}
	}
}
