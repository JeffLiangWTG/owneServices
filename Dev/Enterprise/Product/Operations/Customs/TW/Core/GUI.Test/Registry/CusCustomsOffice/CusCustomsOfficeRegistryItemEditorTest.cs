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
	[TestedType(typeof(CusCustomsOfficeRegistryItemEditor))]
	public sealed class CusCustomsOfficeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new CusCustomsOfficeRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((CusCustomsOfficeRegistryItemUserControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(CusCustomsOfficeRegistryItemUserControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CusCustomsOfficeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override object[] GetValidRegistryValues()
		{
			var element = new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			element.CustomsOfficeCode = "CE";
			Factory.Save();
			return new object[] { element };
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateCustomsOffice();
		}
	}
}
