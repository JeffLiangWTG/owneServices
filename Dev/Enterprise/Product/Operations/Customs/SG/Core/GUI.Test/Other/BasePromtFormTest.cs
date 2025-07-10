using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(BasePromtForm))]
	sealed class BasePromtFormTest : ZFormBasherTest
	{
		public void TestOKButtonState()
		{
			using (var form = new BasePromtForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory)))
			{
				form.Show();
				AssertEquals(true, form.OKButton.Enabled);
				form.DeclarationCheckBox.Checked = false;
				AssertEquals(false, form.OKButton.Enabled);
				form.DeclarationCheckBox.Checked = true;
				AssertEquals(true, form.OKButton.Enabled);
			}
		}

		public void TestShowMessageCheck()
		{
			var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = GetSG4Broker().GS_Code;
			additionalMessageInformation.AM_BrokerPassword = "Password";
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (var form = new BasePromtForm(additionalMessageInformation))
			{
				form.Show();
				Assert(!form.zCheckShowMessage.Visible);
				AssertEquals(CheckState.Unchecked, form.zCheckShowMessage.CheckState);
				form.OKButton.PerformClick();
				AssertEquals("Message text", additionalMessageInformation.MessageCreated("Message text"));
			}

			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (var form = new BasePromtForm(additionalMessageInformation))
			{
				form.Show();
				Assert(form.zCheckShowMessage.Visible);
				form.zCheckShowMessage.Checked = true;
				form.OKButton.PerformClick();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				additionalMessageInformation.MessageCreated("Message text");
				AssertEquals("Message Text", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		public void TestCancel_ButtonClick()
		{
			using (var form = new BasePromtForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory)))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestOKButtonClick()
		{
			var additionalMessageInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			additionalMessageInformation.AM_Broker = GetSG4Broker().GS_Code;
			additionalMessageInformation.AM_BrokerPassword = "Password";
			using (var form = new BasePromtForm(additionalMessageInformation))
			{
				form.Show();
				form.DeclarationCheckBox.Checked = true;
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				((AdditionalMessageInformation)form.BusinessEntity).SupportingDocuments.AddNew();
				form.OKButton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}
		}

		GlbStaff GetSG4Broker()
		{
			var factory = new BusinessObjectFactory();
			var result = factory.New<GlbStaff>();
			result.GS_Code = "TST";
			result.GS_LoginName = "testadd";
			result.GS_WorkPhone = "1234";
			var wrapper = SGGlbStaffWrapper.Get(result);
			wrapper.Tradenetv4Password.GP_UserID = "vv123";
			wrapper.Tradenetv4Password.GP_MailBoxID = "uitest";
			wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
			wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Password";
			factory.Save();
			return result;
		}

		protected override Form GetFormToBashCore() => new BasePromtForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory));
	}
}
