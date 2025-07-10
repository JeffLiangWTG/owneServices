using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class PackagingProviderTest : Customs.Business.Testing.DataProviderTestCase<PackagingProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null ncts package", "Value cannot be null.\r\nParameter name: package", () => new PackagingProvider(99, null, false));
		AssertNoExceptionThrown("All ok", () => new PackagingProvider(99, package, false));
	});

	public void TestSequenceNumber() => AssertEquals(99, Provider.SequenceNumber);

	public void TestTypeOfPackages()
	{
		package.B5_UnitType = "UT";
		AssertEquals("UT", Provider.TypeOfPackages);
	}

	public void TestNumberOfPackages()
	{
		var bulkType = Factory.SetupBulkCusCode();
		CombineAssertions(() =>
		{
			package.B5_UnitType = bulkType;
			package.B5_UnitCount = 123;
			AssertNull("If type is in CL181 (bulk)", GetProvider().NumberOfPackages);

			package.B5_UnitType = "BX";
			AssertEquals("If type is not in CL181 (bulk)", 123, GetProvider().NumberOfPackages);
		});
	}

	public void TestShippingMarks()
	{
		package.B5_MarksAndNumbers = "TEST MARKS 123";
		AssertEquals("TEST MARKS 123", Provider.ShippingMarks);
	}

	public void TestShippingMarksMaxLength()
	{
		const int inNCTSTPPeriod = 42;
		const int outNCTSTPPeriod = 512;

		CombineAssertions(() =>
		{
			AssertEquals("TestShippingMarks max length in transition period", inNCTSTPPeriod, new PackagingProvider(99, package, true).ShippingMarksMaxLength);
			AssertEquals("TestShippingMarks max length outside transition period", outNCTSTPPeriod, GetProvider().ShippingMarksMaxLength);
		});
	}

	protected override PackagingProvider GetProvider()
	{
		return new PackagingProvider(99, package, false);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = "NC5";
		header.SetMovementType(NctsMovementType.Codes.Departure);
		package = header.Bills.AddNew().GoodsItems.AddNew().Packages.AddNew();
	}

	NctsHeader header;
	NctsPackage package;
}
