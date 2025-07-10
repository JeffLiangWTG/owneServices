using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class OrgSalesActualsInformation : AutoOrgSalesActualsInformation
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : AutoOrgSalesActualsInformation.Schema
		{
			public const string ActualsLastTraded = "ActualsLastTraded";
			public const string ActualsAnnualTotal = "ActualsAnnualTotal";
			public const string ActualsAnnualTEUTotalQuantity = "ActualsAnnualTEUTotalQuantity";
			public const string ActualsMonthlyAverage = "ActualsMonthlyAverage";
			public const string Status = "Status";
			public const string TradedPeriodString = "TradedPeriodString";
		}

		#endregion

		public OrgSalesActualsInformation(EntitySalesWrapper prospectSales, OrgHeader parentOrg)
			: base(parentOrg.Factory, parentOrg.PK)
		{
			this.prospectSales = prospectSales;
			this.parentOrg = parentOrg;
			this.companyFilter = Env.CurrentCompany.PK;
			this.revenueDisplayOption = SalesHeader.RevenueDisplay.FinancialYear;
		}

		#region Properties

		readonly EntitySalesWrapper prospectSales;
		readonly OrgHeader parentOrg;

		public SalesHeader.RevenueDisplay RevenueDisplayOption
		{
			get => revenueDisplayOption;
			set
			{
				if (value != revenueDisplayOption)
				{
					revenueDisplayOption = value;
					RefreshCalculatedValues();
				}
			}
		}
		SalesHeader.RevenueDisplay revenueDisplayOption;

		public ZGuid CompanyFilter
		{
			get => companyFilter;
			set
			{
				if (value != companyFilter)
				{
					companyFilter = value;
					RefreshCalculatedValues();
				}
			}
		}
		ZGuid companyFilter;

		void RefreshCalculatedValues()
		{
			ActualsAnnualTotalInfo.RefreshBinding();
			CommittedYearToDateValueInfo.RefreshBinding();
			CommittedYearToDateTEUQuantityInfo.RefreshBinding();
			CommittedCurrentYearValueInfo.RefreshBinding();
			CommittedCurrentYearTEUQuantityInfo.RefreshBinding();
			CommittedAndForecastCurrentYearValueInfo.RefreshBinding();
			CommittedAndForecastCurrentYearTEUQuantityInfo.RefreshBinding();
			CommittedNextYearValueInfo.RefreshBinding();
			CommittedNextYearTEUQuantityInfo.RefreshBinding();
			CommittedAndForecastNextYearValueInfo.RefreshBinding();
			CommittedAndForecastNextYearTEUQuantityInfo.RefreshBinding();
		}

		#endregion

		#region Actuals

		public void SetActuals(OrgSales[] value)
		{
			this.actuals = value;
			ClearAllPropertyCaches();
		}

		public bool HasActuals()
		{
			return Actuals.Length > 0;
		}

		OrgSales[] Actuals
		{
			get { return actuals ?? (actuals = Array.Empty<OrgSales>()); }
		}
		OrgSales[] actuals;

		void ClearAllPropertyCaches()
		{
			actualsFirstTraded = null;
			actualsLastTraded = null;
		}

		#region Actuals Traded Date

		ZDate ActualsFirstTraded
		{
			get
			{
				if (!actualsFirstTraded.HasValue)
				{
					if (Actuals.Length > 0)
					{
						var periods = Actuals.SelectMany(x => x.TradeDetails.Cast<OrgTradeDetail>()).SelectMany(x => x.TradedPeriods.Cast<OrgTradePeriod>());
						actualsFirstTraded = periods.Any() ? periods.Min(x => x.PAS_Period) : ZDate.Empty;
					}
					else
					{
						actualsFirstTraded = ZDate.Empty;
					}
				}

				return actualsFirstTraded.Value;
			}
		}
		ZDate? actualsFirstTraded;

		[ResourceStringData("OrgSalesActualsInformation|ActualsLastTraded", Caption = "Last Traded")]
		public ZDateTime ActualsLastTraded
		{
			get
			{
				if (!actualsLastTraded.HasValue)
				{
					if (Actuals.Length > 0)
					{
						actualsLastTraded = Actuals.Max(x => x.LastTraded);
					}
					else
					{
						actualsLastTraded = ZDateTime.Empty;
					}
				}

				return actualsLastTraded.Value;
			}
		}
		ZDateTime? actualsLastTraded;

		public ZPropertyInfo ActualsLastTradedInfo => GetZPropertyInfo(Schema.ActualsLastTraded);

		#endregion

		#region Traded Period String

		[ResourceStringData("OrgSalesActualsInformation|TradedPeriodString", Caption = "First Trade")]
		public ZString TradedPeriodString
		{
			get
			{
				var firstTraded = ActualsFirstTraded;
				if (!firstTraded.IsValid)
				{
					return ZString.Empty;
				}

				var today = ZDate.Today;
				var monthDifference = GetMonthDifference(today, firstTraded);
				if (monthDifference < 0)
				{
					// traded in the future?!
					return ZString.Empty;
				}
				else if (monthDifference < 1)
				{
					return Res.GetString("9cba2d60-42c0-495d-ac93-afacb7cc300f", "{0} Day(s)", (today - firstTraded).Days);
				}
				else if (monthDifference > 12)
				{
					return Res.GetString("2a931670-f32c-4c47-b99f-2044fe972916", ">12 Mths.");
				}
				else
				{
					return Res.GetString("00f0d22f-27e7-4da6-b7d0-dc23b2ac3cb9", "{0} Mths.", monthDifference);
				}
			}
		}

		public ZPropertyInfo TradedPeriodStringInfo => GetZPropertyInfo(Schema.TradedPeriodString);

		static int GetMonthDifference(ZDate x, ZDate y)
		{
			return (x.Month - y.Month) + 12 * (x.Year - y.Year);
		}

		#endregion

		#region ActualsAnnualTotal

		[ResourceStringData("OrgSalesActualsInformation|ActualsAnnualTotal", Caption = "Traded Revenue", ShortCaption = "Traded Rev")]
		public ZDecimal ActualsAnnualTotal => RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? ActualsCurrentFinancialYearTotal : ActualsTrailing12MonthsTotal;
		public ZPropertyInfo ActualsAnnualTotalInfo => GetZPropertyInfo(Schema.ActualsAnnualTotal);

		ZDecimal ActualsTrailing12MonthsTotal => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.Trailing12Months, Env.CurrentCompanyPK, Actuals).TotalRevenue;

		ZDecimal ActualsCurrentFinancialYearTotal => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.CurrentFinancialYear, Env.CurrentCompanyPK, Actuals).TotalRevenue;

		#endregion

		#region ActualsAnnualTEUTotalQuantity

		[ResourceStringData("OrgSalesActualsInformation|ActualsAnnualTEUTotalQuantity", Caption = "Actual TEU (c.f.y)")]
		public ZDecimal ActualsAnnualTEUTotalQuantity => RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? ActualsCurrentFinancialYearTEUTotalQuantity : ActualsTrailing12MonthsTEUTotalQuantity;
		public ZPropertyInfo ActualsAnnualTEUTotalQuantityInfo => GetZPropertyInfo(Schema.ActualsAnnualTEUTotalQuantity);

		ZDecimal ActualsTrailing12MonthsTEUTotalQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.Trailing12Months, Env.CurrentCompanyPK, Actuals).TEUQuantity;

		ZDecimal ActualsCurrentFinancialYearTEUTotalQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Traded, SalesValueCalculator.PeriodRange.CurrentFinancialYear, Env.CurrentCompanyPK, Actuals).TEUQuantity;

		#endregion

		#region ActualsMonthlyAverage

		[ResourceStringData("OrgSalesActualsInformation|ActualsMonthlyAverage", Caption = "Traded Average (m/avg)", ShortCaption = "Traded Avg. (m/avg)")]
		public ZDecimal ActualsMonthlyAverage
		{
			get
			{
				var firstTraded = ActualsFirstTraded;
				if (!firstTraded.IsValid)
				{
					return ZDecimal.Zero;
				}

				var tradedMonths = Math.Max(1, Math.Min(12, GetMonthDifference(ZDate.Today, firstTraded)));
				return ActualsAnnualTotal / tradedMonths;
			}
		}

		public ZPropertyInfo ActualsMonthlyAverageInfo => GetZPropertyInfo(Schema.ActualsMonthlyAverage);

		#endregion

		#endregion

		#region Prospect

		public ZDecimal CommittedCurrentYearValue
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearValue : CommittedTrailing12MonthsValue; }
		}
		public ZPropertyInfo CommittedCurrentYearValueInfo => GetZPropertyInfo(nameof(CommittedCurrentYearValue));

		public ZDecimal CommittedCurrentYearTEUQuantity
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearTEUQuantity : CommittedTrailing12MonthsTEUQuantity; }
		}
		public ZPropertyInfo CommittedCurrentYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedCurrentYearTEUQuantity));

		public ZDecimal CommittedNextYearValue
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedNextFinancialYearValue : CommittedNext12MonthsValue; }
		}
		public ZPropertyInfo CommittedNextYearValueInfo => GetZPropertyInfo(nameof(CommittedNextYearValue));

		public ZDecimal CommittedNextYearTEUQuantity
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedNextFinancialYearTEUQuantity : CommittedNext12MonthsTEUQuantity; }
		}
		public ZPropertyInfo CommittedNextYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedNextYearTEUQuantity));

		public ZDecimal CommittedYearToDateValue
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearToDateValue : ZDecimal.Zero; }
		}
		public ZPropertyInfo CommittedYearToDateValueInfo => GetZPropertyInfo(nameof(CommittedYearToDateValue));

		public ZDecimal CommittedYearToDateTEUQuantity
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? CommittedCurrentFinancialYearToDateTEUQuantity : ZDecimal.Zero; }
		}
		public ZPropertyInfo CommittedYearToDateTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedYearToDateTEUQuantity));

		public ZDecimal CommittedAndForecastCurrentYearValue
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? (CommittedCurrentFinancialYearValue + ForecastCurrentFinancialYearValue) : (CommittedTrailing12MonthsValue + ForecastTrailing12MonthsValue); }
		}
		public ZPropertyInfo CommittedAndForecastCurrentYearValueInfo => GetZPropertyInfo(nameof(CommittedAndForecastCurrentYearValue));

		public ZDecimal CommittedAndForecastCurrentYearTEUQuantity
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? (CommittedCurrentFinancialYearTEUQuantity + ForecastCurrentFinancialYearTEUQuantity) : (CommittedTrailing12MonthsTEUQuantity + ForecastTrailing12MonthsTEUQuantity); }
		}
		public ZPropertyInfo CommittedAndForecastCurrentYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedAndForecastCurrentYearTEUQuantity));

		public ZDecimal CommittedAndForecastNextYearValue
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? (CommittedNextFinancialYearValue + ForecastNextFinancialYearValue) : (CommittedNext12MonthsValue + ForecastNext12MonthsValue); }
		}
		public ZPropertyInfo CommittedAndForecastNextYearValueInfo => GetZPropertyInfo(nameof(CommittedAndForecastNextYearValue));

		public ZDecimal CommittedAndForecastNextYearTEUQuantity
		{
			get { return RevenueDisplayOption == SalesHeader.RevenueDisplay.FinancialYear ? (CommittedNextFinancialYearTEUQuantity + ForecastNextFinancialYearTEUQuantity) : (CommittedNext12MonthsTEUQuantity + ForecastNext12MonthsTEUQuantity); }
		}
		public ZPropertyInfo CommittedAndForecastNextYearTEUQuantityInfo => GetZPropertyInfo(nameof(CommittedAndForecastNextYearTEUQuantity));

		#region Committed Values

		ZDecimal CommittedCurrentFinancialYearToDateValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal CommittedCurrentFinancialYearToDateTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYearToDate, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal CommittedCurrentFinancialYearValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal CommittedCurrentFinancialYearTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal CommittedNextFinancialYearValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal CommittedNextFinancialYearTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal CommittedTrailing12MonthsValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal CommittedTrailing12MonthsTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal CommittedNext12MonthsValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal CommittedNext12MonthsTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Committed, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		#endregion

		#region Forecast Values

		ZDecimal ForecastCurrentFinancialYearValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal ForecastCurrentFinancialYearTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.CurrentFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal ForecastNextFinancialYearValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal ForecastNextFinancialYearTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.NextFinancialYear, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal ForecastTrailing12MonthsValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal ForecastTrailing12MonthsTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Trailing12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		ZDecimal ForecastNext12MonthsValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		ZDecimal ForecastNext12MonthsTEUQuantity => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Forecast, SalesValueCalculator.PeriodRange.Next12Months, companyPk: CompanyFilter, tradeLane: prospectSales).TEUQuantity;

		#endregion

		#region Pipeline Value

		[ResourceStringData("OrgSalesActualsInformation|PipelineValue", Caption = "Pipeline (p.a)")]
		public ZDecimal PipelineValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Pipeline, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		public ZPropertyInfo PipelineValueInfo => GetZPropertyInfo(nameof(PipelineValue));

		#endregion

		#region Unsuccessful Value

		[ResourceStringData("OrgSalesActualsInformation|UnsuccessfulValue", Caption = "Unsuccessful (p.a)")]
		public ZDecimal UnsuccessfulValue => ValueCalculator.GetTradeLaneValue(SalesValueCalculator.ValueType.Unsuccessful, SalesValueCalculator.PeriodRange.None, companyPk: CompanyFilter, tradeLane: prospectSales).TotalRevenue;

		public ZPropertyInfo UnsuccessfulValueInfo => GetZPropertyInfo(nameof(UnsuccessfulValue));

		#endregion

		#endregion

		#region Status

		[ResourceStringData("OrgSalesActualsInformation|Status", Caption = "Status")]
		public ZString Status
		{
			get
			{
				if (ActualsFirstTraded.IsEmpty || prospectSales.OW_LatestProspectDate > ActualsLastTraded)
				{
					return OrgSalesActualsStatusList.Codes.Prospective;
				}
				else if (ActualsLastTraded < OrgSales.GetLostCutoffDate())
				{
					return OrgSalesActualsStatusList.Codes.Lost;
				}
				else
				{
					return OrgSalesActualsStatusList.Codes.Traded;
				}
			}
		}

		public ZPropertyInfo StatusInfo => GetZPropertyInfo(Schema.Status);

		#endregion

		#region Calculators

		OrgSalesRevenueCalculator RevenueCalculator => revenueCalculator ?? (revenueCalculator = new OrgSalesRevenueCalculator(Factory, parentOrg));
		OrgSalesRevenueCalculator revenueCalculator;

		SalesValueCalculator ValueCalculator => valueCalculator ?? (valueCalculator = new SalesValueCalculator(Factory, parentOrg.PK, RevenueCalculator));
		SalesValueCalculator valueCalculator;

		#endregion

		public void RefreshCachedValues()
		{
			ValueCalculator.InvalidateCache();
			CommittedYearToDateValueInfo.RefreshBinding();
			CommittedYearToDateTEUQuantityInfo.RefreshBinding();
			CommittedCurrentYearValueInfo.RefreshBinding();
			CommittedCurrentYearTEUQuantityInfo.RefreshBinding();
			CommittedAndForecastCurrentYearValueInfo.RefreshBinding();
			CommittedAndForecastCurrentYearTEUQuantityInfo.RefreshBinding();
			CommittedNextYearValueInfo.RefreshBinding();
			CommittedNextYearTEUQuantityInfo.RefreshBinding();
			CommittedAndForecastNextYearValueInfo.RefreshBinding();
			CommittedAndForecastNextYearTEUQuantityInfo.RefreshBinding();
		}
	}
}
