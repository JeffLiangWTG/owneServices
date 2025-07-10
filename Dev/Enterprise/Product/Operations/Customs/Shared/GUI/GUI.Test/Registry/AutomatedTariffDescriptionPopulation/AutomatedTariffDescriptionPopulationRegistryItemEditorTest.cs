using System;
using System.Windows.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutomatedTariffDescriptionPopulationRegistryItemEditor))]
	sealed class AutomatedTariffDescriptionPopulationRegistryItemEditorTest : Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new AutomatedTariffDescriptionPopulationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AutomatedTariffDescriptionPopulationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutomatedTariffDescriptionPopulationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutomatedTariffDescriptionPopulationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var automatedDescriptionPopulation1 = new AutomatedTariffDescriptionPopulation();
			automatedDescriptionPopulation1.EnableCommercialInvoice = false;

			var automatedDescriptionPopulation2 = new AutomatedTariffDescriptionPopulation();
			automatedDescriptionPopulation2.EnableCustomsDeclaration = false;

			return new object[] { automatedDescriptionPopulation1, automatedDescriptionPopulation2 };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
