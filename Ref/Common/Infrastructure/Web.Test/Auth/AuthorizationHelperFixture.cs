using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Web.Auth;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	class AuthorizationHelperFixture
	{
		[Test]
		public void PermissionOnAllTables()
		{
			var helper = new AuthorizationHelperForTest(new[] { usrAuthForAllDataSets });
			Assert.IsTrue(helper.IsAuthorized(new RefCusCodeListUserView(), "USR1"));
			Assert.IsTrue(helper.IsAuthorized(new RefExchangeRateZZ(), "USR1"));
			Assert.IsTrue(helper.IsAuthorized(new RefCusTariff(), "USR1"));
			Assert.IsTrue(helper.InitAuthorizations(new[] { new RefCusCodeListUserView() }, "USR1").FirstOrDefault().ZZD_IsEditable);
			Assert.IsTrue(helper.InitAuthorizations(new[] { new RefCusTariff() }, "USR1").FirstOrDefault().ZZ1_IsEditable);
		}

		[Test]
		public void PermissionOnColumnConditions()
		{
			var helper = new AuthorizationHelperForTest(new[] { usrAuth1, usrAuth2, usrAuth3, usrAuth4 });
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

		[Test]
		public void CheckAuthorized_PermissionOnAllTables()
		{
			var helper = new AuthorizationHelperForTest(new[] { usrAuthForAllDataSets });
			var dummyDict = new Dictionary<string, object>() { };
			var dummyList = new string[] { };

			Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefCusCodeListUserView), dummyList));
			Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefExchangeRateZZ), dummyList));
			Assert.IsTrue(helper.IsAuthorizedTypeNotMatch(dummyDict, "USR1", typeof(RefCusTariff), dummyList));
		}

		[Test]
		public void CheckAuthorized_PermissionOnColumnConditions()
		{
			var helper = new AuthorizationHelperForTest(new[] { usrAuth1, usrAuth2, usrAuth3, usrAuth4 });
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

		[Test]
		public void IsAuthorized()
		{
			var usrAuth = new RefUserAuthorization("USR1", "RefCusTariff", "RefCusTariff", "", "");
			var helper = new AuthorizationHelperForTest(new[] { usrAuth });
			Assert.IsTrue(helper.IsAuthorized<RefCusTariff>("USR1"));
			Assert.IsFalse(helper.IsAuthorized<RefCusTariff>("USR2"));
			Assert.IsFalse(helper.IsAuthorized<RefCusCodeListUserView>("USR1"));
		}

		[Test]
		public void TestFilterAuthorizedData()
		{
			var refCusCodeListUserView = new RefCusCodeListUserView { ZZD_CodeType = "CUSOF", ZZD_CountryOrGrouping = "US" };
			var refCusTariff = new RefCusTariff();
			var helper = new AuthorizationHelperForTest(new[] { usrAuthForAllDataSets });
			Assert.That(helper.FilterAuthorizedData(new[] { refCusCodeListUserView }, "USR1"), Is.EquivalentTo(new[] { refCusCodeListUserView }));
			Assert.That(helper.FilterAuthorizedData(new[] { refCusTariff }, "USR1"), Is.EquivalentTo(new[] { refCusTariff }));
		}

		RefUserAuthorization usrAuthForAllDataSets = new RefUserAuthorization("USR1", "All data sets", "*", "*", "*");
		RefUserAuthorization usrAuth1 = new RefUserAuthorization("USR1", "CUSOFF US", "RefCusCodeListUserView", "ZZD_CodeType", "CUSOF");
		RefUserAuthorization usrAuth2 = new RefUserAuthorization("USR1", "CUSOFF US", "RefCusCodeListUserView", "ZZD_CountryOrGrouping", "US");
		RefUserAuthorization usrAuth3 = new RefUserAuthorization("USR1", "CUSOFF ZA", "RefCusCodeListUserView", "ZZD_CodeType", "CUSOF");
		RefUserAuthorization usrAuth4 = new RefUserAuthorization("USR1", "CUSOFF ZA", "RefCusCodeListUserView", "ZZD_CountryOrGrouping", "ZA");
	}
}
