using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class SupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<SupportingDocumentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsSupportingDocument", "Value cannot be null.\r\nParameter name: nctsSupportingDocument",
			() => new SupportingDocumentProvider(99, null, false));

		AssertNoExceptionThrown("All ok", () => new SupportingDocumentProvider(99, supportingDocument, false));
	});

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestDocumentLineItemNumber() => CombineAssertions(() =>
	{
		AssertNull("Goods item number is not assigned a value", Provider.DocumentLineItemNumber);

		supportingDocument.CSI_ItemNumber = 213;
		AssertEquals("The value should equal to CSI_ItemNumber", "213", GetProvider().DocumentLineItemNumber);
	});

	public void TestComplementOfInformation() => AssertEquals("ReferenceNumber2", Provider.ComplementOfInformation);

	public void TestComplementOfInformationMaxLength()
	{
		const int inNCTSTPPeriod = 26;
		const int outNCTSTPPeriod = 35;

		CombineAssertions(() =>
		{
			AssertEquals("ComplementOfInformation max length in transition period", inNCTSTPPeriod, new SupportingDocumentProvider(99, supportingDocument, true).ComplementOfInformationMaxLength);
			AssertEquals("ComplementOfInformation max length outside transition period", outNCTSTPPeriod, GetProvider().ComplementOfInformationMaxLength);
		});
	}

	public void TestDocumentType() => AssertEquals("123", Provider.DocumentType);

	public void TestReferenceNumber()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			using var validationDeciderTestContext = new NctsSupportingDocumentValidationDeciderTestContext<INctsSupportingDocumentDeparturePhase5ValidationDecider>(Factory).ClearCachedValidationDecider(supportingDocument);
			AssertEquals("When the validation related to G0321 is not met", "ReferenceNumber1", GetProvider().ReferenceNumber);

			validationDeciderTestContext.EnableRule(decider => decider.IsRuleG0321Active);
			supportingDocument.CSI_Code = "aA";
			supportingDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals("When the validation related to G0321 is met", "0", GetProvider().ReferenceNumber);

			validationDeciderTestContext.DisableRule(decider => decider.IsRuleG0321Active);
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertEquals("When the validation related to G0321 is not met", string.Empty, GetProvider().ReferenceNumber);
		});
	}

	protected override SupportingDocumentProvider GetProvider() => new(99, supportingDocument, false);

	public void TestReferenceNumberMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber max length in transition period", inNCTSTPPeriod, new SupportingDocumentProvider(99, supportingDocument, true).ReferenceNumberMaxLength);
			AssertEquals("ReferenceNumber max length outside transition period", outNCTSTPPeriod, GetProvider().ReferenceNumberMaxLength);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		supportingDocument = header.MovementHeader.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "123";
		supportingDocument.CSI_ReferenceNumber = "ReferenceNumber1";
		supportingDocument.CSI_ReferenceNumber2 = "ReferenceNumber2";
	}
	NctsSupportingDocument supportingDocument;
	NctsHeader header;
}
