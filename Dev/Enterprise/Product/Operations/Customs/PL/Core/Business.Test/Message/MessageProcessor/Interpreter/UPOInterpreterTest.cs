using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.PL.Business.Declaration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(UPOMessageInterpreter))]
public class UPOInterpreterTest : MessageInterpreterTest<IUpo>
{
	protected IUpo CreateDataProvider()
	{
		var errors = GetRecords().ToList();
		return Mock.Of<IUpo>(x =>
			x.TransmitDocumentAbbreviated == "Transmitted document" &&
			x.TransmitDocumentType == "Transmitted document type" &&
			x.CorrelationIdentifier == "Transmitted document number" &&
			x.ExternalSystemID == "External system ID" &&
			x.NameOfIssuingSystem == "Issuing system" &&
			x.NameOfTheApplicant == "Applicant" &&
			x.IdentifierEcipSeap == "ECIP/SEAP ID" &&
			x.DateOfCreation == "Date of creation" &&
			x.DateOfCompletion == "Date of completion" &&
			x.Errors == errors);
	}

	public void TestInterpret()
	{
		var dataProvider = CreateDataProvider();
		var interpreter = CreateMessageInterpreter();

		var actualInterpretation = interpreter.Interpret(dataProvider).ToString().ToSingleLineHtml();
		var expectedInterpretation = GetExpectedInterpretation().ToSingleLineHtml();
		AssertEquals(expectedInterpretation, actualInterpretation);
	}

	protected virtual string GetExpectedInterpretation() => GlobalHtmlStyle + """
		<h2>UPO - Official Confirmation of Receipt (Transmitted document type)</h2>
		<hr />

		<table><tbody>
			<tr><th>Transmitted document</th><td>Transmitted document</td></tr>
			<tr><th>Transmitted document number</th><td>Transmitted document number</td></tr>
			<tr><th>External system ID</th><td>External system ID</td></tr>
			<tr><th>ECIP/SEAP ID</th><td>ECIP/SEAP ID</td></tr>
			<tr><th>Applicant</th><td>Applicant</td></tr>
			<tr><th>Issuing system</th><td>Issuing system</td></tr>
			<tr><th>Date of creation</th><td>Date of creation</td></tr>
			<tr><th>Date of completion</th><td>Date of completion</td></tr>
		</tbody></table>
		<hr />

		<h3 style="margin-bottom:5px;">Error</h3>
		<h4 style="margin-top:8px;margin-bottom:5px;">Location</h4>
		//Error/XPath
		<h4 style="margin-top:10px;margin-bottom:3px;">Problem</h4>
		<h5 style="margin-top:5px;margin-bottom:2px;">PL</h5>Polish text<br />
		<h5 style="margin-top:5px;margin-bottom:2px;">EN</h5>English text
		""";

	protected virtual IMessageInterpreter<IUpo> CreateMessageInterpreter()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		return new UPOMessageInterpreter(entryHeader);
	}

	protected virtual IEnumerable<IUpoError> GetRecords()
	{
		ITextInLanguage[] errorTexts = [
			Mock.Of<ITextInLanguage>(x => x.Language == "PL" && x.Text == "Polish text"),
			Mock.Of<ITextInLanguage>(x => x.Language == "EN" && x.Text == "English text"),
		];
		IReadOnlyCollection<string> xPaths = ["//Error/XPath"];
		yield return Mock.Of<IUpoError>(x =>
			x.Kind == "Kind" &&
			x.XPathPointers == xPaths &&
			x.ErrorTexts == errorTexts);
	}
}
