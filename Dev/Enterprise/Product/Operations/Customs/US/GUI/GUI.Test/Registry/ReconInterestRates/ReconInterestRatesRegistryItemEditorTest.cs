using System;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(ReconInterestRatesRegistryItemEditor))]
	sealed class ReconInterestRatesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ReconInterestRatesRegistryItem("", null, null, null, RegistryStorageFlags.System, new ReconInterestRateCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ReconInterestRatesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ReconInterestRatesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ReconInterestRateCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ReconInterestRatesControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
