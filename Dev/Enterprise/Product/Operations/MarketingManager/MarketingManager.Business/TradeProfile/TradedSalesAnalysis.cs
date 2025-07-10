using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesAnalysis : AutoTradedSalesAnalysis
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public TradedSalesAnalysis(SalesHeader salesHeader)
			: base(salesHeader.Factory)
		{
			Argument.NotNull(salesHeader, "salesHeader");

			this.SalesHeader = salesHeader;
			LocationGroupingType = GetDefaultLocationGroupingType(SalesHeader.SalesProductCode);
			MainGroupingType = GetDefaultMainGroupingType(SalesHeader.SalesProductCode);

			IBusinessObjectCollection collection = this.SalesHeader.TradedSalesCollectionProductView;
			collection.ListChanged += TradedSalesCollection_ListChanged;
		}

		public readonly SalesHeader SalesHeader;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			Period = SalesAnalysisPeriodList.Codes.Trailing12Months;
			IncludeJobValue = true;
		}

		static ZString GetDefaultLocationGroupingType(ZString productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.Transport:
					return SalesAnalysisLocationGroupingTypeList.Codes.StateToState;

				case SystemDefinedSalesProductList.Codes.Warehouse:
					return SalesAnalysisLocationGroupingTypeList.Codes.Warehouse;

				default:
					return SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry;
			}
		}

		static ZString GetDefaultMainGroupingType(ZString productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.Warehouse:
					return SalesAnalysisMainGroupingTypeList.Codes.Service;

				default:
					return SalesAnalysisMainGroupingTypeList.Codes.ModeAndType;
			}
		}

		#endregion

		#region Properties

		[List("MainGroupingTypeList")]
		public override ZString MainGroupingType
		{
			get { return base.MainGroupingType; }
			set
			{
				if (base.MainGroupingType != value)
				{
					base.MainGroupingType = value;
					RefreshMainGroupings();
				}
			}
		}

		[List("LocationGroupingTypeList")]
		public override ZString LocationGroupingType
		{
			get { return base.LocationGroupingType; }
			set
			{
				if (base.LocationGroupingType != value)
				{
					base.LocationGroupingType = value;
					if (MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Location)
					{
						RefreshMainGroupings();
					}
					else
					{
						RefreshTreeGroupers();
					}
				}
			}
		}

		[List("PeriodList")]
		public override ZString Period
		{
			get { return base.Period; }
			set
			{
				if (base.Period != value)
				{
					base.Period = value;
					RefreshMainGroupings();
				}
			}
		}

		[BusinessObjectTestExclude]
		[List("PeriodDescriptionList")]
		[MaxLength("PeriodDescriptionMaxLength")]
		[ResourceStringData("TradedSalesAnalysis|PeriodDescription", Caption = "Analysis Period")]
		public ZString PeriodDescription
		{
			get { return PeriodList.GetDescriptionFromCode(Period); }
			set
			{
				Period = PeriodList.GetCodeFromDescription(value);
			}
		}

		public int PeriodDescriptionMaxLength
		{
			get { return PeriodDescriptionList.MaxCodeLength; }
		}

		public ZPropertyInfo PeriodDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PeriodDescription), x => PeriodInfo); }
		}

		public SalesAnalysisPeriodList PeriodList
		{
			get { return periodList ?? (periodList = new SalesAnalysisPeriodList()); }
		}
		SalesAnalysisPeriodList periodList;

		#endregion

		#region ViewpointOrg

		public OrgHeader ViewpointOrg
		{
			get { return viewpointOrg; }
		}
		OrgHeader viewpointOrg;

		public void SetViewpointOrg(OrgHeader org)
		{
			viewpointOrg = org;
		}

		#endregion

		#region MainGrouping

		public TradePeriodGroupingCollection MainGroupings
		{
			get
			{
				if (mainGroupings == null)
				{
					mainGroupings = new TradePeriodGroupingCollection();
					RefreshMainGroupings();
				}

				return mainGroupings;
			}
		}
		TradePeriodGroupingCollection mainGroupings;

		void RefreshMainGroupings()
		{
			if (mainGroupings != null)
			{
				var firstDayOfCurrentCalendarMonth = new ZDate(ZDate.Today.Year, ZDate.Today.Month, 1);
				var earliestStartDate = SalesAnalysisPeriodListUtils.GetEarliestStartDateForPeriod(firstDayOfCurrentCalendarMonth, Period);
				var endDateBound = SalesAnalysisPeriodListUtils.GetEndDateBoundForPeriod(firstDayOfCurrentCalendarMonth, Period);

				var tradedSales = SalesHeader.TradedSales.Where(x => !x.IsDeleted);
				var context = new SalesLoader.LoadingContext()
				{
					OrgPk = SalesHeader.ViewingOrg.PK,
					PeriodFrom = earliestStartDate,
					PeriodTo = endDateBound,
					IsPeriodToInclusive = false,
					IsTraded = true,
					IsForecast = false,
					TradeStatus = ZString.Empty,
					CompanyPk = SalesHeader.CompanyFilter,
					LoadByOption = SalesLoader.LoadingContext.LoadBy.SelectedSales,
					SelectedSalesPks = tradedSales.Select(x => x.PK)
				};
				var tradedPeriods = SalesLoader.LoadPeriods(Factory, context);

				var grouper = GetMainGrouper();
				using (mainGroupings.SuspendListChanged())
				{
					mainGroupings.RemoveAndDeleteAll();
					if (grouper != null && tradedPeriods.Any())
					{
						grouper.AddFetchHints(tradedPeriods);
						mainGroupings.AddRange(grouper.GetGroupings(tradedPeriods).Select(x => new TradePeriodGrouping(this, x.GroupedTradePeriods, grouper.GetType())));
					}

					RefreshTreeGroupers();
				}
			}

			OnMainGroupingChanged();
		}

		void OnMainGroupingChanged()
		{
			MainGroupingChanged?.Invoke(this, EventArgs.Empty);
		}
		public event EventHandler MainGroupingChanged;

		TradePeriodGrouper GetMainGrouper()
		{
			switch (MainGroupingType)
			{
				case SalesAnalysisMainGroupingTypeList.Codes.Location:
					return GetMainGrouperForLocation(LocationGroupingType);

				case SalesAnalysisMainGroupingTypeList.Codes.Service:
					return new TradePeriodServiceGrouper();

				case SalesAnalysisMainGroupingTypeList.Codes.Mode:
					return new TradePeriodModeGrouper();

				case SalesAnalysisMainGroupingTypeList.Codes.ModeAndType:
					return new TradePeriodModeAndTypeGrouper();

				case SalesAnalysisMainGroupingTypeList.Codes.Product:
					return new TradePeriodSupplierPartGrouper();
			}

			return null;
		}

		static TradePeriodGrouper GetMainGrouperForLocation(string locationGroupingType)
		{
			switch (locationGroupingType)
			{
				case SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry:
					return new TradePeriodCountryToCountryGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry:
					return new TradePeriodDestinationCountryGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.DestinationState:
					return new TradePeriodDestinationStateGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest:
					return new TradePeriodOriginToDestinationDescriptionGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry:
					return new TradePeriodOriginCountryGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.OriginState:
					return new TradePeriodOriginStateGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.StateToState:
					return new TradePeriodStateToStateGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco:
					return new TradePeriodUnlocoToUnlocoGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.Warehouse:
					return new TradePeriodWarehouseGrouper();

				case SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry:
					return new TradePeriodWarehouseCountryGrouper();
			}
			return null;
		}

		#endregion

		#region TreeGroupers

		public IEnumerable<TradePeriodGrouper> TreeGroupers
		{
			get
			{
				if (treeGroupers == null)
				{
					treeGroupers = GetTreeModelGroupers().ToArray();
				}
				return treeGroupers;
			}
			set
			{
				if (treeGroupers != value)
				{
					treeGroupers = value;
					OnTreeGroupingChanged();
				}
			}
		}
		IEnumerable<TradePeriodGrouper> treeGroupers;

		void RefreshTreeGroupers()
		{
			TreeGroupers = GetTreeModelGroupers().ToArray();
		}

		TradePeriodGrouper GetLowestLevelGrouper()
		{
			switch (SalesHeader.SalesProductCode)
			{
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					return new TradePeriodUnlocoToUnlocoGrouper();

				case SystemDefinedSalesProductList.Codes.Warehouse:
					if (MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Product)
					{
						return new TradePeriodServiceGrouper();
					}
					else
					{
						return new TradePeriodSupplierPartGrouper();
					}

				default:
					return new TradePeriodOriginToDestinationDescriptionGrouper();
			}
		}

		IEnumerable<TradePeriodGrouper> GetTreeModelGroupers()
		{
			foreach (var grouper in GetTreeModelGroupersInner())
			{
				yield return grouper;
			}

			yield return new TradePeriodSupplierAndBuyerGrouper();
		}

		IEnumerable<TradePeriodGrouper> GetTreeModelGroupersInner()
		{
			if (MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Location)
			{
				if (SalesHeader.SalesProductCode == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					yield return new TradePeriodServiceGrouper();
					yield return new TradePeriodSupplierPartGrouper();
				}
				else
				{
					yield return new TradePeriodModeGrouper();
					yield return new TradePeriodTypeGrouper();
				}
			}
			else if (MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Service
				|| MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Mode
				|| MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.ModeAndType
				|| MainGroupingType == SalesAnalysisMainGroupingTypeList.Codes.Product)
			{
				if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry)
				{
					yield return new TradePeriodCountryToCountryGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest)
				{
					yield return new TradePeriodOriginToDestinationDescriptionGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco)
				{
					yield return new TradePeriodUnlocoToUnlocoGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry)
				{
					yield return new TradePeriodOriginCountryGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry)
				{
					yield return new TradePeriodDestinationCountryGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.DestinationState)
				{
					yield return new TradePeriodDestinationStateGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.OriginState)
				{
					yield return new TradePeriodOriginStateGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.StateToState)
				{
					yield return new TradePeriodStateToStateGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.Warehouse)
				{
					yield return new TradePeriodWarehouseGrouper();
					yield return GetLowestLevelGrouper();
				}
				else if (LocationGroupingType == SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry)
				{
					yield return new TradePeriodWarehouseCountryGrouper();
					yield return GetLowestLevelGrouper();
				}
			}

			if (SalesHeader.SalesProductCode == SystemDefinedSalesProductList.Codes.CustomsBrokerage)
			{
				yield return new TradePeriodCompanyGrouper();
			}
		}

		#region TreeGroupingChanged

		void OnTreeGroupingChanged()
		{
			TreeGroupingChanged?.Invoke(this, EventArgs.Empty);
		}
		public event EventHandler TreeGroupingChanged;

		#endregion

		#endregion

		#region Collections

		void TradedSalesCollection_ListChanged(object sender, EventArgs e)
		{
			RefreshMainGroupings();
		}

		#endregion

		#region Lookups

		#region MainGroupingTypeList

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ICodeDescriptionPairList MainGroupingTypeList
		{
			get
			{
				var mainGroupingTypeCodes = GetMainGroupingTypeCodes(SalesHeader.SalesProductCode);
				return GetSubsetList(new SalesAnalysisMainGroupingTypeList(), mainGroupingTypeCodes);
			}
		}

		static IEnumerable<string> GetMainGroupingTypeCodes(string productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.Warehouse:
					return GetMainGroupingTypeCodes_Warehouse();

				default:
					return GetMainGroupingTypeCodes_ForwardingShipment();
			}
		}

		static IEnumerable<string> GetMainGroupingTypeCodes_ForwardingShipment()
		{
			yield return SalesAnalysisMainGroupingTypeList.Codes.Location;
			yield return SalesAnalysisMainGroupingTypeList.Codes.Mode;
			yield return SalesAnalysisMainGroupingTypeList.Codes.ModeAndType;
		}

		static IEnumerable<string> GetMainGroupingTypeCodes_Warehouse()
		{
			yield return SalesAnalysisMainGroupingTypeList.Codes.Location;
			yield return SalesAnalysisMainGroupingTypeList.Codes.Product;
			yield return SalesAnalysisMainGroupingTypeList.Codes.Service;
		}

		#endregion

		#region LocationGroupingTypeList

		public ICodeDescriptionPairList LocationGroupingTypeList
		{
			get
			{
				var locationGroupingTypeCodes = GetLocationGroupingTypeCodes(SalesHeader.SalesProductCode);
				return GetSubsetList(new SalesAnalysisLocationGroupingTypeList(), locationGroupingTypeCodes);
			}
		}

		static IEnumerable<string> GetLocationGroupingTypeCodes(string productCode)
		{
			switch (productCode)
			{
				case SystemDefinedSalesProductList.Codes.ForwardingShipment:
				case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
				case SystemDefinedSalesProductList.Codes.LinerAgency:
					return GetLocationGroupingTypeCodes_ForwardingShipment();

				case SystemDefinedSalesProductList.Codes.Transport:
					return GetLocationGroupingTypeCodes_Transport();

				case SystemDefinedSalesProductList.Codes.Warehouse:
					return GetLocationGroupingTypeCodes_Warehouse();
			}

			return Enumerable.Empty<string>();
		}

		static IEnumerable<string> GetLocationGroupingTypeCodes_Transport()
		{
			yield return SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.DestinationState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.LowestToLowest;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.OriginState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.StateToState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.Warehouse;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry;
		}

		static IEnumerable<string> GetLocationGroupingTypeCodes_ForwardingShipment()
		{
			yield return SalesAnalysisLocationGroupingTypeList.Codes.CountryToCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.DestinationCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.DestinationState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.OriginCountry;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.OriginState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.StateToState;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.UnlocoToUnloco;
		}

		static IEnumerable<string> GetLocationGroupingTypeCodes_Warehouse()
		{
			yield return SalesAnalysisLocationGroupingTypeList.Codes.Warehouse;
			yield return SalesAnalysisLocationGroupingTypeList.Codes.WarehouseCountry;
		}

		#endregion

		static CodeDescriptionPairList GetSubsetList(ICodeDescriptionPairList list, IEnumerable<string> itemCodesToInclude)
		{
			var codeLookup = new HashSet<string>(itemCodesToInclude);
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription item in list)
			{
				if (codeLookup.Contains(item.Code))
				{
					result.Add(item);
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public CodeDescriptionPairList PeriodDescriptionList => SalesAnalysisPeriodListUtils.GetPeriodDescriptionList(PeriodList);

		#endregion
	}
}
