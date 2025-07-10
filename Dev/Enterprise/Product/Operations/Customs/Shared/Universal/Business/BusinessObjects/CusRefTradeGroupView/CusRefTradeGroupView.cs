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
	[CodeProperty(CusRefTradeGroupViewSchema.Constants.ZZA_TradeGroup), DescriptionProperty(CusRefTradeGroupViewSchema.Constants.ZZA_Description)]
	public sealed class CusRefTradeGroupView : AutoCusRefTradeGroupView, ITranslatableZZBusinessObject
	{
		public CusRefTradeGroupView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("DataGrouping")]
		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupView|ZZA_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZZA_ZZZ_NKDataGrouping
		{
			get { return base.ZZA_ZZZ_NKDataGrouping; }
			set { base.ZZA_ZZZ_NKDataGrouping = value; }
		}

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupView|ZZA_TradeGroup", Caption = "Trade Group")]
		public override ZString ZZA_TradeGroup { get => base.ZZA_TradeGroup; set => base.ZZA_TradeGroup = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupView|ZZA_Description", Caption = "Description")]
		public override ZString ZZA_Description
		{
			get => TranslationHelper.GetTranslatedValue(this, base.ZZA_Description, RefCusTradeGroupLanguageSchema.ZXD_Description);
			set => base.ZZA_Description = value;
		}

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupView|ZZA_StartDate", Caption = "Start Date")]
		public override ZDateTime ZZA_StartDate { get => base.ZZA_StartDate; set => base.ZZA_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.CusRefTradeGroupView|ZZA_EndDate", Caption = "End Date")]
		public override ZDateTime ZZA_EndDate { get => base.ZZA_EndDate; set => base.ZZA_EndDate = value; }

		[ReadOnly(true)]
		public override ZString ZZA_DataSet
		{
			get { return base.ZZA_DataSet; }
			set { base.ZZA_DataSet = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZA_ZZZ_NKDataGrouping); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TranslatableZZBusinessObjectFetchStrategy<CusRefTradeGroupView>(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusRefTradeGroupView.Loader, Integration.Customs.Shared.ICusRefTradeGroupViewLoader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZBool IsCountryPartOfTradeGroup(ZString countryCode, ZString tradeGroup, ZString dataGrouping, ZDateTime dateOfValuation)
			{
				return IsCountryPartOfTradeGroupAny(countryCode, new ZString[] { tradeGroup }, dataGrouping, dateOfValuation);
			}

			public CusRefTradeGroupView[] Load(ZString dataGrouping, ZDateTime dateOfValuation, ZString countryOfOrigin)
			{
				if (!dateOfValuation.IsValid)
				{
					return Array.Empty<CusRefTradeGroupView>();
				}
				var query = new ZQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				return Factory.Load<CusRefTradeGroupView>(query)
					.Where(x => countryOfOrigin.IsEmpty || x.TradeGroupCountries.Any(y => y.ZZB_RN_NKTradeGroupCountryCode == countryOfOrigin && y.ZZB_StartDate <= dateOfValuation && y.ZZB_EndDate >= dateOfValuation))
					.OrderBy(x => x.ZZA_TradeGroup + x.ZZA_StartDate.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture)).ToArray();
			}

			public CusRefTradeGroupView Load(ZString dataGrouping, ZString tradeGroup, ZDateTime dateOfValuation, string dataSet = "")
			{
				var query = GetQueryForLoad(dataGrouping, new ZString[] { tradeGroup }, dateOfValuation, dataSet);
				return Factory.LoadTop1<CusRefTradeGroupView>(query);
			}

			public CusRefTradeGroupView[] Load(ZString dataGrouping, ZString[] tradeGroups, ZDateTime dateOfValuation, string dataSet = "")
			{
				var query = GetQueryForLoad(dataGrouping, tradeGroups, dateOfValuation, dataSet);
				return Factory.Load<CusRefTradeGroupView>(query);
			}

			ZQuery GetQueryForLoad(ZString dataGrouping, ZString[] tradeGroups, ZDateTime dateOfValuation, string dataSet = "")
			{
				var query = new ZQuery(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				if (tradeGroups.Length > 0)
				{
					query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, tradeGroups);
				}
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				if (!string.IsNullOrEmpty(dataSet))
				{
					query.AddToFilter(CusRefTradeGroupViewSchema.ZZA_DataSet, dataSet);
				}

				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusRefTradeGroupView);
			}

			#region ICusRefTradeGroupViewLoader Members

			public ZBool IsCountryPartOfTradeGroupAny(ZString countryCode, ZString[] tradeGroups, ZString dataGrouping, ZDateTime dateOfValuation)
			{
				var refCusTradeGroupCountryQuery = new ZDBOnlySubQuery(typeof(RefCusTradeGroupCountry), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				refCusTradeGroupCountryQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, countryCode);

				var refCusTradeGroupQuery = new ZDBOnlyQuery(typeof(CusRefTradeGroupView));
				refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_TradeGroup, tradeGroups);
				refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, dataGrouping);
				refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, dateOfValuation);
				refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, dateOfValuation);
				refCusTradeGroupQuery.AddSubQuery(CusRefTradeGroupViewSchema.PK, refCusTradeGroupCountryQuery, JoinCondition.And);

				return Factory.Exists(typeof(CusRefTradeGroupView), refCusTradeGroupQuery);
			}

			#endregion
		}

		#region TradeGroupCountries

		[ChildEditable]
		public CusRefTradeGroupCountryViewCollection TradeGroupCountries
		{
			get
			{
				if (tradeGroupCountries == null)
				{
					tradeGroupCountries = new CusRefTradeGroupCountryViewCollection(this);
					RegisterEditableChildObject(tradeGroupCountries);
				}
				return tradeGroupCountries;
			}
		}
		CusRefTradeGroupCountryViewCollection tradeGroupCountries;

		public IEnumerable<CusRefTradeGroupCountryView> GetApplicableTradeGroupCountries(ZDateTime effectiveDate)
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

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZA_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var name = Res.GetString("66a3e4d6-f29d-4302-ae21-a5e089d77680", "Customs Trade Group");
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

			ZZA_TradeGroup = "TradeGroup";
			ZZA_Description = "Description";
			ZZA_ZZZ_NKDataGrouping = "AU";
		}
#endif
	}
}
