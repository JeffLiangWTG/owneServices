using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CustomsDeclarationSendingForm))]
sealed class CustomsDeclarationSendingFormTest : ZFormBasherTest
{
	public void TestCheckIsOKToSend_AllowNoObjectsToSend()
	{
		using var form = GetNewMessageSendingForm();
		using (sendingObjectParent.GetValidationSuspender())
		{
			form.Show();
			var sendButton = (ZButton)form.Controls.Find("SendButton", true).First();
			sendingObjectParent.SelectedSendingObjects.First().ShouldSend = false;
			sendButton.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("There's nothing selected to be sent to Customs", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				sendingObjectParent.EDocs.AddNew();
				sendButton.PerformClick();
				AssertEquals("No declaration data will be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestTabInAdditionalDataTabControl()
	{
		RunForForm(form =>
		{
			CombineAssertions(() =>
			{
				AssertEquals("MessageTabPage is contained in AdditionalDataTabControl", true, form.AdditionalDataTabControl.Contains(form.MessageTabPage));
				AssertEquals("EDocsTabPage is contained in AdditionalDataTabControl", true, form.AdditionalDataTabControl.Contains(form.EDocsTabPage));
			});
		});
	}

	public void TestMessageTabPage()
	{
		RunForForm(form =>
		{
			CombineAssertions(() =>
			{
				AssertEquals("MessageTabPage contains MessageSendingObjectsGrid", 2, form.MessageTabPage.Controls.Count);
				AssertEquals("Caption of MessageTabPage", "Messages", form.MessageTabPage.CaptionResourceString.Caption);
			});
		});
	}

	public void TestSpecificDataUserControl()
	{
		RunForForm(form =>
		{
			CombineAssertions(() =>
			{
				var sendingObj1 = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();
				AssertEquals("SpecificDataUserControl should be invisible because there are no CC583", false, form.SpecificDataUserControl.Visible);

				sendingObj1.Action = ExportMessageSendingObjectActionList.Codes.CC583;
				AssertEquals("SpecificDataUserControl should be visible because CC583 is created", true, form.SpecificDataUserControl.Visible);
			});
		});
	}

	public void TestEDocsTabPage()
	{
		RunForForm(form =>
		{
			CombineAssertions(() =>
			{
				AssertEquals("EDocsTabPage contains MessageSendingEDocsUserControl", 1, form.EDocsTabPage.Controls.Count);
				AssertEquals("Caption of EDocsTabPage", "EDocs", form.EDocsTabPage.CaptionResourceString.Caption);
			});
		});
	}

	protected override Form GetFormToBashCore() => GetNewMessageSendingForm();

	void RunForForm(Action<CustomsDeclarationSendingForm> methodToRun)
	{
		using var form = GetNewMessageSendingForm();
		sendingObjectParent.SuspendValidation();
		form.Show();
		methodToRun.Invoke(form);
	}

	CustomsDeclarationSendingForm GetNewMessageSendingForm()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
		return new CustomsDeclarationSendingForm(sendingObjectParent);
	}

	CustomsDeclarationMessageSendingObjectParent sendingObjectParent;
}
