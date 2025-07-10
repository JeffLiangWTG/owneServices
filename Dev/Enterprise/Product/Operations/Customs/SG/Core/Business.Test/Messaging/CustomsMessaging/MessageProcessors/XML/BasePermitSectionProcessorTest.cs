using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	abstract class BasePermitSectionProcessorTest<T> : BaseResponseSectionProcessorTest where T : ITradeNetOutPermitSection
	{
		protected void AssertPermitInfos(CusEntryHeader entryHeader, Permit permit, ZDateTime authorisationDate)
		{
			AssertEquals("EntryNumber", (ZString)permit.PermitNumber, entryHeader.EntryNumber);
			AssertEquals("CertificateNumber", (ZString)permit.CertificateNumber, entryHeader.CertificateNumber);
			AssertEquals("AuthorisationDate", authorisationDate, entryHeader.Declaration.JE_EntryAuthorisationDate);
		}

		protected void AssertEntryPayInfos(CusEntryHeader entryHeader, Header header, Permit permit, ZDateTime authorisationDate, ZDecimal totalPayable, ZString paymentParty)
		{
			var messageReferenceNumber = header.MessageReference.Replace("WTG", string.Empty);
			var payInfo = entryHeader.EntryPayInfos.GetItemByMessageNum(messageReferenceNumber);
			AssertNotNull("Should create a valid EntryPayInfo as the total value of summary is greater than 0.", payInfo);
			AssertEquals("MessageReferenceNumber", (ZString)messageReferenceNumber, payInfo.C9_IncomingPayResponseNo);
			AssertEquals("TotalPayable", totalPayable, payInfo.C9_PaymentAmount);
			AssertEquals("DocumentName", (ZString)header.DeclarationType, payInfo.C9_TransactionType);
			AssertEquals("AuthorisationDate", authorisationDate, payInfo.C9_PaymentDate);
			AssertEquals("PermitNumber", (ZString)permit.PermitNumber, payInfo.C9_PaymentReference);
			AssertEquals("PaymentParty", paymentParty, payInfo.C9_PaymentParty);
		}

		protected void AssertEmailContent(EmailDef email, Permit permit, JobDeclaration declaration, string referenceNumber, string status, string messageInterpretation)
		{
			var expectedHtml = LoadHtmlFile("CommonPermitEmailContent.htm").Replace("##JobNumber##", EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference)).Replace("##PermitNumber##", permit.PermitNumber).Replace("##CertificateNumber##", permit.CertificateNumber).Replace("##ReferenceNumber##", referenceNumber).Replace("##EntryStatus##", status);
			AssertEquals("Subject", "Customs Permit for " + declaration.JE_DeclarationReference, email.Subject);
			AssertContains("Body", expectedHtml, email.Body);
			AssertContains("Message Interpretation", expectedHtml, messageInterpretation);
		}
	}
}
