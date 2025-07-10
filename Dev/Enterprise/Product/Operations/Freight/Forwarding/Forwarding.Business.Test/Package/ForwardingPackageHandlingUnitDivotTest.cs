using CargoWise.EntityFramework;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackageHandlingUnitDivot))]
	public class ForwardingPackageHandlingUnitDivotTest : PkgPackageHandlingUnitDivotTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<ForwardingPackageHandlingUnitDivot>();
	}
}
