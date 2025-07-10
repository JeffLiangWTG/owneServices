using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(GoodsReferenceProvider))]
sealed class GoodsReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("NctsContainerItem constructor", () => new GoodsReferenceProvider(null, 1));
		AssertExceptionThrown<ArgumentNullException>("NctsCommonCargoDesc constructor", () => new GoodsReferenceProvider(null, (ZInt)1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, provider.SequenceNumeric);
	}

	public void TestGoodsItemNumericValue_CusCodeData()
	{
		nctsContainerItemNumber.CY_DataNumeric = 123;
		AssertEquals(123, provider.GoodsItemNumericValue);
	}

	public void TestGoodsItemNumericValue_NctsDepartureCargoDesc()
	{
		var bill = header.Bills.AddNew();
		var cargoDesc = bill.GoodsItems.AddNew();
		cargoDesc.BY_DeclarationGoodsItemNumber = 987;
		provider = new GoodsReferenceProvider(cargoDesc, 1);

		AssertEquals(987, provider.GoodsItemNumericValue);
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);

		nctsContainerItemNumber = header.EnRouteIncidents.AddNew().IncidentContainers.AddNew().ItemNumbers.AddNew();
		provider = new GoodsReferenceProvider(nctsContainerItemNumber, 1);
	}

	GoodsReferenceProvider provider;
	NctsContainerItem nctsContainerItemNumber;
	NctsHeader header;

	protected override GoodsReferenceProvider GetProvider() => provider;
}
