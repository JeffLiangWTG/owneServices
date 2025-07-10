using System;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(OrgAddressMessageDataForm))]
	sealed class OrgAddressMessageDataImporterConsigneeCreateUpdateFormTest : ZFormBasherTest
	{
		public void TestCancelButton_Click()
		{
			using (var form = (OrgAddressMessageDataForm)GetFormToBashCore())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				form.CancelButton_Click(form.CancelButton, EventArgs.Empty);
				AssertEquals("IsOKToSendMessage", false, form.IsOKToSendMessage);
			}
		}

		public void TestSendButton_Click()
		{
			using (var form = (OrgAddressMessageDataForm)GetFormToBashCore())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.SendButton_Click(form.SendButton, EventArgs.Empty);

				AssertEquals("HasMessageErrors", true, AddressData.HasMessageErrors);
				AssertEquals("Message Error is warned", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors"));
				AssertEquals("IsOKToSendMessage - users have not agreed to send messages despite message errors", false, form.IsOKToSendMessage);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.SendButton_Click(form.SendButton, EventArgs.Empty);
				AssertEquals("Message Error is warned", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains("There are message errors"));
				AssertEquals("IsOKToSendMessage - users agreed to send messages despite message errors", true, form.IsOKToSendMessage);
			}
		}

		protected override Form GetFormToBashCore() => new OrgAddressMessageDataForm(AddressData);

		OrgAddressMessageData AddressData
		{
			get
			{
				var organisation = Factory.New<OrgHeader>();
				var wrapper = OrgHeaderWrapper.New(organisation);
				return addressData ?? (addressData = new OrgAddressMessageData(wrapper));
			}
		}
		OrgAddressMessageData addressData;
	}
}
