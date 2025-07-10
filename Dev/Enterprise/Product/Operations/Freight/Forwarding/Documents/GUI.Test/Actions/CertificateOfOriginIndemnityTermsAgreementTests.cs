using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.GUI.Actions;

namespace Enterprise.Freight.Forwarding.Documents.Testing.GUI.Actions
{
	sealed class CertificateOfOriginIndemnityTermsAgreementTests : TestCaseWithFactory
	{
		public void TestType()
		{
			var term = new CertificateOfOriginIndemnityTermsAgreement();
			AssertEquals("COI", term.Type);
		}

		public void TestIsCurrentUserAllowedToAcknowledgeAgreement()
		{
			var term = new CertificateOfOriginIndemnityTermsAgreement();
			AssertEquals(expected: true, term.IsCurrentUserAllowedToAcknowledgeAgreement);
		}

		public void TestErrorMessageForAcknowledgementNotAllowed()
		{
			var term = new CertificateOfOriginIndemnityTermsAgreement();
			AssertEquals(expected: null, term.ErrorMessageForAcknowledgementNotAllowed);
		}

		public void TestIsLocalDisplayConditionSatisfied()
		{
			var term = new CertificateOfOriginIndemnityTermsAgreement();
			term.ParentConditionSatisfied = false;
			var result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();
			AssertEquals(expected: false, result);
		}

		public void TestGetLocalFallbackTerm()
		{
			var term = new CertificateOfOriginIndemnityTermsAgreement();
			var result = term.IsDisplayConditionSatisfied().GetAwaiter().GetResult();
			var expectedContent = @"The Applicant (or the Applicant on behalf of the Consignor), by utilizing WiseTech Certification, certifies that:
	a)	the goods mentioned in the certificates and/or other documents originate in the country (countries) specified in the documents(s) and comply with the rules of origin applicable in the country (countries) to those goods
	b)	the information in the certificates and/or other documents provided to WiseTech Certification is accurate, true and complete
	c)	they (the applicant) will advise WiseTech Certification and any other person(s) to whom the applicant provides the Certificates and/or other documents promptly in writing of any inaccuracy, omission or change in such information, or in the origin of the goods
	d)	they (the applicant) will maintain, and present upon request, such documentation as is necessary to verify the truth, accuracy and completeness of all Certificates, and/or other documents, issued by WiseTech Certification
	e)	in consideration for WiseTech Certification’s issuance of Certificates or Origin and/or other documents, the applicant agrees to release, discharge and hold harmless WiseTech Certification from any liability in connection with the issuance of the Certificates and/or other documents and to indemnify WiseTech Certification in respect of any costs herewith
	f)	in the event of requests which stem from a legitimate enquiry from someone in possession of statutory authority e.g. Police, Department of Foreign Affairs & Trade or officials acting with authority of a Court Order, I/we hereby permit WiseTech Certification to allow direct access, under the power of statutory authority, to such commercial information as may be required as part of the enquiry
	g)	the applicant is authorized to give the undertakings set out herein";

			AssertEquals(expected: true, result);
			AssertEquals("WiseTech Certificate of Origin Indemnity User Terms", term.Title);
			AssertEquals(expectedContent, term.Contents);
			AssertEquals(0, term.VersionNo);
		}
	}
}
