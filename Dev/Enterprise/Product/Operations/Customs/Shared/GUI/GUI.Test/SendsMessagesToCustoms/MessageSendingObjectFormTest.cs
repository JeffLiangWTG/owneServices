using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MessageSendingObjectForm))]
	public class MessageSendingObjectFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public virtual void TestValidationOnSending()
		{
			var testingParent = new BaseMessageSendingObjectParentForTest(Factory);
			using (var form = new MessageSendingObjectForm(testingParent))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				sendButton.PerformClick();
				AssertEquals("There's nothing selected to be sent to Customs", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				var testingObject1 = new BaseMessageSendingObjectForTest(Factory);
				var testingObject2 = new BaseMessageSendingObjectForTest(Factory);
				testingParent.SendingObjectsCollection.Add(testingObject1);
				testingParent.SendingObjectsCollection.Add(testingObject2);
				testingObject1.ShouldSend = false;
				testingObject2.ShouldSend = false;
				sendButton.PerformClick();
				AssertEquals("There's nothing selected to be sent to Customs", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testingObject2.ShouldSend = true;
				testingObject2.MockValidationMessage = (CargoWise.EntityFramework.ZPropertyInfo x) =>
				{
					x.AddError("Red");
				};
				sendButton.PerformClick();
				AssertMultilineASCIIEquals("Checking Error", @"Please fix these errors before sending any messages:

Send?: Red", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testingObject2.ShouldSend = true;
				testingObject2.MockValidationMessage = (CargoWise.EntityFramework.ZPropertyInfo x) =>
				{
					x.AddMessageError("Blue");
				};
				sendButton.PerformClick();
				AssertMultilineASCIIEquals("Checking Message Error", @"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Send?: Blue

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				testingObject2.ShouldSend = true;
				testingObject2.MockValidationMessage = null;
				sendButton.PerformClick();
				AssertEquals("Checking no error", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, form.Visible);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		[RequiresSTA]
		public void TestCheckIsOKToSend()
		{
			var testingParent = new BaseMessageSendingObjectParentForTest(Factory);
			using (var form = new MessageSendingObjectForm(testingParent))
			{
				form.Show();
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				sendButton.PerformClick();
				AssertEquals("There's nothing selected to be sent to Customs", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				var testingObject = new BaseMessageSendingObjectForTest(Factory);
				testingParent.SendingObjectsCollection.Add(testingObject);
				testingObject.ShouldSend = true;
				testingObject.MockValidationMessage = null;
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				var declaration = testingParent.TopLevelBusinessObject as BaseJobDeclaration;
				declaration.JE_MessageType = "YYY";
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				declaration.JE_MessageType = string.Empty;
				sendButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new MessageSendingObjectForm(testingParent))
			{
				var declaration = testingParent.TopLevelBusinessObject as BaseJobDeclaration;
				form.Show();
				bool oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
				try
				{
					GlbStaff staff = Factory.New<GlbStaff>();
					staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
					staff.GS_IsActive = true;
					GlbSecurity se = Factory.New<GlbSecurity>();
					se.GU_SecurityRight = "MergeByDefault";
					se.GU_GG = Core.Constants.Groups.AllPK;
					se.GU_SecurityItemIsAllowed = true;
					se.GU_GS = staff.PK;
					Factory.Save();
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
					declaration.JE_MergeBy = "ZZZ";
					var testingObject2 = new BaseMessageSendingObjectForTest(Factory);
					testingParent.SendingObjectsCollection.Add(testingObject2);
					testingObject2.ShouldSend = true;
					testingObject2.MockValidationMessage = null;
					var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
					sendButton.PerformClick();
					AssertEquals(DialogResult.OK, form.DialogResult);
				}
				finally
				{
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
				}
			}
		}

		[RequiresSTA]
		public void TestRightSecurityRightIsChecked()
		{
			var testingParent = new BaseMessageSendingObjectParentForTest(Factory);
			var declaration = testingParent.TopLevelBusinessObject as BaseJobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			testingParent.SecurityCheckpointToSendWithMessageErrorOverride = Env.Security.CustomsDISSendWithMessageErrors;
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			using (var form = new MessageSendingObjectForm(testingParent))
			{
				form.Show();
				try
				{
					GlbStaff staff = Factory.New<GlbStaff>();
					staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
					staff.GS_IsActive = true;
					GlbSecurity se = Factory.New<GlbSecurity>();
					se.GU_SecurityRight = "MergeByDefault";
					se.GU_GG = Core.Constants.Groups.AllPK;
					se.GU_SecurityItemIsAllowed = true;
					se.GU_GS = staff.PK;
					Factory.Save();
					Env.Security.CustomsDISSendWithMessageErrors.IsAllowed = true;
					declaration.JE_MergeBy = "ZZZ";
					var testingObject2 = new BaseMessageSendingObjectForTest(Factory);
					testingParent.SendingObjectsCollection.Add(testingObject2);
					testingObject2.ShouldSend = true;
					testingObject2.MockValidationMessage = null;
					var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
					sendButton.PerformClick();
					AssertEquals(DialogResult.OK, form.DialogResult);
				}
				finally
				{
					Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				}
			}
		}

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore() => new MessageSendingObjectForm(MessageSendingObjectForTest);

		BaseMessageSendingObjectParentForTest MessageSendingObjectForTest => messageSendingObjectForTest ?? (messageSendingObjectForTest = new BaseMessageSendingObjectParentForTest(Factory));
		BaseMessageSendingObjectParentForTest messageSendingObjectForTest;
	}
}
