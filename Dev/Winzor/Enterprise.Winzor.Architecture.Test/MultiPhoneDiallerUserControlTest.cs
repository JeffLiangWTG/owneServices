using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.Testing.MultiPhoneDiallerUserControlTest;

namespace Enterprise.Winzor.Architecture.Test;

internal class MultiPhoneDiallerUserControlTest
{
	[TestCaseSource(nameof(ImageButtonWithoutTextInternalPaddingTextSource))]
	public async Task ImageButtonWithoutTextInternalPaddingText(PhoneDialInfo phoneDialInfo, string expectedType)
	{
		// Arrange
		using var context = new EnterpriseTestContext();
		var phoneDiallerForTest = new TestPhoneDialler();
		var phoneDiallerOverrideForTest = new TestPhoneDialler();
		var rendered = await context.RenderControlOnFormAsync((() =>
		{
			var multiPhoneDiallerUserControlForTest = new MultiPhoneDiallerUserControlForTest();
			// Act
			multiPhoneDiallerUserControlForTest.PhoneDiallerOverrideForTest = (PhoneDialler)(object)phoneDiallerOverrideForTest;
			multiPhoneDiallerUserControlForTest.DefaultDialInfo = phoneDialInfo;

			return multiPhoneDiallerUserControlForTest;
		}));
		// Assert
		Assert.That(rendered.Find("div.button__text").InnerHtml, Is.EqualTo(expectedType));
	}

	[Test]
	public async Task MultiPhoneDiallerImageButtonTextIsUpdating()
	{
		MultiPhoneDiallerUserControl.ImageButtonWithoutTextInternalPadding callButtonControl = null;

		using var context = new EnterpriseTestContext();
		var rendered = await context.RenderControlOnFormAsync((() =>
		{
			callButtonControl = new MultiPhoneDiallerUserControl.ImageButtonWithoutTextInternalPadding();
			callButtonControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			callButtonControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
			callButtonControl.ForeColor = System.Drawing.Color.BlueViolet;
			callButtonControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			callButtonControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			callButtonControl.Name = "CallButton";
			callButtonControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, 1, 0, 0, true);
			callButtonControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 30, true);
			callButtonControl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			callButtonControl.TextIgnoringInternalPadding = "Call Caption";
			return callButtonControl;
		}));

		Assert.That(rendered.Find("div.button__text").InnerHtml, Is.EqualTo("Call Caption"));

		await callButtonControl.InvokeWinzorDispatcherAsync(() => callButtonControl.TextIgnoringInternalPadding = "Updated Call Caption" );
		Assert.That(rendered.Find("div.button__text").InnerHtml, Is.EqualTo("Updated Call Caption"));
	}

	static IEnumerable<TestCaseData> ImageButtonWithoutTextInternalPaddingTextSource()
	{
		yield return new TestCaseData(null, "Call");
		yield return new TestCaseData(new PhoneDialInfo("02 11119999", "Office"), "Office");
	}
}
