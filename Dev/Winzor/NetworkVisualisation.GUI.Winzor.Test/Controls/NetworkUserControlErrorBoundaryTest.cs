using CargoWise.Common;
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.ZArchitecture.Core.Testing;

namespace NetworkVisualisation.GUI.Winzor.Test.Controls;
class NetworkUserControlErrorBoundaryTest : BunitTestContext
{
	[Test]
	public void NoExceptionThrown_ShouldShowChildContent()
	{
		var cut = RenderComponent<NetworkUserControlErrorBoundary>(parameters => parameters
			.Add(p => p.ErrorContent, exception => builder => builder.AddMarkupContent(0, "<p>This is the error content</p>"))
			.Add(p => p.ChildContent, builder =>
			{
				builder.OpenComponent(0, typeof(ChildContentComponentForTest));
				builder.CloseComponent();
			})
		);

		Assert.That(cut.Markup, Does.Contain("This is the child content"));
		Assert.That(cut.Markup, Does.Not.Contain("This is the error content"));
	}

	[Test]
	public void TaskCanceledExceptionThrown_ShouldShowChildContent()
	{
		using var cancellationTokenSource = new CancellationTokenSource();
		var cut = RenderComponent<NetworkUserControlErrorBoundary>(parameters => parameters
			.Add(p => p.ErrorContent, exception => builder => builder.AddMarkupContent(0, "<p>This is the error content</p>"))
			.Add(p => p.ChildContent, builder =>
			{
				builder.OpenComponent(0, typeof(ChildContentComponentForTest));
				builder.CloseComponent();
			})
		);

		var paragraph = cut.Find("p");
		paragraph.Click();

		Assert.That(ErrorReporter.LastMessageReported, Is.EqualTo("A task was canceled."));

		Assert.That(cut.Markup, Does.Contain("This is the child content"));
		Assert.That(cut.Markup, Does.Not.Contain("This is the error content"));
	}

	[Test]
	public void ExceptionThrown_ShouldShowErrorContent()
	{
		var exception = new Exception("Error boundary please catch me!");
		var cut = RenderComponent<NetworkUserControlErrorBoundary>(parameters => parameters
			.Add(p => p.ErrorContent, exception => builder => builder.AddMarkupContent(0, "<p>This is the error content</p>"))
			.Add(p => p.ChildContent, builder =>
			{
				builder.OpenComponent(0, typeof(ChildContentComponentForTest));
				builder.AddAttribute(1, "OnAfterRenderAction", new Action(() => throw exception));
				builder.CloseComponent();
			})
		);

		Assert.That(cut.Markup, Does.Not.Contain("This is the child content"));
		Assert.That(cut.Markup, Does.Contain("This is the error content"));

		Assert.That(ExceptionReporterTestListener.Instance.GetExceptionMessage(0), Is.EqualTo("Failed to load the NCN"));
		Assert.That(ExceptionReporterTestListener.Instance.Count, Is.EqualTo(1));
		ExceptionReporterTestListener.Instance.Clear();
	}
}
