using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackageCollection))]
	public class ForwardingPackageCollectionTest : PkgPackageCollectionTest
	{
		protected override PkgPackageCollection GetCollectionToTest()
		{
			var parent = Factory.New<ForwardingPackLine>();
			return new ForwardingPackageCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingPackage>();
		}
	}
}
