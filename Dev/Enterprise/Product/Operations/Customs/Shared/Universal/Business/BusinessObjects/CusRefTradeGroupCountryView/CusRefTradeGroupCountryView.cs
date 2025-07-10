using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class CusRefTradeGroupCountryView : AutoCusRefTradeGroupCountryView
	{
		public CusRefTradeGroupCountryView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(CusTradeGroup))]
		public override ZGuid ZZB_ZZA_TradeGroup
		{
			get { return base.ZZB_ZZA_TradeGroup; }
			set { base.ZZB_ZZA_TradeGroup = value; }
		}

		[ReadOnly(true)]
		public override ZString ZZB_DataSet
		{
			get { return base.ZZB_DataSet; }
			set { base.ZZB_DataSet = value; }
		}

		public CusRefTradeGroupView CusTradeGroup
		{
			get { return Factory.Load<CusRefTradeGroupView>(ZZB_ZZA_TradeGroup); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZB_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusRefTradeGroupCountryView.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}
			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusRefTradeGroupCountryView);

			public static CusRefTradeGroupCountryView[] LoadTradeGroupCountries(BusinessObjectFactory factory, ZString tradeGroup, ZString dataGroupingCode)
			{
				return LoadTradeGroupCountries(factory, tradeGroup, dataGroupingCode, ZDateTime.Today);
			}

			public static CusRefTradeGroupCountryView[] LoadTradeGroupCountries(BusinessObjectFactory factory, ZString tradeGroup, ZString dataGroupingCode, ZDateTime valuationDate)
			{
				var query = new ZDBOnlyQuery(typeof(CusRefTradeGroupCountryView));
				query.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
				query.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
				var tradeGroupSubQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup);
				if (!tradeGroup.IsEmpty)
				{
					tradeGroupSubQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, tradeGroup);
				}
				var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(factory, dataGroupingCode);
				if (!parentDataGroupingCode.IsEmpty)
				{
					tradeGroupSubQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, new ZString[] { dataGroupingCode, parentDataGroupingCode });
				}
				else
				{
					tradeGroupSubQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, dataGroupingCode);
				}
				tradeGroupSubQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
				tradeGroupSubQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
				query.AddSubQuery(tradeGroupSubQuery, JoinCondition.And);
				return factory.Load<CusRefTradeGroupCountryView>(query);
			}

			public static CodeDescriptionPairList GetCachedListTradeGroupCountries(BusinessObjectFactory factory, ZString tradeGroup, ZString dataGroupingCode)
			{
				var key = $@"TradeGroupCountries_{tradeGroup}|{dataGroupingCode}|{ZDate.Today.ToString("yyyyMMdd")}";
				return factory.GetCachedValue(key, () =>
				{
					var result = new CodeDescriptionPairList();
					var tradeGroupCountries = LoadTradeGroupCountries(factory, tradeGroup, dataGroupingCode);
					var distinctCountries = tradeGroupCountries.Select(x => x.ZZB_RN_NKTradeGroupCountryCode).Distinct().OrderBy(y => y);
					foreach (var country in distinctCountries)
					{
						result.AddPair(country, tradeGroupCountries.Where(x => x.ZZB_RN_NKTradeGroupCountryCode == country && !x.ZZB_Description.IsEmpty).FirstOrDefault()?.ZZB_Description ?? country);
					}
					return result;
				});
			}
		}

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (ZZB_StartDate >= ZZB_EndDate)
			{
				var swapHolder = ZZB_StartDate;
				ZZB_StartDate = ZZB_EndDate;
				ZZB_EndDate = swapHolder;
			}
		}
#endif
	}
}
