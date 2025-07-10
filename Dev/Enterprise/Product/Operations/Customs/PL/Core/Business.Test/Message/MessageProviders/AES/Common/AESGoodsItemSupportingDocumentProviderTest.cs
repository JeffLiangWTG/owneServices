using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

class AESGoodsItemSupportingDocumentProviderTest : DataProviderTestCase<AESGoodsItemSupportingDocumentProvider>
{
	public void TestConstructor()
	{
		var stub = cusSupportingInfo = Factory.New<CusSupportingInfo>();
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusSupportingInfo", "Value cannot be null.\r\nParameter name: cusSupportingInfo",
				() => new AESGoodsItemSupportingDocumentProvider(null, () => 0m, () => 0m));
			AssertNoExceptionThrown("Null getAmount", () => new AESGoodsItemSupportingDocumentProvider(stub, null, () => 0m));
			AssertNoExceptionThrown("Null getQuantity", () => new AESGoodsItemSupportingDocumentProvider(stub, () => 0m, null));
		});
	}

	public void TestDocumentLineItemNumber()
	{
		AssertEquals("Goods Item Number", 111, Provider.DocumentLineItemNumber);
	}

	public void TestIssuingAuthorityName()
	{
		AssertEquals("Type Of Packages", "XYZ", Provider.IssuingAuthorityName);
	}

	[TestDate(2022, 5, 22)]
	public void TestValidityDateValue()
	{
		AssertEquals("Validity Date", new DateTime(2022, 5, 22), Provider.ValidityDateValue);
	}

	public void TestMeasurementUnitAndQualifier()
	{
		AssertEquals("Measurement Unit", "KG", Provider.MeasurementUnitAndQualifier);
	}

	public void TestQuantityValue()
	{
		AssertEquals("Quantity Value", 333.33M, Provider.QuantityValue);
	}

	public void TestQuantityValueFromGetQuantity()
	{
		var expectedQuantity = 111.33m;
		var provider = GetProvider(() => 0M, () => expectedQuantity);
		AssertEquals("getQuantity Value", expectedQuantity, provider.QuantityValue);
	}

	public void TestCurrency()
	{
		AssertEquals("Currency", "EUR", Provider.Currency);
	}

	public void TestAmountValue()
	{
		AssertEquals("Amount", 444.44M, Provider.AmountValue);
	}

	public void TestAmountValueFromGetAmount()
	{
		var expectedAmount = 111.44m;
		var provider = GetProvider(() => expectedAmount, () => 0m);
		AssertEquals("getAmount", expectedAmount, provider.AmountValue);
	}

	public void TestType()
	{
		AssertEquals("Type", "ABC", Provider.Type);
	}

	public void TestDescription()
	{
		AssertEquals("Description", "123", Provider.Description);
	}

	protected override AESGoodsItemSupportingDocumentProvider GetProvider() => GetProvider(null, null);

	protected virtual AESGoodsItemSupportingDocumentProvider GetProvider(Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity) =>
		new AESGoodsItemSupportingDocumentProvider(cusSupportingInfo, getAmount, getQuantity);

	protected override void SetUp()
	{
		base.SetUp();
		cusSupportingInfo = Factory.New<CusSupportingInfo>();
		cusSupportingInfo.CSI_Code = "ABC";
		cusSupportingInfo.CSI_ReferenceNumber = "123";
		cusSupportingInfo.CSI_ItemNumber = 111;
		cusSupportingInfo.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
		cusSupportingInfo.CSI_Quantity = 333.33M;
		cusSupportingInfo.CSI_AdditionalDescription = "XYZ";
		cusSupportingInfo.CSI_DateOfExpiry = ZDateTime.Today;
		cusSupportingInfo.CSI_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
		cusSupportingInfo.CSI_Value = 444.44M;
	}

	protected CusSupportingInfo cusSupportingInfo;
}
