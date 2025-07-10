using Bunit;

namespace CargoWiseNext.Blazor.Components.Test.Components;

public class CwnDialogTest : BunitTestContext
{
	const string ExpectedScript =
"""
window.showModal = (element) => {
  element.showModal();
};

window.isOpen = (element) => {
  return element.open;
};

window.close = (element) => {
  element.close();
};
""";

	[Test]
	public async Task SnapshotDialog_ShowModal_Delegates_to_Native_JS()
	{
		var cut = RenderComponent<CwnDialog>();
		var jsInterop = JSInterop.SetupVoid("showModal", cut.Instance.DialogRef).SetVoidResult();

		await cut.Instance.ShowModalAsync();

		jsInterop.VerifyInvoke("showModal");
	}

	[Test]
	public async Task SnapshotDialog_Close_Delegates_to_Native_JS()
	{
		var cut = RenderComponent<CwnDialog>();
		var jsInterop = JSInterop.SetupVoid("close", cut.Instance.DialogRef).SetVoidResult();

		await cut.Instance.CloseAsync();

		jsInterop.VerifyInvoke("close");
	}

	[Test]
	public async Task SnapshotDialog_IsOpen_Delegates_to_Native_JS([Values(true, false)] bool expectedResult)
	{
		var cut = RenderComponent<CwnDialog>();
		JSInterop
			.Setup<bool>("isOpen", cut.Instance.DialogRef)
			.SetResult(expectedResult);

		var isOpen = await cut.Instance.IsOpen();

		Assert.That(isOpen, Is.EqualTo(expectedResult));
	}

	[Test]
	public void CwnDialog_RenderTest()
	{
		// Act
		var cut = RenderComponent<CwnDialog>();

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-dialog"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-dialog__content"), Has.Count.EqualTo(1));
			Assert.That(NormalizeWhiteSpace(cut.Find("script").TextContent), Is.EqualTo(NormalizeWhiteSpace(ExpectedScript)));
		});
	}

	[Test]
	public void CwnDialog_CustomClass()
	{
		const string customClass = "custom-class";
		// Act
		var cut = RenderComponent<CwnDialog>(parameters => parameters
			.Add(c => c.Class, customClass));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll($".cwn-dialog.{customClass}"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-dialog__content"), Has.Count.EqualTo(1));
		});
	}

	[Test]
	public void CwnDialog_CustomContentClass()
	{
		const string customClass = "custom-class";
		// Act
		var cut = RenderComponent<CwnDialog>(parameters => parameters
			.Add(c => c.ClassContent, customClass));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-dialog"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll($".cwn-dialog__content.{customClass}"), Has.Count.EqualTo(1));
		});
	}

	[Test]
	public void CwnDialog_CustomStyle()
	{
		const string customStyle = "top: 0;";
		// Act
		var cut = RenderComponent<CwnDialog>(parameters => parameters
			.Add(c => c.Style, customStyle));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.Find(".cwn-dialog").GetAttribute("style"), Is.EqualTo(customStyle));
			Assert.That(cut.FindAll(".cwn-dialog__content"), Has.Count.EqualTo(1));
		});
	}

	[Test]
	public void CwnDialog_Content()
	{
		var expectedContent = "<span>Content</span>";
		// Act
		var cut = RenderComponent<CwnDialog>(parameters => parameters
			.AddChildContent(expectedContent));

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(cut.FindAll(".cwn-dialog"), Has.Count.EqualTo(1));
			Assert.That(cut.FindAll(".cwn-dialog__content"), Has.Count.EqualTo(1));
			var content = cut.Find(".cwn-dialog__content").InnerHtml;
			Assert.That(content, Is.EqualTo(expectedContent));
		});
	}

	static string NormalizeWhiteSpace(string input) => new(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
}
