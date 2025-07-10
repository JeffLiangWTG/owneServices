using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(NCTSPhase5CredentialsRegistryItemUserControl))]
	sealed class NCTSPhase5CredentialsRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new NCTSPhase5Credentials(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var userControl = (NCTSPhase5CredentialsRegistryItemUserControl)control;
			return 				
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "BasicAuthUsernameTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "BasicAuthPasswordTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "FirmIDTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "RequestUserIDTextBox").ReadOnly &&
				control.FindSingleOrDefault<ZTextBox>(x => x.Name == "RequestPasswordTextBox").ReadOnly;
		}

		public void TestNCTSPhase5Credentials_CharacterCasing()
		{
			using (var userControl = new NCTSPhase5CredentialsRegistryItemUserControl())
			{
				CombineAssertions("Character casing test for Normal", () =>
				{
					AssertEquals("Casing should be normal for BasicAuthUsernameTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.BasicAuthUsernameTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for BasicAuthPasswordTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.BasicAuthPasswordTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for FirmIDTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.FirmIDTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for RequestUserIDTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.RequestUserIDTextBox.CharacterCasing);
					AssertEquals("Casing should be normal for RequestPasswordTextBox", System.Windows.Forms.CharacterCasing.Normal, userControl.RequestPasswordTextBox.CharacterCasing);
				});
			}
		}
	}
}
