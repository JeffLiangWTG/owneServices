using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class AmendmentInvalidationReasonUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using (var control = new AmendmentInvalidationReasonUserControl())
		{
			AssertEquals(typeof(BaseMessageSendingObject), control.DataSourceType);
		}
	}

	public void TestControls()
	{
		var expectedControlsAmount = 2;
		using (var control = new AmendmentInvalidationReasonUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals($"AmendmentInvalidationReasonUserControl should countain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
				AssertNotNull("AmendmentInvalidationReasonLabel", control.FindSingle<ZLabel>("AmendmentInvalidationReasonLabel"));
				AssertNotNull("AmendmentInvalidationReasonTextBox", control.FindSingle<ZTextBox>("AmendmentInvalidationReasonTextBox"));
			});
		}
	}

	public void TestAmendmentInvalidationReasonLabel()
	{
		using (var control = new AmendmentInvalidationReasonUserControl())
		{
			var label = control.FindSingleOrDefault<ZLabel>("AmendmentInvalidationReasonLabel");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, label.Visible);
				AssertEquals("Caption", "Reason For Amendment / Invalidation", label.CaptionResourceString.Caption);
			});
		}
	}

	public void TestAmendmentInvalidationReasonTextBox()
	{
		using (var control = new AmendmentInvalidationReasonUserControl())
		{
			var textBox = control.FindSingleOrDefault<ZTextBox>("AmendmentInvalidationReasonTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("Visibility", true, textBox.Visible);
				AssertEquals("BindingMember", nameof(BaseMessageSendingObject.AmendmentInvalidationReason), textBox.GetBindingMember());
			});
		}
	}
}
