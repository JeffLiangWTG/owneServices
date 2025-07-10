using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using RateCriteriaTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpRateSelectionCriteria_V2;
using SecondTradeGroupTvp = Enterprise.Customs.Universal.Constants.ZZStoreProcedureReference.TvpSecondTradeGroup;

namespace Enterprise.Customs.Universal.Testing
{
	public class RateSelectionCriteriaInfoTest : TestCaseWithFactory
	{
		public void TestTypesMatch_TVP_RateSelectionCriteria_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(RateCriteriaTvp.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 11, columnInfos.Count);

				var columnDataGrouping = columnInfos[RateCriteriaTvp.Columns.DataGrouping];
				var zzColumnDataGrouping = RefDataGroupingSchema.ZZZ_DataGrouping;
				AssertEquals("DataGrouping.DataType", zzColumnDataGrouping.SqlDbType.ToString().ToUpper(), columnDataGrouping.DataType.ToUpper());
				AssertEquals("DataGrouping.MaxLength", zzColumnDataGrouping.MaxLength, columnDataGrouping.MaxLength);

				var columnTradeGroupCountry = columnInfos[RateCriteriaTvp.Columns.TradeGroupCountry];
				var zzColumnTradeGroupCountry = RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode;
				AssertEquals("TradeGroupCountry.DataType", zzColumnTradeGroupCountry.SqlDbType.ToString().ToUpper(), columnTradeGroupCountry.DataType.ToUpper());
				AssertEquals("TradeGroupCountry.MaxLength", zzColumnTradeGroupCountry.MaxLength, columnTradeGroupCountry.MaxLength);

				var columnRateType = columnInfos[RateCriteriaTvp.Columns.RateType];
				var zzColumnRateType = RefCusRateTypeSchema.ZZR_RateType;
				AssertEquals("RateType.DataType", zzColumnRateType.SqlDbType.ToString().ToUpper(), columnRateType.DataType.ToUpper());
				AssertEquals("RateType.MaxLength", zzColumnRateType.MaxLength, columnRateType.MaxLength);

				var columnRateCode = columnInfos[RateCriteriaTvp.Columns.RateCode];
				var zzColumnRateCode = CusRefRateCodeViewSchema.ZY1_RateCode;
				AssertEquals("RateCode.DataType", zzColumnRateCode.SqlDbType.ToString().ToUpper(), columnRateCode.DataType.ToUpper());
				AssertEquals("RateCode.MaxLength", zzColumnRateCode.MaxLength, columnRateCode.MaxLength);

				var columnPreference = columnInfos[RateCriteriaTvp.Columns.Preference];
				var zzColumnPreference = RefCusPreferenceSchema.ZZS_Preference;
				AssertEquals("Preference.DataType", zzColumnPreference.SqlDbType.ToString().ToUpper(), columnPreference.DataType.ToUpper());
				AssertEquals("Preference.MaxLength", zzColumnPreference.MaxLength, columnPreference.MaxLength);

				var columnOrderNumber = columnInfos[RateCriteriaTvp.Columns.OrderNumber];
				var zzColumnOrderNumber = RefCusApplicabilitySchema.ZZT_OrderNumber;
				AssertEquals("OrderNumber.DataType", zzColumnOrderNumber.SqlDbType.ToString().ToUpper(), columnOrderNumber.DataType.ToUpper());
				AssertEquals("OrderNumber.MaxLength: The MaxLength of nvarchar column in sys.Columns is double defined length since each nvarchar character takes 2 bytes.", zzColumnOrderNumber.MaxLength, columnOrderNumber.MaxLength / 2);

