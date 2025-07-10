using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestDoNotSendWithMessageErrorsSecurityRight()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var header = Factory.New<CusISFHeader>();
			using (var ediMenu = new EDIMenu(header))
			using (var form = new ISFForm(header))
			{
				form.Menu.MenuItems.Add(ediMenu);
				var sendItem = ediMenu.MenuItems.FindByText(EDIMenu.MenuItemText.Send);
				AssertNotNull(sendItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bool oldAllowed = Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed;
				try
				{
					Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = false;
					sendItem.PerformClick();
					Assert("Pre-condition", header.HasMessageErrors());
					AssertContains(MessageSendingValidation.MessageErrorsExistWithNoSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text.Replace("\r\n", ""));
				}
				finally
				{
					Env.Security.ImporterSecurityFilingSendWithMessageErrors.IsAllowed = oldAllowed;
				}
			}
		}

		[TestDate(2016, 11, 02)]
		public void TestDoNotSendBBlockData()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var nonUSCompany = Factory.New<GlbCompany>();
			nonUSCompany.GC_Code = "Z!Z";
			nonUSCompany.GC_Name = "BOB THE BUILDER";
			nonUSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonUSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var nonUSBranch = nonUSCompany.Branches.AddNew();
			nonUSBranch.GB_Code = "Z!Z";
			nonUSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_Name = "DUMMY COMPANY";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = ZString.Empty;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(nonUSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(nonUSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_GB = nonUSBranch.PK;
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			header.BF_CustomsReference = "ZXJ-23432342323";
			using (EDIMenu testMenu = new EDIMenu(header))
			using (ZForm form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				MenuItem item = testMenu.MenuItems[0];
				header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("1 message was created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(true, header.Messages[0].EM_SendWithMessageErrors);
				AssertEquals(true, header.Messages[0].EM_MessageText.Contains("B01       SF                                               EDIZ!ZDAT_1          SF10201RCT                          11ZXJ-23432342323                           SF50UN                UN                                                        Y         SF00002"));
			}
		}

		public void TestDeleteIsNotAllowedForDeletedMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var header = Factory.New<CusISFHeader>();
			using (var ediMenu = new EDIMenu(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(ediMenu);
				header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
				header.BF_CustomsReference = "XXXXX";
				var delItem = ediMenu.MenuItems.FindByText(EDIMenu.MenuItemText.Delete);
				delItem.PerformClick();
				AssertEquals("Cannot send 'Delete' message as message has been already deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.BF_CustomsStatus = MessageStatusList.Codes.ErrorISFDelete;
				delItem.PerformClick();
				AssertEquals("You are about to send 'Delete' message. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFAdd;
				delItem.PerformClick();
				AssertEquals("You are about to send 'Delete' message. Are you sure?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFurtherMessagingNotAllowedForDeletedCustomsReferenceMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var header = Factory.New<CusISFHeader>();
			using (var ediMenu = new EDIMenu(header))
			using (var form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(ediMenu);
				header.BF_CustomsStatus = MessageStatusList.Codes.ClearISFDelete;
				header.BF_CustomsReference = "XXXXX";
				var sendItem = ediMenu.MenuItems.FindByText(EDIMenu.MenuItemText.Send);
				sendItem.PerformClick();
				AssertEquals("Cannot send further messages as this Customs Reference 'XXXXX' has been deleted from Customs system.\r\nYou can use 'Reset to Original' to reuse the job and get a new Customs Reference.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.ResetToOriginal();
				sendItem.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendClick()
		{
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var nonUSCompany = Factory.New<GlbCompany>();
			nonUSCompany.GC_Code = "Z!Z";
			nonUSCompany.GC_Name = "BOB THE BUILDER";
			nonUSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			nonUSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var nonUSBranch = nonUSCompany.Branches.AddNew();
			nonUSBranch.GB_Code = "Z!Z";
			nonUSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_Code = "Z1Z";
			uSCompany.GC_Name = "DUMMY COMPANY";
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var uSBranch = uSCompany.Branches.AddNew();
			uSBranch.GB_Code = "Z1Z";
			uSBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = ZString.Empty;
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(uSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(nonUSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(nonUSCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_GB = nonUSBranch.PK;
			header.BF_OH_Importer = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			header.BF_CustomsReference = "ZXJ-23432342323";
			using (EDIMenu testMenu = new EDIMenu(header))
			using (ZForm form = new ZForm(header))
			{
				form.Menu.MenuItems.Add(testMenu);
				MenuItem item = testMenu.MenuItems[0];
				header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
				AssertEquals("PreCondition: Test Menu 0 should equals Send", "&Send", item.Text);
				AssertEquals("PreCondition: Has Changes", true, header.HasChanges);
				item.PerformClick();
				AssertEquals("The data has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, header.Messages.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				item.PerformClick();
				AssertEquals("1 message was created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, header.Messages.Count);
				AssertEquals(true, header.Messages[0].EM_SendWithMessageErrors);
				AssertEquals("ZXJ-23432342323", header.Messages[0].EM_ApplicationReference);
			}
		}
	}
}
