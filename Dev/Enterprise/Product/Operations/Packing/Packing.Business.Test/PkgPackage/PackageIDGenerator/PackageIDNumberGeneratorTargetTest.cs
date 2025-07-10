using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageIDNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(PackingRegistry.Instance.PackageIDCustomisation, "TEST");

			var target = new PackageIDNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the PackageIdCustomisation", "TEST", target.NumberCustomisation);
			AssertLocation(PackingRegistry.Instance.PackageIDCustomisation, target.NumberCustomisationLocation);
			AssertEquals(PkgPackageHeaderSchema.KPH_PackageID.MaxLength, target.MaxLength);
			AssertEquals("Package ID", target.Name);

			AssertCustomisation("Should find the Package ID Number Customisation", "TEST", target.NumberCustomisation);
			AssertEquals(46, target.MaxLength);
		}
	}
}
