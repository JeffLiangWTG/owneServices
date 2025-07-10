using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public abstract class RateEntryCollection : BusinessObjectCollection<RateEntry>, IHaveZQueryForZGridExcelExport, IImportCollectionElementMatchingSupporter
	{
		protected RateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(factory)
		{
			Parent = (RatingHeader)master;
			IsManagedForDataRefresh = false;
		}

		#region Rate Entry Category

		/// <summary>
		/// Returns the type of Rate Entry (LCL, FCL, AIR, DST, ORG).
		/// Used when loading collections
		/// </summary>
		public abstract ZString RateEntryType { get; }

		protected virtual ZString DefaultRateEntryMode { get; private set; }

		public virtual ZString CategoryForFiltering
		{
			get { return RateEntryType; }
		}

		#endregion

		#region Type

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			if (fTypeOfElements == null)
			{
				fTypeOfElements = Parent != null ? Parent.RateEntryType : base.GetTypeOfElementsFromPK(pK);
			}

			return fTypeOfElements;
		}

		Type fTypeOfElements;

		#endregion

		public RatingHeader Parent { get; }

		#region Load

		protected override ZQuery CreateRelationshipFilter()
		{
			return Parent.GetRelationshipFilter();
		}

		/// <summary>
		/// Load records matching the filter, up to the maximum number allowed by the registry setting 
		/// SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value
		/// </summary>
		public void LoadAndSortForGUI(bool includeUserFilter = true, bool allowOneMoreThanMaximumNumber = false)
		{
			var filter = GetLoadFilter(includeUserFilter);
			filter.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value
				+ (allowOneMoreThanMaximumNumber ? 1 : 0);

			using (SuspendListChanged())
			{
				base.Load(filter);
				Sort();
			}

			UpdateLineFilter(includeUserFilter);

			if (Parent.IsAdditionalTariff())
			{
				SetReadOnlyIncludingChildren(false);
			}
		}

		void UpdateLineFilter(bool includeUserFilter)
		{
			if (includeUserFilter && userFilterStrip != null && Count > 0)
			{
				var lineFilter = userFilterStrip.RateLineFilter;
				foreach (RateEntry entry in this)
				{
					entry.SetLineFilter(lineFilter);
				}
			}
		}

		public int GetDatabaseCount() => Factory.GetDatabaseCount(typeof(RateEntry), GetCompleteLoadFilter(GetLoadFilter()));

		/// <summary>
		/// Load the collection based on RateEntryType. Will also include the Query from the UserFilter
		/// (i.e. the Grid above the Collection on RatingHeader Forms) or fallback to the DefaultUserFilter.
		/// </summary>
		public override void Load()
		{
			base.Load(GetLoadFilter());

			if (Parent.IsAdditionalTariff())
			{
				SetReadOnlyIncludingChildren(false);
			}
		}

		internal ZQuery GetLoadFilter(bool includeUserFilter = true)
		{
			var filter = new ZQuery();

			if (!RateEntryType.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, RateEntryType);
			}

			if (includeUserFilter)
			{
				var userFilter = userFilterStrip?.Filter ?? DefaultUserFilter;
				filter.AddToFilter(userFilter, JoinCondition.And);
			}

			return filter;
		}

		ZQuery IHaveZQueryForZGridExcelExport.Query
		{
			get
			{
				return GetCompleteLoadFilter(GetLoadFilter());
			}
		}

		internal ZQuery GetEntireLoadFilterWithoutUserFilter()
		{
			return GetCompleteLoadFilter(GetLoadFilter(false));
		}

		internal void ClearUserFilterStrips()
		{
			userFilterStrip?.ClearRateEntryFilterStrips();
		}

		#endregion

		#region User Filter

		public void SetUserFilter(IRateEntryFilterStripBusinessObject strip)
		{
			userFilterStrip = strip ?? throw new ArgumentNullException(nameof(strip));
		}

		IRateEntryFilterStripBusinessObject userFilterStrip;

		/// <summary>
		/// Hard coded filter for non-Quotation RatingHeaders that will filter out Rate Entries that have expired or published globally.
		/// </summary>
		ZQuery DefaultUserFilter => Factory.GetCachedValue("RateEntryCollection.DefaultUserFilter" + Parent.PK + Parent.GlobalRatingHeaderKey, GetDefaultUserFilter);

		ZQuery GetDefaultUserFilter()
		{
			if (Parent.IsQuote())
			{
				return new ZQuery();
			}

			var notExpiredQuery = new ZQuery(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			notExpiredQuery.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, null);

			var defaultQuery = new ZQuery(notExpiredQuery);

			var hasGlobalRatingHeader = !Parent.IsAdditionalTariff() && Parent.GlobalRatingHeader != null;
			if (hasGlobalRatingHeader)
			{
				defaultQuery.AddToFilter(RateEntrySchema.TI_TH, SQLComparisonOperator.NotEqual, Parent.GlobalRatingHeader.PK);
				defaultQuery.AddToFilter(RateEntrySchema.TI_GC_Publisher, Parent.TH_GC);
			}

			return defaultQuery;
		}

		#endregion

		#region Adding

		protected override BusinessObject AddNewCore()
		{
			var newEntry = (RateEntry)base.AddNewCore();
			newEntry.TI_TH = Parent.PK;

			return newEntry;
		}

		#endregion

		#region Sort

		void Sort()
		{
			if (this.HasErrors())
			{
				Sort(new RateEntryWithErrorsComparer());
			}
			else if (SortInformation == null)
			{
				Sort(RateEntrySchema.TI_LineOrder.Name, System.ComponentModel.ListSortDirection.Ascending);
			}
			else
			{
				Sort(SortInformation);
			}
		}

		/// <summary>
		/// Sorts Rate Entries so that Rate Entries with errors appear at the top of the collection.
		/// If not sorting entries with errors, then use ascending line order.
		/// </summary>
		class RateEntryWithErrorsComparer : IComparer<RateEntry>
		{
			int IComparer<RateEntry>.Compare(RateEntry x, RateEntry y)
			{
				if (x.HasErrors)
				{
					if (y.HasErrors)
					{
						return GetLineOrderDifference(x, y);
					}

					return -1;
				}

				if (y.HasErrors)
				{
					return 1;
				}

				return GetLineOrderDifference(x, y);
			}

			static int GetLineOrderDifference(RateEntry x, RateEntry y)
			{
				var lineOrderDifference = x.TI_LineOrder - y.TI_LineOrder;
				var result = lineOrderDifference > 0
					? 1
					: lineOrderDifference < 0
						? -1
						: 0;

				return result;
			}
		}

		#endregion

		#region AllowNew

		protected override bool AllowNewCore
		{
			get { return !Parent.IsAdditionalTariff() && base.AllowNewCore; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var rateEntryChild = (RateEntry)child;

			rateEntryChild.TI_TH = Parent.PK;

			if (!Parent.EntryLineOrderSuspended)
			{
				rateEntryChild.TI_LineOrder = (ZShort)RatingHelper.MaxPlus1(this, RateEntrySchema.TI_LineOrder.Name);
			}

			if (!RateEntryType.IsEmpty)
			{
				rateEntryChild.TI_RateCategory = RateEntryType;
			}

			if (!DefaultRateEntryMode.IsEmpty)
			{
				rateEntryChild.TI_Mode = DefaultRateEntryMode;
			}

			if (Parent.TH_OneTimeQuote || Parent.IsQuote())
			{
				rateEntryChild.TI_RateStartDate = Parent.TH_QuoteDate;
				rateEntryChild.TI_RateEndDate = Parent.TH_QuoteEndDate;
			}

			if (Parent.IsCosting())
			{
				rateEntryChild.TI_RateEndDate = rateEntryChild.DefaultEndDate(Env.Registry.Rating.CostRateValidityPeriod);
			}

			if (Parent.IsTariff())
			{
				rateEntryChild.TI_RateEndDate = rateEntryChild.DefaultEndDate(Env.Registry.Rating.GlobalTariffValidityPeriod);
			}

			if (Parent.IsClientRate())
			{
				rateEntryChild.TI_RateEndDate = rateEntryChild.DefaultEndDate(Env.Registry.Rating.RateValidityPeriod);
			}

			if (rateEntryChild.IsWHS() || rateEntryChild.IsTRW() || rateEntryChild.IsTWU())
			{
				rateEntryChild.TI_Mode = Core.Constants.RateMode.ALL;
			}

			var parentCompany = Parent.Company;
			if (parentCompany != null)
			{
				rateEntryChild.TI_GC_Publisher = parentCompany.PK;
				rateEntryChild.TI_RX_NKCurrency = parentCompany.GC_RX_NKLocalCurrency;
			}
			else
			{
				rateEntryChild.TI_GC_Publisher = Env.CurrentCompanyPK;
				rateEntryChild.TI_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			}

			if (!Parent.TH_OneTimeQuote && !rateEntryChild.IsWHS() && !rateEntryChild.IsTRW() && !rateEntryChild.IsTWU() && !rateEntryChild.IsPortTransport() && !rateEntryChild.IsDomesticTransport() && !(rateEntryChild.TI_RateCategory == RatingConstants.RateCategory.CST) && !rateEntryChild.IsContainerYard() && !rateEntryChild.IsContainerYardTPU())
			{
				var defaultCommodityCode = Factory.Load<RefCommodityCode>(Env.Registry.CommodityCode);
				if (defaultCommodityCode != null)
				{
					rateEntryChild.TI_RH_NKCommodityCode = defaultCommodityCode.RH_Code;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		protected override void OnAdded(BusinessObject bizO)
		{
			base.OnAdded(bizO);

			var entry = (RateEntry)bizO;
			AddDefaultRateLines(entry);
		}

		/// <summary>
		/// A RateEntryCollection holds RateEntries.
		/// When a RateEntry is added to a RateEntryCollection, it may be necessary to add some
		/// default RateLines. For example, creating a RateEntry in the FCL RateEntryCollection
		/// can result in FRT RateLines being added automatically.
		/// However, when importing a RateEntry it is necessary to avoid adding these default
		/// RateLines. Otherwise can end up with duplicated and unnecessary RateLines.
		/// </summary>
		void AddDefaultRateLines(RateEntry entry)
		{
			var parent = entry.Parent ?? Parent;

			if (!entry.IsInDatabase && !entry.HasRateLines && !parent.IsPreSaveValidating)
			{
				// Checks whether the RateEntryCollection's Factory is marked as importing, or
				// whether the entry being added is marked as imported.
				var isImporting = DataImportIndicatorService.GetInstance(Factory).IsDataImportInProgress
					|| ((ISupportDataImporting)parent).IsImportingData;

				if (!isImporting)
				{
					var chargeCodesPKs = DefaultChargeCodes(entry);
					foreach (var chargeCodePK in chargeCodesPKs)
					{
						var chargeCode = GetChargeCode(entry, chargeCodePK);

						var rateLine = entry.RateLines.AddNew();
						rateLine.TL_AC = chargeCode != null ? chargeCode.PK : ZGuid.Invalid;
						rateLine.TL_WeightVolume = rateLine.TL_WeightVolumeInfo.ReadOnly ? ZString.Empty : entry.Unit;
						rateLine.TL_RX_NKCurrency = entry.TI_RX_NKCurrency;

						if (chargeCodePK == Env.Registry.GetFreightChargeCode(entry.Company()?.PK.ToGuid() ?? Guid.Empty))
						{
							rateLine.TL_RateCalculator = DefaultFreightChargeCalculator;
							AddFreightLineItems(rateLine);
						}
					}

					((IBusinessObjectCollectionInternals)entry.RateLines).FireListResetEvent();
				}
			}
		}

		AccChargeCode GetChargeCode(RateEntry entry, Guid chargeCodePK)
		{
			var chargeCode = Factory.Load<AccChargeCode>(chargeCodePK);
			if (entry.IsGlobal())
			{
				var ledgerType = entry.IsCosting() || entry.IsWiseCost() ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
				chargeCode = chargeCode.GetGlobalChargeCode(ledgerType, null);
			}

			return chargeCode;
		}

		protected virtual Guid[] DefaultChargeCodes(RateEntry entry)
		{
			return Array.Empty<Guid>();
		}

		protected virtual string DefaultFreightChargeCalculator
		{
			get { return CombinedCalculator.Code; }
		}

		protected virtual void AddFreightLineItems(RateLine freightRateLine)
		{
		}

		#endregion

		#region Validation

		protected override bool RunPreSaveValidationCore()
		{
			var validationResult = base.RunPreSaveValidationCore();

			using (SuspendListChanged())
			{
				Sort();
			}

			return validationResult;
		}

		public new void SetReadOnlyIncludingChildren(bool isReadOnly)
		{
			foreach (RateEntry rateEntry in this)
			{
				rateEntry.ReadOnly = isReadOnly || Parent.IsAdditionalTariff();
				rateEntry.RateLines.SetReadOnlyIncludingChildren(isReadOnly);
			}
		}

		#endregion

		#region Pre-Fetch

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new RateEntryCollectionFetchStrategy(this);
		}

		#endregion

		#region IImportCollectionElementMatchingSupporter

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => String.Empty;

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => true;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => true;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string value)
		{
			return null;
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
		}

		#endregion
	}
}
