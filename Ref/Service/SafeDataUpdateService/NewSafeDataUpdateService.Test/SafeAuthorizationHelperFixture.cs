using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Web.Auth;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	public class SafeAuthorizationHelperFixture
	{
		string dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task PermissionOnAllTables()
		{
			var usrAuth = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "",
				UA_ColumnValue = "",
				UA_DataSetName = "All data sets",
				UA_TableName = "*"
			};
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(usrAuth);
				await repo.SaveChangesAsync(null);
				var helper = new SafeAuthorizationHelper(repo);
				Assert.IsTrue(helper.IsAuthorized(new RefCusCodeList(), "USR1"));
				Assert.IsTrue(helper.IsAuthorized(new RefExchangeRateZZ(), "USR1"));
				Assert.IsTrue(helper.IsAuthorized(new RefCusTariff(), "USR1"));
				Assert.IsTrue(helper.InitAuthorizations(new[] { new RefCusCodeListAttributeUserView() }, "USR1").FirstOrDefault().ZZE_IsEditable);
				Assert.IsTrue(helper.InitAuthorizations(new[] { new RefCusCodeListUserView() }, "USR1").FirstOrDefault().ZZD_IsEditable);
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task PermissionOnColumnConditions()
		{
			var usrAuth1 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CodeType",
				UA_ColumnValue = "CUSOF",
				UA_DataSetName = "CUSOFF US",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth2 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CountryOrGrouping",
				UA_ColumnValue = "US",
				UA_DataSetName = "CUSOFF US",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth3 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CodeType",
				UA_ColumnValue = "CUSOF",
				UA_DataSetName = "CUSOFF ZA",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth4 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CountryOrGrouping",
				UA_ColumnValue = "ZA",
				UA_DataSetName = "CUSOFF ZA",
				UA_TableName = "RefCusCodeListUserView"
			};
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(usrAuth1);
				repo.Add(usrAuth2);
				repo.Add(usrAuth3);
				repo.Add(usrAuth4);
				await repo.SaveChangesAsync(null);

				var helper = new SafeAuthorizationHelper(repo);
				Assert.IsTrue(helper.IsAuthorized(new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "US" }, "USR1"));
				Assert.IsTrue(helper.IsAuthorized(new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "ZA" }, "USR1"));
				Assert.IsFalse(helper.IsAuthorized(new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "AU" }, "USR1"));
				Assert.IsFalse(helper.IsAuthorized(new RefCusCodeListUserView { ZZD_CodeType = "FDA", ZZD_CountryOrGrouping = "US" }, "USR1"));
				Assert.IsFalse(helper.IsAuthorized(new RefExchangeRateZZ(), "USR1"));

				var initData = helper.InitAuthorizations(new[]
				{
				new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "US" },
				new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "ZA" },
				new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "AU" },
				new RefCusCodeListUserView { ZZD_CodeType = "FDA", ZZD_CountryOrGrouping = "US" }
				}, "USR1").ToArray();

				Assert.IsTrue(initData[0].ZZD_IsEditable);
				Assert.IsTrue(initData[1].ZZD_IsEditable);
				Assert.IsFalse(initData[2].ZZD_IsEditable);
				Assert.IsFalse(initData[3].ZZD_IsEditable);
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task CheckAuthorized_PermissionOnAllTables()
		{
			var usrAuth = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "",
				UA_ColumnValue = "",
				UA_DataSetName = "All data sets",
				UA_TableName = "*"
			};
			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(usrAuth);
				await repo.SaveChangesAsync(null);

				var helper = new SafeAuthorizationHelper(repo);
				var dummyDict = new Dictionary<string, object>() { };
				var dummyList = new string[] { };

				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefCusCodeListUserView), dummyList));
				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefCusCodeList), dummyList));
				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefExchangeRateZZ), dummyList));
				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefCusTariff), dummyList));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task CheckAuthorized_PermissionOnColumnConditions()
		{
			var usrAuth1 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CodeType",
				UA_ColumnValue = "CUSOF",
				UA_DataSetName = "CUSOFF US",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth2 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CountryOrGrouping",
				UA_ColumnValue = "US",
				UA_DataSetName = "CUSOFF US",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth3 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CodeType",
				UA_ColumnValue = "CUSOF",
				UA_DataSetName = "CUSOFF ZA",
				UA_TableName = "RefCusCodeListUserView"
			};
			var usrAuth4 = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "ZZD_CountryOrGrouping",
				UA_ColumnValue = "ZA",
				UA_DataSetName = "CUSOFF ZA",
				UA_TableName = "RefCusCodeListUserView"
			};

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(usrAuth1);
				repo.Add(usrAuth2);
				repo.Add(usrAuth3);
				repo.Add(usrAuth4);
				await repo.SaveChangesAsync(null);

				var helper = new SafeAuthorizationHelper(repo);

				string[] keyList = new string[] { "ZZD_Code", "ZZD_CodeType", "ZZD_CountryOrGrouping", "ZZD_IsEditable" };
				var cusCodeList1 = new Dictionary<string, object>()
				{
					{ "ZZD_Code", "AAA" },
					{ "ZZD_CodeType", "CUSOF" },
					{ "ZZD_CountryOrGrouping", "US" },
					{ "ZZD_IsEditable", false }
				};
				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(cusCodeList1, "USR1", typeof(RefCusCodeListUserView), keyList));
				cusCodeList1 = new Dictionary<string, object>()
				{
					{ "ZZD_Code", "BBB" },
					{ "ZZD_CodeType", "CUSOF" },
					{ "ZZD_CountryOrGrouping", "ZA" },
					{ "ZZD_IsEditable", false }
				};
				Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(cusCodeList1, "USR1", typeof(RefCusCodeListUserView), keyList));
				cusCodeList1 = new Dictionary<string, object>()
				{
					{ "ZZD_Code", "CCC" },
					{ "ZZD_CodeType", "CUSOF" },
					{ "ZZD_CountryOrGrouping", "AU" },
					{ "ZZD_IsEditable", false }
				};
				Assert.IsFalse(helper.IsAuthorizedTypeNotMatch(cusCodeList1, "USR1", typeof(RefCusCodeListUserView), keyList));
				cusCodeList1 = new Dictionary<string, object>()
				{
					{ "ZZD_Code", "DDD" },
					{ "ZZD_CodeType", "FDA" },
					{ "ZZD_CountryOrGrouping", "US" },
					{ "ZZD_IsEditable", false }
				};
				Assert.IsFalse(helper.IsAuthorizedTypeNotMatch(cusCodeList1, "USR1", typeof(RefCusCodeListUserView), keyList));

				keyList = new string[] { "ZZD_Code", "ZZD_CodeType", "ZZD_IsEditable" };
				var cusCodeList2 = new Dictionary<string, object>()
				{
					{ "ZZD_Code", "AA" },
					{ "ZZD_CodeType", "CUSOF" },
					{ "ZZD_CountryOrGrouping", "US" },
					{ "ZZD_IsEditable", false }
				};
				Assert.IsFalse(helper.IsAuthorizedTypeNotMatch(cusCodeList2, "USR1", typeof(RefCusCodeListUserView), keyList));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task IsAuthorized()
		{
			var usrAuth = new UserAuthorization
			{
				UA_User = "USR1",
				UA_ColumnName = "",
				UA_ColumnValue = "",
				UA_DataSetName = "RefCusTariff",
				UA_TableName = "RefCusTariff"
			};

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(usrAuth);
				await repo.SaveChangesAsync(null);

				var helper = new SafeAuthorizationHelper(repo);
				Assert.IsTrue(helper.IsAuthorized<RefCusTariff>("USR1"));
			}
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetAuthorizationData()
		{
			var userAuth1 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "All DataSets", UA_TableName = "*", UA_ColumnName = "", UA_ColumnValue = "" };
			var userAuth2 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "CodeList1", UA_TableName = "RefCusCodeList", UA_ColumnName = "ZZD_CodeType", UA_ColumnValue = "TST" };
			var userAuth3 = new UserAuthorization { UA_User = "User1", UA_DataSetName = "CodeType", UA_TableName = "RefCusCodeType", UA_ColumnName = "", UA_ColumnValue = "" };
			var userAuth4 = new UserAuthorization { UA_User = "User2", UA_DataSetName = "All DataSets", UA_TableName = "*", UA_ColumnName = "", UA_ColumnValue = "" };
			var userAuth5 = new UserAuthorization { UA_User = "User2", UA_DataSetName = "CodeList2", UA_TableName = "RefCusCodeList", UA_ColumnName = "", UA_ColumnValue = "" };

			using (var repo = new ReferenceDataRepository(false, TestConnectionString.GetAdmin(dbName)))
			{
				repo.Add(userAuth1);
				repo.Add(userAuth2);
				repo.Add(userAuth3);
				repo.Add(userAuth4);
				repo.Add(userAuth5);
				await repo.SaveChangesAsync(null);

				var helper = new SafeAuthorizationHelperForTest(repo);
				var auths = helper.GetAuthorizationDataForTest("User1", typeof(RefCusCodeList)).OrderBy(x => x.UA_DataSetName).ToArray();
				Assert.AreEqual(2, auths.Length);
				Assert.AreEqual("All DataSets", auths[0].UA_DataSetName);
				Assert.AreEqual("*", auths[0].UA_TableName);
				Assert.AreEqual("CodeList1", auths[1].UA_DataSetName);
				Assert.AreEqual("RefCusCodeList", auths[1].UA_TableName);
				Assert.AreEqual("ZZD_CodeType", auths[1].UA_ColumnName);
				Assert.AreEqual("TST", auths[1].UA_ColumnValue);
			}
		}
	}

	class SafeAuthorizationHelperForTest : SafeAuthorizationHelper
	{
		public SafeAuthorizationHelperForTest(IReferenceDataRepository repository) : base(repository)
		{
		}

		public IEnumerable<RefUserAuthorization> GetAuthorizationDataForTest(string user, Type type)
		{
			return base.GetAuthorizationData(user, type);
		}
	}
}
