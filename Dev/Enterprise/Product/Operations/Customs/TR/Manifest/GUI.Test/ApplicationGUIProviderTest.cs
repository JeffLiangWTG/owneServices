using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.TR.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.TR.Manifest.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);
		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl), typeof(TRBillAdditionalUserControl), typeof(VisitedPortsForBillUserControl) };
		protected override IEnumerable<Type> ExpectedGetHeaderAdditionalTabPageUserControlsTypes => new List<Type>()
		{ typeof(VisitedPortsForManifestHeaderUserControl), typeof(ManifestToOpenForManifestHeaderUserControl) };
		protected override Type ExpectedBillLayoutType => typeof(TRBillLayouts);
		protected override Type ExpectedBillPartiesLayoutType => typeof(TRBillPartiesLayouts);
		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[] { common.VesselCodeFindBox, common.LloydsNumberTextBox, common.VoyageFlightTextBox, common.RadioCallSignTextBox, common.ConveyanceCountryCodeFindBox });
			groups.Add("Road Transport", new[] { common.VehicleRegistrationTextBox, common.Trailer1RegNoTextBox, common.Trailer2RegNoTextBox, common.Trailer1RegCountryCodeFindBox, common.Trailer2RegCountryCodeFindBox });
			return groups;
		}

		protected override void AssertGetContainersGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(1, columnInfos.Length);
		}

		protected override void AssertGetContainersGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(13, columnsOrder.Length);
		}

		public void TestGetPackedItemColumns()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packItems = applicationGUIProvider.GetPackedItemColumns();
			AssertEquals(12, packItems.Length);
		}

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(10, columnsOrder.Length);
		}

		public void TestGetActionsExtraMenuItemsCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var actionsExtraMenuItems = applicationGUIProvider.GetActionsExtraMenuItems();
			var menuItem = actionsExtraMenuItems.FindByText("Manual Registration No Entry");
			AssertNotNull("Manual Registration No Entry", menuItem);
		}

		public void TestManualRegistrationNoEntry()
		{
			Env.Security.TRModifyRegistrationNumbers.IsAllowed = true;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Factory.Save();
			using (var form = new ManifestForm(header))
			{
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new ManifestForm(header))
			{
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				header.RegistrationDate = ZDateTime.Now;
				header.RegistrationNumber = "TestRegno001";
				header.Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object formOrDialog)
				{
					var be = (ManualRegistrationNoEntry)((ZForm)formOrDialog).BusinessEntity;
					be.RegistrationNumber = "TestRegno222";
					be.RegistrationDate = ZDateTime.BrettsBirthday;
				});
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals(ZDateTime.BrettsBirthday, header.RegistrationDate);
				AssertEquals("TestRegno222", header.RegistrationNumber);
				var logEntry = header.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_Reference.StartsWith("Manual Change Registration No"));
				AssertNotNull(logEntry);
			}
		}

		public void TestModifyRegistrationNumbersSecurityPoint()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Factory.Save();
			using (var form = new ManifestForm(header))
			{
				Env.Security.TRModifyRegistrationNumbers.IsAllowed = true;
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.TRModifyRegistrationNumbers.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs Global -> Global Manifest -> Manual Registration No Entry", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIsMessageGridUserFullNameVisible()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			AssertEquals(true, applicationGUIProvider.IsMessageGridUserFullNameVisible());
		}
	}
}
