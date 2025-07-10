using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	sealed class DeclarationTabLockInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTabPageList()
		{
			var sourceListType = typeof(Constants.Customs.DeclarationTabPages.Codes);
			var infos = sourceListType.GetFields(BindingFlags.Public | BindingFlags.Static);
			var expectedList = new List<string>();

			foreach (var info in infos)
			{
				if (info.FieldType == typeof(string) && !info.GetValue(null).ToString().In(new string[] { "ARN", "AGO", "HDR", "TRA", "HOU", "ULR", "GDS" }))
				{
					expectedList.Add((string)info.GetValue(null));
				}
			}

			var actualList = GetLookupsToTest().TabPageList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}

		public void TestTabPageList_ARN()
		{
			var list = GetLookupsToTest("ARN");
			AssertEquals("Only 'ARN - NCTS Arrival Notification' should be available for declaration type 'ARN'", "ALL, ARN", list.TabPageList.CodesAsString);
		}

		public void TestTabPageList_ARN_CH()
		{
			var list = GetLookupsToTest("ARN", Core.Constants.CountryCodes.Switzerland);
			AssertEquals("Only 'ARN - NCTS Arrival Notification' should be available for declaration type 'ARN'", "ALL, ARN, AGO", list.TabPageList.CodesAsString);
		}

		public void TestTabPageList_DEP()
		{
			var list = GetLookupsToTest("DEP");
			AssertEquals("Tab codes 'HDR, TRA, HOU, GDS' should be available for declaration type 'DEP'", "ALL, HDR, TRA, HOU, GDS", list.TabPageList.CodesAsString);
		}

		public void TestTabPageList_ULR()
		{
			var list = GetLookupsToTest("ULR");
			AssertEquals("Only 'ULR - NCTS Unloading Remarks' should be available for declaration type 'ULR'", "ULR", list.TabPageList.CodesAsString);
		}

		public DeclarationTabLockInfoLookups GetLookupsToTest(string declarationType = null, string countryCode = null)
		{
			if (countryCode != null)
			{
				var company = (BusinessObject)Factory.Load<IGlbCompany>(Env.CurrentCompany.PK);
				company[GlbCompanySchema.GC_RN_NKCountryCode] = countryCode;
				Factory.Save();
			}
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			var lockConfig = new DeclarationLockConfig(fallbackLevel, Factory);
			lockConfig.DeclarationType = declarationType;
			var lockInfo = lockConfig.TabInfos.AddNew();
			return lockInfo.Lookups;
		}
	}
}
