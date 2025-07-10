using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.ToolBar;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnToolBarTests : BunitTestContext
{
	[Test]
	public void ToolBarWrapContentTest()
	{
		var component = RenderComponent<ToolBarWrapContentTest>();
		var wtgToolBar = component.Find(".cwn-toolbar");

		Assert.That(wtgToolBar.ClassList, Does.Contain("cwn-toolbar--wrap-content"));
	}

	/// <summary>
	/// ToolBar's WrapContent should be false by default
	/// </summary>
	[Test]
	public void ToolBar_WrapContent_ShouldBeFalseByDefault()
	{
		var comp = RenderComponent<CwnToolBar>();
		Assert.That(comp.Instance.WrapContent, Is.False);
	}
}
