using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(PackagingProvider))]
sealed class PackagingProviderTest : Customs.Business.Testing.DataProviderTestCase<PackagingProvider>
{
	public void TestTypeOfPackages()
	{
		package.B5_UnitType = "PAK";
		AssertEquals("PAK", Provider.TypeOfPackages);
	}

	public void TestNumberOfPackages()
	{
		CombineAssertions(() =>
		{
			package.B5_UnitType = "XX";
			package.B5_UnitCount = 10;
			AssertEquals("B5_UnitType isn't unpacked and B5_UnitCount isn't 0", 10, Provider.NumberOfPackages);
			package.B5_UnitCount = 0;
			AssertEquals("B5_UnitType isn't unpacked and B5_UnitCount is 0", 0, Provider.NumberOfPackages);
			package.B5_UnitType = "NE";
			AssertNull("B5_UnitType is NE and B5_UnitCount is 0", Provider.NumberOfPackages);
			package.B5_UnitType = "NF";
			AssertNull("B5_UnitType is NF and B5_UnitCount is 0", Provider.NumberOfPackages);
			package.B5_UnitType = "NG";
			AssertNull("B5_UnitType is NG and B5_UnitCount is 0", Provider.NumberOfPackages);
			package.B5_UnitCount = 10;
			AssertEquals("B5_UnitType is unpacked and B5_UnitCount isn't 0", 10, Provider.NumberOfPackages);
		});
	}

	public void TestShippingMarks()
	{
		package.B5_MarksAndNumbers = "shippingmarks";
		AssertEquals("shippingmarks", Provider.ShippingMarks);
	}

	public void TestSequenceNumeric()
	{
		package.B5_SequenceNumber = 2;
		AssertEquals(2, Provider.SequenceNumeric);
	}

	protected override void SetUp()
	{
		base.SetUp();

		package = Factory.New<NctsPackage>();
		package.B5_UnitType = "NE";

		provider = new PackagingProvider(package, 2);
	}
	PackagingProvider provider;
	NctsPackage package;

	protected override PackagingProvider GetProvider() => provider;
}
