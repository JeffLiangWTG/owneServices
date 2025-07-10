using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class DeclarationLockConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestDeclarationTypeList()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var info = new DeclarationLockConfig(fallbackLevel, Factory);
			var list = info.Lookups.DeclarationTypeList;

			AssertNotNull(list);
		}

		public void TestLockModeList()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var info = new DeclarationLockConfig(fallbackLevel, Factory);

			var expectedList = new[] { Constants.Customs.DeclarationLockModes.Codes.All, Constants.Customs.DeclarationLockModes.Codes.Any };
			var actualList = info.Lookups.LockModeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}
	}
}
