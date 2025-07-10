using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusRulingFilterStripBusinessObject))]
	public class ZZRefCusRulingFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestRulingNumberFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusRulingFilterStripBusinessObject();
			var rulingNumber = (ModuleTextFilter)filterStrip[ZZRefCusRulingFilters.RulingNumber];
			AssertEquals("rulingNumberFilter.Visibility", FilterVisibility.AlwaysVisible, rulingNumber.Visibility);
			rulingNumber.IsActive = true;
			rulingNumber.Property = "11111";
			var filter = filterStrip.Filter;
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", false, zzCusRuling2.MatchesFilter(filter));
		}

		public void TestRulingTypeFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusRulingFilterStripBusinessObject();
			var rulingType = (ModuleTextFilter)filterStrip[ZZRefCusRulingFilters.RulingType];
			AssertEquals("rulingTypeFilter.Visibility", FilterVisibility.Visible, rulingType.Visibility);
			rulingType.IsActive = true;
			rulingType.Property = "Ty1";
			var filter = filterStrip.Filter;
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", false, zzCusRuling2.MatchesFilter(filter));
		}

		public void TestEffectiveDateFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusRulingFilterStripBusinessObject();
			var effectiveDateFilter = (ModuleSingleDateFilter)filterStrip[ZZRefCusRulingFilters.EffectiveDate];
			AssertEquals("effectiveDateFilter.Visibility", FilterVisibility.Visible, effectiveDateFilter.Visibility);
			effectiveDateFilter.IsActive = true;
			effectiveDateFilter.Property1 = new ZDate(2018, 5, 30);
			var filter = filterStrip.Filter;
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", false, zzCusRuling2.MatchesFilter(filter));
		}

		public void TestAppliesToFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusRulingFilterStripBusinessObject();
			var appliesToFilter = (ModuleGuidFilter)filterStrip[ZZRefCusRulingFilters.AppliesTo];
			AssertEquals("appliesToFilter.Visibility", FilterVisibility.Visible, appliesToFilter.Visibility);
			appliesToFilter.IsActive = true;
			appliesToFilter.Property = orgAddress1PK;
			var filter = filterStrip.Filter;
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", false, zzCusRuling2.MatchesFilter(filter));
		}

		public void TestAppliesToOrgFilter()
		{
			SetupData();
			var filterStrip = new ZZRefCusRulingFilterStripBusinessObject();
			var appliesToFilter = (ModuleGuidFilter)filterStrip[ZZRefCusRulingFilters.AppliesToOrg];
			AssertEquals("appliesToFilter.Visibility", FilterVisibility.Visible, appliesToFilter.Visibility);
			appliesToFilter.IsActive = true;
			appliesToFilter.Property = orgHeader.PK;
			zzCusRuling2.ZZX_OA_AppliesTo = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();
			var filter = filterStrip.Filter;
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", false, zzCusRuling2.MatchesFilter(filter));
			zzCusRuling2.ZZX_OA_AppliesTo = ZGuid.Empty;
			Factory.Save();
			AssertEquals("zzCusRuling1", true, zzCusRuling1.MatchesFilter(filter));
			AssertEquals("zzCusRuling2", true, zzCusRuling2.MatchesFilter(filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ZZRefCusRulingFilterStripBusinessObject();
		}

		void SetupData()
		{
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.Address1 = "Address One";
			var orgAddress2 = orgHeader.Addresses.AddNew();
			orgAddress2.Address1 = "Address Two";
			orgAddress1PK = orgAddress1.PK;
			orgAddress2PK = orgAddress2.PK;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling1 = helper.CreateZZRefCusRulingCombined(Core.Constants.CountryCodes.Canada, "11111", "Ty1", orgAddress1PK, new ZDate(2018, 1, 1), new ZDate(2018, 12, 1));
			var cusRuling2 = helper.CreateZZRefCusRulingCombined(Core.Constants.CountryCodes.Canada, "22222", "Ty2", orgAddress2PK, new ZDate(2018, 11, 1), new ZDate(2018, 12, 1));
			Factory.Save();
			zzCusRuling1 = Factory.Load<ZZRefCusRulingCombined>(cusRuling1.PK);
			zzCusRuling2 = Factory.Load<ZZRefCusRulingCombined>(cusRuling2.PK);
		}

		ZZRefCusRulingCombined zzCusRuling1;
		ZZRefCusRulingCombined zzCusRuling2;
		OrgHeader orgHeader;
		ZGuid orgAddress1PK;
		ZGuid orgAddress2PK;
	}
}
