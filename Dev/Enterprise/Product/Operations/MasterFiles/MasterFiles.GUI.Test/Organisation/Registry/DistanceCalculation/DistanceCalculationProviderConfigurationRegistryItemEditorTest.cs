using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DistanceCalculationProviderConfigurationRegistryItemEditor))]
	sealed class DistanceCalculationProviderConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DistanceCalculationProviderConfigurationRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DistanceCalculationProviderConfigurationRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DistanceCalculationProviderConfigurationRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DistanceCalculationProviderConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, new DistanceCalculationProviderConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DistanceCalculationProviderConfiguration.GetDefault() };
		}
	}
}
