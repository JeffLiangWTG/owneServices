using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Testing
{
	[TestedType(typeof(ServiceTaskCreatorOptionRegistryItemEditor))]
	public class ServiceTaskCreatorOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ServiceTaskCreatorOptionRegistryItemEditor(new ServiceTaskCreatorOptionRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ServiceTaskCreatorOptionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ServiceTaskCreatorOptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ServiceTaskCreatorOptionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ServiceTaskCreatorOptionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ServiceTaskCreatorOptionCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
