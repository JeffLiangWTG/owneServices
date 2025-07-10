using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(GoodsRegistrationNumberGeneratorObject))]
sealed class GoodsRegistrationNumberGeneratorObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructorParams() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When parent is null", () => new GoodsRegistrationNumberGeneratorObject(null, Factory));
		AssertExceptionThrown<NullReferenceException>("When factory is null", () => new GoodsRegistrationNumberGeneratorObject(Factory.New<CusTempStorageRegHeader>(), null));
	});

	public void TestLookupsType() => AssertType<GoodsRegistrationNumberGeneratorObjectLookups>(goodsNumberGenerator.Lookups);

	public void TestValidationType() => AssertType<GoodsRegistrationNumberGeneratorObjectValidation>(goodsNumberGenerator.Validation);

	[TestDate(2025, 01, 01)]
	public void TestSetDefaultValues()
	{
		AssertEquals("GoodsRegistrationDate", ZDateTime.Today, goodsNumberGenerator.GoodsRegistrationDate);
		AssertEquals("WarehouseAuthorisationId", "09123", goodsNumberGenerator.WarehouseAuthorisationId);
	}

	public void TestGoodsRegistrationDate_Attributes() => CombineAssertions(() =>
		AssertEntity<GoodsRegistrationNumberGeneratorObject>()
			.HasProperty(x => x.GoodsRegistrationDate)
			.WithCaption("Goods Registration Date")
			.WithFullDescription("Goods Registration Date is normally the date of Norwegian border passing for the means of transport."));

	public void TestWarehouseAuthorisationId_Attributes() => CombineAssertions(() =>
		AssertEntity<GoodsRegistrationNumberGeneratorObject>()
			.HasProperty(x => x.WarehouseAuthorisationId)
			.WithMaxLength(5)
			.WithList($"{nameof(GoodsRegistrationNumberGeneratorObject.Lookups)}.{nameof(GoodsRegistrationNumberGeneratorObjectLookups.AuthorizationsList)}")
			.WithCaption("Customs Warehouse Authorization ID")
			.WithFullDescription("A 5-digit Authorization is connected to a given Customs Warehouse address. (Code CWP in Authorization module). One company may have several Customs Warehouses (= several CWP IDs)."));

	[TestDate(2025, 01, 01)]
	public void TestSetGoodsRegistrationNumber()
	{
		goodsNumberGenerator.SetGoodsRegistrationNumber();
		AssertEquals("With default values", "2025010109123001", header.SRH_Reference);
	}

	protected override BusinessObject GetNewBusinessObject() => goodsNumberGenerator;

	protected override void SetUp()
	{
		base.SetUp();
		_ = OrgHeaderTestDataHelper.CreateCusAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP);
		header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		goodsNumberGenerator = new GoodsRegistrationNumberGeneratorObject(header, Factory);
	}
	GoodsRegistrationNumberGeneratorObject goodsNumberGenerator;
	CusTempStorageRegHeader header;
}
