using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing.Helper;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZConditionSelectionCriteriaTest : TestCase
	{
		public void TestTypesMatch_TVP_ConditionSelectionCriteria_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(TvpConditionSelectionCriteria.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 11, columnInfos.Count);

				var columnDataGrouping = columnInfos[TvpConditionSelectionCriteria.Columns.DataGrouping];
				var zzColumnDataGrouping = RefDataGroupingSchema.ZZZ_DataGrouping;
				AssertEquals("DataGrouping.DataType", zzColumnDataGrouping.SqlDbType.ToString().ToUpper(), columnDataGrouping.DataType.ToUpper());
				AssertEquals("DataGrouping.MaxLength", zzColumnDataGrouping.MaxLength, columnDataGrouping.MaxLength);

				var columnTradeGroupCountry = columnInfos[TvpConditionSelectionCriteria.Columns.TradeGroupCountry];
				var zzColumnTradeGroupCountry = RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode;
				AssertEquals("TradeGroupCountry.DataType", zzColumnTradeGroupCountry.SqlDbType.ToString().ToUpper(), columnTradeGroupCountry.DataType.ToUpper());
				AssertEquals("TradeGroupCountry.MaxLength", zzColumnTradeGroupCountry.MaxLength, columnTradeGroupCountry.MaxLength);

				var columnConditionType = columnInfos[TvpConditionSelectionCriteria.Columns.ConditionType];
				var zzColumnConditionType = RefCusConditionTypeSchema.ZX2_ConditionType;
				AssertEquals("ConditionType.DataType", zzColumnConditionType.SqlDbType.ToString().ToUpper(), columnConditionType.DataType.ToUpper());
				AssertEquals("ConditionType.MaxLength", zzColumnConditionType.MaxLength, columnConditionType.MaxLength);

				var columnConditionClass = columnInfos[TvpConditionSelectionCriteria.Columns.ConditionClass];
				var zzColumnConditionClass = RefCusConditionTypeSchema.ZX2_ConditionClass;
				AssertEquals("ConditionClass.DataType", zzColumnConditionClass.SqlDbType.ToString().ToUpper(), columnConditionClass.DataType.ToUpper());
				AssertEquals("ConditionClass.MaxLength", zzColumnConditionClass.MaxLength, columnConditionClass.MaxLength);

				var columnPreference = columnInfos[TvpConditionSelectionCriteria.Columns.Preference];
				var zzColumnPreference = RefCusPreferenceSchema.ZZS_Preference;
				AssertEquals("Preference.DataType", zzColumnPreference.SqlDbType.ToString().ToUpper(), columnPreference.DataType.ToUpper());
				AssertEquals("Preference.MaxLength", zzColumnPreference.MaxLength, columnPreference.MaxLength);

				var columnOrderNumber = columnInfos[TvpConditionSelectionCriteria.Columns.OrderNumber];
				var zzColumnOrderNumber = RefCusApplicabilitySchema.ZZT_OrderNumber;
				AssertEquals("OrderNumber.DataType", zzColumnOrderNumber.SqlDbType.ToString().ToUpper(), columnOrderNumber.DataType.ToUpper());
				AssertEquals("OrderNumber.MaxLength: The MaxLength of nvarchar column in sys.Columns is double defined length since each nvarchar character takes 2 bytes.", zzColumnOrderNumber.MaxLength, columnOrderNumber.MaxLength / 2);
			});
		}

		public void TestTypesMatch_TVP_AdditionalCodes_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(TvpAdditionalCodes.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 3, columnInfos.Count);

				var columnAdditionalCode = columnInfos[TvpAdditionalCodes.Columns.AdditionalCode];
				var zztAdditionalCode = RefCusApplicabilitySchema.ZZT_AdditionalCode;
				AssertEquals("AdditionalCode.DataType", zztAdditionalCode.SqlDbType.ToString().ToUpper(), columnAdditionalCode.DataType.ToUpper());
				AssertEquals("AdditionalCode.MaxLength: The MaxLength of nvarchar column in sys.Columns is double defined length since each nvarchar character takes 2 bytes.", zztAdditionalCode.MaxLength, columnAdditionalCode.MaxLength / 2);
			});
		}

		public void TestTypesMatch_TVP_SecondTradeGroup_ColumnDefinition()
		{
			var columnInfos = TVPTestHelper.GetTableTypesColumnInfoFromDb(TvpSecondTradeGroup.QualifiedName).ToDictionary(x => x.ColumnName);

			CombineAssertions(() =>
			{
				AssertEquals("TVP column Count", 3, columnInfos.Count);

				var columnSecondTradeGroup = columnInfos[TvpSecondTradeGroup.Columns.SecondTradeGroup];
				var zzaSecondTradeGroup = RefCusTradeGroupSchema.ZZA_TradeGroup;
				AssertEquals("SecondTradeGroup.DataType", zzaSecondTradeGroup.SqlDbType.ToString().ToUpper(), columnSecondTradeGroup.DataType.ToUpper());
				AssertEquals("SecondTradeGroup.MaxLength", zzaSecondTradeGroup.MaxLength, columnSecondTradeGroup.MaxLength);
			});
		}

		public void TestZZConditionSelectionCriteria()
		{
			var dateTime = new ZDateTime(2020, 8, 12, 23, 40, 21);
			var critieria = new ZZConditionSelectionCriteria(dateTime, "TG", "PP", new HashSet<ZString> { "AC1", "AC2" }, "CO", "DG", ConditionChecker.ConditionDirection.Import, "CC", "CT", new HashSet<ZString> { "TR1", "TR2" });
			CombineAssertions(() =>
			{
				AssertEquals("EffectiveDate", dateTime, critieria.EffectiveDate);
				AssertEquals("TradeGroupCountry", "TG", critieria.TradeGroupCountry);
				AssertEquals("PrimaryPreference", "PP", critieria.PrimaryPreference);
				AssertArrayEqualsByElements("AdditionalCodes", new ZString[] { "AC1", "AC2" }, critieria.AdditionalCodes.ToArray());
				AssertEquals("ConcessionOrder", "CO", critieria.ConcessionOrder);
				AssertEquals("DataGrouping", "DG", critieria.DataGrouping);
				AssertEquals("Direction", ConditionChecker.ConditionDirection.Import, critieria.Direction);
				AssertEquals("ConditionClass", "CC", critieria.ConditionClass);
				AssertEquals("ConditionType", "CT", critieria.ConditionType);
				AssertArrayEqualsByElements("SecondTradeGroups", new ZString[] { "TR1", "TR2" }, critieria.SecondTradeGroups.ToArray());
			});
		}
	}
}
