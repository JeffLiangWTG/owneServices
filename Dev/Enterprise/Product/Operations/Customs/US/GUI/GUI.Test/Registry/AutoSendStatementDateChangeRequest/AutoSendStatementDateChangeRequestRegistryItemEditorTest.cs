using System;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(AutoSendStatementDateChangeRequestRegistryItemEditor))]
	sealed class AutoSendStatementDateChangeRequestRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AutoSendStatementDateChangeRequestRegistryItem("", null, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new AutoSendStatementDateChangeRequestRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(AutoSendStatementDateChangeRequestControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			AutoSendStatementDateChangeRequest autoChangeRequestData = new AutoSendStatementDateChangeRequest();

			autoChangeRequestData.OverrideAllOrByOrganisation = OverrideAllOrByOrganisationList.Codes.ALL;
			return new object[] { autoChangeRequestData };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((AutoSendStatementDateChangeRequestControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
