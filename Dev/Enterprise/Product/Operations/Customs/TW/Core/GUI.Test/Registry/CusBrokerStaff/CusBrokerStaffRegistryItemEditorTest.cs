using System;
using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CusBrokerStaffRegistryItemEditor))]
	public sealed class CusBrokerStaffRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CusBrokerStaffRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CusBrokerStaffRegistryItemUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(CusBrokerStaffRegistryItemUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CusBrokerStaffRegistryItem("", null, null, null, RegistryStorageFlags.System);
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override object[] GetValidRegistryValues()
		{
			var brokerStaff = new CusBrokerStaff(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			brokerStaff.BrokerStaffCode = "CYO";
			brokerStaff.Mailbox = "TBK0461-0";
			Factory.Save();
			return new object[] { brokerStaff };
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateBrokerStaff();
		}
	}
}
