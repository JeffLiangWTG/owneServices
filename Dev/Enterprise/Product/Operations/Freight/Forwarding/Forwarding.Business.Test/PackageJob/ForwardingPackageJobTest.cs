using CargoWise.EntityFramework;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackageJob))]
	public class ForwardingPackageJobTest : PkgPackageJobTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<ForwardingPackageJob>();
	}
}
