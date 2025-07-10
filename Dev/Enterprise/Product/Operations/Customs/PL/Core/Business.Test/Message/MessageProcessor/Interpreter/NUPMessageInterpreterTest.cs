using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class NUPMessageInterpreterTest : UPOInterpreterTest
{
	protected override string GetExpectedInterpretation() => GlobalHtmlStyle + """
		<h2>NUP - Rejection Of Communication (Transmitted document type)</h2>
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

	protected override IMessageInterpreter<IUpo> CreateMessageInterpreter()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		return new NUPMessageInterpreter<CusEntryHeader>(entryHeader);
	}
}
