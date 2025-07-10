using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.PackageJob.Testing
{
	[TestedType(typeof(ForwardingPackageCollection))]
	public class ForwardingPackageCollectionTest : PkgPackageCollectionTest
	{
		protected override PkgPackageCollection GetCollectionToTest()
		{
			var parent = Factory.New<ForwardingPackageJob>();
			return new ForwardingPackageCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingPackage>();
		}
	}
}
