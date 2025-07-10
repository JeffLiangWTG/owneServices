using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackageHandlingUnitHandlingUnitDivotCollection))]
	public class ForwardingPackageHandlingUnitHandlingUnitDivotCollectionTest : PkgPackageHandlingUnitHandlingUnitDivotCollectionTest
	{
		protected override PkgPackageHandlingUnitHandlingUnitDivotCollection GetCollectionToTest()
		{
			var parent = Factory.New<ForwardingPackage>();
			return new ForwardingPackageHandlingUnitHandlingUnitDivotCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingPackageHandlingUnitDivot>();
		}
	}
}
