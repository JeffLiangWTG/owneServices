using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RateView : AutoRateView, ITariffEffectiveDatesRelatedBusinessObject, IUniversalRateCalcData
	{
		public RateView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoRateView.Schema
		{
			public const string ZZ2_EndDate_ForDisplay = "ZZ2_EndDate_ForDisplay";
			public const string ZZ2_ZZR_RateTypeCode = "ZZ2_ZZR_RateTypeCode";
			public const string ZZ2_ZZR_RateTypeDesc = "ZZ2_ZZR_RateTypeDesc";
			public const string RateCode = "RateCode";
			public const string RateDescription = "RateDescription";
			public const string PreferenceCode = "PreferenceCode";
			public const string PreferenceDescription = "PreferenceDescription";
			public const string RateFormulaDescription = "RateFormulaDescription";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRateView.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			static ZQuery GetEffectiveRateFilter(TariffView tariff, ZString rateType, ZDateTime valuationDate)
			{
				var result = new ZQuery();

				ZGuid rateCodePk;
				if (tariff == null
					|| rateType.IsEmpty
					|| !valuationDate.IsValid
					|| !(rateCodePk = CusRefRateCodeView.Loader.LoadByRateType(tariff.Factory, tariff.ZZ1_ZZZ_NKDataGrouping, rateType)?.FirstOrDefault()?.PK ?? ZGuid.Empty).IsValid)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					result.AddToFilter(RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode, tariff.PK);
					result.AddToFilter(RateViewSchema.ZZ2_StartDate, SQLComparisonOperator.LessThanOrEqualTo, valuationDate);
					result.AddToFilter(RateViewSchema.ZZ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, valuationDate);
					result.AddToFilter(RateViewSchema.ZZ2_ZY1_RateCode, rateCodePk);
				}

				return result;
			}

			public static RateView LoadMostRecentCachedRate(TariffView tariff, ZString rateType, ZDateTime valuationDate)
			{
				RateView result = null;
				if (tariff != null && !rateType.IsEmpty)
				{
					var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
					var factory = tariff.Factory;
					result = factory.GetCachedValue(string.Join("_", "LoadMostRecentCachedRate", tariff.PK, rateType, effectiveValuationDate), () =>
					{
						var query = GetEffectiveRateFilter(tariff, rateType, effectiveValuationDate);
						return factory.Load<RateView>(query).OrderByDescending(x => x.ZZ2_StartDate).FirstOrDefault();
					});
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RateView);
		}

		#region New Properties

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_EndDate", Caption = "End Date")]
		public override ZDateTime ZZ2_EndDate { get => base.ZZ2_EndDate; set => base.ZZ2_EndDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_EndDate_ForDisplay", Caption = "End Date")]
		public ZDateTime ZZ2_EndDate_ForDisplay => ZZ2_EndDate == ZDateTime.MaxSmallDateTime ? ZDateTime.Empty : ZZ2_EndDate;

		public ZPropertyInfo ZZ2_EndDate_ForDisplayInfo => GetZPropertyInfo(Schema.ZZ2_EndDate_ForDisplay);

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_StartDate", Caption = "Start Date")]
		public override ZDateTime ZZ2_StartDate { get => base.ZZ2_StartDate; set => base.ZZ2_StartDate = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_ZZR_RateTypeCode", Caption = "Rate Type")]
		public ZString ZZ2_ZZR_RateTypeCode => Factory.GetValue(ref rateTypeCodeCached, () => ZZ2_IsSystem
			? CusRateType?.ZZR_RateType ?? ZString.Empty
			: CusRateCode?.ZY1_RateType ?? ZString.Empty);
		CachedProperty<ZString> rateTypeCodeCached;

		public ZPropertyInfo ZZ2_ZZR_RateTypeCodeInfo => GetZPropertyInfo(Schema.ZZ2_ZZR_RateTypeCode);

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_ZZR_RateTypeDesc", Caption = "Type Description", ShortCaption = "Type Desc.")]
		public ZString ZZ2_ZZR_RateTypeDesc => Factory.GetValue(ref rateTypeDescCached, () => ZZ2_IsSystem
			? CusRateType?.ZZR_Description ?? ZString.Empty
			: (ZString)Factory.GetCachedValue<RefCusRateTypeCustomizableList>().GetDescriptionFromCode(ZZ2_ZZR_RateTypeCode));
		CachedProperty<ZString> rateTypeDescCached;

		public ZPropertyInfo ZZ2_ZZR_RateTypeDescInfo => GetZPropertyInfo(Schema.ZZ2_ZZR_RateTypeDesc);

		public ZString ZZ2_ZZR_ZZZ_NKDataGrouping => CusRateType?.ZZR_ZZZ_NKDataGrouping ?? ZString.Empty;

		public RefCusRateType CusRateType => CusRateCode?.RateType;

		[ResourceStringData("Enterprise.Customs.Universal.RateView|RateFormulaDescription", Caption = "Rate Formula Description", ShortCaption = "Rate Formula Desc.")]
		public ZString RateFormulaDescription => !ZZ2_RateFormulaDerivedFrom.IsEmpty ? ZZ2_RateFormulaDerivedFrom : new ZString(RateFormulaParseResult?.HumanReadableString);

		public ZPropertyInfo RateFormulaDescriptionInfo => GetZPropertyInfo(Schema.RateFormulaDescription);

		public ZDecimal? RateFormulaNumber => RateFormulaParseResult?.Number;

		RateFormulaHumanReadableVisitor.Result RateFormulaParseResult => (rateFormulaParseResult ?? (rateFormulaParseResult = new RecalculableCachedValue<RateFormulaHumanReadableVisitor.Result>(GetRateFormulaParseResult))).Value;
		RecalculableCachedValue<RateFormulaHumanReadableVisitor.Result> rateFormulaParseResult;

		RateFormulaHumanReadableVisitor.Result GetRateFormulaParseResult()
		{
			RateFormulaHumanReadableVisitor.Result result = null;

			var rateFormula = ZZ2_RateFormula;
			if (!rateFormula.IsEmpty)
			{
				var errorListener = new FormulaErrorListener();

				var input = new AntlrInputStream(rateFormula);
				var lexer = new RateFormulaLexer(input);
				lexer.RemoveErrorListener(ConsoleErrorListener<int>.Instance);
				var tokens = new CommonTokenStream(lexer);
				var parser = new RateFormulaParser(tokens);
				parser.RemoveErrorListener(ConsoleErrorListener<IToken>.Instance);
				parser.AddErrorListener(errorListener);
				var tree = parser.expression();
				if (errorListener.Errors.Any())
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Unable to parse formula: {rateFormula}"));
				}
				else
				{
					var dataGrouping = ZZ2_ZZZ_NKDataGrouping;
					var unitOfMeasureList = ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(Factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today)
						.ToDictionary(x => x.ZZD_Code.ToString(), x => x.ZZD_Description.ToString());
					var currency = ZZ2_RX_NKCurrencyOverride;
					if (currency.IsEmpty)
					{
						var country = RefCountry.LoadFromCountryCode(Factory, dataGrouping);
						currency = country?.RN_RX_NKLocalCurrency ??
										(dataGrouping == Constants.DataGrouping.EuropeanUnion
											? Core.Constants.CurrencyCodes.EuropeanUnion
											: GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency.ToString());
					}

					var data = new RateFormulaHumanReadableVisitorData(currency,
						unitOfMeasureList,
						new Dictionary<string, string>(),
						new Dictionary<string, string>(),
						new Dictionary<string, string>());
					var visitorResult = new RateFormulaHumanReadableVisitor(data, errorListener).VisitExpression(tree);
					if (errorListener.Errors.Any())
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"Unable to convert formula to human readable string: {rateFormula}"));
					}
					else
					{
						result = visitorResult;
					}
				}
			}
			return result;
		}

		[ResourceStringData("Enterprise.Customs.Universal.RateView|PreferenceCode", Caption = "Preference")]
		public ZString PreferenceCode => Preference?.ZZS_Preference ?? ZString.Empty;

		public ZPropertyInfo PreferenceCodeInfo => GetZPropertyInfo(Schema.PreferenceCode);

		[ResourceStringData("Enterprise.Customs.Universal.RateView|PreferenceDescription", Caption = "Preference Description", ShortCaption = "Description")]
		public ZString PreferenceDescription => Preference?.ZZS_Description ?? ZString.Empty;

		public ZPropertyInfo PreferenceDescriptionInfo => GetZPropertyInfo(Schema.PreferenceDescription);

		[ResourceStringData("Enterprise.Customs.Universal.RateView|RateCode", Caption = "Rate")]
		public ZString RateCode => CusRateCode?.ZY1_RateCode ?? ZString.Empty;

		public ZPropertyInfo RateCodeInfo => GetZPropertyInfo(Schema.RateCode);

		[ResourceStringData("Enterprise.Customs.Universal.RateView|RateDescription", Caption = "Rate Description", ShortCaption = "Description")]
		public ZString RateDescription => CusRateCode?.ZY1_Description ?? ZString.Empty;

		public ZPropertyInfo RateDescriptionInfo => GetZPropertyInfo(Schema.RateDescription);

		#endregion

		#region Override Properties

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZZ2_ZZ1_ParentTariffOrNationalCode
		{
			get { return base.ZZ2_ZZ1_ParentTariffOrNationalCode; }
			set { base.ZZ2_ZZ1_ParentTariffOrNationalCode = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZZ2_ZZ1_ParentTariffOrNationalCode);

		public CusRefPreferenceView Preference => Factory.Load<CusRefPreferenceView>(ZZ2_ZZS_Preference);

		[RelatedBusinessObject("Preference")]
		[List(nameof(Lookups) + "." + nameof(RateViewLookups.PreferenceCodeList))]
		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_ZZS_Preference", Caption = "Preference")]
		public override ZGuid ZZ2_ZZS_Preference { get => base.ZZ2_ZZS_Preference; set => base.ZZ2_ZZS_Preference = value; }

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_ZZZ_NKDataGrouping", Caption = "Country/Region or Grouping")]
		public override ZString ZZ2_ZZZ_NKDataGrouping
		{
			get => base.ZZ2_ZZZ_NKDataGrouping;
			set
			{
				var oldValue = ZZ2_ZZZ_NKDataGrouping;
				base.ZZ2_ZZZ_NKDataGrouping = value;
				if (!IsCopying && ZZ2_ZZZ_NKDataGrouping != oldValue)
				{
					rateFormulaParseResult?.InvalidateCache();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_RX_NKCurrencyOverride", Caption = "Currency Override", MediumCaption = "Curr. Override")]
		public override ZString ZZ2_RX_NKCurrencyOverride
		{
			get => base.ZZ2_RX_NKCurrencyOverride;
			set
			{
				var oldValue = ZZ2_RX_NKCurrencyOverride;
				base.ZZ2_RX_NKCurrencyOverride = value;
				if (!IsCopying && ZZ2_RX_NKCurrencyOverride != oldValue)
				{
					rateFormulaParseResult?.InvalidateCache();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_RateFormula", Caption = "Rate Formula")]
		public override ZString ZZ2_RateFormula
		{
			get => base.ZZ2_RateFormula;
			set
			{
				var oldValue = ZZ2_RateFormula;
				base.ZZ2_RateFormula = value;
				if (!IsCopying && ZZ2_RateFormula != oldValue)
				{
					rateFormulaParseResult?.InvalidateCache();
				}
			}
		}

		public override ZString ZZ2_RateFormulaDerivedFrom
		{
			get => base.ZZ2_RateFormulaDerivedFrom;
			set
			{
				var oldValue = ZZ2_RateFormulaDerivedFrom;
				base.ZZ2_RateFormulaDerivedFrom = value;
			}
		}

		[ReadOnly(true)]
		public override ZString ZZ2_DataSet { get => base.ZZ2_DataSet; set => base.ZZ2_DataSet = value; }

		public CusRefRateCodeView CusRateCode => Factory.Load<CusRefRateCodeView>(ZZ2_ZY1_RateCode);

		[List(nameof(Lookups) + "." + nameof(RateViewLookups.RateCodeList))]
		[ResourceStringData("Enterprise.Customs.Universal.RateView|ZZ2_ZY1_RateCode", Caption = "Rate Code")]
		public override ZGuid ZZ2_ZY1_RateCode { get => base.ZZ2_ZY1_RateCode; set => base.ZZ2_ZY1_RateCode = value; }

		#endregion

		#region New Methods

		public IEnumerable<QuestionForFormulaSpecificValue> GetQuestionForFormulaSpecificValues(IUniversalRateCalcData rateCalcData)
		{
			var calculator = new UniversalRateCalculator(ZZ2_RateFormula, rateCalcData);
			return calculator.GetQuestionForFormulaSpecificValues();
		}

		internal IEnumerable<CusRefTradeGroupView> GetApplicableTradeGroups(ZString tradeGroupCountry, ZDateTime assessmentDate)
		{
			var effectiveDate = assessmentDate.IsValid ? assessmentDate : ZDateTime.Today;

			return RateApplicabilities
				.Where(applicability => applicability.IsApplicable(tradeGroupCountry, effectiveDate))
				.Select(applicability => applicability.TradeGroup);
		}

		#endregion

		#region Collection

		[ChildEditable]
		public CusRefApplicabilityViewCollection RateApplicabilities
		{
			get
			{
				if (rateApplicabilities == null)
				{
					rateApplicabilities = new CusRefApplicabilityViewCollection(this);
					RegisterEditableChildObject(rateApplicabilities);
				}
				return rateApplicabilities;
			}
		}
		CusRefApplicabilityViewCollection rateApplicabilities;

		[ChildEditable]
		public FilteredCusRefApplicabilityViewCollection FilteredRateApplicabilities
		{
			get
			{
				if (filteredRateApplicabilities == null)
				{
					filteredRateApplicabilities = new FilteredCusRefApplicabilityViewCollection(this, CusTariff.IsParentDataGrouping);
					RegisterEditableChildObject(filteredRateApplicabilities);
				}
				return filteredRateApplicabilities;
			}
		}
		FilteredCusRefApplicabilityViewCollection filteredRateApplicabilities;

		[ChildEditable]
		public CusRefRateUOMViewCollection UnitsOfMeasure
		{
			get
			{
				if (unitsOfMeasure == null)
				{
					unitsOfMeasure = new CusRefRateUOMViewCollection(this);
					RegisterEditableChildObject(unitsOfMeasure);
				}
				return unitsOfMeasure;
			}
		}
		CusRefRateUOMViewCollection unitsOfMeasure;

		#endregion

		#region Override Method

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => !ZZ2_IsSystem;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZ2_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			ZZ2_ParentTableType = CusRefTariffSchema.Constants.Prefix;
		}

		public override void Delete()
		{
			RateApplicabilities.DeleteAll();
			base.Delete();
		}

		#endregion

		#region ITariffEffectiveDatesRelatedBusinessObject Members
		ZString ITariffDataGroupingRelatedBusinessObject.DataGrouping => ZZ2_ZZZ_NKDataGrouping;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.StartDate => ZZ2_StartDate;

		ZDateTime ITariffEffectiveDatesRelatedBusinessObject.EndDate => ZZ2_EndDate;
		#endregion

		#region IUniversalRateCalcData Members

		DateTime IUniversalRateCalcData.DateOfValuation => ZDateTime.Today.ToDateTime();

		decimal IUniversalRateCalcData.ValueForDuty => 0m;

		decimal IUniversalRateCalcData.CustomsValue => 0m;

		IDictionary<string, decimal> IUniversalRateCalcData.UnitOfMeasureValueList
		{
			get
			{
				var unitsOfMeasureValues = new Dictionary<string, decimal>();
				var tariffUOMList = CusTariff?.UnitList;
				if (tariffUOMList != null)
				{
					foreach (var unitCode in tariffUOMList.GetAllCodes())
					{
						unitsOfMeasureValues.Add(unitCode, 0m);
					}
				}
				return unitsOfMeasureValues;
			}
		}

		IDictionary<string, decimal> IUniversalRateCalcData.CountrySpecificValueList => new Dictionary<string, decimal>();

		IList<Tuple<string, string>> IUniversalRateCalcData.AdditionalInformationList => new List<Tuple<string, string>>();

		IDictionary<string, string> IUniversalRateCalcData.MeursingExpressionList => new Dictionary<string, string>();

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(RateView rate)
				: base(rate)
			{
			}

			protected new RateView BusinessObject
			{
				get { return (RateView)base.BusinessObject; }
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				if (columns.Any(x => x.ColumnName == Schema.ZZ2_ZZR_RateTypeCode || x.ColumnName == Schema.ZZ2_ZZR_RateTypeDesc || x.ColumnName == Schema.ZZ2_ZY1_RateCode))
				{
					Factory.AddFetchHint(RefCusRateTypeSchema.PK, BusinessObject.ZZ2_ZY1_RateCode);
				}
				base.FetchForViewCore(columns);
			}
		}

		#endregion
	}
}
