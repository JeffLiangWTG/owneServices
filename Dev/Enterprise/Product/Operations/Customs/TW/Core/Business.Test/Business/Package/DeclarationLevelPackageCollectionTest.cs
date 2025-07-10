using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DeclarationLevelPackageCollection))]
	sealed class DeclarationLevelPackageCollectionTest : Customs.Business.Testing.BaseDeclarationLevelPackageCollectionTest
	{
		public void TestSetDefaultsForFirstPackage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "PLT";
			var package1 = declaration.Packages.AddNew();
			AssertEquals("PLT", package1.CW_PackType);
			AssertEquals("KG", package1.CW_NetWeightUQ);
			AssertEquals("KG", package1.CW_GrossWeightUQ);
			AssertEquals(ZString.Empty, package1.CW_VolumeUQ);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DeclarationLevelPackageCollection(Declaration);
		}

		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}

		new JobDeclaration Declaration
		{
			get
			{
				return (JobDeclaration)base.Declaration;
			}
		}
	}
}
