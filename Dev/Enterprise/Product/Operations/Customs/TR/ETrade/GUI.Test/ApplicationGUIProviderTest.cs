using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Form GetFormToBashCore()
		{
			var header = CreateNewManifest();
			Factory.Save();

			var result = new ETradeForm(header);
			result.ControllerID = ControllerIDs.Customs.TR.ETrade;
			return result;
		}

		public override Type FormToBashType => typeof(ETradeForm);

		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Dictionary<string, ControlReference[]> GetManifestControlGroups()
		{
			var common = CommonManifestControlBag.Instance;
			var groups = new Dictionary<string, ControlReference[]>();
			groups.Add("Sea Vessel", new[]
			{
				common.VesselCodeFindBox
			});
			return groups;
		}

		public void TestMainTabPageGroupBoxNameCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.FillWithValidTestData();
			Factory.Save();

			using (var form = new ETradeForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();

				ZGroupBox manifestGroupBox = (control.Controls.Find("ManifestGroupBox", true)[0]) as ZGroupBox;
				AssertEquals(NoResourceStringData.GetData("E-Trade"), manifestGroupBox.CaptionResourceString);
			}
		}

		protected override Type ExpectedBillLayoutType => typeof(ETradeBillDetailsLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(ETradeBillPartiesLayouts);

		public void TestGetBillPartiesLayoutControlBag()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var layout = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetBillPartiesLayout().Layout;
			Assert(layout.ControlBags.Any(c => c.GetType() == typeof(ETradeBillPartiesControlBag)));
			AssertEquals(2, layout.ControlBags.Count);
		}

		protected override int MaxColumnsOfManifestLayout => 3;

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] { typeof(AsycudaPackUserControl), typeof(TRSupportingDocumetsUserControl) };

		public void TestGetPacksGridColumnsOrderCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var applicationGUIProvider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var packs = applicationGUIProvider.GetPacksGridColumnsOrder();
			AssertEquals(5, packs.Length);
		}

		protected override void AssertGetPacksGridColumnsOrder(string[] columnsOrder)
		{
			AssertEquals(5, columnsOrder.Length);
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
			Env.Security.TRETradeModifyRegistrationNumbers.IsAllowed = true;
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Factory.Save();

			using (var form = new ETradeForm(header))
			{
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			using (var form = new ETradeForm(header))
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
			using (var form = new ETradeForm(header))
			{
				Env.Security.TRETradeModifyRegistrationNumbers.IsAllowed = true;
				var manualRegNoEntryMenuItem = (form as IFileMenuItemsProvider).ActionsMenuItem.MenuItems.FindByText("Manual Registration No Entry");
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals("Do you want to enter Registration No manually?", UnitTestUserNotification.Instance.LastMessage.Text);
				Env.Security.TRETradeModifyRegistrationNumbers.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				manualRegNoEntryMenuItem.PerformClick();
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs -> E-Trade -> Manual Registration No Entry", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void AssertGetTaxesGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertEquals(1, columnInfos.Length);
			AssertEquals(AsycudaTax.Schema.AET_TypeDescription, columnInfos[0].ColumnName);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] {
				"Header+AMA_Nature",
				"NatureOfBusiness",
				"ExemptionCode1",
				"ExemptionCode2",
				"DepartureCountry",
				"TradeCountry",
				"ExportCountry",
				"ArrivalCountry",
				"PaymentMethod",
				"ABL_Incoterm",
				"AccountantName",
				"AccountantVAT",
				"ABL_NetWeight",
				"ABL_NetWeightUQ",
				"ABL_OtherValue",
				"ABL_RX_NKOtherValueCurrency",
				"ABL_GoodsValue",
				"ABL_RX_NKGoodsValueCurrency",
				"ABL_Procedure",
				"ABL_BillStatus",
				"GuaranteeType",
				"GuaranteeRefNo",
				"GuaranteeAmount",
			}, columnInfos.Select(s => s.ColumnName));
		}
	}
}
