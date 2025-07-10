using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		Factory.Save();
		return new MessageSendingForm(new MessageSendingObjectParent(declaration));
	}

	public void TestControls()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		using var form = new MessageSendingFormForTest(new MessageSendingObjectParent(declaration));
		form.Show();

		CombineAssertions(() =>
		{
			form.AssertContainsControl<ZGrid>(nameof(form.MessageSendingObjectsGrid));
			form.AssertContainsControl<ZButton>(nameof(form.SendButton), x => x.WithCaption("&Send"));
			form.AssertContainsControl<ZButton>(nameof(form.CancelButton2), x => x.WithCaption("&Cancel"));
		});
	}

	public void TestMessageSendingObjectsGrid()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		using var form = new MessageSendingFormForTest(new MessageSendingObjectParent(declaration));
		form.Show();
		var gridColumnStyleList = form.MessageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

		void AssertColumnOrder(ZString columnName, ZInt index)
		{
			var columnStyle = gridColumnStyleList.SingleOrDefault(x => x.ColumnName == columnName);
			AssertNotNull($"{columnName} not null", columnStyle);
			Assert($"{columnName}", columnStyle.IsVisible);
			AssertEquals(index, Array.IndexOf(gridColumnStyleList, columnStyle));
		}

		CombineAssertions(() =>
		{
			ZInt index = 0;
			AssertColumnOrder(MessageSendingObject.SchemaShouldSend, index++);
			AssertColumnOrder(MessageSendingObject.Schema.DeclarationType, index++);
			AssertColumnOrder(MessageSendingObject.Schema.Description, index++);
			AssertColumnOrder(MessageSendingObject.Schema.Procedure, index++);
			AssertColumnOrder(MessageSendingObject.Schema.MessageType, index++);
			AssertColumnOrder(MessageSendingObject.Schema.CustomsOffice, index++);
			AssertColumnOrder(MessageSendingObject.Schema.EntryStatus, index++);
			AssertColumnOrder(MessageSendingObject.Schema.EntryNumber, index++);
			AssertColumnOrder(MessageSendingObject.Schema.PaymentMethod, index++);
		});
	}

	public void TestCheckIsOKToSend_ForEmptyMessageType() 
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		declaration.ActiveEntryHeaders.AddNew();

		var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
		var messageSendingObjectCollection = messageSendingObjectParent.SendingObjectsCollection;
		AssertEquals("Message Sending Object Collection Count", 2, messageSendingObjectCollection.Count);

		CombineAssertions(() =>
		{
			var childOne = messageSendingObjectCollection[0];
			var childTwo = messageSendingObjectCollection[1];
			using var form = new MessageSendingFormForTest(messageSendingObjectParent);
			form.Show();

			childOne.ShouldSend = true;
			childTwo.ShouldSend = true;

			childOne.MessageType = ZString.Empty;
			childTwo.MessageType = ZString.Empty;
			AssertEquals("When MessageType is empty on two message sending object children", false, form.CheckIsOKToSend_Exposed());

			childOne.MessageType = "MA";
			AssertEquals("When MessageType is empty on second child item", false, form.CheckIsOKToSend_Exposed());

			childOne.MessageType = ZString.Empty;
			childTwo.MessageType = "MA";
			AssertEquals("When MessageType is empty on first child item", false, form.CheckIsOKToSend_Exposed());

			childOne.MessageType = "MA";
			childTwo.MessageType = "MA";
			AssertEquals("When MessageType is present on both children", true, form.CheckIsOKToSend_Exposed());

			childOne.MessageType = ZString.Empty;
			childOne.ShouldSend = false;
			AssertEquals("When MessageType is not present on one child and that child is not selected for sending", true, form.CheckIsOKToSend_Exposed());
		});
	}

	public void TestChangeSendButtonAvailability_ForEmptyMessageType()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		declaration.ActiveEntryHeaders.AddNew();

		var messageSendingObjectParent = new MessageSendingObjectParent(declaration);
		var messageSendingObjectCollection = messageSendingObjectParent.SendingObjectsCollection;
		AssertEquals("Message Sending Object Collection Count", 2, messageSendingObjectCollection.Count);

		CombineAssertions(() =>
		{
			var childOne = messageSendingObjectCollection[0];
			var childTwo = messageSendingObjectCollection[1];
			using var form = new MessageSendingFormForTest(messageSendingObjectParent);
			form.Show();

			var button = form.FindSingle<ZButton>("SendButton");
			var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
			sendWithValidationErrorsCheckBox.Checked = true;

			childOne.MessageType = ZString.Empty;
			childTwo.MessageType = ZString.Empty;
			AssertEquals($"'{childOne.MessageType}'/'{childTwo.MessageType}': Send-Button should be disabled.", expected: false, button.Enabled);

			childOne.MessageType = "MA";
			AssertEquals($"'{childOne.MessageType}'/'{childTwo.MessageType}': Send-Button should be disabled.", expected: false, button.Enabled);

			childOne.MessageType = ZString.Empty;
			childTwo.MessageType = "MA";
			AssertEquals($"'{childOne.MessageType}'/'{childTwo.MessageType}': Send-Button should be disabled.", expected: false, button.Enabled);

			childOne.MessageType = "MA";
			childTwo.MessageType = "MA";
			AssertEquals($"'{childOne.MessageType}'/'{childTwo.MessageType}': Send-Button should be enabled.", expected: true, button.Enabled);

			childOne.MessageType = ZString.Empty;
			AssertEquals($"'{childOne.MessageType}'/'{childTwo.MessageType}': Send-Button should be disabled.", expected: false, button.Enabled);
		});
	}

	class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(MessageSendingObjectParent messageSendingObjectParent)
			: base(messageSendingObjectParent)
		{
		}

		public new ZGrid MessageSendingObjectsGrid => base.MessageSendingObjectsGrid;
		public new ZButton SendButton => base.SendButton;
		public new ZButton CancelButton2 => base.CancelButton2;
		public bool CheckIsOKToSend_Exposed() => base.CheckIsOKToSend();
	}
}
