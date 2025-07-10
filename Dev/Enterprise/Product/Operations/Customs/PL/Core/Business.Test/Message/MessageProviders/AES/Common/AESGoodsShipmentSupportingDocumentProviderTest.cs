using System;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESGoodsShipmentSupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<AESGoodsShipmentSupportingDocumentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null AdditionalInfo", "Value cannot be null.\r\nParameter name: document",
			() => new AESGoodsShipmentSupportingDocumentProvider(null, 999));
	}

	public void TestSequenceNumber() => AssertEquals(999, GetProvider().SequenceNumber);

	public void TestType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", string.Empty, GetProvider().Type);

			supportingDocument.CSI_Code = "123A";
			AssertEquals("Type is not empty", "123A", GetProvider().Type);
		});
	}

	public void TestDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty declaration", string.Empty, GetProvider().Description);

			supportingDocument.CSI_ReferenceNumber = "asd123";
			AssertEquals("CSI_ReferenceNumber is not empty", "asd123", GetProvider().Description);
		});
	}

	public void TestCurrency() => AssertNull(GetProvider().Currency);

	public void TestAmountValue() => AssertNull(GetProvider().AmountValue);

	public void TestQuantityValue() => AssertNull(GetProvider().QuantityValue);

	public void TestMeasurementUnitAndQualifier() => AssertNull(GetProvider().MeasurementUnitAndQualifier);

	public void TestDocumentLineItemNumber()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty declaration", GetProvider().DocumentLineItemNumber);

			supportingDocument.CSI_ItemNumber = 123;
			AssertEquals("DocumentLineItemNumber is not empty", 123, GetProvider().DocumentLineItemNumber);
		});
	}

	public void TestIssuingAuthorityName()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty declaration", GetProvider().IssuingAuthorityName);

			supportingDocument.CSI_AdditionalDescription = "asd123";
			AssertEquals("IssuingAuthorityName is not empty", "asd123", GetProvider().IssuingAuthorityName);
		});
	}

	public void TestValidityDateValue()
	{
		CombineAssertions(() =>
		{
			AssertNull("Empty declaration", GetProvider().ValidityDateValue);

			supportingDocument.CSI_DateOfExpiry = new ZDateTime(2022, 01, 01);
			AssertEquals("CSI_DateOfExpiry is 2022.01.01", new ZDateTime(2022, 01, 01), GetProvider().ValidityDateValue);
		});
	}

	protected override AESGoodsShipmentSupportingDocumentProvider GetProvider() => new AESGoodsShipmentSupportingDocumentProvider(supportingDocument, 999);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		supportingDocument = declaration.SupportingDocuments.AddNew();
	}
	SupportingDocument supportingDocument;
}
