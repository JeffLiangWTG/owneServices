using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.ButtonGroup;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnButtonGroupTests : BunitTestContext
{
	[Test]
	public void WithFullWidthAndNoneButtonIsStreched_ThenAllButtonsStreched()
	{
		var comp = RenderComponent<ButtonGroupWithThreeButtons>(
			parameters => parameters
				.Add(c => c.ButtonGroupFullWidth, true)
				.Add(c => c.Button1FullWidth, false)
				.Add(c => c.Button2FullWidth, false)
				.Add(c => c.Button3FullWidth, false)
		);

		Assert.That(comp.FindAll(".cwn-button-group.cwn-button-group--full-width").Count, Is.EqualTo(1));
		Assert.That(comp.FindAll(".cwn-button.cwn-button--full-width").Count, Is.EqualTo(3));
	}

	[Test]
	public void WithFullWidthAndOneButtonIsStreched_ThenOtherButtonsNotStreched()
	{
		var comp = RenderComponent<ButtonGroupWithThreeButtons>(
			parameters => parameters
				.Add(c => c.ButtonGroupFullWidth, true)
				.Add(c => c.Button1FullWidth, true)
				.Add(c => c.Button2FullWidth, false)
				.Add(c => c.Button3FullWidth, false)
		);

		Assert.That(comp.FindAll(".cwn-button-group.cwn-button-group--full-width").Count, Is.EqualTo(1));
		var buttonComps = comp.FindAll(".cwn-button");
		Assert.That(buttonComps[0].ClassList, Does.Contain("cwn-button--full-width"));
		Assert.That(buttonComps[1].ClassList, Does.Not.Contain("cwn-button--full-width"));
		Assert.That(buttonComps[2].ClassList, Does.Not.Contain("cwn-button--full-width"));
	}

	[Test]
	public void WithFullWidth_WhenButtonWithFullWidthIsRemoved_ThenOtherButtonsAreStreched()
	{
		// Arrange

		var comp = RenderComponent<ButtonGroupWithThreeButtons>(
			parameters => parameters
				.Add(c => c.ButtonGroupFullWidth, true)
				.Add(c => c.Button1FullWidth, true)
				.Add(c => c.Button2FullWidth, false)
				.Add(c => c.Button3FullWidth, false)
		);

		comp.SetParam(c => c.Button1Displayed, false);

		Assert.That(comp.FindAll(".cwn-button-group.cwn-button-group--full-width").Count, Is.EqualTo(1));
		var buttonComps = comp.FindAll(".cwn-button");
		Assert.That(buttonComps.Count, Is.EqualTo(2));
		Assert.That(buttonComps[0].ClassList, Does.Contain("cwn-button--full-width"));
		Assert.That(buttonComps[1].ClassList, Does.Contain("cwn-button--full-width"));
	}
}

