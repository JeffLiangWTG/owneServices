using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public abstract class MessageSendingFormAbstractTest<TMessageSendingForm, TJobDeclarationMessageSendingObjectParent> : Customs.GUI.Testing.MessageSendingObjectFormTest
		where TMessageSendingForm : MessageSendingForm
		where TJobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent
	{
		public void TestSendToCustomsMenuItem()
		{
			var testMenu = new EDIMenuForTest();
			testMenu.Declaration = Declaration;
			UnitTestUserNotification.Instance.ClearMessages();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendGoodsExaminationApplicationMenuItem.Visible);
			testMenu.SendGoodsExaminationApplicationMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestClickOKButton()
		{
			var nothingSelected = "There's nothing selected to be sent to Customs";
			using (var form = new MessageSendingForm(DeclarationWrapper))
			{
				form.Show();
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = false;
				Assert(!sendButton.Enabled);
				messageSendingObject.ShouldSend = true;
				sendButton.PerformClick();
				AssertNotContains("No select send message", nothingSelected, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestDialogInCheckIsOKToSend()
		{
			using (var form = new MessageSendingForm(DeclarationWrapper))
			{
				form.Show();
				DeclarationWrapper.AllowSendWithError = true;
				var sendButton = form.Controls.Find("SendButton", true)[0] as ZButton;
				var messageSendingObject = DeclarationWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				sendButton.PerformClick();
				AssertEquals("IsTestMode", MessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TWCustomsDataRegistry.Instance.TWIsTestMode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				sendButton.PerformClick();
				AssertNotEquals("IsTestMode", MessageSendingForm.Constants.TestingEnvironment, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new MessageSendingForm(DeclarationWrapper);
		}

		protected abstract TJobDeclarationMessageSendingObjectParent GetDeclarationWrapper();

		protected TJobDeclarationMessageSendingObjectParent DeclarationWrapper
		{
			get
			{
				if (declarationWrapper == null)
				{
					declarationWrapper = GetDeclarationWrapper();
				}

				return declarationWrapper;
			}
		}

		TJobDeclarationMessageSendingObjectParent declarationWrapper;
		internal JobDeclarationForTestSendingObject Declaration
		{
			get
			{
				if (declaration == null)
				{
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
					declaration = Factory.NewWithValidTestData<JobDeclarationForTestSendingObject>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					cusHead1.CH_CEI_Instruction = entryInstruction.PK;
					cusHead1.CH_DeclarationIncoterm = "CFR";
					entryInstruction.CEI_CustomsOffice = "BB";
					entryInstruction.CEI_Style = "G1";
					entryInstruction.CEI_BoxNumber = "123";
					var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
					cusNum1.CE_ParentID = cusHead1.PK;
					cusNum1.CE_Category = "CUS";
					cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Export;
					cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
					cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
					cusNum1.CE_EntryNum = "NO1";
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_OH_Supplier = orgHeader.PK;
					Factory.Save();
				}

				return declaration;
			}
		}

		JobDeclarationForTestSendingObject declaration;
	}
}