				var columnDirection = columnInfos[RateCriteriaTvp.Columns.Direction];
				AssertEquals("Direction.DataType", "INT", columnDirection.DataType.ToUpper());
			});
		}

		public void TestTypesMatch_TVP_SecondTradeGroup_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(SecondTradeGroupTvp.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 3, columnInfos.Count);

				var columnAdditionalCode = columnInfos[SecondTradeGroupTvp.Columns.SecondTradeGroup];
				var zzaTradeGroup = RefCusTradeGroupSchema.ZZA_TradeGroup;
				AssertEquals("AdditionalCode.DataType", zzaTradeGroup.SqlDbType.ToString().ToUpper(), columnAdditionalCode.DataType.ToUpper());
				AssertEquals("AdditionalCode.MaxLength", zzaTradeGroup.MaxLength, columnAdditionalCode.MaxLength);
			});
		}

		public void TestRateSelectionCriteriaInfo_SecondTradeGroup()
		{
			var criteriaDate = new ZDateTime(2020, 01, 15, 14, 00, 30);
			var effectiveDate = new ZDateTime(2020, 01, 15, 14, 00, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "Pre1", "Ord1", new HashSet<ZString> { "Add1", "Add3" }, criteriaDate, "DTY", "RC1", new HashSet<ZString> { "SecondTG1", "SecondTG2" });
			var criteria2 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "Pre1", "Ord1", new HashSet<ZString> { "Add1", "Add3" }, criteriaDate, "DTY", "RC1", new HashSet<ZString> { "", "XX" });
			var criteria3 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "Pre1", "Ord1", new HashSet<ZString> { "Add1", "Add3" }, criteriaDate, "DTY", "RC1", new HashSet<ZString> { "", "" });

			var rateSelectionCriteriaInfo = new RateSelectionCriteriaInfo { EffectiveDate = effectiveDate, TradeGroupCountry = "AU", RateType = "DTY", RateCode = "RC1", ZZT_OrderNumber = "Ord1", ZZT_AdditionalCode = "Add1", ZZA_TradeGroup = "TT", ZZA_Description = "TT DEC", ZZS_Preference = "Pre1", ZZS_Description = "Pre1 Dec", SecondTradeGroup = "SecondTG1" };
			CombineAssertions(() =>
			{
				AssertEquals("MatchExcludingConcessionOrder: SecondTradeGroup matched", true, rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria1));
				AssertEquals("MatchExcludingConcessionOrder: SecondTradeGroup not matched", false, rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria2));
				AssertEquals("MatchExcludingConcessionOrder: criteria.SecondTradeGroups is empty and ignored matched", true, rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria3));

				AssertEquals("MatchExcludingAdditionalCodes: SecondTradeGroup matched", true, rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria1));
				AssertEquals("MatchExcludingAdditionalCodes: SecondTradeGroup not matched", false, rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria2));
				AssertEquals("MatchExcludingAdditionalCodes: criteria.SecondTradeGroups is empty and ignored matched", true, rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria3));

				AssertEquals("MatchExcludingPrimaryPreference: SecondTradeGroup matched", true, rateSelectionCriteriaInfo.MatchExcludingPrimaryPreference(criteria1));
				AssertEquals("MatchExcludingPrimaryPreference: SecondTradeGroup not matched", false, rateSelectionCriteriaInfo.MatchExcludingPrimaryPreference(criteria2));
				AssertEquals("MatchExcludingPrimaryPreference: criteria.SecondTradeGroups is empty and ignored matched", true, rateSelectionCriteriaInfo.MatchExcludingPrimaryPreference(criteria3));

				AssertEquals("MatchExcludingTradeGroup: SecondTradeGroup matched", true, rateSelectionCriteriaInfo.MatchExcludingTradeGroup(criteria1));
				AssertEquals("MatchExcludingTradeGroup: SecondTradeGroup not matched", false, rateSelectionCriteriaInfo.MatchExcludingTradeGroup(criteria2));
				AssertEquals("MatchExcludingTradeGroup: criteria.SecondTradeGroups is empty and ignored matched", true, rateSelectionCriteriaInfo.MatchExcludingTradeGroup(criteria3));
			});
		}

		public void TestRateSelectionCriteriaInfo()
		{
			var criteriaDate = new ZDateTime(2020, 01, 15, 14, 00, 30);
			var effectiveDate = new ZDateTime(2020, 01, 15, 14, 00, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var criteria1 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "Pre1", "Ord1", new HashSet<ZString> { "Add1", "Add3" }, criteriaDate, "DTY", "RC1");
			var criteria2 = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "Pre2", "Ord2", new HashSet<ZString> { "Add2", "Add3" }, criteriaDate, "ADD", "RC2");
			var rateSelectionCriteriaInfo = new RateSelectionCriteriaInfo { EffectiveDate = effectiveDate, TradeGroupCountry = "AU", RateType = "DTY", RateCode = "RC1", ZZT_OrderNumber = "Ord1", ZZT_AdditionalCode = "Add1", ZZA_TradeGroup = "TT", ZZA_Description = "TT DEC", ZZS_Preference = "Pre1", ZZS_Description = "Pre1 Dec", };
			Assert(rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria1));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingConcessionOrder(criteria2));
			Assert(rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria1));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingAdditionalCodes(criteria2));
			Assert(rateSelectionCriteriaInfo.MatchExcludingPrimaryPreference(criteria1));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingPrimaryPreference(criteria2));
			Assert(rateSelectionCriteriaInfo.MatchExcludingTradeGroup(criteria1));
			Assert(!rateSelectionCriteriaInfo.MatchExcludingTradeGroup(criteria2));
		}

		void AssertAll(IZZRateSelectionCriteria criteria, RateSelectionCriteriaInfo rateInfo, bool expected, bool excludingAdditionalCodes = false, bool excludingConcessionOrder = false, bool excludingPrimaryPreference = false, bool excludingTradeGroup = false)
		{
			if (!excludingAdditionalCodes)
			{
				AssertEquals("MatchExcludingAdditionalCodes", expected, rateInfo.MatchExcludingAdditionalCodes(criteria));
			}

			if (!excludingConcessionOrder)
			{
				AssertEquals("MatchExcludingConcessionOrder", expected, rateInfo.MatchExcludingConcessionOrder(criteria));
			}

			if (!excludingPrimaryPreference)
			{
				AssertEquals("MatchExcludingPrimaryPreference", expected, rateInfo.MatchExcludingPrimaryPreference(criteria));
			}

			if (!criteria.PrimaryPreference.IsEmpty)
			{
				AssertEquals("MatchExcludingTradeGroup", expected, rateInfo.MatchExcludingTradeGroup(criteria));
			}
			else
			{
				AssertEquals("MatchExcludingTradeGroup", rateInfo.ZZS_Preference.IsEmpty, rateInfo.MatchExcludingTradeGroup(criteria));
			}
		}

		public void TestMatchBehaviourForPrimaryPreference()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("Rate should match when it doesn't have a PrimaryPreference.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "", "");
				AssertAll(criteria, rateInfo, true, excludingPrimaryPreference: true);
			});

			CombineAssertions("PrimaryPreference should match exactly if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true, excludingPrimaryPreference: true);
			});

			CombineAssertions("PrimaryPreference should not match another value if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "200", "PREFERENTIAL");
				AssertAll(criteria, rateInfo, false, excludingPrimaryPreference: true);
			});
		}

		public void TestMatchBehaviourForRateType()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("RateType should match exactly if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "DTY", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true);
			});

			CombineAssertions("RateType should not match empty if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false);
			});

			CombineAssertions("RateType should not match another value if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "ADD", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false);
			});
		}

		public void TestMatchBehaviourForRateCode()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("RateCode should match exactly if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "", "RC1");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "RC1", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true);
			});

			CombineAssertions("RateCode should not match empty if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "", "RC1");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false);
			});

			CombineAssertions("RateCode should not match another value if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "", "RC1");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "RC2", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false);
			});
		}

		public void TestMatchBehaviourForAdditionalCode()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("Rate should match when it doesn't have an AdditionalCode.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>()
				{ "Add1", "Add3" }, date, "ANY", "ANY");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "ANY", "ANY", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true, excludingAdditionalCodes: true);
			});

			CombineAssertions("Rate should match when it has an AdditionalCode and the selection criteria has a matching code.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>()
				{ "Add1", "Add3" }, date, "ANY", "ANY");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "ANY", "ANY", "", "Add1", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true, excludingAdditionalCodes: true);
			});

			CombineAssertions("Rate should not match when it has an AdditionalCode and the selection criteria doesn't have a matching code.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>()
				{ "Add1", "Add3" }, date, "ANY", "ANY");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "ANY", "ANY", "", "Add2", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false, excludingAdditionalCodes: true);
			});
		}

		public void TestMatchBehaviourForConcessionOrder()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("Rate should match when it doesn't have both a ConcessionOrder (OrderNumber) and a PrimaryPreference.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "ORD1", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "", "");
				AssertAll(criteria, rateInfo, true, excludingConcessionOrder: true);
			});

			CombineAssertions("Rate should not match when it doesn't have a ConcessionOrder (OrderNumber) but has a PrimaryPreference.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "ORD1", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false, excludingConcessionOrder: true);
			});

			CombineAssertions("ConcessionOrder (OrderNumber) should match exactly if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "ORD1", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "ORD1", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, true, excludingConcessionOrder: true);
			});

			CombineAssertions("ConcessionOrder (OrderNumber) should not match another value if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "100", "ORD1", new HashSet<ZString>(), date, "", "");
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "", "", "ORD2", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false, excludingConcessionOrder: true);
			});
		}

		public void TestMatch_MatchPrimaryPreference()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			const string countryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				var rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "", "");
				var criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("excludingPrimaryPreference always return true 1", true, rateInfo.Match(criteria, excludingPrimaryPreference: true, excludingTradeGroup: false));
				AssertEquals("excludingPrimaryPreference always return true 2", true, rateInfo.Match(criteria, excludingPrimaryPreference: true, excludingTradeGroup: true));
				AssertEquals("ZZS_Preference.IsEmpty always return true 1", true, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: false));
				AssertEquals("ZZS_Preference.IsEmpty always return true 2", true, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: true));

				rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "PRE", "PRE DESC");
				AssertEquals("primaryPreference.IsEmpty && excludingTradeGroup = false", true, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: false));
				AssertEquals("primaryPreference.IsEmpty && excludingTradeGroup = true", false, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: true));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "PRE", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("primaryPreference == ZZS_Preference 1", true, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: false));
				AssertEquals("primaryPreference == ZZS_Preference 2", true, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: true));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "TTT", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("primaryPreference <> ZZS_Preference 1", false, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: false));
				AssertEquals("primaryPreference <> ZZS_Preference 2", false, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: true));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "TTT", "", null, date, "", "");
				AssertEquals("primaryPreference <> ZZS_Preference 1: null additionalCode", false, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: false));
				AssertEquals("primaryPreference <> ZZS_Preference 2: null additionalCode", false, rateInfo.Match(criteria, excludingPrimaryPreference: false, excludingTradeGroup: true));
			});
		}

		public void TestMatch_MatchConcessionOrder()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			const string countryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				var rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "", "");
				var criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("excludingConcessionOrder always return true", true, rateInfo.Match(criteria, excludingConcessionOrder: true));
				AssertEquals("ZZS_Preference.IsEmpty && ZZT_OrderNumber.IsEmpty always return true", true, rateInfo.Match(criteria, excludingConcessionOrder: false));

				rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "Ord", "", "TG", "TG DESC", "", "");
				AssertEquals("concessionOrder.IsEmpty always return true", true, rateInfo.Match(criteria, excludingConcessionOrder: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "Ord", new HashSet<ZString>(), date, "", "");
				AssertEquals("concessionOrder == ZZT_OrderNumber return true", true, rateInfo.Match(criteria, excludingConcessionOrder: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "Ord1", new HashSet<ZString>(), date, "", "");
				AssertEquals("concessionOrder <> ZZT_OrderNumber return true", false, rateInfo.Match(criteria, excludingConcessionOrder: false));
			});
		}

		public void TestMatch_MatchAdditionalCodes()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			const string countryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				var rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "", "");
				var criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("excludingAdditionalCodes always return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: true));
				AssertEquals("ZZT_AdditionalCode.IsEmpty always return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: false));

				rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "Add", "TG", "TG DESC", "", "");
				AssertEquals("!additionalCodes.Any() always return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString> { "" }, date, "", "");
				AssertEquals("!additionalCodes.Any() always return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("additionalCodes.All(l => l.IsEmpty) return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString> { "Add", "TT" }, date, "", "");
				AssertEquals("additionalCodes.Contains(ZZT_AdditionalCode) return true", true, rateInfo.Match(criteria, excludingAdditionalCodes: false));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString> { "XX", "TT" }, date, "", "");
				AssertEquals("additionalCodes Not Contains(ZZT_AdditionalCode) return true", false, rateInfo.Match(criteria, excludingAdditionalCodes: false));
			});
		}

		public void TestMatch_MatchSecondTradeGroups()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			const string countryOfOrigin = Core.Constants.CountryCodes.Australia;

			CombineAssertions(() =>
			{
				var rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "", "", "");
				var criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "");
				AssertEquals("SecondTradeGroup.IsEmpty always return true", true, rateInfo.Match(criteria));

				rateInfo = new RateSelectionCriteriaInfo(date, countryOfOrigin, "", "", "", "", "TG", "TG DESC", "", "", "STG");
				AssertEquals("secondTradeGroups == null always return true", true, rateInfo.Match(criteria));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "", new HashSet<ZString> { "", "" });
				AssertEquals("secondTradeGroups.All(l => l.IsEmpty) return true", true, rateInfo.Match(criteria));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "", new HashSet<ZString> { "STG", "TT" });
				AssertEquals("secondTradeGroups.Contains(SecondTradeGroup) return true", true, rateInfo.Match(criteria));

				criteria = Helper.CreateRateSelectionCriteria(countryOfOrigin, dataGrouping, "", "", new HashSet<ZString>(), date, "", "", new HashSet<ZString> { "XX", "TT" });
				AssertEquals("secondTradeGroups Not Contains(SecondTradeGroup) return true", false, rateInfo.Match(criteria));
			});
		}

		public void TestMatchBehaviourForDirection()
		{
			var date = new ZDate(2020, 01, 15);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			CombineAssertions("Direction should match exactly if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "", direction: RateDirection.Import);
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "DTY", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD", direction: RateDirection.Import);
				AssertAll(criteria, rateInfo, true);
			});

			CombineAssertions("Direction should not match empty if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "", direction: RateDirection.Import);
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "DTY", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD");
				AssertAll(criteria, rateInfo, false);
			});

			CombineAssertions("Direction should not match another value if provided.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "", direction: RateDirection.Import);
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "DTY", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD", direction: RateDirection.Export);
				AssertAll(criteria, rateInfo, false);
			});

			CombineAssertions("Direction should match if it is both.", () =>
			{
				var criteria = Helper.CreateRateSelectionCriteria(Core.Constants.CountryCodes.Australia, dataGrouping, "", "", new HashSet<ZString>(), date, "DTY", "", direction: RateDirection.Both);
				var rateInfo = new RateSelectionCriteriaInfo(date, "AU", "DTY", "", "", "", "ERGA OMNES", "ERGA OMNES", "100", "STANDARD", direction: RateDirection.Export);
				AssertAll(criteria, rateInfo, true);
			});
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
