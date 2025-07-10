using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class TranshipmentMessageMenuItemTest : TestCaseWithFactory
	{
		public void TestMenuItems()
		{
			var header = Factory.New<CusInBondHeader>();
			var amsMenuItem = new TranshipmentMessageMenuItem(header);
			AssertNotNull(amsMenuItem.MenuItems.FindByText("Send Transhipment Application"));
		}

		public void TestSendCustomsDeclarationMenuItem()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			header.BH_GS_NKCusAgent = "TT";
			header.BH_CustomsProfile = "123-3";
			using (var formForTest = new TranshipmentForm(header))
			{
				formForTest.Show();
				var menuItem = formForTest.Menu.MenuItems.FindByText("Send Transhipment Application", true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(TranshipmentMessageMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();
				AssertEquals(TranshipmentMessageMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, header.IsInDatabase);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem.PerformClick();
				AssertEquals(true, header.IsInDatabase);
				var validCredential = string.Format(CultureInfo.CurrentCulture, "Please create a valid Credential for {0} on {1} - Brokerage.", header.BH_CustomsProfile, header.BH_GS_NKCusAgent);
				var continueSendCredential = string.Format(CultureInfo.CurrentCulture, "The credential (for {0} on {1}) you have selected is not valid. Sending this message will most likely result in an error. Do you still want to proceed with this action?", header.BH_CustomsProfile, header.BH_GS_NKCusAgent);
				menuItem.PerformClick();
				AssertContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var extPassword1 = Factory.New<Business.GlbExternalPassword>();
				extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
				extPassword1.GP_GC = company.PK;
				extPassword1.GP_MailBoxID = "123-3";
				extPassword1.GP_UserID = "001";
				extPassword1.GP_GS = staff.PK;
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				menuItem.PerformClick();
				AssertNotContains(validCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains(continueSendCredential, UnitTestUserNotification.Instance.LastMessage.Text);
				var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
				cusNum1.CE_ParentID = header.PK;
				cusNum1.CE_Category = "CUS";
				cusNum1.CE_EntryType = "TRS";
				cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
				cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
				cusNum1.CE_EntryNum = "NO1";
				Factory.Save();
			}
		}
	}
}
