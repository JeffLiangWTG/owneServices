using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(OutturnAndGateInOutForm))]
	sealed class OutturnAndGateInOutFormTest : ZFormBasherTest
	{
		public void TestCopyFromManifestJobMenuItem_GlobalManifestFilterStripLoaded_NoExceptionIfAsycudaManifestReportingIsNotAllowed()
		{
			var isAllowed = Env.Security.AsycudaManifestReporting.IsAllowed;
			using (new DisposableAction(() => Env.Security.AsycudaManifestReporting.IsAllowed = false, () => Env.Security.AsycudaManifestReporting.IsAllowed = isAllowed))
			{
				var header = CreateAsycudaManifestHeader();
				Factory.Save();
				using (OutturnAndGateInOutForm form = new OutturnAndGateInOutForm(header))
				{
					form.Show();
					var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Copy From Manifest Job");
					AssertNoExceptionThrown(() =>
					{
						menuItem.PerformClick();
						var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
						((ZFilterStripCommonControl)((ZDisplayGrid)((ZFilterGridModule)popup.Module_ForTest).DisplayGrid).GetRootContainer()).FilterStripLoaded();
					});
				}
			}
		}

		public void TestCopyFromManifestJobMenuItem()
		{
			var header = CreateAsycudaManifestHeader();
			Factory.Save();
			using (OutturnAndGateInOutForm form = new OutturnAndGateInOutForm(header))
			{
				form.Show();
				var menuItem = form.Menu.MenuItems.FindByText("Actions").MenuItems.FindByText("Copy From Manifest Job");
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				var moduleDecisionProvider = popup.Module_ForTest.ModuleDecisionProvider as AsycudaManifestViewChooser.AsycudaManifestViewChooserModuleDecisionProvider;
				AssertNotNull(moduleDecisionProvider);
				Assert(!moduleDecisionProvider.AllowMultiSelect);
			}
		}

		public void TestOutturnMenuItemEnabled()
		{
			var header = CreateAsycudaManifestHeader();
			using var form = new OutturnAndGateInOutForm(header);
			form.Show();
			var menuItem = form.Menu.MenuItems.FindByText("Outturn");
			AssertNullOrEmpty("PRE-CONDITION", header.GateInOutMessageType);
			AssertEquals("Enabled", expected: true, menuItem.Enabled);

			header.GateInOutMessageType = "DGI";
			menuItem = form.Menu.MenuItems.FindByText("Outturn");
			AssertEquals("Disabled", expected: false, menuItem.Enabled);

			header.GateInOutMessageType = ZString.Empty;
			menuItem = form.Menu.MenuItems.FindByText("Outturn");
			AssertEquals("Enabled", expected: true, menuItem.Enabled);
		}

		public void TestGateInOutMenuItemEnabled()
		{
			var header = CreateAsycudaManifestHeader();
			using var form = new OutturnAndGateInOutForm(header);
			form.Show();
			var menuItem = form.Menu.MenuItems.FindByText("Gate In/Out");
			AssertNullOrEmpty("PRE-CONDITION", header.AMA_ManifestType);
			AssertEquals("Enabled", expected: true, menuItem.Enabled);

			header.AMA_ManifestType = "AOR";
			menuItem = form.Menu.MenuItems.FindByText("Gate In/Out");
			AssertEquals("Disabled", expected: false, menuItem.Enabled);

			header.AMA_ManifestType = ZString.Empty;
			menuItem = form.Menu.MenuItems.FindByText("Gate In/Out");
			AssertEquals("Enabled", expected: true, menuItem.Enabled);
		}

		public void TestAuditPlugInPresent()
		{
			var header = CreateAsycudaManifestHeader();
			using var form = new OutturnAndGateInOutForm(header);
			Assert("Audit PlugIn should be available", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		protected override Form GetFormToBashCore()
		{
			var header = CreateAsycudaManifestHeader();
			Factory.Save();
			var result = new OutturnAndGateInOutForm(header);
			result.ControllerID = ZAControllerIDs.OutturnAndGateInOut;
			return result;
		}

		AsycudaManifestHeader CreateAsycudaManifestHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "VWG";
			var masterBill = header.MasterBill;
			var bills = header.Bills;
			var bill = bills.AddNew();
			return header;
		}
	}
}
