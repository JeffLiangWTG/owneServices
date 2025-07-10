using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing;

[TestedType(typeof(ValidationToolFailurePopup))]
sealed class ValidationToolFailurePopupTest : ZFormBasherTest
{
	[RequiresSTA]
	public void TestButtonDeliver_Click()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();
		var deliverButton = popup.GetControl<ZButton>("ButtonDeliver", true);
		deliverButton.PerformClick();
		var senderForm = ZFormModaliser.LastFormShownDialogForTest;
		AssertEquals("DocDeliveryForm", senderForm.GetType().Name);
	}

	public void TestGetNotifyModesList()
	{
		var list = ValidationToolFailurePopup.GetNotifyModesList();
		AssertNotContains("Fax", list.CodesAsString);
		AssertNotContains("ePrint", list.CodesAsString);
		AssertContains("E-Mail", list.CodesAsString);
		AssertContains("Print", list.CodesAsString);
		foreach (ICodeDescription item in list)
		{
			AssertEquals("Code and Description should be the same for DocDeliveryForm", item.Code, item.Description);
		}
	}

	public void TestFormText()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		using var popup = new ValidationToolFailurePopup(failure);
		AssertEquals(failure.Message, popup.Text);
	}

	public void TestButtonProceed_Caption()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		using var popup = new ValidationToolFailurePopup(failure);
		AssertEquals("Caption", "Proceed", popup.ButtonProceed.CaptionResourceString.Caption);
	}

	public void TestButtonProceed_Enabled() => CombineAssertions(() =>
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertButtonProceedEnabled("Empty FailedRuleResults", true);

		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		AssertButtonProceedEnabled("Warning", true);

		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		AssertButtonProceedEnabled("Message", true);

		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		AssertButtonProceedEnabled("Error", false);

		return;

		void AssertButtonProceedEnabled(string message, bool expectedEnabled)
		{
			using var popup = new ValidationToolFailurePopup(failure);
			AssertEquals(message, expectedEnabled, popup.ButtonProceed.Enabled);
		}
	});

	[RequiresSTA]
	public void TestButtonProceed_Warning()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();
		popup.ButtonProceed.PerformClick();
		AssertEquals(DialogResult.Yes, popup.DialogResult);
	}

	public void TestButtonProceed_MessageError_Yes() => AssertButtonProceed_MessageError(true);

	public void TestButtonProceed_MessageError_No() => AssertButtonProceed_MessageError(false);

	void AssertButtonProceed_MessageError(bool proceed)
	{
		var mockMessageErrorsHandle = new Mock<IValidationToolMessageErrorsHandle>();
		mockMessageErrorsHandle.Setup(x => x.Handle()).Returns(proceed);

		var mockObjectHandle = new Mock<ObjectHandle>();
		mockObjectHandle.Setup(x => x.GetObject(It.IsAny<BusinessObject>())).Returns(mockMessageErrorsHandle.Object);
		var hashTable = new Hashtable
		{
			{ DummyBusinessObject.Schema.TablePrefix, mockObjectHandle.Object }
		};
		using var substitute = ObjectFactory.Substitute("ValidationToolMessageErrorsHandles", hashTable);

		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithEntity(Factory.New<DummyBusinessObject>()).WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();
		popup.ButtonProceed.PerformClick();
		AssertEquals(proceed ? DialogResult.Yes : DialogResult.No, popup.DialogResult);
	}

	public void TestLabelStatus_BindingMember()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();
		AssertEquals("Status", popup.LabelStatus.GetBindingMember());
	}

	[RequiresSTA]
	public void TestLabelStatus_Color() => CombineAssertions(() =>
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		AssertLabelStatusColor("Empty FailedRuleResults", Color.Gray);

		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		AssertLabelStatusColor("Warning", Color.LightYellow);
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		AssertLabelStatusColor("Message", Color.LightBlue);
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		AssertLabelStatusColor("Error", Color.Red);

		return;

		void AssertLabelStatusColor(string message, Color expectedColor)
		{
			using var popup = new ValidationToolFailurePopup(failure);
			AssertEquals(message, expectedColor, popup.LabelStatus.BackColor);
		}
	});

	public void TestButtonClose() => CombineAssertions(() =>
	{
		var count = 0;
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		using var popup = new ValidationToolFailurePopup(failure);
		popup.FormClosed += (_, _) => { count++; };
		popup.Show();
		popup.ButtonClose.PerformClick();
		AssertEquals("Caption", "Close", popup.ButtonClose.CaptionResourceString.Caption);
		AssertEquals("Form closed", 1, count);
	});

	public void TestRowColor() => CombineAssertions(() =>
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().Build());

		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();

		var grid = popup.FailedRuleResultsGrid;
		AssertEquals("Binding Member", "FailedRuleResults", grid.GetBindingMember());
		AssertEquals("Error", Color.Red, grid.GetCustomRowBackgroundColour(0));
		AssertEquals("Message", Color.LightBlue, grid.GetCustomRowBackgroundColour(1));
		AssertEquals("Warning", Color.LightYellow, grid.GetCustomRowBackgroundColour(2));
		AssertEquals("Unknown", Color.Gray, grid.GetCustomRowBackgroundColour(3));
	});

	[RequiresSTA]
	public void TestFailedRuleResultsGridColumns()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Error).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Message).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().WithSeverity(ProcessTemplateValidationSeverityList.Codes.Warning).Build());
		failure.FailedRuleResults.Add(NonPersistentRuleValidationResultBuilder.Fail().Build());

		using var popup = new ValidationToolFailurePopup(failure);
		popup.Show();

		var grid = popup.FailedRuleResultsGrid;
		var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
		AssertContainsExactElementsInAnyOrder(new [] { "Rule", "Severity", "Message" }, columnNames);
	}

	protected override Form GetFormToBashCore()
	{
		var failure = new NonPersistentValidationFailure(Factory, "SAV", "On Save");
		var popup = new ValidationToolFailurePopup(failure);
		return popup;
	}
}
