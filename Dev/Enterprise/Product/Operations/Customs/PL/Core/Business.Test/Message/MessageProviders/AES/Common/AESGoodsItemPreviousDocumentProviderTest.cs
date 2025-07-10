using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

class AESGoodsItemPreviousDocumentProviderTest : DataProviderTestCase<AESGoodsItemPreviousDocumentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null CusSupportingInfo", "Value cannot be null.\r\nParameter name: cusSupportingInfo",
			() => new AESGoodsItemPreviousDocumentProvider(null, null));
	}

	public void TestGoodsItemNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("should equal to CSI_LineNo", 123, GetProvider().GoodsItemNumber);

			AssertNotEquals("should not equal to CSI_ItemNumber", 111, GetProvider().GoodsItemNumber);
		});
	}

	public void TestTypeOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertNull("ProcedureCode is empty", GetProvider().TypeOfPackages);
			AssertNull("ProcedureCode is not 31", GetProvider(Constants.ProcedureCodes._61).TypeOfPackages);
			AssertEquals("ProcedureCode is 31", Core.Constants.PkgUnit.Box, GetProvider(Constants.ProcedureCodes._31).TypeOfPackages);

			cusSupportingInfo.CSI_PackType = ZString.Empty;
			AssertNull("Empty CSI_UnitOfQuantity2", GetProvider(Constants.ProcedureCodes._31).TypeOfPackages);

			cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
			cusSupportingInfo.CSI_PackQty = 0;
			AssertNull("CSI_Quantity2 is 0", GetProvider(Constants.ProcedureCodes._31).TypeOfPackages);

			cusSupportingInfo.CSI_PackQty = -2;
			AssertNull("CSI_Quantity2 is negative", GetProvider(Constants.ProcedureCodes._31).TypeOfPackages);
		});
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			AssertNull("ProcedureCode is empty", GetProvider().NumberOfPackages);
			AssertNull("ProcedureCode is not 31", GetProvider(Constants.ProcedureCodes._61).NumberOfPackages);
			AssertEquals("ProcedureCode is 31", 222, GetProvider(Constants.ProcedureCodes._31).NumberOfPackages);

			cusSupportingInfo.CSI_PackType = ZString.Empty;
			AssertNull("Empty CSI_UnitOfQuantity2", GetProvider(Constants.ProcedureCodes._31).NumberOfPackages);

			cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
			cusSupportingInfo.CSI_PackQty = 0;
			AssertNull("CSI_Quantity2 is 0", GetProvider(Constants.ProcedureCodes._31).NumberOfPackages);

			cusSupportingInfo.CSI_PackQty = -2;
			AssertNull("CSI_Quantity2 is negative", GetProvider(Constants.ProcedureCodes._31).TypeOfPackages);
		});
	}

	public void TestMeasurementUnitAndQualifier()
	{
		CombineAssertions(() =>
		{
			AssertNull("ProcedureCode is empty", GetProvider().MeasurementUnitAndQualifier);
			AssertNull("ProcedureCode is not 31", GetProvider(Constants.ProcedureCodes._61).MeasurementUnitAndQualifier);
			AssertEquals("ProcedureCode is 31", "KG", GetProvider(Constants.ProcedureCodes._31).MeasurementUnitAndQualifier);
		});
	}

	public void TestQuantityValue()
	{
		CombineAssertions(() =>
		{
			AssertNull("ProcedureCode is empty", GetProvider().QuantityValue);
			AssertNull("ProcedureCode is not 31", GetProvider(Constants.ProcedureCodes._61).QuantityValue);
			AssertEquals("ProcedureCode is 31", 333.33M, GetProvider(Constants.ProcedureCodes._31).QuantityValue);
		});
	}

	public void TestType()
	{
		AssertEquals("Type is not empty", "ABC", Provider.Type);
	}

	public void TestDescription()
	{
		AssertEquals("Description is not empty", "123", Provider.Description);
	}

	protected virtual AESGoodsItemPreviousDocumentProvider GetProvider(ZString procedureCode) => new AESGoodsItemPreviousDocumentProvider(cusSupportingInfo, procedureCode);

	protected override AESGoodsItemPreviousDocumentProvider GetProvider() => GetProvider(ZString.Empty);

	protected override void SetUp()
	{
		base.SetUp();
		cusSupportingInfo = Factory.New<CusSupportingInfo>();
		cusSupportingInfo.CSI_Code = "ABC";
		cusSupportingInfo.CSI_ReferenceNumber = "123";
		cusSupportingInfo.CSI_ItemNumber = 111;
		cusSupportingInfo.CSI_LineNo = 123;
		cusSupportingInfo.CSI_PackType = Core.Constants.PkgUnit.Box;
		cusSupportingInfo.CSI_PackQty = 222;
		cusSupportingInfo.CSI_UnitOfQuantity = Core.Constants.Weight.Kilograms;
		cusSupportingInfo.CSI_Quantity = 333.33m;
	}

	protected CusSupportingInfo cusSupportingInfo;
}
