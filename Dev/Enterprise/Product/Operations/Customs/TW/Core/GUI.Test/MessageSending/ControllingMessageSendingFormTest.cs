using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(ControllingMessageSendingForm))]
	sealed class ControllingMessageSendingFormTest : TWMessageSendingFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ControllingMessageSendingForm(Wrapper);
		}

		public override void TestMessageMenuItem()
		{
			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = Declaration;
			UnitTestUserNotification.Instance.ClearMessages();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendMessagesToNCATKMenuItem.Visible);
			testMenu.SendApplicationMessageForCertificateOfOriginMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(ControllingMessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public override void TestDialogInCheckIsOKToSend()
		{
			using (var form = GetFormToBashCore())
			{
				var clientSetting = new TWNCATKClientSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				clientSetting.MachineName = "Machine Name";
				clientSetting.SendToFolder = @"D:\Folders\SendFolder";
				clientSetting.RunningIntervalInSeconds = 15;
				using (TWCustomsDataRegistry.Instance.TWNCATKClientSetting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, clientSetting))
				{
					form.Show();
					var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
					var messageSendingObject = Wrapper.SendingObjectsCollection.Cast<BaseMessageSendingObject>().FirstOrDefault();
					messageSendingObject.ShouldSend = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
					sendButton.PerformClick();
					AssertNotEquals("IsTestMode", TranshipmentMessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
					sendButton.PerformClick();
					AssertNotEquals("IsTestMode", TWMessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
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
					var extPassword1 = Factory.New<Business.GlbExternalPassword>();
					extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
					extPassword1.GP_GC = company.PK;
					extPassword1.GP_MailBoxID = "123-3";
					extPassword1.GP_UserID = "001";
					extPassword1.GP_GS = staff.PK;
					extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					var entry = declaration.ActiveEntryHeaders.AddNew();
					entry.EntryNumber = "AAA";
					var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
					messageHeader.TW1_ControllingMessageType = "X101";
					messageHeader.TW1_RequestDescription = "AA";
					new Business.Testing.TestTWCreator(Factory).CreateAndSetProxyOrganization();
					declaration.JE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
					Factory.Save();
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		protected override BaseMessageSendingObjectParent Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = new ControllingMessageSendingObjectParent(Declaration, "X101");
				}

				return wrapper;
			}
		}
		ControllingMessageSendingObjectParent wrapper;
	}
}
