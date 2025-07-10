using Bunit;
namespace CargoWiseNext.Blazor.Components.Test;

public class CwnTaskStatusTest : BunitTestContext
{
	[Test]
	public void CwnTaskStatus_RenderWithoutStatusPassedTest()
	{
		// Act
		var cut = RenderComponent<CwnTaskStatus>(p => p.AddChildContent("Status Text"));

		// Assert
		cut.MarkupMatches(@"
			<div class=""cwn-chip cwn-chip--outlined cwn-neutral""><span class=""cwn-icon cwn-icon--cancelled cwn-neutral cwn-icon--small"" aria-hidden=""true"" role=""img""></span><span class=""cwn-chip--outlined__content"">Status Text</span></div> ");
	}

	[TestCase("WRK", "cwn-icon--working")]
	[TestCase("SUS", "cwn-icon--suspended")]
	public void CwnTaskStatus_RenderStatusIconTest(string statusPassed, string expectedIconClass)
	{
		//Act
		var cut = RenderComponent<CwnTaskStatus>(parameters => parameters
			.Add(p => p.Status, statusPassed)
			.AddChildContent("Status Text")
		);

		var iconElement = cut.FindAll($".{expectedIconClass}");

		//Assert
		Assert.That(iconElement, Has.Count.EqualTo(1));
	}

	[TestCase("WRK", "cwn-success")]
	[TestCase("SUS", "cwn-warning")]
	public void CwnTaskStatus_RenderStatusColorTest(string statusPassed, string expectedColorClass)
	{
		//Act
		var cut = RenderComponent<CwnTaskStatus>(parameters => parameters
			.Add(p => p.Status, statusPassed)
			.AddChildContent("Status Text")
		);

		var colorElement = cut.FindAll($".{expectedColorClass}");

		//Assert
		Assert.That(colorElement, Has.Count.EqualTo(2));
	}

	//other than WRK, SUS status should render Cancelled icon with default color
	[TestCase("XYZ", "cwn-icon--cancelled", "cwn-neutral")]
	[TestCase("CAN", "cwn-icon--cancelled", "cwn-neutral")]
	public void CwnTaskStatus_RenderDefaultIcon_DefaultColor(string statusPassed, string expectedIconClass, string expectedColorClass)
	{
		//Act
		var cut = RenderComponent<CwnTaskStatus>(parameters => parameters
			.Add(p => p.Status, statusPassed)
			.AddChildContent(statusPassed)
		);

		var iconElement = cut.FindAll($".{expectedIconClass}");
		var colorElement = cut.FindAll($".{expectedColorClass}");

		//Assert
		Assert.That(iconElement, Has.Count.EqualTo(1));
		Assert.That(colorElement, Has.Count.EqualTo(2));
	}
}
