using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCDataVersionModule))]
	sealed class USCDataVersionModuleTest : USCFilterGridModuleTest
	{
		public void TestResetVersion()
		{
			var dataVersion = USCDataVersion.GetLastHTSAttempt(Factory);
			dataVersion.UZ_Version = 1001;
			dataVersion.UZ_Note = ExtractReferenceFilesResult.Pending;
			Env.Security.HTSReferenceFilesDataVersionReset.IsAllowed = false;
			Factory.Save();
			using (var module = new USCDataVersionModuleForTest())
			using (var form = new ZForm(module.FilterBusinessObject))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ResetVersion_Click();
				AssertEquals(1001, dataVersion.UZ_Version);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("You do not have the appropriate security rights to run this function."));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Env.Security.HTSReferenceFilesDataVersionReset.IsAllowed = true;
				module.ResetVersion_Click();
				AssertEquals(9805, dataVersion.UZ_Version);
				AssertEquals(ExtractReferenceFilesResult.Success, dataVersion.UZ_Note);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCDataVersion;

		sealed class USCDataVersionModuleForTest : USCDataVersionModule
		{
			public void ResetVersion_Click() => ResetHTSVersion_Click(null, null);
		}
	}
}
