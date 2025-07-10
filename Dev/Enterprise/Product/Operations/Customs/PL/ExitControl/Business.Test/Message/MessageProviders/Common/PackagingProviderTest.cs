using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

sealed class PackagingProviderTest : DataProviderTestCase<PackagingProvider>
{
	public void TestConstructor()
	{
		var expectedMessage = string.Empty;

#if NETFRAMEWORK
		expectedMessage = "Value cannot be null.\r\nParameter name: exitPackage";
#else
		expectedMessage = "Value cannot be null. (Parameter 'exitPackage')";
#endif

		AssertExceptionThrown<ArgumentNullException>("Null CusExitConsignmentPackage", expectedMessage,
			() => new PackagingProvider(null));
	}

	public void TestSequenceNumber() => AssertEquals(12, Provider.SequenceNumber);

	public void TestTypeOfPackages() => AssertEquals("B", Provider.TypeOfPackages);

	public void TestNumberOfPackages() => AssertEquals(432, Provider.NumberOfPackages);

	public void TestShippingMarks() => AssertEquals("ABCD123", Provider.ShippingMarks);

	protected override PackagingProvider GetProvider() => new PackagingProvider(package);

	protected override void SetUp()
	{
		base.SetUp();
		package = Factory.New<CusExitConsignmentPackage>();
		package.CXP_Sequence = 12;
		package.CXP_PackageType = "B";
		package.CXP_Quantity = 432;
		package.CXP_MarksAndNumbers = "ABCD123";
	}

	CusExitConsignmentPackage package;
}
