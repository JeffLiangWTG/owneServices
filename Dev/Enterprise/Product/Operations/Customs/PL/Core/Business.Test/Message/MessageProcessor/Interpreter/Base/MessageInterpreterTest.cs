using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestsSubclassesOf(typeof(IMessageInterpreter<>))]
public abstract class MessageInterpreterTest<TBaseProvider> : TestCaseWithFactory
	where TBaseProvider : class
{
	protected const string GlobalHtmlStyle =
"<style>" +
	"table, th, td { " +
		"border: 1px solid black; " +
		"border-collapse: collapse; " +
	"} " +
	"th, td { " +
		"padding: 5px; " +
		"text-align: left; " +
	"}" +
"</style>";

	protected const string ExtendedGlobalHtmlStyle =
	"<style>" +
		"body { font-family: Arial, sans-serif; text-align: left; } " +
		"caption { text-align: left; } " +
		"h2 { color:steelblue; font-weight: normal; } " +
		"table {margin-top: 2em; margin-bottom: 2em;} " +
		"table, caption, th, td { border: 1px solid black; border-collapse: collapse; padding: 3px; font-weight: normal; text-align: left; } " +
		".bold-font th { font-weight:bold; } " +
		".no-border, .no-border * { border: none; } " +
		".fixed-table { width: 100 %; } " +
		".fixed-table h3 { margin-top: 0; margin-bottom: 0em; } " +
		".fixed-table th { width: 300px; }" +
	"</style>";

	protected void AssertInterpret(IMessageInterpreter<TBaseProvider> interpreter, TBaseProvider dataProvider, string expectedResult, string description = null)
	{
		var result = interpreter.Interpret(dataProvider);
		AssertEquals(description, expectedResult, result.ToString());
	}

	protected void AssertLineExists(IMessageInterpreter<TBaseProvider> interpreter, TBaseProvider dataProvider, string expectedResult, bool isExisted = true, string description = null)
	{
		var result = interpreter.Interpret(dataProvider);
		AssertEquals(description, isExisted, result.Contains(expectedResult));
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock = new Mock<TBaseProvider>();
	}

	protected Mock<TBaseProvider> dataProviderMock;
}
