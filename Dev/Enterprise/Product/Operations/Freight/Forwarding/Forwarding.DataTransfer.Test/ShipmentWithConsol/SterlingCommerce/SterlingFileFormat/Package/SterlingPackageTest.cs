using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingPackage))]
	public class SterlingPackageTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingPackage();
		}

		public void TestSterlingPackageRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPackageRecord1, SterlingForTest.PackageInfo[0].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPackageRecord2, SterlingForTest.PackageInfo[1].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPackageRecord3, SterlingForTest.PackageInfo[2].Record);
		}
		const string ExpectedSterlingPackageRecord1 = "PKG|PackType1|1|101|WEIGHT1|201|LENGTH1|301|WIDTH1|401|HEIGHT1>\r\n";
		const string ExpectedSterlingPackageRecord2 = "PKG|PackType2|2|102|WEIGHT2|202|LENGTH2|302|WIDTH2|402|HEIGHT2>\r\n";
		const string ExpectedSterlingPackageRecord3 = "PKG|PackType3|3|103|WEIGHT3|203|LENGTH3|303|WIDTH3|403|HEIGHT3>\r\n";
	}
}
