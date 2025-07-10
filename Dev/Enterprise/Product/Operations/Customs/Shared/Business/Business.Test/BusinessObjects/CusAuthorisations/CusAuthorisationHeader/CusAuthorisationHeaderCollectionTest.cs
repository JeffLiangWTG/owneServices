using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationHeaderCollection))]
	class CusAuthorisationHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusAuthorisationHeaderCollection>
	{
		public void TestMatchesFilterCore()
		{
			var item1 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;
			var item2 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var item3 = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			item3.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			item3.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			var collection = new CusAuthorisationHeaderCollection(Factory);
			var completeFilter = collection.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Unmatched CPH_ApplicationCode", false, item1.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_RN_NKCountryCode", false, item2.MatchesFilter(completeFilter));
				AssertEquals("matched", true, item3.MatchesFilter(completeFilter));
			});
		}

		public void TestMatchesFilterExtended_IsNoResultQuery()
		{
			var permitHolder = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new CusAuthorisationHeaderCollection(Factory, Array.Empty<ZString>(), new ZGuid[] { permitHolder.PK }, ZDate.Today, ZString.Empty, ZString.Empty);
			AssertEquals("types is empty", true, collection.CompleteFilter.IsNoResultQuery);

			collection = new CusAuthorisationHeaderCollection(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration }, Array.Empty<ZGuid>(), ZDate.Today, ZString.Empty, ZString.Empty);
			AssertEquals("permitHolders is empty", true, collection.CompleteFilter.IsNoResultQuery);

			collection = new CusAuthorisationHeaderCollection(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration }, new ZGuid[] { permitHolder.PK }, ZDate.Invalid, ZString.Empty, ZString.Empty);
			AssertEquals("transactionDate is invalid", true, collection.CompleteFilter.IsNoResultQuery);
		}

		public void TestMatchesFilterExtended()
		{
			var permitHolder1 = Factory.NewWithValidTestData<OrgHeader>();
			var permitHolder2 = Factory.NewWithValidTestData<OrgHeader>();
			var item1 = CreateCusAuthorisationHeader(permitHolder1);
			item1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;

			var item2 = CreateCusAuthorisationHeader(permitHolder1);
			item2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var item3 = CreateCusAuthorisationHeader(permitHolder1);
			item3.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

			var item4 = CreateCusAuthorisationHeader(permitHolder2);

			var item5 = CreateCusAuthorisationHeader(permitHolder1);
			item5.CPH_StartDate = ZDate.Today.AddDays(2);

			var item6 = CreateCusAuthorisationHeader(permitHolder1);
			item6.CPH_EndDate = ZDate.Today.AddDays(-2);

			var item7 = CreateCusAuthorisationHeader(permitHolder1);
			var item7Rule = item7.CusAuthorisationRules.AddNew();
			item7Rule.CPR_RuleCode = "USE";
			item7Rule.CPR_ValueFrom = "IMP";

			var item8 = CreateCusAuthorisationHeader(permitHolder1);
			var item8Rule = item8.CusAuthorisationRules.AddNew();
			item8Rule.CPR_RuleCode = "MRE";
			item8Rule.CPR_ValueFrom = "IMP";

			var item9 = CreateCusAuthorisationHeader(permitHolder1);
			var item9Rule = item9.CusAuthorisationRules.AddNew();
			item9Rule.CPR_RuleCode = "USE";
			item9Rule.CPR_ValueFrom = "CWP";

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(new CusAuthorisationHeaderCollection(Factory, ZString.Empty, permitHolder1.PK, ZDate.Today).CompleteFilter.IsNoResultQuery);
				Assert(new CusAuthorisationHeaderCollection(Factory, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, ZGuid.Empty, ZDate.Today).CompleteFilter.IsNoResultQuery);

				var collection = new CusAuthorisationHeaderCollection(Factory, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, permitHolder1.PK, ZDate.Today);
				var completeFilter = collection.CompleteFilter;
				AssertEquals("Unmatched CPH_ApplicationCode", false, item1.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_RN_NKCountryCode", false, item2.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_Type", false, item3.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_OH_PermitHolder", false, item4.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_StartDate", false, item5.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPH_EndDate", false, item6.MatchesFilter(completeFilter));
				AssertEquals("Matched", true, item7.MatchesFilter(completeFilter));

				collection = new CusAuthorisationHeaderCollection(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir }, new ZGuid[] { permitHolder1.PK }, ZDate.Today, ZString.Empty, ZString.Empty);
				completeFilter = collection.CompleteFilter;
				AssertEquals("Matched multiple CPH_Type, ACT", true, item3.MatchesFilter(completeFilter));
				AssertEquals("Matched multiple CPH_Type, SDE", true, item7.MatchesFilter(completeFilter));

				collection = new CusAuthorisationHeaderCollection(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration }, new ZGuid[] { permitHolder1.PK, permitHolder2.PK }, ZDate.Today, ZString.Empty, ZString.Empty);
				completeFilter = collection.CompleteFilter;
				AssertEquals("Matched multiple CPH_OH_PermitHolder, permitHolder2", true, item4.MatchesFilter(completeFilter));
				AssertEquals("Matched multiple CPH_OH_PermitHolder, permitHolder1", true, item7.MatchesFilter(completeFilter));

				collection = new CusAuthorisationHeaderCollection(Factory, new ZString[] { CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration }, new ZGuid[] { permitHolder1.PK }, ZDate.Today, "USE", "IMP");
				completeFilter = collection.CompleteFilter;
				AssertEquals("Matched rule", true, item7.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPR_RuleCode", false, item8.MatchesFilter(completeFilter));
				AssertEquals("Unmatched CPR_ValueFrom", false, item9.MatchesFilter(completeFilter));
			});
		}

		protected override CusAuthorisationHeaderCollection GetCollectionToTest()
		{
			return new CusAuthorisationHeaderCollection(Factory);
		}

		CusAuthorisationHeader CreateCusAuthorisationHeader(OrgHeader permitHolder)
		{
			var cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			cusAuthorisationHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			cusAuthorisationHeader.CPH_OH_PermitHolder = permitHolder.PK;
			cusAuthorisationHeader.CPH_StartDate = ZDate.Today.AddDays(-2);
			cusAuthorisationHeader.CPH_EndDate = ZDate.Today.AddDays(2);
			return cusAuthorisationHeader;
		}
	}
}
