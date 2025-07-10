using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(D365CredentialsRegistryItemEditor))]
	sealed class D365CredentialsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor() => new D365CredentialsRegistryItemEditor(new D365CredentialsRegistryDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((D365CredentialsControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(D365CredentialsControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new D365CredentialsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);

		protected override object[] GetValidRegistryValues() => new object[] { new D365Credentials() { ClientID = "id", ClientSecret = "secret", TenantID = "tenant" } };

		#endregion
	}
}
