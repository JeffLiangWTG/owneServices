using System;
using System.Windows.Forms;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(ASNRefreshOptionsConfigRegistryItemEditor))]
	sealed class ASNRefreshOptionsConfigRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ASNRefreshOptionsConfigRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ASNRefreshOptionsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ASNRefreshOptionsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var collection = new ASNRefreshOptionsConfigCollection(null, Factory);
			return new ASNRefreshOptionsConfigRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, collection);
		}

		protected override object[] GetValidRegistryValues()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var collection = new ASNRefreshOptionsConfigCollection(fallbackLevel, Factory);

			var config = collection.AddNew();
			config.FieldType = Core.Constants.Customs.ASNRefreshDefaultsOptions.Codes.Classification;

			return new object[] { collection };
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

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

		#endregion
	}
}
