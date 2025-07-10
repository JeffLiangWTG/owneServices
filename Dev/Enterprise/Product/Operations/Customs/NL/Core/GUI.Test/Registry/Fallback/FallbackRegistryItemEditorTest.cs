using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(FallbackRegistryItemEditor))]
sealed class FallbackRegistryItemEditorTest : Registry.GUI.Testing.RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new FallbackRegistryItemEditor(RegistryItem.DataType, null, null);

	protected override bool GetEditorPaneEnabledState(Control editorPane) => !((FallbackControl)editorPane).ReadOnly;

	protected override Type GetExpectedEditorPaneType() => typeof(FallbackControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new FallbackConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company);

	protected override object[] GetValidRegistryValues()
	{
		var fallbackConfig1 = new FallbackConfiguration();
		fallbackConfig1.Start = new ZDateTime(2024, 12, 18);
		fallbackConfig1.InvocationReason = "Invocation Reason 1";

		var fallbackConfig2 = new FallbackConfiguration();
		fallbackConfig2.Start = new ZDateTime(2024, 12, 19);
		fallbackConfig1.InvocationReason = "Invocation Reason 2";

		return
		[
			fallbackConfig1,
			fallbackConfig2
		];
	}

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
}
