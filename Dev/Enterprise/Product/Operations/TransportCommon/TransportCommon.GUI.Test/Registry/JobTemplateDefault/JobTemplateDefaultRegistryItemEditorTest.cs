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
	[TestedType(typeof(JobTemplateDefaultRegistryItemEditor))]
	public class JobTemplateDefaultRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new JobTemplateDefaultRegistryItemEditor(new JobTemplateDefaultRegistryDataType(), new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobTemplateDefaultControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobTemplateDefaultControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobTemplateDefaultRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobTemplateDefaultCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new JobTemplateDefaultCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
