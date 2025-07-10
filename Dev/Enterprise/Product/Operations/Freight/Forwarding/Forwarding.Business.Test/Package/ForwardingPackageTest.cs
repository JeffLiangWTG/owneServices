using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingPackage))]
	public class ForwardingPackageTest : PkgPackageTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var packageJob = Factory.New<ForwardingPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return packageJob.Packages.AddNew();
		}

		public override void TestScreeningMethod()
		{
			var package = Factory.New<ForwardingPackage>();
			package.SetScreeningMethod("CMD");
			AssertEquals("CMD", package.ScreeningMethod);
		}

		public override void TestIsHighRisk()
		{
			var package = Factory.New<ForwardingPackage>();
			package.SetIsHighRisk(ZBool.True);
			AssertEquals(ZBool.True, package.IsHighRisk);
		}

		public override void TestAdditionalScreeningMethod()
		{
			var package = Factory.New<ForwardingPackage>();
			package.SetAdditionalScreeningMethod("VCK");
			AssertEquals("VCK", package.AdditionalScreeningMethod);
		}
	}
}
