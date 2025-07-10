using System;
using System.Windows.Forms;
using Enterprise.Customs.NL.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(SenderInfoRegistryItemEditor))]
class SenderInfoRegistryItemEditorTest : RegistryItemEditorTestCase
{
	#region Implementation

	protected override RegistryItemEditor GetEditor()
	{
		return new SenderInfoRegistryItemEditor(RegistryItem.DataType, null, null);
	}

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		return !((SenderInfoUserControl)editorPane).ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType()
	{
		return typeof(SenderInfoUserControl);
	}

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
	{
		return new SenderInfoRegistryItem("", null, null, null, RegistryStorageFlags.System);
	}

	protected override object[] GetValidRegistryValues()
	{
		OrgHeader org1 = Factory.New<OrgHeader>();
		org1.OH_FullName = "Test Company 1";
		org1.OH_Code = "TestComp1";
		OrgHeader org2 = Factory.New<OrgHeader>();
		org2.OH_FullName = "Test Company 2";
		org2.OH_Code = "TestComp2";
		Factory.Save();

		var coll = new SenderInfoCollection();
		var mapping1 = coll.AddNew();
		mapping1.OrganizationPK = org1.PK;
		mapping1.SenderID = "123";
		mapping1.DefaultSenderID = true;

		var mapping2 = coll.AddNew();
		mapping2.OrganizationPK = org2.PK;
		mapping2.SenderID = "456";
		mapping2.DefaultSenderID = false;

		Factory.Save();

		return new object[] { coll };
	}

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
	{
		get { return RegistryItemEditor.EditorPaneAnchor.All; }
	}

	#endregion
}
