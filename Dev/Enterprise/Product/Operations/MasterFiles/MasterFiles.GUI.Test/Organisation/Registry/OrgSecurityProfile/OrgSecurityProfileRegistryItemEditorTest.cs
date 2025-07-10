using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSecurityProfileRegistryItemEditor))]
	sealed class OrgSecurityProfileRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OrgSecurityProfileRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OrgSecurityProfileControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OrgSecurityProfileControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OrgSecurityProfileRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new OrgSecurityProfileCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new OrgSecurityProfileCollection() };
		}
	}
}
