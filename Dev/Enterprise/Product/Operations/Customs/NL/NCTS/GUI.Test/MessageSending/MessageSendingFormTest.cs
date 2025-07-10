using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.NL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
{
	protected override Form GetFormToBashCore() => new MessageSendingForm(new MessageSendingActionParent(NctsHeader));

	public void TestAvailableColumns()
	{
		form.Show();
		AssertSequencesEqual("Columns", new[]
			{
				NctsHeaderMessageSendingObject.Schema.ShouldSend,
				AutoNctsHeaderMessageSendingObject.Schema.LRN,
				AutoNctsHeaderMessageSendingObject.Schema.MRN,
				AutoNctsHeaderMessageSendingObject.Schema.MessageType,
			},
			messageSendingActionGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
	}

	public void TestMessageSendingObjectParentType()
	{
		form.Show();
		AssertType<MessageSendingActionParent>(form.MessageSendingObjectParent);
	}

	public void TestColumnsWidth()
	{
		CombineAssertions(() =>
		{
			AssertEquals(nameof(NctsHeaderMessageSendingObject.Schema.ShouldSend), 40, messageSendingActionGrid.GetColumnStyle(NctsHeaderMessageSendingObject.Schema.ShouldSend).Width);
			AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.LRN), 160, messageSendingActionGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.LRN).Width);
			AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.MRN), 160, messageSendingActionGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.MRN).Width);
			AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.MessageType), 100, messageSendingActionGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.MessageType).Width);
		});
	}

	public void TestBottomSectionUserControl()
	{
		form.Show();
		var bottomSectionUserControl = form.Controls.Find("MessageSendingFormBottomSectionUserControl", true).Single();
		CombineAssertions(() =>
		{
			AssertType<MessageSendingFormBottomSectionUserControl>("Control Type", bottomSectionUserControl);
			AssertEquals("Binding", ".", bottomSectionUserControl.GetBindingMember());
		});
	}

	public void TestSurpressValidationsByMessageType_INV()
	{
		var movementHeader = NctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
		movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;

		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	public void TestSurpressValidationsByMessageType_RNM()
	{
		var movementHeader = NctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
		movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;

		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	public void TestSurpressValidationsByMessageType_RRL()
	{
		var movementHeader = NctsHeader.MovementHeader;
		movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
		movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
		movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;

		form.Show();
		var validationErrorsTextBox = form.FindSingle<ZTextBox>(x => x.Name == "ValidationErrorsTextBox");
		var sendWithValidationErrorsCheckBox = form.FindSingle<ZCheckBox>(x => x.Name == "SendWithValidationErrorsCheckBox");
		var senderButton = form.FindSingle<ZButton>(x => x.Name == "SendButton");

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is visible", validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is visible", sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is disabled", !senderButton.Enabled);

			sendWithValidationErrorsCheckBox.Checked = true;
			Assert("SendButton is enabled after checked the SendWithValidationErrorsCheckBox", senderButton.Enabled);
		});

		action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;

		CombineAssertions(() =>
		{
			Assert("ValidationErrorsTextBox is invisible", !validationErrorsTextBox.Visible);
			Assert("SendWithValidationErrorsCheckBox is invisible", !sendWithValidationErrorsCheckBox.Visible);
			Assert("SendButton is enabled", senderButton.Enabled);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		messageSendingActionParent = new MessageSendingActionParent(NctsHeader);
		action = messageSendingActionParent.SendingObjectsCollection[0];
		action.ShouldSend = true;
		form = new MessageSendingForm(messageSendingActionParent);
		messageSendingActionGrid = form.FindSingle<ZGrid>();
	}

	MessageSendingForm form;
	ZGrid messageSendingActionGrid;
	MessageSendingActionParent messageSendingActionParent;
	MessageSendingAction action;

	protected override void TearDown()
	{
		base.TearDown();
		form.Dispose();
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNtcsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNtcsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return nctsHeader;
	}
}
