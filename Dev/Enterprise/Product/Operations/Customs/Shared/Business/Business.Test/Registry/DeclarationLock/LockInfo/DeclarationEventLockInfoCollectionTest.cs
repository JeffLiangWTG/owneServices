using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationEventLockInfoCollection))]
	sealed class DeclarationEventLockInfoCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DeclarationEventLockInfoCollection>
	{
		public void TestCustomClone()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);

			var info1 = Collection.AddNew();
			var info2 = Collection.AddNew();

			info1.EventType = "ARV";
			info1.EventReference = "DEP=AAA";
			info1.EventSource = Constants.Customs.EventLockSourceTypes.Codes.Declaration;

			info2.EventType = "DEP";
			info2.EventReference = string.Empty;
			info2.EventSource = Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;

			var newCollection = Collection.Clone(lockConfig, fallbackLevel, Factory);

			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(DeclarationEventLockInfoCollection)).GetValue(newCollection, null));
			AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, newCollection.CurrentFallbackLevel);
			AssertEquals("Clone.LockConfig", lockConfig, newCollection.LockConfig);

			AssertEquals("Clone.Count", 2, newCollection.Count);

			AssertEquals("Info1 should be in the Clone.", "ARV", newCollection[0].EventType);
			AssertEquals("Info1 should be in the Clone.", "DEP=AAA", newCollection[0].EventReference);
			AssertEquals("Info1 should be in the Clone.", Constants.Customs.EventLockSourceTypes.Codes.Declaration, newCollection[0].EventSource);

			AssertEquals("Info2 should be in the Clone.", "DEP", newCollection[1].EventType);
			AssertEquals("Info2 should be in the Clone.", string.Empty, newCollection[1].EventReference);
			AssertEquals("Info2 should be in the Clone.", Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, newCollection[1].EventSource);
		}

		#region Implementation

		protected override DeclarationEventLockInfoCollection GetCollectionToTest()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);

			return new DeclarationEventLockInfoCollection(lockConfig, fallbackLevel, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeclarationEventLockInfo(null, Factory);
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
