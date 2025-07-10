using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.TR.ETrade.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	class ETradeMenuTest : TestCaseWithFactory
	{
		public void TestWarnUserWhenMessageStatusIsAwaiting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();
			using (var menu = new ETradeMenuForTest(header, mainTestForm))
			{
				menu.ShowPopupMenu();
				var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
				menuItem.PerformClick();

				AssertNotContains("The status is wait for response message, if you send again, the message will be rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.AMA_MessageStatus = "AWA";
			Factory.Save();
			using (var menu = new ETradeMenuForTest(header, mainTestForm))
			{
				menu.ShowPopupMenu();
				var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
				menuItem.PerformClick();

				AssertContains("The status is wait for response message, if you send again, the message will be rejected.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestMenuItemVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (var menu = new ETradeMenuForTest(header, mainTestForm))
			{
				var calculateDuty = "Calculate Duty";
				var sendForTemporaryRegistration = "Send for Temporary Registration";
				var queryForRegistrationNo = "Query for Registration No";
				var sendForRegistrationNo = "Send for Registration No";
				var queryForInspectionClerk = "Query for Inspection Clerk";
				var queryForInspectionLine = "Query for Inspection Line";
				var queryRemainingBills = "Query Remaining Bills for Imp. Dec.";
				var sendForDischargeList = "Send for Discharge List";
				var sendForComplementaryDeclaration = "Send for Complementary Declaration";
				var allMenusToTest = new string[] { calculateDuty, sendForTemporaryRegistration, queryForRegistrationNo, sendForRegistrationNo, queryForInspectionClerk,
				queryForInspectionLine, queryRemainingBills, sendForDischargeList, sendForComplementaryDeclaration };

				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, sendForTemporaryRegistration });

				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.TempRegNo = "test000";
				header.RegistrationNumber = "test001";

				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, sendForTemporaryRegistration });

				header.MessageMode = TRMessageTypes.Codes.TRQ;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, queryForRegistrationNo });

				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				header.MessageMode = TRMessageTypes.Codes.TRI;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, queryForInspectionClerk });

				header.MessageMode = TRMessageTypes.Codes.TRL;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, queryForInspectionLine });

				header.MessageMode = TRMessageTypes.Codes.TRB;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, queryRemainingBills });

				header.MessageMode = TRMessageTypes.Codes.TRD;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, sendForDischargeList });

				header.MessageMode = TRMessageTypes.Codes.TCD;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { calculateDuty, sendForComplementaryDeclaration });

				header.AMA_Nature = ShipmentTypeList.Codes.Export22;
				header.MessageMode = TRMessageTypes.Codes.TRS;
				AssertMenuItemVisible(menu, allMenusToTest, new string[] { sendForRegistrationNo });
			}
		}

		void AssertMenuItemVisible(ETradeMenuForTest menu, string[] allMenuItems, string[] visibleMenuItems)
		{
			menu.ShowPopupMenu();

			foreach (var visibleMenuItem in visibleMenuItems)
			{
				AssertEquals(visibleMenuItem, true, menu.MenuItems.FindByText(visibleMenuItem).Visible);
			}

			foreach (var invisibleMenuItem in allMenuItems.Except(visibleMenuItems))
			{
				AssertEquals(invisibleMenuItem, false, menu.MenuItems.FindByText(invisibleMenuItem).Visible);
			}
		}

		public void TestRecalculateDutyBeforeSendingTemporaryRegistrationSucceededAndStampTaxIsCalculatedCorrectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExemptionCode1 = "HK18";
				bill.ExemptionCode2 = "DOC";
				bill.ABL_CustomsValue = 80;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Factory.Save();

				using (var menu = new ETradeMenuForTest(header, mainTestForm))
				{
					menu.ShowPopupMenu();
					var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
					menuItem.PerformClick();

					CombineAssertions(() =>
					{
						AssertNotEquals("Duties have been calculated.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(119m, header.StampTaxValue);
					});
				}
			}
		}

		public void TestRecalculateDutyBeforeSendingTemporaryRegistrationSucceededAndCustomsDutyIsCalculatedCorrectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExemptionCode1 = "HK18";
				bill.ExemptionCode2 = "DOC";
				bill.ABL_CustomsValue = 80;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill.ExportCountry = Core.Constants.CountryCodes.Germany;
				var pack = bill.Packs.AddNew();
				pack.PackedItem.API_GoodsValue = 120;
				pack.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack.PackedItem.API_Tariff = "1000";
				Factory.Save();

				using (var menu = new ETradeMenuForTest(header, mainTestForm))
				{
					menu.ShowPopupMenu();
					var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
					menuItem.PerformClick();

					var customsDuty = bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
					var taxCount = bill.AsycudaTaxes.Cast<AsycudaTax>().Count(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
					CombineAssertions(() =>
					{
						AssertNotEquals("Duties have been calculated.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(1, taxCount);
						AssertEquals(customsDuty.AET_ChargeType, TaxCodeList.Codes.CustomsDuty);
						AssertEquals(customsDuty.AET_ChargeAmount, (ZDecimal)258.4m);
					});
				}
			}
		}

		public void TestRecalculateDutyBeforeSendingTemporaryRegistrationFailedDueToEmptyDateAtCustomsOffice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Empty;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill = header.Bills.AddNew();
				bill.ExemptionCode1 = "HK18";
				bill.ExemptionCode2 = "DOC";
				bill.ABL_CustomsValue = 80;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack = bill.Packs.AddNew();
				pack.PackedItem.API_GoodsValue = 120;
				pack.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack.PackedItem.API_Tariff = "1000";
				Factory.Save();

				using (var menu = new ETradeMenuForTest(header, mainTestForm))
				{
					menu.ShowPopupMenu();
					var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
					menuItem.PerformClick();

					CombineAssertions(() =>
					{
						AssertNotEquals("Arrival Date is required for Duty Calculation", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(0m, header.StampTaxValue);
						AssertEquals(0, bill.AsycudaTaxes.Count);
					});
				}
			}
		}

		public void TestOnlyMessagingProcessNotificationsDisplayed()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			using (var menu = new ETradeMenuForTest(header, mainTestForm))
			{
				menu.ShowPopupMenu();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.Yes); // Save
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No); // Validation message

				var menuItem = menu.MenuItems.FindByText("Send for Temporary Registration");
				menuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("Save message", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertContains("Review first line", "Please review the following notifications:", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertContains("Customs rejection message", "It is likely that your message(s) will be rejected by Customs", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
					AssertContains("Message error details", "Goods Location: You have not entered a Goods Location.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				});
			}
		}

		public void TestInspectionLineMessageBillNumberReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				header.MessageMode = "TRL";
				var bill = header.Bills.AddNew();
				bill.ExemptionCode1 = "HK18";
				bill.ExemptionCode2 = "DOC";
				bill.ABL_CustomsValue = 80;
				bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				Factory.Save();

				using (var menu = new ETradeMenuForTest(header, mainTestForm))
				{
					menu.ShowPopupMenu();
					var menuItem = menu.MenuItems.FindByText("Query for Inspection Line");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					menuItem.PerformClick();

					AssertEquals("ABL_MessageStatus", "AWA", bill.ABL_MessageStatus);
					AssertEquals("ABL_BillNumberInfo", true, bill.ABL_BillNumberInfo.ReadOnly);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			mainTestForm = new ZForm();
			CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
			helper.SetRefValues();
			GlbStaff.CurrentUser.GS_EmailAddress = "bob@where.com";
			var password = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			password.TR_Chipset = ChipsetList.Codes.GEMPLUS;
			password.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			password.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
		}
		protected override void TearDown()
		{
			mainTestForm?.Dispose();
			base.TearDown();
		}
		ZForm mainTestForm;
	}

	class ETradeMenuForTest : ETradeMenu
	{
		public ETradeMenuForTest(AsycudaManifestHeader header, ZForm testForm) : base(header)
		{
			this.testForm = testForm;
		}

		readonly ZForm testForm;

		protected override ZForm MainForm => testForm;
	}
}
