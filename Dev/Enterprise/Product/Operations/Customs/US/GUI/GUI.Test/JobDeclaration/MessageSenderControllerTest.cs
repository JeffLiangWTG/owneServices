using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class MessageSenderControllerTest : TestCaseWithFactory
	{
		public void TestContinueWithNotifications()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = USJobMessageTypeList.Codes.Import;
			using (var frm = new ZForm())
			{
				var form = frm;
				var mainForm = new Lazy<ZForm>(() => form);
				declaration.RunPreSaveValidation();
				var messageSendingNotificationCollection = new MessageSendingNotificationCollection();
				var contorller = new MessageSenderController(declaration, mainForm, null);
				var se = Factory.New<GlbSecurity>();
				se.GU_SecurityRight = "AllMessageErrors";
				se.GU_GG = Core.Constants.Groups.AllPK;
				se.GU_SecurityItemIsAllowed = true;
				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				Assert(contorller.ContinueWithNotifications(messageSendingNotificationCollection));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				Assert(contorller.ContinueWithNotifications(messageSendingNotificationCollection));
			}
		}

		public void TestUpdateWarehouseWithdrawal()
		{
			var whsDataHelper = new WhsDataTestHelper(Factory);
			var declaration = whsDataHelper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B000000002", "XJ5", "ENT326", 50m);
			declaration.US_QtyInWHBeforeWithdrawal = 150m;
			declaration.US_QtyBeingWithdrawn = 0m;
			Factory.Save();
			using (var frm = new ZForm())
			{
				var form = frm;
				var mainForm = new Lazy<ZForm>(() => form);
				declaration.RunPreSaveValidation();
				var messageSendingNotificationCollection = new MessageSendingNotificationCollection();
				var contorller = new MessageSenderController(declaration, mainForm, null);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				contorller.SendMessages<MainMessageSender>(ImportMessageSendingMessageType.Original, ImportMessageSendingMessageTypeAdditionalFilter.None);
				AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
				AssertEquals(50m, declaration.US_QtyBeingWithdrawn);
				AssertEquals(100m, declaration.US_QtyInWHAfterWithdrawal);
				AssertEquals(false, declaration.US_IsFinalWHS);
			}
		}
	}
}
