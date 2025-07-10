using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[UseSnapshotProtection]
	public abstract class DuplicationFinderProxyTest : TestCase
	{
		public abstract void TestGetPotentialTargets();

		public abstract void TestGetPotentialTargetsAsync();
	}

	public abstract class DuplicationFinderProxyTestWithFactory : TestCaseWithFactory
	{
		public abstract void TestCompareBusinessObjects();

		public abstract void TestCompareBusinessObjects_WithIgnores();

		public abstract void TestAddIgnore();

		public abstract void TestAddExclusion();
	}
}
