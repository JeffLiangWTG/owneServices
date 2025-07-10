using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Registry.AWB;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(AWBDisplayOptionRegistryItemEditor))]
	public class AWBDisplayOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AWBDisplayOptionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AWBDisplayOptionRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AWBDisplayOptionRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AWBDisplayOptionRegistryItem("", null, null, null, RegistryStorageFlags.System, AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB));
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { AWBDisplayOptionCollection.GetDefault(AWBDisplayOptionType.HAWB) };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
