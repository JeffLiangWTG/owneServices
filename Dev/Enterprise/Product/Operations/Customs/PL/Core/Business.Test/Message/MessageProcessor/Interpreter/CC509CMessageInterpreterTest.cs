using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC509CMessageInterpreter))]
sealed class CC509CMessageInterpreterTest : MessageInterpreterTest<ICC509C>
{
	public void TestInterpret()
	{
		const string testLrn = "TST_LRN";
		const string testMrn = "TST_MRN";
		const string testOffice = "TEST_OFFICE";
		const string expectedInterpretation = ExtendedGlobalHtmlStyle +
			"<h2>CC509C - Export cancellation decision</h2><hr />" +
			"<table class=\"no-border bold-font\">" +
				"<tbody>" +
					$"<tr><th>LRN</th><td>: {testLrn}</td></tr>" +
					$"<tr><th>MRN</th><td>: {testMrn}</td></tr>" +
					"<tr><th>Declaration is invalidated</th><td>: </td></tr>" +
					$"<tr><th>Customs Office of Export</th><td>: {testOffice}</td></tr>" +
					"<tr><th>Invalidation applies to the application submitting by the Declarant</th><td></td></tr>" +
					"<tr><th>Invalidation justification</th><td>: </td></tr>" +
				"</tbody>" +
			"</table>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC509CMessageInterpreter(entryHeader);

		string interpretation = interpreter.Interpret(Mock.Of<ICC509C>(x =>
			x.LRN == testLrn &&
			x.MRN == testMrn &&
			x.CustomsOfficeOfExportReferenceNumber == testOffice));
		AssertEquals(expectedInterpretation, interpretation);
	}

	public void TestDeclarationIsInvalidated()
	{
		var testDateTime = DateTime.Now;
		var dataProvider = Mock.Of<ICC509C>(x =>
			x.ExportOperation == Mock.Of<ICC509CExportOperation>(o =>
				o.InvalidationDecisionDateAndTime == testDateTime));
		var expectedLine = $"<tr><th>Declaration is invalidated</th><td>: {testDateTime.ToShortDateString()}</td></tr>";

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC509CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedLine);
	}

	public void TestInvalidationInitiatedBy() => CombineAssertions(() =>
	{
		const string expectedByCustoms = "Invalidated by customs office of Export";
		const string expectedByDeclarant = "Invalidation applies to the application submitting by the Declarant";
		var exportOperation = new Mock<ICC509CExportOperation>();
		var dataProvider = Mock.Of<ICC509C>(x =>
			x.ExportOperation == exportOperation.Object);

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC509CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, expectedByDeclarant, description: "Default InitiatedBy");

		exportOperation.Setup(x => x.InvalidationInitiatedByCustoms).Returns(true);
		AssertLineExists(interpreter, dataProvider, expectedByCustoms, description: "InitiatedByCustoms is true");

		exportOperation.Setup(x => x.InvalidationInitiatedByCustoms).Returns(false);
		AssertLineExists(interpreter, dataProvider, expectedByDeclarant, description: "InitiatedByCustoms is false");
	});

	public void TestInvalidationJustification() => CombineAssertions(() =>
	{
		const string testJustification = "TEST INVALIDATION JUSTIFICATION";
		var exportOperation = new Mock<ICC509CExportOperation>();
		var dataProvider = Mock.Of<ICC509C>(x =>
			x.ExportOperation == exportOperation.Object);

		var entryHeader = Factory.New<CusEntryHeader>();
		var interpreter = new CC509CMessageInterpreter(entryHeader);

		AssertLineExists(interpreter, dataProvider, string.Empty, description: "Default justification");

		exportOperation.Setup(x => x.InvalidationJustification).Returns(testJustification);
		AssertLineExists(interpreter, dataProvider, testJustification, description: "Justification is defined");
	});
}
