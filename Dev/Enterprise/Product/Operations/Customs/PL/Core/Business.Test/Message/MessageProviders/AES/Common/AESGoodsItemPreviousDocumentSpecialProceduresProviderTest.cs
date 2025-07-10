using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESGoodsItemPreviousDocumentSpecialProceduresProviderTest : DataProviderTestCase<AESGoodsItemPreviousDocumentSpecialProceduresProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusSupportingInfo", "Value cannot be null.\r\nParameter name: cusSupportingInfo",
			() => new AESGoodsItemPreviousDocumentSpecialProceduresProvider(null));
	}

	public void TestGoodsItemNumber()
	{
		AssertEquals("Goods Item Number", 111, Provider.GoodsItemNumber);
	}

	public void TestTypeOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CSI_Quantity2 and CSI_UnitOfQuantity2 are set", Core.Constants.PkgUnit.Box, Provider.TypeOfPackages);

			cusSupportingInfo.CSI_PackType = ZString.Empty;
			AssertNull("Empty CSI_UnitOfQuantity2", GetProvider().TypeOfPackages);

			cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
			cusSupportingInfo.CSI_PackQty = 0;
			AssertNull("CSI_Quantity2 is 0", GetProvider().TypeOfPackages);

			cusSupportingInfo.CSI_PackQty = -2;
			AssertNull("CSI_Quantity2 is negative", GetProvider().TypeOfPackages);
		});
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CSI_Quantity2 and CSI_UnitOfQuantity2 are set", 222, Provider.NumberOfPackages);

			cusSupportingInfo.CSI_PackType = ZString.Empty;
			AssertNull("Empty CSI_UnitOfQuantity2", GetProvider().NumberOfPackages);

			cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
			cusSupportingInfo.CSI_PackQty = 0;
			AssertNull("CSI_Quantity2 is 0", GetProvider().NumberOfPackages);

			cusSupportingInfo.CSI_PackQty = -2;
			AssertNull("CSI_Quantity2 is negative", GetProvider().TypeOfPackages);
		});
	}

	public void TestMeasurementUnitAndQualifier()
	{
		AssertEquals("Measurement Unit", "KG", Provider.MeasurementUnitAndQualifier);
	}

	public void TestQuantityValue()
	{
		AssertEquals("Quantity Value", 333.33M, Provider.QuantityValue);
	}

	public void TestType()
	{
		AssertEquals("Type is not empty", "ABC", Provider.Type);
	}

	public void TestDescription()
	{
		AssertEquals("Description is not empty", "123", Provider.Description);
	}

	public void TestGoodsShipmentNumber()
	{
		CombineAssertions(() =>
		{
			cusSupportingInfo.CSI_LineNo = 1;
			AssertEquals("CSI_LineNo is > 0", 543, GetProvider().GoodsShipmentNumber);

			cusSupportingInfo.CSI_LineNo = 0;
			AssertNull("CSI_LineNo is not > 0", Provider.GoodsShipmentNumber);
		});
	}

	protected override AESGoodsItemPreviousDocumentSpecialProceduresProvider GetProvider() => new AESGoodsItemPreviousDocumentSpecialProceduresProvider(cusSupportingInfo);

	protected override void SetUp()
	{
		base.SetUp();
		cusSupportingInfo = Factory.New<CusSupportingInfo>();
		cusSupportingInfo.CSI_Code = "ABC";
		cusSupportingInfo.CSI_ReferenceNumber = "123";
		cusSupportingInfo.CSI_ReferenceNumber2 = "543";
		cusSupportingInfo.CSI_LineNo = 111;
		cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
		cusSupportingInfo.CSI_PackQty = 222;
		cusSupportingInfo.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
		cusSupportingInfo.CSI_Quantity = 333.33m;
	}

	CusSupportingInfo cusSupportingInfo;
}
