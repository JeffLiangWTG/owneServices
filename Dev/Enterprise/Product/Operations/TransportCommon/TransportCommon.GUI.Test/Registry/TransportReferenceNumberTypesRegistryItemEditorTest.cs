using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.TransportCommon.GUI.Registry;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TransportReferenceNumberTypesRegistryItemEditor))]
	public class TransportReferenceNumberTypesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TransportReferenceNumberTypesRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, new TransportReferenceNumberTypeCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TransportReferenceNumberTypesRegistryItemEditor(new TransportReferenceNumberTypesDataType(), null, null);
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(TransportReferenceNumberTypesRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new TransportReferenceNumberTypeCollection();
			collection.Add("XXX", (NoResString)"DESC1");
			collection.Add("YYY", (NoResString)"DESC2");
			collection.Add("ZZZ", (NoResString)"DESC3");
			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TransportReferenceNumberTypesRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
