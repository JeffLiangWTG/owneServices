namespace Enterprise.Packing.Business.Testing
{
	public class AutoPrintingEventArgsTest : PackingTestCaseWithFactory
	{
		public void TestAutoPrintingEventArgs()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();

			var e = new AutoPrintingEventArgs(package);
			AssertEquals(package, e.Package);
		}
	}
}
