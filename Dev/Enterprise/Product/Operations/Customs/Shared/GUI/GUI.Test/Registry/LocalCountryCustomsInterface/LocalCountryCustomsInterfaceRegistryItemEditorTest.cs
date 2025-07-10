using System;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(LocalCountryCustomsInterfaceRegistryItemEditor))]
	[MasterFiles.Business.Testing.CountrySpecificTest("FR")]
	sealed class LocalCountryCustomsInterfaceRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem) => new RegistryFormForTest(registryItem);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new LocalCountryCustomsInterfaceRegistryItemForTest("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company);

		protected override RegistryItemEditor GetEditor() => new LocalCountryCustomsInterfaceRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(LocalCountryCustomsInterfaceUserControl);

		protected override object[] GetValidRegistryValues()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			return new object[] { customsInterface };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((LocalCountryCustomsInterfaceUserControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeft;

		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem) : base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		class LocalCountryCustomsInterfaceRegistryItemForTest : StronglyTypedRegistryItem<LocalCountryCustomsInterface>
		{
			public LocalCountryCustomsInterfaceRegistryItemForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage) : base(new RegistryItemImpl(name, category, caption, hint, new LocalCountryCustomsInterfaceRegistryItemDataType(), storage, RegistryOptions.Default))
			{
			}
		}
	}
}
