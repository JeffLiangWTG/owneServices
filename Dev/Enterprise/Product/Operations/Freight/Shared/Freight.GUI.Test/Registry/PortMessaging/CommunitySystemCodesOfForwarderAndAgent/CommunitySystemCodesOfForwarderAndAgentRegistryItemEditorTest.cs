using System;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor))]
	sealed class CommunitySystemCodesOfForwarderAndAgentRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CommunitySystemCodesOfForwarderAndAgentRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CommunitySystemCodesOfForwarderAndAgentControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CommunitySystemCodesOfForwarderAndAgentControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CommunitySystemCodesOfForwarderAndAgentRegistryItem("", null, null, null, RegistryStorageFlags.System, new CommunitySystemCodesOfForwarderAndAgentCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CommunitySystemCodesOfForwarderAndAgentCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
