
namespace CargoWiseNext.Blazor.Components.Test;

public class EnumExtensionsTest
{
	[TestCaseSource(nameof(TestCasesForGetDescription))]
	public void GetDescriptionTest((Enum input, string expected) testCase)
	{
		// Arrange
		var (input, expected) = testCase;

		// Act
		var actual = input.GetDescription();

		// Assert
		Assert.That(actual, Is.EqualTo(expected));
	}

	static readonly (Enum input, string expected)[] TestCasesForGetDescription = {
		(Color.Warning, "warning"),
		(Align.Center, "center"),
		(ElementType.button, "button")
	};
}
