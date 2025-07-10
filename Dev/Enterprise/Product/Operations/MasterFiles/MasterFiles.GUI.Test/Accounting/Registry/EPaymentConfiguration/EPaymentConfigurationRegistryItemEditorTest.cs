using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EPaymentConfigurationRegistryItemEditor))]
	public class EPaymentConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new EPaymentConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((EPaymentConfigurationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(EPaymentConfigurationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new EPaymentConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);

		protected override object[] GetValidRegistryValues()
		{
			var collectionForAUCompany = new EPaymentConfigurationCollection();
			var configForAUCompany = collectionForAUCompany.AddNew();
			configForAUCompany.CountryCode = Core.Constants.CountryCodes.Australia;
			configForAUCompany.CountryDescription = "Australia";
			configForAUCompany.OFXEPaymentEnabled = true;

			var collectionForLKCompany = new EPaymentConfigurationCollection();
			var configForLKCompany = collectionForLKCompany.AddNew();
			configForLKCompany.CountryCode = Core.Constants.CountryCodes.SriLanka;
			configForLKCompany.CountryDescription = "Sri Lanka";
			configForLKCompany.OFXEPaymentEnabled = false;

			return new[] { collectionForAUCompany, collectionForLKCompany };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}
	}
}
