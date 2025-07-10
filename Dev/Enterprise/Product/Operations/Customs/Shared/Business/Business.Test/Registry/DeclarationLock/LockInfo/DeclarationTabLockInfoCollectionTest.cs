using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationTabLockInfoCollection))]
	sealed class DeclarationTabLockInfoCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DeclarationTabLockInfoCollection>
	{
		public void TestCustomClone()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);

			var info1 = Collection.AddNew();
			var info2 = Collection.AddNew();

			info1.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers;
			info2.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.Misc;

			var newCollection = Collection.Clone(lockConfig, fallbackLevel, Factory);

			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(DeclarationTabLockInfoCollection)).GetValue(newCollection, null));
			AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, newCollection.CurrentFallbackLevel);
			AssertEquals("Clone.LockConfig", lockConfig, newCollection.LockConfig);

			AssertEquals("Clone.Count", 2, newCollection.Count);

			AssertEquals("Info1 should be in the Clone.", Core.Constants.Customs.DeclarationTabPages.Codes.DeclarationNumbers, newCollection[0].TabPage);
			AssertEquals("Info2 should be in the Clone.", Core.Constants.Customs.DeclarationTabPages.Codes.Misc, newCollection[1].TabPage);
		}

		#region Implementation

		protected override DeclarationTabLockInfoCollection GetCollectionToTest()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);

			return new DeclarationTabLockInfoCollection(lockConfig, fallbackLevel, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeclarationTabLockInfo(null, Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
