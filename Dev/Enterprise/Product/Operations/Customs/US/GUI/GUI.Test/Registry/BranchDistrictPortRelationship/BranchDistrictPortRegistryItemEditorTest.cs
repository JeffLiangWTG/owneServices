using System;
using System.Windows.Forms;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(BranchDistrictPortRegistryItemEditor))]
	sealed class BranchDistrictPortRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new BranchDistrictPortRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((BranchDistrictPortUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(BranchDistrictPortUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new BranchDistrictPortRegistryItem("", null, null, null, RegistryStorageFlags.All);

		protected override object[] GetValidRegistryValues() => new object[] { GetValidValue() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		BranchDistrictPortCollection GetValidValue()
		{
			var branchPK = Environment.Env.CurrentBranch.PK;
			var collection = new BranchDistrictPortCollection(new FallbackLevel(Guid.Empty, branchPK, Guid.Empty), Factory);
			var element = collection.AddNew();
			element.PortCode = "3901";
			element.BranchPK = branchPK;
			Factory.Save();
			return collection;
		}
	}
}
