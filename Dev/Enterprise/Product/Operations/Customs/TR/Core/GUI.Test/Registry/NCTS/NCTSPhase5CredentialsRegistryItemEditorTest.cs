using System;
using System.Windows.Forms;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(NCTSPhase5CredentialsRegistryItemEditor))]
	sealed class NCTSPhase5CredentialsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new NCTSPhase5CredentialsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((NCTSPhase5CredentialsRegistryItemUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(NCTSPhase5CredentialsRegistryItemUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new NCTSPhase5CredentialsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues()
		{
			var credentialsSettings = new NCTSPhase5Credentials(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
			credentialsSettings.BasicAuthUsername = "NCTSTraderUser";
			credentialsSettings.BasicAuthPassword = "3vAT.2G98Ar!";
			credentialsSettings.FirmID = "WiseTech";
			credentialsSettings.RequestUserID = "11111111108";
			credentialsSettings.RequestPassword = "12345678";
			Factory.Save();

			return new object[] { credentialsSettings };
		}
	}
}
