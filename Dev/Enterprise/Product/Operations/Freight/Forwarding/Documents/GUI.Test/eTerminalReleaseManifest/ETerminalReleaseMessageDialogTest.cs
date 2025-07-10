using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	[TestedType(typeof(ETerminalReleaseMessageDialog))]
	sealed class ETerminalReleaseMessageDialogTest : ZFormBasherTest
	{
		public void TestSendMessage_CheckSecurity()
		{
			var voyage = Factory.New<JobVoyage>();

			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.JobConsol))
			using (var form = new ETerminalReleaseMessageDialogForTesting(voyage))
			{
				var eTerminalReleaseManifestMenu = Factory.Load<IStmMenuItem>(ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK);
				var parentCheckpoint = Env.Security.FindOrCreateVisualizerFormsCheckpoint(ModuleIDs.JobConsol, module.SecurityCheckpoint);

				var menuItemCheckpoint = Env.Security.FindOrCreateVisualizerFormCheckpoint(ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK.ToGuid(), eTerminalReleaseManifestMenu.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint);
				menuItemCheckpoint.IsAllowed = true;

				Env.Security.FindOrCreateVisualizerFormSendMessageCheckpoint(ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK.ToGuid(), eTerminalReleaseManifestMenu.SU_MenuNameMultilingual, ModuleIDs.JobConsol, parentCheckpoint).IsAllowed = false;

				form.Show();
				form.SendButton.PerformClick();

				AssertEquals("Show error when the security is not allowed", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Forms -> eTerminal Release Manifest"
						, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void AddNewForLoadedBoundListCheck(IBusinessObjectCollection collection)
		{
			if (collection is ETerminalReleaseMessageConsolCollection messageConsolCollection)
			{
				messageConsolCollection.Add(new ETerminalReleaseMessageConsol(Factory.New<ForwardingConsol>()));
			}
			else
			{
				collection.AddNew();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var voyage = Factory.New<JobVoyage>();

			return new ETerminalReleaseMessageDialog(voyage);
		}
	}

	sealed class ETerminalReleaseMessageDialogForTesting : ETerminalReleaseMessageDialog
	{
		public ETerminalReleaseMessageDialogForTesting(JobVoyage voyage)
			: base(voyage)
		{
		}

		public Button SendButton => sendButton;
	}
}
