using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Packing.Business
{
	public abstract class PackageIDBaseValueSourceTest : PackingTestCaseWithFactory
	{
		#region TestJobNo

		public void TestJobNo()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			var package1 = GetNewPackageForIDGeneration(packageJob1);
			var valueProviders1 = new NumberGeneratorValueProviderCollection();
			valueProviders1.AddRange(new PackageIDValueSource(packageJob1));

			// package job with no parent
			packageJob1.KJ_JobID = "P000001";
			AssertEquals("JobNo", "P000001", valueProviders1[Keys.JobNo].GetValue(Generator, ""));

			// package job with parent
			Data.CreatePackingData();
			Data.Dummy.JobNoForPackingParent = "D00000123";

			var packageJob2 = Data.PackageJob;
			var package2 = GetNewPackageForIDGeneration(packageJob2);

			var valueProviders2 = new NumberGeneratorValueProviderCollection();
			valueProviders2.AddRange(new PackageIDValueSource(packageJob2));

			AssertEquals("JobNo", "D00000123", valueProviders2[Keys.JobNo].GetValue(Generator, ""));
		}

		#endregion

		#region Implementation

		protected abstract ISupportPackageIDGeneration GetNewPackageForIDGeneration(PkgPackageJob packageJob);

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}

		NumberGenerator generator;

		#endregion
	}

	public class PackageIDValueSourceTest : PackageIDBaseValueSourceTest
	{
		protected override ISupportPackageIDGeneration GetNewPackageForIDGeneration(PkgPackageJob packageJob)
		{
			return packageJob.Packages.AddNew("PLT");
		}
	}

	public class PackageHeaderIDValueSourceTest : PackageIDBaseValueSourceTest
	{
		protected override ISupportPackageIDGeneration GetNewPackageForIDGeneration(PkgPackageJob packageJob)
		{
			var pkgHeader = packageJob.LoosePackageIDs.AddNew();
			pkgHeader.CurrentPackageJob = packageJob;
			return pkgHeader;
		}
	}
}
