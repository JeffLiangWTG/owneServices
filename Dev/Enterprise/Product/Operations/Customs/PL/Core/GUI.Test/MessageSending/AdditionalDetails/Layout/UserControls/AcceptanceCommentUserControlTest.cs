using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class AcceptanceCommentControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new AcceptanceCommentUserControl())
		{
			AssertEquals(typeof(BaseMessageSendingObject), control.DataSourceType);
		}
	}

	public void TestControls()
	{
		var expectedControlsAmount = 2;
		using (var control = new AcceptanceCommentUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals($"AcceptanceCommentUserControl should countain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
				AssertNotNull("AcceptanceCommentLabel", control.FindSingle<ZLabel>("AcceptanceCommentLabel"));
				AssertNotNull("AcceptanceCommentTextBox", control.FindSingle<ZTextBox>("AcceptanceCommentTextBox"));
			});
		}
	}

	public void TestAcceptanceCommentLabel()
	{
		using (var control = new AcceptanceCommentUserControl())
		{
			var label = control.FindSingleOrDefault<ZLabel>("AcceptanceCommentLabel");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, label.Visible);
				AssertEquals("Caption", "Acceptance Comment", label.CaptionResourceString.Caption);
			});
		}
	}

	public void TestAcceptanceCommentTextBox()
	{
		using (var control = new AcceptanceCommentUserControl())
		{
			var textBox = control.FindSingleOrDefault<ZTextBox>("AcceptanceCommentTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, textBox.Visible);
				AssertEquals("BindingMember", nameof(BaseMessageSendingObject.AcceptanceComment), textBox.GetBindingMember());
			});
		}
	}
}
