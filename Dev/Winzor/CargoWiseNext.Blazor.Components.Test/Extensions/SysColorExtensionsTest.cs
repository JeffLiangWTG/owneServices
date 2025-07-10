using SysColor = System.Drawing.Color;

namespace CargoWiseNext.Blazor.Components.Test;

public class SysColorExtensionsTest
{
	[TestCaseSource(nameof(TestCasesForRgb))]
	public void ToRgbTest((SysColor, string) testCase)
	{
		// Arrange
		var (color, expected) = testCase;

		// Act
		var actual = color.ToRgb();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	[TestCaseSource(nameof(TestCasesForRgba))]
	public void ToRgbaTest((SysColor, string) testCase)
	{
		// Arrange
		var (color, expected) = testCase;

		// Act
		var actual = color.ToRgba();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	static IEnumerable<(SysColor, string)> TestCasesForRgb()
	{
		yield return (SysColor.FromArgb(255, 213, 2, 71), "rgb(213, 2, 71)");
		yield return (SysColor.FromArgb(255, 246, 248, 255), "rgb(246, 248, 255)");
		yield return (SysColor.FromArgb(255, 0, 0, 255), "rgb(0, 0, 255)");
	}

	static IEnumerable<(SysColor, string)> TestCasesForRgba()
	{
		yield return (SysColor.FromArgb(76, 213, 2, 71), "rgba(213, 2, 71, 0.298)");
		yield return (SysColor.FromArgb(255, 246, 248, 255), "rgba(246, 248, 255, 1)");
		yield return (SysColor.FromArgb(127, 0, 0, 255), "rgba(0, 0, 255, 0.498)");
	}
}
