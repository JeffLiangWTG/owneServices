using System;

namespace Enterprise.Packing.Business.Testing
{
	class PackageEventArgsTest : PackingTestCaseWithFactory
	{
		public void TestConstructor()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package, new PackageEventArgs(package).Package);
			AssertExceptionThrown(typeof(ArgumentNullException), () => new PackageEventArgs(null));
		}
	}
}
