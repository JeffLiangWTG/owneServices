using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ExportAdditionalDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			AssertEquals(typeof(BaseMessageSendingObject), control.DataSourceType);
		}
	}

	public void TestControls()
	{
		var expectedControlsAmount = 4;
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals($"ExportAdditionalDetailsUserControl should contain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
				AssertNotNull("SecurityDropEdit", control.FindSingle<ZDropEdit>("SecurityDropEdit"));
				AssertNotNull("AmendmentInvalidationReasonUserControl", control.FindSingle<AmendmentInvalidationReasonUserControl>("AmendmentInvalidationReasonUserControl"));
				AssertNotNull("CorrectionAcceptanceDropEdit", control.FindSingle<ZDropEdit>("CorrectionAcceptanceDropEdit"));
				AssertNotNull("AcceptanceCommentTextBox", control.FindSingle<ZTextBox>("AcceptanceCommentTextBox"));
			});
		}
	}

	public void TestSecurityDropEdit()
	{
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			var dropEdit = control.FindSingleOrDefault<ZDropEdit>("SecurityDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, dropEdit.Visible);
				AssertEquals("BindingMember", nameof(BaseMessageSendingObject.Security), dropEdit.GetBindingMember());
			});
		}
	}

	public void TestAmendmentInvalidationReasonUserControl()
	{
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			var userControl = control.FindSingleOrDefault<AmendmentInvalidationReasonUserControl>("AmendmentInvalidationReasonUserControl");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, userControl.Visible);
				AssertEquals("BindingMember", ".", userControl.GetBindingMember());
			});
		}
	}

	public void TestCorrectionAcceptanceDropEdit()
	{
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			var dropEdit = control.FindSingleOrDefault<ZDropEdit>("CorrectionAcceptanceDropEdit");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, dropEdit.Visible);
				AssertEquals("BindingMember", nameof(BaseMessageSendingObject.CorrectionAcceptance), dropEdit.GetBindingMember());
			});
		}
	}

	public void TestAcceptanceCommentTextBox()
	{
		using (var control = new ExportAdditionalDetailsUserControl())
		{
			var dropEdit = control.FindSingleOrDefault<ZTextBox>("AcceptanceCommentTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, dropEdit.Visible);
				AssertEquals("BindingMember", nameof(BaseMessageSendingObject.AcceptanceComment), dropEdit.GetBindingMember());
			});
		}
	}
}
