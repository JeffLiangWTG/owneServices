namespace Enterprise.Packing.Business.Testing
{
	class PackageDataChangeEventArgsTest : PackingTestCaseWithFactory
	{
		public void TestConstructor()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			AssertEquals(package, new PackageDataChangeEventArgs(package, PackageDataChangeType.PackageContent).Package);
			AssertEquals(PackageDataChangeType.PackageContent, new PackageDataChangeEventArgs(package, PackageDataChangeType.PackageContent).DataChangeType);
		}
	}
}
