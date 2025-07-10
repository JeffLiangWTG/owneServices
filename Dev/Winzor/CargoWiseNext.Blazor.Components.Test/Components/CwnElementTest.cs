using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnElementTest : BunitTestContext
{
	[TestCase("a", "<a />")]
	[TestCase("p", "<p />")]
	[TestCase("span", "<span />")]
	[TestCase("button", "<button />")]
	public void CwnElement_HtmlTagTest(string input, string expected)
	{
		// Arrange
		var htmlTag = Parameter(nameof(CwnElement.HtmlTag), input);

		// Act
		var cut = RenderComponent<CwnElement>(htmlTag);

		// Assert
		cut.MarkupMatches(expected);
	}

	[Test]
	public void CwnElement_ClassTest()
	{
		// Arrange
		var @class = Parameter(nameof(CwnElement.Class), "my-class");

		// Act
		var cut = RenderComponent<CwnElement>(@class);

		// Assert
		cut.MarkupMatches("<span class=\"my-class\" />");
	}

	[Test]
	public void CwnElement_StyleTest()
	{
		// Arrange
		var style = Parameter(nameof(CwnElement.Style), "background-color: blue;");

		// Act
		var cut = RenderComponent<CwnElement>(style);

		// Assert
		cut.MarkupMatches("<span style=\"background-color: blue;\" />");
	}

	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD101:Avoid unsupported async delegates", Justification = "Aysnc Method Required for Test")]
	public async Task SingleClickHandlerWrapperTestAsync()
	{
		bool asyncMethodInvoked = false;
		bool syncMethodInvoked = false;
		var comp1 = RenderComponent<CwnElement>(p => p.Add(p => p.OnSingleClick, (e) => { syncMethodInvoked = true; })
		.Add(p => p.Class, "test1"));
		await comp1.Find(".test1").ClickAsync(new WebMouseEventArgs() { Detail = 1 });
		Assert.That(syncMethodInvoked);

		var tcs = new TaskCompletionSource<bool>();
		var comp2 = RenderComponent<CwnElement>(p => p.Add(p => p.OnSingleClick, async (e) =>
		{
			await Task.Yield();
			asyncMethodInvoked = true;
			tcs.TrySetResult(true);
		})
.Add(p => p.Class, "test2"));

		await comp2.Find(".test2").ClickAsync(new WebMouseEventArgs() { Detail = 1 });
		await tcs.Task;
		Assert.That(asyncMethodInvoked);

		asyncMethodInvoked = false;
		syncMethodInvoked = false;
		await comp1.Find(".test1").ClickAsync(new WebMouseEventArgs() { Detail = 2 });
		Assert.That(!syncMethodInvoked);
		await comp2.Find(".test2").ClickAsync(new WebMouseEventArgs() { Detail = 2 });
		Assert.That(!asyncMethodInvoked);
	}
}
