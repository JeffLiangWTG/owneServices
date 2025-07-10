using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(SupervisorOverridesRegistryItemEditor))]
	sealed class SupervisorOverridesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SupervisorOverrideRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SupervisorOverridesRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SupervisorOverridesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			SupervisorOverrideData supervisorOverrideData = new SupervisorOverrideData();
			supervisorOverrideData.FillWithValidTestData();
			return new object[] { supervisorOverrideData };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((SupervisorOverridesControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
