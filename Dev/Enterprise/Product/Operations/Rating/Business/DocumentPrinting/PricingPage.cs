using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration.DocRollUpSort;
using Enterprise.Rating.Business.RatingEnums;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public sealed class PricingPage : NonPersistentBusinessObject, IVisualizerNoteSupporter, IDocHeader
	{
		internal PricingPage(RateEntry rateEntry, BusinessObjectFactory factory, PricingPageStyle style, IEqualityComparer<RateEntry> rateEntryComparer, bool viewAgentRates = false)
			: base(factory)
		{
			Style = style;
			uniqueRateEntries = new HashSet<RateEntry>(rateEntryComparer);
			ContainerSet = new HashSet<RefContainer>();
			FirstRateEntry = rateEntry;
			ViewAgentRates = viewAgentRates;
			AddRateEntry(rateEntry);
		}

		public PricingPage(RateEntry rateEntry, BusinessObjectFactory factory, PricingPageStyle style, bool viewAgentRates = false)
			: this(rateEntry, factory, style, new PricingPageRateEntryComparer(), viewAgentRates)
		{
		}

		public new object this[string propertyName] => FirstRateEntry?[propertyName] ?? ZString.Empty;

		/// <summary>
		/// If adding a series of entries, please use the AddEntries method as
		/// it is optimized for bulk adding
		/// </summary>
		/// <param name="rateEntry"></param>
		public void AddRateEntry(RateEntry rateEntry)
		{
			if (rateEntry != null)
			{
				uniqueRateEntries.Add(rateEntry);

				if (rateEntry.Container != null)
				{
					ContainerSet.Add(rateEntry.Container);
				}

				if (rateEntry.IsFCL() && rateEntry.Container == null)
				{
					HasFCLWithEmptyContainer = true;
				}

				if (rateEntry.IsLCLFreight() || rateEntry.TI_RateCategory == RatingConstants.RateCategory.SNC)
				{
					ContainsLCL = true;
				}
			}
		}

		public IEnumerable<RateEntry> RateEntries
		{
			[DebuggerStepThrough]
			get { return uniqueRateEntries; }
		}

		public bool ContainsLCL { get; private set; }
		public PricingPageStyle Style { get; }
		public HashSet<RefContainer> ContainerSet { get; }
		public RateEntry FirstRateEntry { get; }
		public bool ViewAgentRates { get; }
		HashSet<RateEntry> uniqueRateEntries { get; }

		public bool HasFCLWithEmptyContainer { get; set; }

		#region Related Entries

		public RelatedRateEntries GetRelatedRateEntries(RateEntry parentRateEntry, bool exactMatch) => RelatedRateEntriesLoader.GetRelatedEntries(parentRateEntry, exactMatch);

		/// <summary>
		/// Related Entries try to match Origin and Destination Rate Entries to Freight Rate Entries and include matching lines.
		/// If all the Rate Lines that are visible on a Pricing Page have already been included, then there is no point showing the
		/// Origin/Destination Rate Entry twice.
		/// </summary>
		internal bool IsOriginEntryPrintedByFreightRelatedLineSet(RateEntry originRateEntry, RateLine[] visibleRateLines)
		{
			foreach (var freightEntry in FreightRateEntries)
			{
				var relatedEntries = relatedRateEntriesCache.GetOrAdd
				(
					freightEntry.PK,
					() => RelatedRateEntriesLoader.GetRelatedEntries(freightEntry, exactMatch: false)
				);
				if (relatedEntries.OriginRateEntries.Any(c => c.PK == originRateEntry.PK))
				{
					return visibleRateLines.All(visibleLine => FreightRelatedOriginRateLineListList.ContainsLine(visibleLine));
				}
			}

			return false;
		}

		internal bool IsDestinationEntryPrintedByFreightRelatedLineSet(RateEntry destinationRateEntry, RateLine[] visibleRateLines)
		{
			foreach (var freightRateEntry in FreightRateEntries)
			{
				var relatedRateEntries = relatedRateEntriesCache.GetOrAdd
				(
					freightRateEntry.PK,
					() => RelatedRateEntriesLoader.GetRelatedEntries(freightRateEntry, exactMatch: false)
				);
				if (relatedRateEntries.DestinationRateEntries.Any(c => c.PK == destinationRateEntry.PK))
				{
					return visibleRateLines.All(visibleLine => FreightRelatedDestinationRateLineListList.ContainsLine(visibleLine));
				}
			}

			return false;
		}

		PricingPageRateLineListList FreightRelatedOriginRateLineListList => freightRelatedOriginRateLineListList ??= new PricingPageRateLineFactory(EntryTypes.Origin).LoadLineSets(this, FreightRateEntries);
		PricingPageRateLineListList freightRelatedOriginRateLineListList;

		PricingPageRateLineListList FreightRelatedDestinationRateLineListList => freightRelatedDestinationRateLineListList ??= new PricingPageRateLineFactory(EntryTypes.Destination).LoadLineSets(this, FreightRateEntries);
		PricingPageRateLineListList freightRelatedDestinationRateLineListList;

		RateEntry[] FreightRateEntries => freightRateEntries ??= RateEntries.Where(rateEntry => rateEntry.IsFreightEntry()).ToArray();
		RateEntry[] freightRateEntries;

		RelatedRateEntriesLoader RelatedRateEntriesLoader => relatedRateEntriesLoader ??= new RelatedRateEntriesLoader(this, Factory);
		RelatedRateEntriesLoader relatedRateEntriesLoader;

		readonly Dictionary<ZGuid, RelatedRateEntries> relatedRateEntriesCache = new Dictionary<ZGuid, RelatedRateEntries>();

		#endregion

		#region RollUp

		public ZString RollUpDisplay
		{
			get
			{
				var (transportMode, containerMode, jobType, orgHeader) = GetInfoForRollUp();
				return new RatingDocRollupOrSortLoader
				(
					Factory,
					orgHeader,
					GlbBranch.CurrentBranch,
					GlbDepartment.CurrentDepartment
				)
				.GetDisplay
				(
					serviceDirection: "",
					transportMode: transportMode,
					containerMode: containerMode,
					jobType
				);
			}
		}

		public ZString RollUpStyle
		{
			get
			{
				var (transportMode, containerMode, jobType, orgHeader) = GetInfoForRollUp();
				return new RatingDocRollupOrSortLoader
				(
					Factory,
					orgHeader,
					GlbBranch.CurrentBranch,
					GlbDepartment.CurrentDepartment
				)
				.GetStyle
				(
					serviceDirection: "",
					transportMode: transportMode,
					containerMode: containerMode,
					jobType
				);
			}
		}

		(string transportMode, string containerMode, string jobType, OrgHeader orgHeader) GetInfoForRollUp()
		{
			var rateEntry = FirstRateEntry;

			var transportMode = GetDocRollupOrSortTransportMode(rateEntry.TI_Mode);
			var containerMode = RatingConstants.GetContainerModeFromMode(rateEntry.TI_Mode);
			var jobType = GetJobType(rateEntry.TI_RateCategory);

			var orgHeader = rateEntry.ParentRatingHeader.Header;

			return (transportMode, containerMode, jobType, orgHeader);
		}

		static string GetDocRollupOrSortTransportMode(string mode)
		{
			switch (mode)
			{
				case Core.Constants.RateMode.AIR:
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
					return DocRollupOrSortTransportModeList.Codes.AirFreight;
				case Core.Constants.RateMode.SEA:
				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.LCL:
					return DocRollupOrSortTransportModeList.Codes.SeaFreight;
				case Core.Constants.RateMode.ROA:
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FTL:
				case Core.Constants.RateMode.FRO:
					return DocRollupOrSortTransportModeList.Codes.RoadFreight;
				case Core.Constants.RateMode.RAI:
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FWL:
				case Core.Constants.RateMode.FRA:
					return DocRollupOrSortTransportModeList.Codes.RailFreight;
				case Core.Constants.RateMode.BBK:
					return DocRollupOrSortTransportModeList.Codes.BreakBulk;
				case Core.Constants.RateMode.BLK:
					return DocRollupOrSortTransportModeList.Codes.Bulk;
				case Core.Constants.RateMode.ROR:
					return DocRollupOrSortTransportModeList.Codes.RollOnRollOff;

				case Core.Constants.RateMode.BCN:
					return DocRollupOrSortTransportModeList.Codes.BuyersConsol;
				default:
					return DocRollupOrSortTransportModeList.Codes.All;
			}
		}

		ZString GetJobType(string rateCategory)
		{
			var groupName = Groups.GetGroupNameByCategory(rateCategory);

			switch (groupName)
			{
				case Groups.Forwarding:
					return DocRollupOrSortJobTypeList.Codes.Forwarding;
				case Groups.CFS:
					return DocRollupOrSortJobTypeList.Codes.CFS;
				case Groups.LinerAndAgency:
					return DocRollupOrSortJobTypeList.Codes.LinerAndAgency;
				case Groups.Transport:
					return DocRollupOrSortJobTypeList.Codes.Transport;
				case Groups.Warehouse:
					return DocRollupOrSortJobTypeList.Codes.Warehouse;
				case Groups.Customs:
					return DocRollupOrSortJobTypeList.Codes.Customs;
				default:
					throw new NotImplementedException($"{groupName} is not supported yet.");
			}
		}

		#endregion

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => FirstRateEntry.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => RateEntrySchema.Constants.Prefix;

		#endregion
	}
}

