using Bunit;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnChipTest : BunitTestContext
{
	[Test]
	public void CwnChip_RenderWithoutIconTest()
	{
		// Act
		var cut = RenderComponent<CwnChip>(p => p.AddChildContent("Chip Text"));

		// Assert
		cut.MarkupMatches(@"
			<div
				class=""
					cwn-chip
					cwn-chip--outlined
				"">
				<span class=""cwn-chip--outlined__content"">
				Chip Text
				</span>
			</div>");
	}

	[TestCase(Icon.ReadyToStart, "cwn-icon--ready-to-start")]
	[TestCase(Icon.Working, "cwn-icon--working")]
	[TestCase(Icon.Suspended, "cwn-icon--suspended")]
	public void CwnChip_RenderWithIconTest(Icon icon, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnChip>(parameters => parameters
			.Add(p => p.Icon, icon)
			.AddChildContent("Chip Text")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}

	[TestCase(Color.Primary, "cwn-primary")]
	[TestCase(Color.Brand, "cwn-brand")]
	[TestCase(Color.Info, "cwn-info")]
	[TestCase(Color.Success, "cwn-success")]
	[TestCase(Color.Warning, "cwn-warning")]
	[TestCase(Color.Error, "cwn-error")]
	public void CwnChip_RenderWithIconColorTest(Color iconColor, string expectedClass)
	{
		// Act
		var cut = RenderComponent<CwnChip>(parameters => parameters
			.Add(p => p.Icon, Icon.Settings)
			.Add(p => p.IconColor, iconColor)
			.AddChildContent("Chip Text")
		);

		var elements = cut.FindAll($".{expectedClass}");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}

	[Test]
	public void CwnChip_DefaultIconColorTest()
	{
		// Act
		var cut = RenderComponent<CwnChip>(parameters => parameters
			.Add(p => p.Icon, Icon.Settings)
			.AddChildContent("Chip Text")
		);

		var elements = cut.FindAll(".cwn-neutral");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}

	[Test]
	public void CwnChip_IconSizeTest()
	{
		// Act
		var cut = RenderComponent<CwnChip>(parameters => parameters
			.Add(p => p.Icon, Icon.Settings)
			.AddChildContent("Chip Text")
		);

		var elements = cut.FindAll(".cwn-icon--small");

		// Assert
		Assert.That(elements, Has.Count.EqualTo(1));
	}
}
