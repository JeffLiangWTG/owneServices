using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(RetrospectiveQuotaRequestSendingForm))]
sealed class RetrospectiveQuotaRequestSendingFormTest : ZFormBasherTest
{
	public void TestEntryLinesTabPage_InitiallyActive()
	{
		var sendingObject = sendingObjectParent.SendingObjectsCollection
			.OfType<RetrospectiveQuotaRequestMessageSendingObject>().First();
		sendingObject.Action = MessageSendingObjectActionCodes.ZCX05;

		RunForForm(form =>
		{
			var tabControl = form.AdditionalDataTabControl;
			AssertEquals("ZCX05", tabControl.SelectedTab, form.EntryLinesTabPage);
		});
	}

	public void TestMessageSendingObjectsGroupBoxCaption()
	{
		RunForForm(form =>
		{
			var messageSendingObjectsGroupBox = (ZGroupBox)form.Controls.Find("messageSendingObjectsGroupBox", false).First();
			AssertEquals("Messages to be sent - Retrospective Quota Request", messageSendingObjectsGroupBox.CaptionResourceString.Caption);
		});
	}

	public void TestEntryLinesTabPage_TabRelevant_ActionChanges()
	{
		var sendingObject = sendingObjectParent.SendingObjectsCollection
			.OfType<RetrospectiveQuotaRequestMessageSendingObject>().First();
		sendingObject.Action = MessageSendingObjectActionCodes.ZCX05;

		RunForForm(form =>
		{
			CombineAssertions(() =>
			{
				AssertEquals("ZCX05", true, form.EntryLinesTabPage.TabRelevant);
				sendingObject.Action = ZString.Empty;
				AssertEquals("No ZCX05", false, form.EntryLinesTabPage.TabRelevant);
			});
		});
	}

	public void TestEntryLinesTabPage_TabRelevant_InitialValues()
	{
		CombineAssertions(() =>
		{
			var sendingObject = sendingObjectParent.SendingObjectsCollection.OfType<RetrospectiveQuotaRequestMessageSendingObject>().First();
			sendingObject.Action = ZString.Empty;

			RunForForm(form =>
			{
				AssertEquals("No ZCX05", false, form.EntryLinesTabPage.TabRelevant);
			});

			sendingObject.Action = MessageSendingObjectActionCodes.ZCX05;

			RunForForm(form =>
			{
				AssertEquals("ZCX05", true, form.EntryLinesTabPage.TabRelevant);
			});
		});
	}

	RetrospectiveQuotaRequestSendingForm GetNewMessageSendingForm()
	{
		return new RetrospectiveQuotaRequestSendingForm(sendingObjectParent);
	}

	void RunForForm(Action<RetrospectiveQuotaRequestSendingForm> methodToRun)
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			methodToRun.Invoke(form);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		sendingObjectParent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
	}

	RetrospectiveQuotaRequestMessageSendingObjectParent sendingObjectParent;

	protected override Form GetFormToBashCore() => GetNewMessageSendingForm();
}
