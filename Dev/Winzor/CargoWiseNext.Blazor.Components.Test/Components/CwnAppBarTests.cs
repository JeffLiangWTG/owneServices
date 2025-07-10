using Bunit;
using static Bunit.ComponentParameterFactory;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnAppBarTests : BunitTestContext
{
	/// <summary>
	/// AppBar with modified Toolbar class
	/// </summary>
	[Test]
	public void AppBarWithModifiedToolBarClass()
	{
		var comp = RenderComponent<CwnAppBar>(Parameter(nameof(CwnAppBar.ToolBarClass), "test-class"));

		// Find the Toolbar inside the AppBar
		Assert.That(comp.Find("div").ToMarkup(), Does.Contain("test-class"));
	}

	/// <summary>
	/// AppBar with <c>Bottom</c> not set.
	/// </summary>
	[Test]
	public void AppBarWithBottomUnset()
	{
		var bar = RenderComponent<CwnAppBar>();
		Assert.That(bar.Markup, Does.StartWith("<header"));
		Assert.That(bar.Markup, Does.Contain("cwn-appbar--fixed-top"));
	}

	/// <summary>
	/// AppBar with <c>Bottom</c> set to <see langword="false" />.
	/// </summary>
	[Test]
	public void AppBarWithBottomSetFalse()
	{
		var bar = RenderComponent<CwnAppBar>(Parameter(nameof(CwnAppBar.Bottom), false));
		Assert.That(bar.Markup, Does.StartWith("<header"));
		Assert.That(bar.Markup, Does.Contain("cwn-appbar--fixed-top"));
	}

	/// <summary>
	/// AppBar with <c>Bottom</c> set to <see langword="true" />.
	/// </summary>
	[Test]
	public void AppBarWithBottomSetTrue()
	{
		var bar = RenderComponent<CwnAppBar>(Parameter(nameof(CwnAppBar.Bottom), true));
		Assert.That(bar.Markup, Does.StartWith("<footer"));
		Assert.That(bar.Markup, Does.Contain("cwn-appbar--fixed-bottom"));
	}

	/// <summary>
	/// AppBar must not set WrapContent true by default as this is not backwards compatible
	/// </summary>
	[Test]
	public void AppBar_WrapContent_ShouldBeFalseByDefault()
	{
		var comp = RenderComponent<CwnAppBar>();
		Assert.That(comp.FindComponent<CwnToolBar>().Instance.WrapContent, Is.False);
	}
}

