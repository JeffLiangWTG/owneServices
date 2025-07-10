using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using BaseMessageSendingObject = Enterprise.Customs.PL.Business.BaseMessageSendingObject;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
{
	public void TestMessagesToBeSentGrid()
	{
		using (var form = GetNewMessageSendingForm())
		{
			var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");

			CombineAssertions(() =>
			{
				AssertEquals("Count", 9, grid.ColumnStyles.Count);
				AssertEquals("Send?", "ShouldSend", ((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("Action", BaseMessageSendingObject.PLSchema.Action, ((ZDropEditColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("DeclarationDate", BaseMessageSendingObject.PLSchema.DeclarationDate, ((ZDateEditColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("ReferenceNumber", BaseMessageSendingObject.Schema.LocalReferenceNumber, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
				AssertEquals("EntryNumber", BaseMessageSendingObject.PLSchema.EntryNumber, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[4]).ColumnName);
				AssertEquals("EntryNumber CharacterCasing", CharacterCasing.Upper, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[4]).CharacterCasing);
				AssertEquals("EntryStatus", BaseMessageSendingObject.Schema.EntryStatus, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[5]).ColumnName);
				AssertEquals("MessageStatus", BaseMessageSendingObject.PLSchema.MessageStatus, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[6]).ColumnName);
				AssertEquals("EntryDescription", BaseMessageSendingObject.PLSchema.EntryDescription, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[7]).ColumnName);
				AssertEquals("ResponseMessage", BaseMessageSendingObject.PLSchema.ResponseMessage, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[8]).ColumnName);
			});
		}
	}

	public void TestMessagesToBeSentGridShouldFitHeaderAndFourLines()
	{
		var headerCount = 1;
		var lineCount = 4;
		var totalRowCount = headerCount + lineCount;

		using (var form = GetNewMessageSendingForm())
		{
			var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
			AssertGreaterThan("Grid should display header and 4 lines of content without scrolling", grid.Height, totalRowCount * grid.PreferredRowHeight);
		}
	}

	public void TestNotifications()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals("ValidationErrorsGroupBox", true, form.ValidationErrorsGroupBox.Visible);
				AssertEquals("AdditionalWarningsGroupBox", true, form.AdditionalWarningsGroupBox.Visible);

				var validationErrorsTextBox = form.ValidationErrorsTextBox;
				AssertEquals("ValidationErrorsTextBox should exist and should be visible", true, validationErrorsTextBox.Visible);
				AssertEquals("ValidationErrorsTextBox should be multiline", true, validationErrorsTextBox.Multiline);

				var additionalWarningsTextBox = form.AdditionalWarningsTextBox;
				AssertEquals("AdditionalWarningsTextBox should exist and should be visible", true, additionalWarningsTextBox.Visible);
				AssertEquals("AdditionalWarningsTextBox should be multiline", true, additionalWarningsTextBox.Multiline);
			});
		}
	}

	public void TestCheckboxControls()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			CombineAssertions(() =>
			{
				var continueToSendCheckBox = form.ContinueToSendCheckBox;
				AssertEquals("ContinueToSendCheckBox should exist and should be visible", true, continueToSendCheckBox.Visible);
				AssertEquals("ContinueToSendCheckBox should be enabled", true, continueToSendCheckBox.Enabled);

				var fallbackSystemCheckBox = form.FallbackSystemCheckBox;
				AssertEquals("FallbackSystemCheckBox should exist and should be visible", true, fallbackSystemCheckBox.Visible);
				AssertEquals("FallbackSystemCheckBox should be enabled", true, fallbackSystemCheckBox.Enabled);
			});
		}
	}

	public void TestCancelButton()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			CombineAssertions(() =>
			{
				var cancelButton = form.FindSingle<ZButton>("CancelButton2");
				AssertEquals("CancelButton should exist and should be visible", true, cancelButton.Visible);
				AssertEquals("CancelButton should be enabled", true, cancelButton.Enabled);
			});
		}
	}

	public void TestSendButton()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			var businessEntity = form.BusinessEntity;
			CombineAssertions(() =>
			{
				var sendButton = form.FindSingle<ZButton>("SendButton");
				AssertEquals("SendButton should exist and should be visible", true, sendButton.Visible);

				businessEntity.AllowSendWithError = false;
				AssertEquals("SendButton should be disabled - AllowSendWithError is false and errors exists", false, sendButton.Enabled);
				AssertEquals("SendButton should should be visible when AllowSendWithError is false", true, sendButton.Visible);

				businessEntity.AllowSendWithError = true;
				AssertEquals("SendButton should be disabled - AllowSendWithError is true and errors exists", true, sendButton.Enabled);
				AssertEquals("SendButton should should be visible when AllowSendWithError is true", true, sendButton.Visible);
			});
		}
	}

	public void TestSendButton_WithoutMessageErrors()
	{
		using (var form = GetNewMessageSendingForm())
		{
			var businessEntity = form.BusinessEntity;
			using (businessEntity.GetValidationSuspender())
			{
				businessEntity.ObjectsToSend.ForEach(x =>
				{
					x.SuspendValidation();
					x.Header.SuspendValidation();
					x.Header.Declaration.SuspendValidation();
				});
				form.Show();
				CombineAssertions(() =>
				{
					var sendButton = form.FindSingle<ZButton>("SendButton");
					AssertEquals("SendButton should exist and should be visible", true, sendButton.Visible);

					businessEntity.AllowSendWithError = false;
					AssertEquals("SendButton should be disabled - AllowSendWithError is false and no errors exists", true, sendButton.Enabled);
				});
			}
		}
	}

	public void TestAdditionalDetailsUserControl()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			CombineAssertions(() =>
			{
				var additionalDetailsGroupBox = form.FindSingle<ZGroupBox>("AdditionalDetailsGroupBox");
				AssertEquals("AdditionalDetailsGroupBox should be visible", true, additionalDetailsGroupBox.Visible);

				var additionalDetailsUserControl = additionalDetailsGroupBox.FindSingle<AdditionalDetailsUserControl>("AdditionalDetailsUserControl");
				AssertEquals("AdditionalDetailsUserControl should be visible", true, additionalDetailsUserControl.Visible);
				AssertEquals("AdditionalDetailsUserControl binding should point to grid objects", typeof(BaseMessageSendingObject), additionalDetailsUserControl.DataSourceType);
			});
		}
	}

	public void TestAdditionalDetailsLayout_Import()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		using (var form = GetNewMessageSendingForm(declaration))
		{
			AssertType<ImportAdditionalDetailsLayout>(form.GetNewAdditionalDetailsUserControlPanelLayoutProvider());
		}
	}

	public void TestAdditionalDetailsLayout_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		using (var form = GetNewMessageSendingForm(declaration))
		{
			AssertType<ExportAdditionalDetailsLayout>(form.GetNewAdditionalDetailsUserControlPanelLayoutProvider());
		}
	}

	public void TestFormIsAutoScroll()
	{
		using (var form = GetNewMessageSendingForm())
		{
			form.Show();
			AssertEquals("AutoScroll should be enabled for providing access to all controls", expected: true, form.AutoScroll);
		}
	}

	protected override Form GetFormToBashCore() => GetNewMessageSendingForm();

	MessageSendingForm GetNewMessageSendingForm(JobDeclaration declaration = null)
	{
		if (declaration == null)
		{
			declaration = Factory.New<JobDeclaration>();
		}
		declaration.CustomsEntryHeaders.AddNew();
		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		return new MessageSendingForm(sendingObjectParent);
	}

	BaseMessageSendingObjectParent sendingObjectParent;
}
