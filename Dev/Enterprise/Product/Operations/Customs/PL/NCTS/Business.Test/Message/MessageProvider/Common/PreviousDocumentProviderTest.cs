using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class PreviousDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<PreviousDocumentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null CommonPreviousDocument", "Value cannot be null.\r\nParameter name: previousDocument",
			() => new PreviousDocumentProvider(99, null, false));
		AssertNoExceptionThrown("All ok", () => new PreviousDocumentProvider(99, previousDocument, false));
	});

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestGoodsItemNumber()
	{
		CombineAssertions(() =>
		{
			AssertNull("Goods item number is not assigned a value", Provider.GoodsItemNumber);

			previousDocument.CSI_ItemNumber = 1;
			AssertEquals("The value should equal with CSI_ItemNumber", "1", GetProvider().GoodsItemNumber);
		});
	}

	public void TestTypeOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Type Of packages is not assigned a value", string.Empty, Provider.TypeOfPackages);

			previousDocument.CSI_UnitOfQuantity2 = "1";
			AssertEquals("The value should equal with CSI_UnitOfQuantity2", "1", GetProvider().TypeOfPackages);
		});
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Number Of packages is not assigned a value", ZDecimal.Zero.ToString(), Provider.NumberOfPackages);

			previousDocument.CSI_Quantity2 = 2;
			AssertEquals("The value should equal with CSI_Quantity2", "2", GetProvider().NumberOfPackages);

			previousDocument.CSI_Quantity2 = 2.999;
			AssertEquals("The value should equal with CSI_Quantity2 without decimal part", "2", GetProvider().NumberOfPackages);
		});
	}

	public void TestMeasurementUnitAndQualifier()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Measurement Unit and Qualifier is not assigned a value", string.Empty, Provider.MeasurementUnitAndQualifier);

			previousDocument.CSI_UnitOfQuantity = "2";
			previousDocument.CSI_Quantity = ZDecimal.Zero;
			AssertEquals("Rule C0298 matched and the value should be empty", string.Empty, GetProvider().MeasurementUnitAndQualifier);

			previousDocument.CSI_Quantity = 1;
			AssertEquals("The value should equal with CSI_UnitOfQuantity", "2", GetProvider().MeasurementUnitAndQualifier);
		});
	}

	public void TestQuantity()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Quantity is not assigned a value", ZDecimal.Zero, Provider.Quantity);

			previousDocument.CSI_Quantity = 1m;
			AssertEquals("The value should equal with CSI_UnitOfQuantity", 1m, GetProvider().Quantity);
		});
	}

	public void TestComplementOfInformation() => AssertEquals("ReferenceNumber2", Provider.ComplementOfInformation);

	public void TestComplementOfInformationMaxLength()
	{
		const int inNCTSTPPeriod = 26;
		const int outNCTSTPPeriod = 35;

		CombineAssertions(() =>
		{
			AssertEquals("E1117 enabled and is in transition period", inNCTSTPPeriod, new PreviousDocumentProvider(99, previousDocument, true).ComplementOfInformationMaxLength);
			AssertEquals("E1117 enabled and is outside transition period", outNCTSTPPeriod, GetProvider().ComplementOfInformationMaxLength);
		});
	}

	public void TestDocumentType() => AssertEquals("123", Provider.DocumentType);

	public void TestReferenceNumber()
	{
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		CombineAssertions(() =>
		{
			AssertEquals("When the validation related to G0321 is not met", "ReferenceNumber1", GetProvider().ReferenceNumber);

			using var ruleTestContext = new CommonPreviousDocumentValidationDeciderTestContext<ICommonPreviousDocumentValidationDecider>(Factory);
			ruleTestContext.ClearCachedValidationDecider(previousDocument);
			ruleTestContext.EnableRule(decider => decider.IsRuleG0321Active);
			previousDocument.CSI_Code = "aA";
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals("When the validation related to G0321 is met", "0", GetProvider().ReferenceNumber);

			ruleTestContext.DisableRule(decider => decider.IsRuleG0321Active);
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals("When the validation related to G0321 is not met", string.Empty, GetProvider().ReferenceNumber);
		});
	}

	public void TestReferenceNumberMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber max length in transition period", inNCTSTPPeriod, new PreviousDocumentProvider(99, previousDocument, true).ReferenceNumberMaxLength);
			AssertEquals("ReferenceNumber max length outside transition period", outNCTSTPPeriod, GetProvider().ReferenceNumberMaxLength);
		});
	}

	protected override PreviousDocumentProvider GetProvider() => new PreviousDocumentProvider(99, previousDocument, false);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var nctsBill = header.Bills.AddNew();
		previousDocument = nctsBill.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "123";
		previousDocument.CSI_ReferenceNumber = "ReferenceNumber1";
		previousDocument.CSI_ReferenceNumber2 = "ReferenceNumber2";
	}
	CommonPreviousDocument previousDocument;
	NctsHeader header;
}
