using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(RefCusTradeGroupSchema.Constants.ZZA_TradeGroup), DescriptionProperty(RefCusTradeGroupSchema.Constants.ZZA_Description)]
	public sealed class RefCusTradeGroup : AutoRefCusTradeGroup, ITranslatableZZBusinessObject
	{
		public RefCusTradeGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("DataGrouping")]
		[ResourceStringData("Enterprise.Customs.Universal.RefCusTradeGroup|ZZA_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZZA_ZZZ_NKDataGrouping
		{
			get { return base.ZZA_ZZZ_NKDataGrouping; }
			set { base.ZZA_ZZZ_NKDataGrouping = value; }
		}

		[ResourceStringData("Enterprise.Customs.Universal.RefCusTradeGroup|ZZA_TradeGroup", Caption = "Trade Group")]
		public override ZString ZZA_TradeGroup { get => base.ZZA_TradeGroup; set => base.ZZA_TradeGroup = value; }

		public override ZString ZZA_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZA_Description, RefCusTradeGroupLanguageSchema.ZXD_Description);
			set => base.ZZA_Description = value;
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZA_ZZZ_NKDataGrouping); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<RefCusTradeGroup>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusTradeGroup.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZBool IsCountryPartOfTradeGroup(ZString countryCode, ZString tradeGroup, ZString dataGrouping, ZDateTime dateOfValuation)
			{
				var refCusTradeGroupCountryQuery = new ZDBOnlySubQuery(typeof(RefCusTradeGroupCountry), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, countryCode);

				var refCusTradeGroupQuery = new ZDBOnlyQuery(typeof(RefCusTradeGroup));
				refCusTradeGroupQuery.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, tradeGroup);
				refCusTradeGroupQuery.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				refCusTradeGroupQuery.AddToFilter(RefCusTradeGroupSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				refCusTradeGroupQuery.AddToFilter(RefCusTradeGroupSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				refCusTradeGroupQuery.AddSubQuery(RefCusTradeGroupSchema.PK, refCusTradeGroupCountryQuery, JoinCondition.And);

				return Factory.LoadTop1<RefCusTradeGroup>(refCusTradeGroupQuery) != null;
			}

			public RefCusTradeGroup[] Load(ZString dataGrouping, ZDateTime dateOfValuation, ZString countryOfOrigin)
			{
				var query = new ZQuery(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				return Factory.Load<RefCusTradeGroup>(query)
					.Where(x => countryOfOrigin.IsEmpty || x.TradeGroupCountries.Any(y => y.ZZB_RN_NKTradeGroupCountryCode == countryOfOrigin && y.ZZB_StartDate <= dateOfValuation && y.ZZB_EndDate >= dateOfValuation))
					.OrderBy(x => x.ZZA_TradeGroup + x.ZZA_StartDate.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture)).ToArray();
			}

			public RefCusTradeGroup Load(ZString dataGrouping, ZString tradeGroup, ZDateTime dateOfValuation)
			{
				var query = new ZQuery(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, tradeGroup);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);

				return Factory.LoadTop1<RefCusTradeGroup>(query);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusTradeGroup);
			}
		}

		#region TradeGroupCountries

		[ChildEditable]
		public RefCusTradeGroupCountryCollection TradeGroupCountries
		{
			get
			{
				if (tradeGroupCountries == null)
				{
					tradeGroupCountries = new RefCusTradeGroupCountryCollection(this);
					RegisterEditableChildObject(tradeGroupCountries);
				}
				return tradeGroupCountries;
			}
		}
		RefCusTradeGroupCountryCollection tradeGroupCountries;

		internal IEnumerable<RefCusTradeGroupCountry> GetApplicableTradeGroupCountries(ZDateTime effectiveDate)
		{
			return TradeGroupCountries.Where(x => x.ZZB_StartDate <= effectiveDate && x.ZZB_EndDate >= effectiveDate);
		}

		#endregion

		public ITableSchema LanguageTableSchema => RefCusTradeGroupLanguageSchema.Instance;

		public Type LanguageTableType => typeof(RefCusTradeGroupLanguage);

		#region Override Method

		public override void Delete()
		{
			TradeGroupCountries.DeleteAll();
			base.Delete();
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var name = Res.GetString("F5BA848A-0C7A-4BC4-A484-5DFF550B986C", "Customs Trade Group");
				if (!ZZA_Description.IsEmpty)
				{
					name += " - " + ZZA_Description;
				}

				return name;
			}
		}

		#endregion

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (ZZA_StartDate >= ZZA_EndDate)
			{
				var swapHolder = ZZA_StartDate;
				ZZA_StartDate = ZZA_EndDate;
				ZZA_EndDate = swapHolder;
			}
		}
#endif
	}
}
