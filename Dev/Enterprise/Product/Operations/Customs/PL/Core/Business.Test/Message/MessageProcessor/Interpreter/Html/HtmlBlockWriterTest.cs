using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class HtmlBlockWriterTest : TestCase
{
	public void TestWriteStyle()
	{
		const string expectedResult = "<style>table, th, td { border: 1px solid black; border-collapse: collapse; }</style>";

		writer.WriteStyle("table, th, td { border: 1px solid black; border-collapse: collapse; }");
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteHeader()
	{
		const string expectedResult = "<h2 class=\"TestClass\">Test caption</h2>";

		writer.WriteHeader(
			caption: "Test caption",
			@class: "TestClass");
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteHeader_OtherLevel()
	{
		const string expectedResult = "<h1>Test caption</h1>";

		writer.WriteHeader(
			caption: "Test caption",
			level: HtmlTextWriterTag.H1);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteHeader_Empty()
	{
		writer.WriteHeader("");
		AssertEquals(string.Empty, writer.ToString());
	}

	public void TestWriteThematicBreak()
	{
		const string expectedResult = "<hr />";

		writer.WriteThematicBreak();
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteLineBreak()
	{
		const string expectedResult = "<br />";

		writer.WriteLineBreak();
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable()
	{
		const string expectedResult =
"<table class=\"TestClass\">" +
	"<caption><h3>Test caption</h3></caption>" +
	"<tbody>" +
		"<tr>" +
			"<th>Param1</th>" +
			"<td>Param1 value special symbols: &lt;&gt;%@#</td>" +
		"</tr><tr>" +
			"<th>Param3</th>" +
			"<td>Test3, new line: <br /> test after new line</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValueTable(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable_SkipEmptyValuesOverridden()
	{
		const string expectedResult =
			"<table class=\"TestClass\">" +
			"<caption><h3>Test caption</h3></caption>" +
			"<tbody>" +
			"<tr>" +
			"<th>Param2</th>" +
			"<td></td>" +
			"</tr>" +
			"</tbody>" +
			"</table>";

		writer.WriteParamValueTable(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "" },
				{ "Param2", "", false },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable_NoSkipEmptyValuesOverridden()
	{
		const string expectedResult =
			"<table class=\"TestClass\">" +
			"<caption><h3>Test caption</h3></caption>" +
			"<tbody>" +
			"<tr>" +
			"<th>Param1</th>" +
			"<td></td>" +
			"</tr>" +
			"</tbody>" +
			"</table>";

		writer.WriteParamValueTable(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: false,
			paramValues: new ParamValueCollection {
				{ "Param1", "" },
				{ "Param2", "", true },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable_NoCaption()
	{
		const string expectedResult =
"<table class=\"TestClass\">" +
	"<tbody>" +
		"<tr>" +
			"<th>Param1</th>" +
			"<td>Param1 value special symbols: &lt;&gt;%@#</td>" +
		"</tr><tr>" +
			"<th>Param3</th>" +
			"<td>Test3, new line: <br /> test after new line</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValueTable(
			@class: "TestClass",
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable_NoCaptionNoClass()
	{
		const string expectedResult =
"<table>" +
	"<tbody>" +
		"<tr>" +
			"<th>Param1</th>" +
			"<td>Param1 value special symbols: &lt;&gt;%@#</td>" +
		"</tr><tr>" +
			"<th>Param3</th>" +
			"<td>Test3, new line: <br /> test after new line</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValueTable(
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTable_NoSkipEmptyValues()
	{
		const string expectedResult =
"<table>" +
	"<tbody>" +
		"<tr>" +
			"<th>Param1</th>" +
			"<td>Param1 value special symbols: &lt;&gt;%@#</td>" +
		"</tr><tr>" +
			"<th>Param2</th>" +
			"<td></td>" +
		"</tr><tr>" +
			"<th>Param3</th>" +
			"<td>Test3, new line: <br /> test after new line</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValueTable(
			skipEmptyValues: false,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose()
	{
		var valuesSample1 = new ZString[] { "value1", "value2" };
		var valuesSample2 = new ZString[] { "value3", "value4" };
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
"<table class=\"TestClass\">" +
	"<caption><h3>Test caption</h3></caption>" +
	"<tbody>" +
		"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
		"</tr><tr>" +
			"<td>Param1</td>" +
			"<td>value1</td>" +
			"<td>value2</td>" +
		"</tr><tr>" +
			"<td>Param2</td>" +
			"<td>value3</td>" +
			"<td>value4</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValuesTableTranspose(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: true,
			titleColumns: columnsTitle,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2),
			],
			rowIndex: true);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose_NoCaption()
	{
		var valuesSample1 = new ZString[] { "value1", "value2" };
		var valuesSample2 = new ZString[] { "value3", "value4" };
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
"<table class=\"TestClass\">" +
	"<tbody>" +
		"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
		"</tr><tr>" +
			"<td>Param1</td>" +
			"<td>value1</td>" +
			"<td>value2</td>" +
		"</tr><tr>" +
			"<td>Param2</td>" +
			"<td>value3</td>" +
			"<td>value4</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValuesTableTranspose(
			@class: "TestClass",
			skipEmptyValues: true,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2),
			],
			titleColumns: columnsTitle,
			rowIndex: true);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose_NoCaptionNoClass()
	{
		var valuesSample1 = new ZString[] { "value1", "value2" };
		var valuesSample2 = new ZString[] { "value3", "value4" };
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
"<table>" +
	"<tbody>" +
		"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
		"</tr><tr>" +
			"<td>Param1</td>" +
			"<td>value1</td>" +
			"<td>value2</td>" +
		"</tr><tr>" +
			"<td>Param2</td>" +
			"<td>value3</td>" +
			"<td>value4</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValuesTableTranspose(
			skipEmptyValues: true,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2),
			],
			titleColumns: columnsTitle,
			rowIndex: true);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose_NoSkipEmptyValues()
	{
		var valuesSample1 = new ZString[] { "value1", "value2" };
		var valuesSample2 = new ZString[] { "value3", "value4" };
		var valuesSampleEmpty = Array.Empty<ZString>();
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
"<table>" +
	"<tbody>" +
		"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
		"</tr><tr>" +
			"<td>Param1</td>" +
			"<td>value1</td>" +
			"<td>value2</td>" +
		"</tr><tr>" +
			"<td>Param2</td>" +
			"<td>value3</td>" +
			"<td>value4</td>" +
		"</tr>" +
	"</tbody>" +
"</table>";

		writer.WriteParamValuesTableTranspose(
			skipEmptyValues: true,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2),
				new("Param3", valuesSampleEmpty),
			],
			titleColumns: columnsTitle,
			rowIndex: true);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose_SkipEmptyValuesOverridden()
	{
		var valuesSample1 = Array.Empty<ZString>();
		var valuesSample2 = Array.Empty<ZString>();
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
			"<table class=\"TestClass\">" +
			"<caption><h3>Test caption</h3></caption>" +
			"<tbody>" +
			"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
			"</tr>" +
			"<tr>" +
			"<td>Param2</td>" +
			"</tr>" +
			"</tbody>" +
			"</table>";

		writer.WriteParamValuesTableTranspose(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: true,
			titleColumns: columnsTitle,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2, ShouldSkipEmptyValue: false),
			],
			rowIndex: true
		);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueTableTranspose_NoSkipEmptyValuesOverridden()
	{
		var valuesSample1 = Array.Empty<ZString>();
		var valuesSample2 = Array.Empty<ZString>();
		var columnsTitle = new ZString[] { "A", "B" };

		const string expectedResult =
			"<table class=\"TestClass\">" +
			"<caption><h3>Test caption</h3></caption>" +
			"<tbody>" +
			"<tr>" +
			"<th></th>" +
			"<th>A</th>" +
			"<th>B</th>" +
			"</tr>" +
			"<tr>" +
			"<td>Param1</td>" +
			"</tr>" +
			"</tbody>" +
			"</table>";

		writer.WriteParamValuesTableTranspose(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: false,
			titleColumns: columnsTitle,
			paramValuesList: [
				new("Param1", valuesSample1),
				new("Param2", valuesSample2, ShouldSkipEmptyValue: true),
			],
			rowIndex: true);
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence()
	{
		const string expectedResult =
"<p class=\"TestClass\">" +
	"<h3>Test caption</h3>" +
	"Param1: Param1 value special symbols: &lt;&gt;%@#<br />" +
	"Param3: Test3, new line: <br /> test after new line" +
"</p>";

		writer.WriteParamValueSequence(
			caption: "Test caption",
			@class: "TestClass",
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence_NoCaption()
	{
		const string expectedResult =
"<p class=\"TestClass\">" +
	"Param1: Param1 value special symbols: &lt;&gt;%@#<br />" +
	"Param3: Test3, new line: <br /> test after new line" +
"</p>";

		writer.WriteParamValueSequence(
			@class: "TestClass",
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence_NoCaptionNoClass()
	{
		const string expectedResult =
"<p>" +
	"Param1: Param1 value special symbols: &lt;&gt;%@#<br />" +
	"Param3: Test3, new line: <br /> test after new line" +
"</p>";

		writer.WriteParamValueSequence(
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence_NoSkipEmptyValues()
	{
		const string expectedResult =
"<p>" +
	"Param1: Param1 value special symbols: &lt;&gt;%@#<br />" +
	"Param2: <br />" +
	"Param3: Test3, new line: <br /> test after new line" +
"</p>";

		writer.WriteParamValueSequence(
			skipEmptyValues: false,
			paramValues: new ParamValueCollection {
				{ "Param1", "Param1 value special symbols: <>%@#" },
				{ "Param2", "" },
				{ "Param3", "Test3, new line: \r\n test after new line" },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence_SkipEmptyValuesOverridden()
	{
		const string expectedResult =
			"<p>" +
			"Param2: " +
			"</p>";

		writer.WriteParamValueSequence(
			skipEmptyValues: true,
			paramValues: new ParamValueCollection {
				{ "Param1", "" },
				{ "Param2", "", false },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteParamValueSequence_NoSkipEmptyValuesOverridden()
	{
		const string expectedResult =
			"<p>" +
			"Param1: " +
			"</p>";

		writer.WriteParamValueSequence(
			skipEmptyValues: false,
			paramValues: new ParamValueCollection {
				{ "Param1", "" },
				{ "Param2", "", true },
			});
		AssertEquals(expectedResult, writer.ToString());
	}

	public void TestWriteInParagraph() => CombineAssertions(() =>
	{
		writer.WriteInParagraph(
			attributes: [(HtmlTextWriterAttribute.Class, "TestClass")],
			writeAction: _ => writer.WriteText("Test"));
		AssertEquals("With class name", "<p class=\"TestClass\">Test</p>", writer.ToString());

		writer = new HtmlBlockWriter();
		writer.WriteInParagraph(_ => writer.WriteText("Test"));
		AssertEquals("Without class name", "<p>Test</p>", writer.ToString());
	});

	public void TestWriteMultipleTextWithCaption() => CombineAssertions(() =>
	{
		writer.WriteMultipleTextWithCaption(new ParamValueCollection {
			{ "ENG", "English text" },
			{ "CN", "" },
			{ "ES", (string)null },
			{ "PL", "Polish text" },
		});
		var expectedResult =
			"""
			<h5 style="margin-top:5px;margin-bottom:2px;">ENG</h5>English text<br />
			<h5 style="margin-top:5px;margin-bottom:2px;">PL</h5>Polish text
			""".ToSingleLineHtml();
		AssertEquals(expectedResult, writer.ToString());

		writer = new();
		writer.WriteMultipleTextWithCaption(new ParamValueCollection {
			{ "ENG", "English text" },
		});
		AssertEquals("English text", writer.ToString());
	});

	public void TestWriteText()
	{
		const string expectedResult = "Multiline<br />String<br />&lt;&gt;&amp;&quot;&#39;";

		writer.WriteText("Multiline\r\nString\n<>&\"\'");
		AssertEquals(expectedResult, writer.ToString());
	}

	protected override void SetUp()
	{
		base.SetUp();

		writer = new HtmlBlockWriter();
	}

	HtmlBlockWriter writer;
}
