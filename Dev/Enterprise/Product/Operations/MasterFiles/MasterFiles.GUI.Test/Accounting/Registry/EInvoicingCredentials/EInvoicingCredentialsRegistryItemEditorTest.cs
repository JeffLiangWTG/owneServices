using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EInvoicingCredentialsRegistryItemEditor))]
	sealed class EInvoicingCredentialsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor() => new EInvoicingCredentialsRegistryItemEditor(new EInvoicingCredentialsRegistryDataType(EInvoicingCredentialsRegistryItem.Behavior.SetPasswordChar), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((EInvoicingCredentialsControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(EInvoicingCredentialsControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new EInvoicingCredentialsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);

		protected override object[] GetValidRegistryValues()
			=> new object[]
			{
				new EInvoicingCredentials(),
				new EInvoicingCredentials() { APIKey = "api-key", ClientId = "id", ClientSecret = "secret" },
			};

		#endregion
	}
}
