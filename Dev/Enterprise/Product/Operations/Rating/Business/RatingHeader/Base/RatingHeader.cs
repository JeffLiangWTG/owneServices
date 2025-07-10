using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using MetaData = CargoWise.ComponentModel.MetaData;

namespace Enterprise.Rating.Business
{
	#region Attributes

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class BusinessContextAttribute : Attribute
	{
		public BusinessContextAttribute(BusinessContext businessContext)
		{
			this.BusinessContext = businessContext;
		}

		public readonly BusinessContext BusinessContext;
	}

	#endregion

	[DebuggerDisplay("{" + nameof(HumanReadableShortcutNameCore) + "}")]
	[ProvideMetaDataProperty("PropertyReadOnly", MetaDataTypes.ReadOnly)]
	[UniversalCopyIgnoreElement(nameof(TH_OneTimeQuote))]
	public class RatingHeader : AutoRatingHeader, IDocManagerSupport, IDocumentSupportable, ISupportDataImporting, IJobNumber, IXMLImportOrgCache,
		Enterprise.Integration.Rating.IRatingHeader,
		IRatingHeader,
		IProgressReporterValidation
	{
		#region Schema

		public new abstract class Schema : AutoRatingHeader.Schema
		{
			public const string TH_ClientCode = "TH_ClientCode";
			public const string TH_ClientFullName = "TH_ClientFullName";
			public const string TH_StatusDate = "TH_StatusDate";
			public const string FilterDate = "FilterDate";
			public const string FilterShowExpired = "FilterShowExpired";
			public const string ShowCosting = "ShowCosting";
			public const string ShowCompanyTariff = "ShowCompanyTariff";
			public const string ShowClientRates = "ShowClientRates";
			public const string SelectedFilterCategory = "SelectedFilterCategory";
		}

		#endregion

		#region Events and EventHandlers

		public delegate void SelectedRateLineChangedEventHandler(RateEntry sender, int index);
		public event SelectedRateLineChangedEventHandler SelectedRateLineChanged;
		public void OnSelectedRateLineChanged(RateEntry sender, int index)
		{
			if (SelectedRateLineChanged != null)
			{
				SelectedRateLineChanged(sender, index);
			}
		}

		public bool HasSelectedRateLineChangedHandlers
		{
			get { return SelectedRateLineChanged != null; }
		}

		public event EventHandler InvalidateRateLines;
		public void OnInvalidateRateLines()
		{
			if (InvalidateRateLines != null)
			{
				InvalidateRateLines(this, EventArgs.Empty);
			}
		}

		public delegate void ShowMessageEventHandler(string message);
		public event ShowMessageEventHandler ShowMessage;
		public void OnShowMessage(string message)
		{
			if (ShowMessage != null)
			{
				ShowMessage(message);
			}
		}

		public delegate bool ProgressChangedEventHandler(int percentComplete, string status);
		public event ProgressChangedEventHandler ProgressChanged;
		public bool OnProgressChanged(int percentComplete, string status)
		{
			if (ProgressChanged != null)
			{
				return ProgressChanged(percentComplete, status);
			}

			return true;
		}

		public delegate bool ValidateDocumentHandler(RatingHeader header);
		public event ValidateDocumentHandler ValidateDocument;
		public bool OnValidateDocument()
		{
			if (ValidateDocument != null)
			{
				return ValidateDocument(this);
			}

			return true;
		}

		#endregion

		public RatingHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetupRelatedRateLineShowOptions();
		}

		#region Default End Date

		internal ZDate DefaultEndDate(int validityPeriod)
		{
			var result = ZDate.Empty;
			if (validityPeriod != 0)
			{
				var quoteDateTime = TH_QuoteDate;
				if (!quoteDateTime.IsValid || quoteDateTime.IsEmpty)
				{
					quoteDateTime = ZDate.Today;
				}

				if (validityPeriod < 0)     // negative value means "End of nth Month"
				{
					var endDate = quoteDateTime.AddMonths(-validityPeriod + 1);
					endDate = new ZDate(endDate.Year, endDate.Month, 1).AddDays(-1);
					result = endDate;
				}
				else
				{
					result = quoteDateTime.AddMonths(validityPeriod);
				}
			}

			return result;
		}

		internal ZDate DefaultEndDate(QuoteValidityRegistryItem registryItem)
		{
			var result = ZDate.Empty;
			if (registryItem.HasExpiry)
			{
				var quoteDateTime = TH_QuoteDate;
				if (!quoteDateTime.IsValid || quoteDateTime.IsEmpty)
				{
					quoteDateTime = ZDate.Today;
				}

				if (registryItem.TillEndOfMonth)
				{
					var endDate = quoteDateTime.AddMonths(registryItem.ValidMonthsFromToday + 1);
					endDate = new ZDate(endDate.Year, endDate.Month, 1).AddDays(-1);
					result = endDate;
				}
				else
				{
					result = quoteDateTime.AddMonths(registryItem.Value);
				}
			}

			return result;
		}

		public ZDate DefaultQuoteEndDate
		{
			get { return DefaultEndDate(Env.Registry.Rating.QuoteValidityPeriod); }
		}

		public ZDate DefaultRateEndDate
		{
			get { return DefaultEndDate(Env.Registry.Rating.RateValidityPeriod); }
		}

		#endregion

		#region Rate Calculator Security

		public bool RateCalculatorOverrideAllowed
		{
			get
			{
				if (!RateCalculatorOverrideAllowedRetrieved)
				{
					RateCalculatorOverrideAllowedRetrieved = true;
					fRateCalculatorOverrideIsAllowed = Env.Security.RateCalculatorOverride.IsAllowed;
				}

				return fRateCalculatorOverrideIsAllowed;
			}
		}

		bool fRateCalculatorOverrideIsAllowed;
		bool RateCalculatorOverrideAllowedRetrieved;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TH_GC = GlbCompany.CurrentCompany.PK;
			TH_RateType = this.DefaultRateType();
		}

		void SetupRelatedRateLineShowOptions()
		{
			ShowClientRates = IsShowClientRates;
			ShowCompanyTariff = IsShowCompanyTariff;
			ShowCosting = IsShowCosting && RatingDataRegistry.Instance.ShowCostingsDuringRating.Value;
		}

		bool IsShowClientRates => this.IsQuote();

		bool IsShowCompanyTariff => this.IsQuote() || this.IsClientRate() || this.IsIntercompanyTariff();

		bool IsShowCosting => !this.IsStandardCostRate();

		#endregion

		#region Type Info

		#region Static Helpers

		public static class TypesAndCodes
		{
			public static Type GetType(string headerCode)
			{
				var pair = TypeCodes.SingleOrDefault(x => x.Item1 == headerCode);
				return pair != null ? pair.Item2 : null;
			}

			static readonly ImmutableArray<Tuple<string, Type>> TypeCodes = new List<Tuple<string, Type>>
			{
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.ClientRate, typeof(ClientRate)),
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.Tariff, typeof(CompanyTariff)),
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.Costing, typeof(Costing)),
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.Quote, typeof(Quote)),
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.IntercompanyTariff, typeof(IntercompanyTariff)),
				new Tuple<string, Type>(RatingConstants.RatingHeaderTypes.WiseCost, typeof(WiseHeader)),
			}.ToImmutableArray();

			public static string GetCode(Type headerType)
			{
				if (headerType == typeof(GlobalTariff))
				{
					headerType = typeof(CompanyTariff);
				}
				else if (headerType.BaseType == typeof(Quote))
				{
					headerType = typeof(Quote);
				}

				var pair = TypeCodes.SingleOrDefault(x => x.Item2 == headerType);
				return pair != null ? pair.Item1 : string.Empty;
			}
		}

		public static bool IsFMCTariffAllowed(string rateType)
		{
			return IsFMCTariffAllowed(TypesAndCodes.GetType(rateType));
		}

		public static bool IsFMCTariffAllowed(Type type)
		{
			return
				typeof(CompanyTariff).IsAssignableFrom(type) || (typeof(ClientRate).IsAssignableFrom(type) || (typeof(Quote)).IsAssignableFrom(type)
				&& !typeof(Costing).IsAssignableFrom(type));
		}

		#endregion

		#endregion

		#region IXMLImportOrgCache Members

		Dictionary<ZString, OrgHeader> IXMLImportOrgCache.CachedOrgsForXMLImport
		{
			get
			{
				if (fCachedOrgsForXMLImport == null)
				{
					fCachedOrgsForXMLImport = new Dictionary<ZString, OrgHeader>();
				}
				return fCachedOrgsForXMLImport;
			}
		}

		Dictionary<ZString, OrgHeader> fCachedOrgsForXMLImport;

		#endregion

		#region Rate Entry Collections

		public void ReloadCollectionsWithNoFilter()
		{
			foreach (var pair in EntryCollectionsExcludingSummary)
			{
				var collection = pair.Value.LazyLoadingCollection;
				collection.ClearUserFilterStrips();
				collection.LoadAndSortForGUI(false);
			}
		}

		#region EntryCollections

		#region LazyEntryCollection

		public class LazyEntryCollection
		{
			public LazyEntryCollection(RatingHeader header, Func<RateEntryCollection> instanceCreator)
			{
				this.header = header;
				this.instanceCreator = instanceCreator;
			}

			public static implicit operator RateEntryCollection(LazyEntryCollection collection)
			{
				return collection.LazyLoadingCollection;
			}

			readonly Func<RateEntryCollection> instanceCreator;

			public RateEntryCollection LazyLoadingCollection
			{
				get
				{
					if (collection == null)
					{
						collection = instanceCreator();

						if (RatingConstants.RateCategory.IsValidEntryCategory(collection.CategoryForFiltering))
						{
							header.RegisterEditableChildObject(collection);
						}
					}

					return collection;
				}
			}

			public RateEntryCollection LoadedCollection
			{
				get
				{
					if (!LazyLoadingCollection.IsLoaded)
					{
						LazyLoadingCollection.Load();
					}

					return LazyLoadingCollection;
				}
			}

			public List<Guid> GetEntryPks()
			{
				if (collection != null)
				{
					return collection.Cast<RateEntry>().Select(entry => entry.PK.ToGuid()).ToList();
				}

				var entries = instanceCreator();
				var filter = entries.GetEntireLoadFilterWithoutUserFilter();
				var queryResult = new DynamicBusinessObjectCollection(header.Factory);
				var pkName = RateEntrySchema.PK.Name;

				var filterQuery = !filter.IsEmpty
					? $" WHERE {filter.ParameterisedText.ParameterisedQueryText}" // SQL query string
					: string.Empty;
				var sql = $"SELECT {pkName} FROM {RateEntrySchema.Constants.SqlSchemaName}.{RateEntrySchema.Constants.TableName} {filterQuery}"; // SQL query string

				using (SQLInjectionDetector.Disable())
				{
					queryResult.Load(sql, filter.ParameterisedText.Parameters);
				}

				return queryResult.Cast<DynamicBusinessObject>().Select(bizo => (ZGuid)bizo[pkName]).Select(pk => pk.ToGuid()).ToList();
			}

			protected readonly RatingHeader header;
			RateEntryCollection collection;
		}

		#endregion

		/// <summary>
		/// An active business object collection that loads all entries into memory.
		/// Should not be used (except for quotes) since there can be too many entries to fit in memory.
		/// Maybe move this to Quote class.
		/// </summary>
		public IBusinessObjectCollection AllEntriesCollection
			=> allEntries ?? (allEntries = GetAllEntriesCore());
		IBusinessObjectCollection allEntries;

		public bool AllEntriesIsLoaded
			=> allEntries != null && allEntries.IsLoaded;

		/// <summary>
		/// The AllEntriesCollection cast to a typed enumerable. Also should be avoided.
		/// </summary>
		public IEnumerable<RateEntry> AllEntries
			=> AllEntriesCollection.Cast<RateEntry>();

		protected virtual IBusinessObjectCollection GetAllEntriesCore()
			=> new ActiveRateEntryCollection(this);

		public Dictionary<string, LazyEntryCollection> EntryCollectionsExcludingSummary
		{
			get
			{
				if (entryCollectionsExcludingSummary == null)
				{
					entryCollectionsExcludingSummary = new Dictionary<string, LazyEntryCollection>(EntryCollections);
					entryCollectionsExcludingSummary.Remove(RatingConstants.RateCategory.SummaryRatesCategory);
				}
				return entryCollectionsExcludingSummary;
			}
		}

		Dictionary<string, LazyEntryCollection> entryCollectionsExcludingSummary;

		public Dictionary<string, LazyEntryCollection> EntryCollections =>
			fEntryCollections ??= new ()
			{
				{ RatingConstants.RateCategory.AIR, GetLazy(() => new AIRRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.FCL, GetLazy(() => new FCLRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.LCL, GetLazy(() => new LCLRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.ORG, GetLazy(() => new ORGRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.DST, GetLazy(() => new DSTRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CAI, GetLazy(() => new CAIRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CFC, GetLazy(() => new CFCRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CLC, GetLazy(() => new CLCRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.COR, GetLazy(() => new CORRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CDS, GetLazy(() => new CDSRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SCO, GetLazy(() => new SCORateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SNC, GetLazy(() => new SNCRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SOR, GetLazy(() => new SORRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SDE, GetLazy(() => new SDERateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SED, GetLazy(() => new SEDRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SID, GetLazy(() => new SIDRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.PAC, GetLazy(() => new PACRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.UNP, GetLazy(() => new UNPRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CST, GetLazy(() => new CSTRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.WHS, GetLazy(() => new WHSRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.TRW, GetLazy(() => new TRWRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.TWU, GetLazy(() => new TWURateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CYU, GetLazy(() => new CYURateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CYM, GetLazy(() => new CYMRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.CYD, GetLazy(() => new CYDRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.TRN, GetLazy(() => new TRNRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.TBC, GetLazy(() => new TBCRateEntryCollection(this, Factory)) },
				{ RatingConstants.RateCategory.SummaryRatesCategory, GetLazy(() => SummaryRateEntries) },
			};

		Dictionary<string, LazyEntryCollection> fEntryCollections;

		LazyEntryCollection GetLazy(Func<RateEntryCollection> instanceCreator)
		{
			return new LazyEntryCollection(this, instanceCreator);
		}

		#endregion

		#region Collections for Binding

		RateEntryCollection GetCollectionForBinding(string category)
		{
			var collection = EntryCollections[category].LazyLoadingCollection;
			collection.LoadAndSortForGUI();
			return collection;
		}

		[ChildEditable(true)]
		public RateEntryCollection AIRRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.AIR); }
		}

		[ChildEditable(true)]
		public RateEntryCollection FCLRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.FCL); }
		}

		[ChildEditable(true)]
		public RateEntryCollection LCLRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.LCL); }
		}

		[ChildEditable(true)]
		public RateEntryCollection ORGRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.ORG); }
		}

		[ChildEditable(true)]
		public RateEntryCollection DSTRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.DST); }
		}

		#region Customs

		[ChildEditable(true)]
		public RateEntryCollection CAIRateEntriesForBinding =>
			GetCollectionForBinding(RatingConstants.RateCategory.CAI);

		[ChildEditable(true)]
		public RateEntryCollection CFCRateEntriesForBinding =>
			GetCollectionForBinding(RatingConstants.RateCategory.CFC);

		[ChildEditable(true)]
		public RateEntryCollection CLCRateEntriesForBinding =>
			GetCollectionForBinding(RatingConstants.RateCategory.CLC);

		[ChildEditable(true)]
		public RateEntryCollection CORRateEntriesForBinding =>
			GetCollectionForBinding(RatingConstants.RateCategory.COR);

		[ChildEditable(true)]
		public RateEntryCollection CDSRateEntriesForBinding =>
			GetCollectionForBinding(RatingConstants.RateCategory.CDS);

		#endregion

		[ChildEditable(true)]
		public RateEntryCollection SCORateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SCO); }
		}

		[ChildEditable(true)]
		public RateEntryCollection SNCRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SNC); }
		}

		[ChildEditable(true)]
		public RateEntryCollection SORRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SOR); }
		}

		[ChildEditable(true)]
		public RateEntryCollection SDERateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SDE); }
		}

		[ChildEditable(true)]
		public RateEntryCollection SEDRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SED); }
		}

		[ChildEditable(true)]
		public RateEntryCollection SIDRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.SID); }
		}

		[ChildEditable(true)]
		public RateEntryCollection PACRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.PAC); }
		}

		[ChildEditable(true)]
		public RateEntryCollection UNPRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.UNP); }
		}

		[ChildEditable(true)]
		public RateEntryCollection CSTRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.CST); }
		}

		[ChildEditable(true)]
		public RateEntryCollection WHSRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.WHS); }
		}

		[ChildEditable(true)]
		public RateEntryCollection TRWRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.TRW); }
		}

		[ChildEditable(true)]
		public RateEntryCollection TWURateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.TWU); }
		}

		[ChildEditable(true)]
		public RateEntryCollection CYMRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.CYM); }
		}

		[ChildEditable(true)]
		public RateEntryCollection CYDRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.CYD); }
		}

		[ChildEditable(true)]
		public RateEntryCollection CYURateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.CYU); }
		}

		[ChildEditable(true)]
		public RateEntryCollection TRNRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.TRN); }
		}

		[ChildEditable(true)]
		public RateEntryCollection TBCRateEntriesForBinding
		{
			get { return GetCollectionForBinding(RatingConstants.RateCategory.TBC); }
		}

		#endregion

		#region Summary Rate Entry Collection

		protected SummaryRateEntryCollection fSummaryRateEntries;
		public SummaryRateEntryCollection SummaryRateEntries
		{
			get
			{
				if (fSummaryRateEntries == null)
				{
					if (_Rating.IsOn && !_Rating.IsSuspended)
					{
						ErrorReporter.ReportOnce("SummaryRateEntries should never be called during the AutoRating Process as it's a collection for the GUI and not a real Rate Category.");
					}
					else
					{
						fSummaryRateEntries = new SummaryRateEntryCollection(this, Factory);
						fSummaryRateEntries.LoadAndSortForGUI();
					}
				}
				return fSummaryRateEntries;
			}
		}

		public IEnumerable<IRateEntry> LoadRateEntriesForAutoRater(ZQuery filter)
		{
			var thisRateFilter = this.GetRelationshipFilter(false);
			thisRateFilter.AddToFilter(filter);

			var result = Factory.GetCachedValue(thisRateFilter.LiteralTextADO, () =>
			{
				return Factory.Load(RateEntryType, thisRateFilter).Cast<RateEntry>().ToList();
			}, CacheStalenessPolicy.StaleWhenDataTableChanges(RateEntrySchema.Constants.TableName, Factory));

			if (this.IsTariff())
			{
				foreach (var rateEntry in result)
				{
					rateEntry.Parent = this;
				}
			}

			return result.Cast<IRateEntry>();
		}

		public IEnumerable<IRateEntry> ChildRateEntries
		{
			get { return AllEntries.Cast<IRateEntry>(); }
		}

		#endregion

		#endregion

		#region Copy

		public virtual RatingHeader CopyIncludingChildren()
		{
			var newHeader = (RatingHeader)Clone();

			foreach (var entry in AllEntries.Where(x => x.TI_RateEndDate.IsEmpty || x.TI_RateEndDate >= ZDate.Today))
			{
				entry.DeepClone(newHeader.EntryCollections[entry.TI_RateCategory]);
			}

			SetNewValuesInHeader(newHeader);
			return newHeader;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected void AddRateEntriesToHeader(RateEntryCollection originalCollection, RateEntryCollection newCollection)
		{
			foreach (RateEntry originalEntry in originalCollection)
			{
				originalEntry.DeepClone(newCollection);
			}
		}

		protected void SetNewValuesInEntryCollection(RateEntryCollection newCollection)
		{
			foreach (RateEntry newEntry in newCollection)
			{
				SetNewValuesInEntry(newEntry);
			}
		}

		protected virtual void SetNewValuesInHeader(RatingHeader newHeader)
		{
			newHeader.TH_OH = ZGuid.Empty;
		}

		protected virtual void SetNewValuesInEntry(RateEntry newEntry)
		{
		}

		public void DuplicateEntries(BusinessObject[] entries, IRateEntrySecurityUIIntractor uiIntractor = null)
		{
			foreach (RateEntry originalEntry in entries.Where(x => !x.IsDeleted))
			{
				var entryCollection = ((IBusinessObjectInternals)originalEntry).ParentCollections[0] as RateEntryCollection;

				if (entryCollection != null && !entryCollection.ReadOnly)
				{
					originalEntry.DeepClone(entryCollection, uiIntractor);
				}
			}
		}

		#endregion

		#region Filtering

		public virtual ZString SelectedFilterCategory
		{
			get { return selectedFilterCategory; }
			set
			{
				selectedFilterCategory = value;
				SelectedFilterCategoryInfo.RefreshBinding();
			}
		}
		ZString selectedFilterCategory;

		public ZPropertyInfo SelectedFilterCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedFilterCategory); }
		}

		#endregion

		#region Properties

		public bool IsFormDelete { get; set; }

		#region TH_GC

		[LightValidationTestExempt]
		public override ZGuid TH_GC
		{
			get { return base.TH_GC; }
			set
			{
				base.TH_GC = value;
				accCFXConfigurations = null;
			}
		}

		#endregion

		#region TH_OH

		[List("Lookups.Clients")]
		public override ZGuid TH_OH
		{
			get { return base.TH_OH; }
			set
			{
				var needToValidateLocations = base.TH_OH.IsEmpty != value.IsEmpty;

				base.TH_OH = value;
				if (!_Rating.IsOn)
				{
					SetupRelatedRateLineShowOptions();

					foreach (var entry in EntryCollections.SelectMany(x => x.Value.LazyLoadingCollection).Cast<RateEntry>())
					{
						entry.ReloadCartageZones();
					}
				}

				if (needToValidateLocations)
				{
					foreach (var entry in EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Cast<RateEntry>())
					{
						entry.Validation.ValidateTI_OriginLRC();
						entry.Validation.ValidateTI_DestinationLRC();
						entry.Validation.ValidateTI_ViaLRC();
					}

					foreach (var entry in EntryCollections[RatingConstants.RateCategory.CAI].LazyLoadingCollection.Cast<RateEntry>())
					{
						entry.Validation.ValidateTI_OriginLRC();
						entry.Validation.ValidateTI_DestinationLRC();
						entry.Validation.ValidateTI_ViaLRC();
					}
				}
			}
		}

		#endregion

		#region TH_ClientFullName

		public virtual ZString TH_ClientFullName
		{
			get { return Header != null ? Header.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo TH_ClientFullNameInfo
		{
			get { return GetZPropertyInfo(Schema.TH_ClientFullName); }
		}

		#endregion

		#region TH_ClientCode

		public ZString TH_ClientCode
		{
			get { return Header != null ? Header.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo TH_ClientCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TH_ClientCode); }
		}

		#endregion

		#region TH_StatusDate

		public ZDateTime TH_StatusDate
		{
			get
			{
				if (TH_IsCancelled)
				{
					return TH_QuoteEndDate;
				}

				if (!TH_Accepted.IsEmpty)
				{
					return TH_Accepted;
				}

				if (!TH_ClientAccepted.IsEmpty)
				{
					return TH_ClientAccepted;
				}

				if (TH_QuoteEndDate < ZDateTime.Today)
				{
					return TH_QuoteEndDate;
				}

				if (TH_QuoteEndDate >= ZDateTime.Today || TH_QuoteEndDate.IsEmpty)
				{
					return ZDateTime.Empty;
				}

				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo TH_StatusDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.TH_StatusDate); }
		}

		#endregion

		protected virtual bool IsCFXEditable
		{
			get { return false; }
		}

		protected bool IsCFXNotEditable
		{
			get { return !IsCFXEditable; }
		}

		#region TH_AirCFX

		[ReadOnlyMember(nameof(IsCFXNotEditable))]
		public override ZDecimal TH_AirCFX
		{
			get { return base.TH_AirCFX; }
			set { base.TH_AirCFX = value; }
		}

		#endregion

		#region TH_SeaCFX

		[ReadOnlyMember(nameof(IsCFXNotEditable))]
		public override ZDecimal TH_SeaCFX
		{
			get { return base.TH_SeaCFX; }
			set { base.TH_SeaCFX = value; }
		}

		#endregion

		#region TH_ExportAirCFX

		[ReadOnlyMember(nameof(IsCFXNotEditable))]
		public override ZDecimal TH_ExportAirCFX
		{
			get { return base.TH_ExportAirCFX; }
			set { base.TH_ExportAirCFX = value; }
		}

		#endregion

		#region TH_ExportSeaCFX

		[ReadOnlyMember(nameof(IsCFXNotEditable))]
		public override ZDecimal TH_ExportSeaCFX
		{
			get { return base.TH_ExportSeaCFX; }
			set { base.TH_ExportSeaCFX = value; }
		}

		#endregion

		#region TH_GlobalRateLevel

		protected bool TH_GlobalRateLevel_ReadOnly
		{
			get { return true; }
		}

#if DEBUG
		[BusinessObjectTestExclude()]
		public override ZByte TH_GlobalRateLevel
		{
			get { return base.TH_GlobalRateLevel; }
			set
			{
				if (ThrowGlobalRateLevelError && !this.IsTariff())
				{
					ErrorReporter.ReportOnce("TH_GlobalRateLevel is valid only for Company Tariffs.");
				}
				base.TH_GlobalRateLevel = value;
			}
		}

		bool ThrowGlobalRateLevelError = true;

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			ThrowGlobalRateLevelError = false;
			try
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
				if (!this.IsTariff())
				{
					TH_GlobalRateLevel = 0;
				}

				if (TH_QuoteNumber.Length > RatingHeader.Schema.TH_QuoteNumberMaxLength - 2)
				{
					TH_QuoteNumber = TH_QuoteNumber.Left(RatingHeader.Schema.TH_QuoteNumberMaxLength - 2);
				}
			}
			finally
			{
				ThrowGlobalRateLevelError = true;
			}
		}
#endif

		#endregion

		#region TH_GS_NKFirstSignatory

		[List("Lookups.FirstSignatories")]
		public override ZString TH_GS_NKFirstSignatory
		{
			get { return base.TH_GS_NKFirstSignatory; }
			set { base.TH_GS_NKFirstSignatory = value; }
		}

		#endregion

		#region TH_GS_NKSecondSignatory

		[List("Lookups.SecondSignatories")]
		public override ZString TH_GS_NKSecondSignatory
		{
			get { return base.TH_GS_NKSecondSignatory; }
			set { base.TH_GS_NKSecondSignatory = value; }
		}

		#endregion

		internal virtual bool IsAnyOtherHeaderWithSameOrgAndType()
		{
			var filter = new ZQuery(RatingHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
			if (TH_GC.IsEmpty)
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}
			else
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, TH_GC);
			}

			filter.AddToFilter(RatingHeaderSchema.TH_OH, TH_OH);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, TH_RateType);

			return Factory.ExistsInDatabase(RatingHeader.Schema.TableName, filter);
		}

		#region TH_PrintInheritedOriginCharges / TH_PrintInheritedDestinationCharges

		protected bool TH_PrintInheritedOriginCharges_ReadOnly
		{
			get { return !TH_PrintRateLevelOriginCharges; }
		}

		protected bool TH_PrintInheritedDestinationCharges_ReadOnly
		{
			get { return !TH_PrintRateLevelDestinationCharges; }
		}

		#endregion

		#region TH_PrintRateLevelOriginCharges / TH_PrintRateLevelDestinationCharges

		public override ZBool TH_PrintRateLevelOriginCharges
		{
			get { return base.TH_PrintRateLevelOriginCharges; }
			set
			{
				if (TH_PrintRateLevelOriginCharges != value)
				{
					base.TH_PrintRateLevelOriginCharges = value;
					TH_PrintInheritedOriginCharges = value;
				}
			}
		}

		public override ZBool TH_PrintRateLevelDestinationCharges
		{
			get { return base.TH_PrintRateLevelDestinationCharges; }
			set
			{
				if (TH_PrintRateLevelDestinationCharges != value)
				{
					base.TH_PrintRateLevelDestinationCharges = value;
					TH_PrintInheritedDestinationCharges = value;
				}
			}
		}

		#endregion

		#region TH_GlobalRateDescription

		/// <summary>
		/// Translatable field allows the user to enter values in other languages
		/// </summary>
		[GlobalRateDescriptionTranslatableField(Schema.TableName, Schema.TH_GlobalRateDescription, MaxLength = Schema.TH_GlobalRateDescriptionMaxLength, Type = typeof(RatingHeader), SecurityCheckpoint = "TariffsRates", Asmid = ResString.AssemblyId)]
		public override ZString TH_GlobalRateDescription
		{
			get { return base.TH_GlobalRateDescription; }
			set
			{
				if (value != TH_GlobalRateDescription)
				{
					CheckMaximumLength(TH_GlobalRateDescriptionInfo, value);
					base.TH_GlobalRateDescription = value;
					globalRateDescriptionInCurrentLanguage = null;

					TH_GlobalRateDescriptionInfo.RefreshBinding();
				}
			}
		}

		public MultilingualString TH_GlobalRateDescriptionMultilingual
		{
			get { return GetMultilingual(TH_GlobalRateDescriptionInfo); }
		}

		/// <summary>
		/// GlobalRateDescription in current language.
		/// Cached for speed. Value is not updated if the user edits the local description, but that is rare and they will have to reload the form.
		/// </summary>
		public string GlobalRateDescriptionInCurrentLanguage
			=> globalRateDescriptionInCurrentLanguage ?? (globalRateDescriptionInCurrentLanguage = TH_GlobalRateDescriptionMultilingual);
		string globalRateDescriptionInCurrentLanguage;

		#endregion

		#region DisplayInfo / RatingHeaderTypeDescription

		/// <summary>
		/// Generic description of this type of RatingHeader, e.g., "Costing" or "Client Rate".
		/// Does not contain details identifying this specific instance such as the client name or code.
		/// Result is cached for speed and is assumed immutable, which is true as of 2021 Mar since the user can't change the type, such as a Costing into a Client Rate.
		/// </summary>
		public ZString RatingHeaderTypeDescription
			=> ratingHeaderTypeDescription ?? (ratingHeaderTypeDescription = RatingHeaderTypeDescriptionCore);
		string ratingHeaderTypeDescription;
		protected virtual string RatingHeaderTypeDescriptionCore => string.Empty;

		public virtual ZString DisplayInfo() => ZString.Empty;

		internal static ZString DisplayInfoWithOrgInfo(string displayInfo, OrgHeader org)
			=> org != null ? $"{displayInfo} {org.OH_Code}" : displayInfo;

		#endregion

		#region Company / Header CompanyData

		[BusinessObjectTestExclude]
		public OrgStaffAssignmentsCollection HeaderStaffAssignments
		{
			get
			{
				return Header != null && Company != null ? Header.GetStaffAssignmentsForGlbCompany(Company) : null;
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public RatingHeader GlobalRatingHeader
		{
			get { return Factory.GetCachedValue(GlobalRatingHeaderKey, () => new RateCreator(this).LoadGlobalRatingHeader(Factory)); }
		}

		internal string GlobalRatingHeaderKey
		{
			get { return Invariant($"GlobalRate{PK}.{TH_OH}"); }
		}

		#endregion

		#region Published

		public ZString Published
		{
			get
			{
				return TH_GC == ZGuid.Empty
					? Res.GetString("8c94c244-9dbf-472e-8f0c-79efd0a815c2", "Global")
					: Res.GetString("cca0d551-5c72-4bbb-bafc-09c9726b8480", "Local");
			}
		}

		public ZPropertyInfo PublishedInfo
		{
			get { return GetZPropertyInfo(nameof(Published)); }
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return RatingHeaderTypeDescription; }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get { return DisplayInfo(); }
		}

		#endregion

		#region TH_OneTimeQuote

		[DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		public override ZBool TH_OneTimeQuote
		{
			get => base.TH_OneTimeQuote;
			set => base.TH_OneTimeQuote = value;
		}

		#endregion

		public bool EntryLineOrderSuspended { get; set; }

		#endregion

		#region Validation

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		public RateEntry[] FetchAllEntriesFromLocalCache()
		{
			var query = new ZQuery(RateEntrySchema.TI_TH, PK) { FetchOnlyFromLocalCache = true };
			return this.IsQuote() ? Factory.Load<QuoteEntry>(query) : Factory.Load<RateEntry>(query);
		}

		public override void MarkAsNeedingValidationIncludingChildren()
		{
			base.MarkAsNeedingValidationIncludingChildren();
			needFullEntryValidation = true;
		}

		public bool IsPreSaveValidating { get; private set; }
		bool needFullEntryValidation;

		protected override void RunPreSaveValidationCore()
		{
			using (new DisposableAction(() => IsPreSaveValidating = true, () => IsPreSaveValidating = false))
			{
				RateEntry[] localEntriesCached = null;
				RateEntry[] globalEntriesCached = null;

				needFullEntryValidation = needFullEntryValidation
					|| ((IBusinessObjectState)this).HasChangesNotIncludingChildren
					|| (localEntriesCached = FetchAllEntriesFromLocalCache()).Any(x => x.HasChanges)
					|| ((globalEntriesCached = GlobalRatingHeader?.FetchAllEntriesFromLocalCache())?.Any(x => x.HasChanges) ?? false);

				var doFullEntryValidation = !RatingDataRegistry.Instance.ValidateOnlyLoadedRatesUponSaving.Value && needFullEntryValidation;

				needFullEntryValidation = false; // for next time

				if (doFullEntryValidation)
				{
					// Fetch hints for children will have been executed at this point.
					// However, since we are validating all entries, there may be entries not in the children list (not loaded, or loaded and filtered out)
					// Have to run their hints too.
					// Note, this is still not ideal, since hints will be done in two lots.
					var entriesToValidate = GetEntriesToValidate();
					var rateCategories = new HashSet<ZString>();

					foreach (var entry in entriesToValidate)
					{
						rateCategories.Add(entry.TI_RateCategory.ToUpperInvariant());

						if (!entry.ShouldRatingHeaderSkipFetchHints)
						{
							((IBusiness)entry).RunPreSaveValidationFetch(false);
						}
					}

					Factory.ExecuteAllFetchHints();

					EntryCollectionValidator.Validate();

					var progressStatus = new ProgressStatus(entriesToValidate.Length);

					for (int i = 0; i < entriesToValidate.Length; i++)
					{
						progressAction?.Invoke(progressStatus.GetStatusReport(i, isShortMessage: true));

						var entry = entriesToValidate[i];
						entry.RunPreSaveValidation();
					}

					foreach (var category in rateCategories)
					{
						var collection = EntryCollections[category].LazyLoadingCollection;
						using (collection.SuspendListChanged())
						{
							var query = collection.GetLoadFilter(includeUserFilter: true);
							query.FetchOnlyFromLocalCache = true;

							collection.Load(query);
						}
					}
				}
				else
				{
					// For a full validation the RateEntryCollectionValidator will clear all row errors first.
					// Since we are not doing a full validation, we can just clear them here.
					// Solves the problem of deleted rows that were causing a row error.

					localEntriesCached ??= FetchAllEntriesFromLocalCache();
					if (localEntriesCached != null)
					{
						RateEntryCollectionValidator.ClearRowNotifications(localEntriesCached);
					}

					globalEntriesCached ??= GlobalRatingHeader?.FetchAllEntriesFromLocalCache();
					if (globalEntriesCached != null)
					{
						RateEntryCollectionValidator.ClearRowNotifications(globalEntriesCached);
					}
				}

				base.RunPreSaveValidationCore();
			}
		}

		RateEntry[] GetEntriesToValidate()
		{
			var query = new ZDBOnlyQuery(typeof(RateEntry));
			query.AddToFilter(RateEntrySchema.TI_TH, PK);
			query.AddToFilter(RateEntrySchema.TI_IsValid, false);

			// Load invalid rates from the DB into the factory. Basically, we want to validate all loaded rates plus not-loaded which
			// are not yet validated so that if they have errors (for example during import), the user could fix them.
			Factory.Load<RateEntry>(query);

			var entries = FetchAllEntriesFromLocalCache();
			entries = entries.Where(e => e.HasChanges || !((ILightValidationInternals)e).IsValid).ToArray();

			return entries;
		}

		IDisposable IProgressReporterValidation.ReportProgress(Action<string> progressActionParameter)
		{
			progressAction = progressActionParameter;
			return new DisposableAction(() => progressAction = null);
		}
		Action<string> progressAction;

		public RateEntryCollectionValidator EntryCollectionValidator
		{
			get { return entryCollectionValidator ?? (entryCollectionValidator = new RateEntryCollectionValidator(this)); }
		}
		RateEntryCollectionValidator entryCollectionValidator;

		public bool HasEntryCollectionValidatorBeenInitialised
		{
			get => entryCollectionValidator != null;
		}

		#endregion

		#region Delete Rate Header + Entries

		/// <summary>
		/// Delete this Rating Header and all child Rate Entries, Lines and Items
		/// </summary>
		public override void Delete()
		{
			if (IsDeleted)
			{
				return;
			}

			if (this is CompanyTariff && TH_GlobalRateLevel > 1)
			{
				var query = GetRateLinesToDeleteQuery();
				var rateLines = Factory.Load<RateLine>(query).ToList();
				rateLines.ForEach(rateLine => rateLine.Delete());
			}

			DeleteRateEntriesInBatches();

			base.Delete();
		}

		ZDBOnlyQuery GetRateLinesToDeleteQuery()
		{
			var ratingHeaderSubQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
			ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, (byte)1);
			ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_RateType, "GLB");
			ratingHeaderSubQuery.AddToFilter(RatingHeaderSchema.TH_GC, TH_GC);

			var rateEntrySubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.PK);
			rateEntrySubQuery.AddSubQuery(RateEntrySchema.TI_TH, ratingHeaderSubQuery, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(RateLine));
			query.AddSubQuery(RateLinesSchema.TL_TI, rateEntrySubQuery, JoinCondition.Or);
			query.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, TH_GlobalRateLevel);
			return query;
		}

		internal int NumOfRateEntriesToDeletePerBatch = 1000;

		void DeleteRateEntriesInBatches()
		{
			//As confirmed by Product, There can be millions of Rate Entries, like for reported client case where entries were ~500K for Global Tariff
			//Since, we can have millions of rows, relying on Rating Header Casecade delete option can take minutes
			//and command will take huge time and run timeout, so it's better we delete rate Entries in batches.

			var result = true;
			while (result)
			{
				result = ((IDbConnected)Factory).Connection.ExecuteNonQuery($@"
DELETE TOP({NumOfRateEntriesToDeletePerBatch})
FROM dbo.RateEntry
WHERE TI_TH = @PK",
					(cmd) => cmd.AddParameter(ZSqlParameter.New("@PK", PK.ToGuid(), RateEntrySchema.TI_TH))
				) == NumOfRateEntriesToDeletePerBatch;
			}
		}

		public override bool CanDelete
		{
			get { return IsNotRestrictedByRateEntriesPublishedInOtherCompanies; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsNotRestrictedByRateEntriesPublishedInOtherCompanies)
				{
					return base.ReasonForNotAbleToDelete;
				}

				var companyCodes = string.Join(", ", OtherPublisherCompanyCodes);

				return ResString.GetMultilingualString("41deb11f-0096-4aae-be49-ca1fd23bc7cd", "This {0} can't be deleted as it contains Rate Entries Published by other Company(s) ({1}).", HumanReadableName, companyCodes);
			}
		}

		#region SuppressResourceStringsCheckRegion

		bool IsNotRestrictedByRateEntriesPublishedInOtherCompanies
		{
			get { return !this.IsGlobal() || CanDeleteRatesFromAnyCompany || !OtherPublisherCompanyCodes.Any(); }
		}

		bool CanDeleteRatesFromAnyCompany => Factory.GetCachedValue(Invariant($"CanDeleteRatesFromAnyCompany.{TH_RateType}"), GetCanDeleteRatesFromAnyCompany);
		bool GetCanDeleteRatesFromAnyCompany()
		{
			if (!this.IsGlobal())
			{
				return false;
			}
			if (this.IsTariff() && Env.Security.GlobalTariffRatesDeleteFromAnyCompany.IsAllowed)
			{
				return true;
			}
			if (this.IsCosting() && Env.Security.GlobalCostingRatesDeleteFromAnyCompany.IsAllowed)
			{
				return true;
			}
			if (this.IsClientRate() && Env.Security.GlobalClientRatesDeleteFromAnyCompany.IsAllowed)
			{
				return true;
			}

			return false;
		}

		ZString[] OtherPublisherCompanyCodes
		{
			get { return Factory.GetCachedValue(Invariant($"OtherPublisherCompanyCodes.{PK}"), GetOtherPublisherCompanyCodes); }
		}

		ZString[] GetOtherPublisherCompanyCodes()
		{
			var sql = "SELECT DISTINCT GC_Code FROM dbo.GlbCompany " +
				"WHERE GC_PK in ( " +
					"SELECT TI_GC_Publisher " +
					"FROM dbo.RateEntry " +
					"WHERE TI_TH = @RatingHeaderPk " +
					"AND TI_GC_Publisher != @CurrentCompanyPk " +
				") " +
				"ORDER BY GC_Code"; // SQL query string

			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@RatingHeaderPk", PK, RatingHeaderSchema.PK));
			parameters.Add(ZSqlParameter.New("@CurrentCompanyPk", Env.CurrentCompanyPK, GlbCompanySchema.PK));
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, parameters);

			return collection.Select(x => (ZString)x["GC_Code"]).ToArray(); // Database column name
		}

		#endregion

		#endregion

		#region ReadOnly

		protected bool GetPropertyReadOnly(PropertyDescriptor property)
		{
			bool result;

			switch (property.Name)
			{
				case RatingHeaderSchema.Constants.TH_OH:
				case RatingHeaderSchema.Constants.TH_AirCFX:
				case RatingHeaderSchema.Constants.TH_SeaCFX:
				case RatingHeaderSchema.Constants.TH_ExportAirCFX:
				case RatingHeaderSchema.Constants.TH_ExportSeaCFX:
				case RatingHeaderSchema.Constants.TH_GlobalRateLevel:
				case RatingHeaderSchema.Constants.TH_QuoteDate:
				case RatingHeaderSchema.Constants.TH_QuoteEndDate:
					result = readOnly;
					break;

				default:
					result = property.IsReadOnly;
					break;
			}

			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		bool readOnly;

		public new void SetReadOnlyIncludingChildren(bool isReadOnly)
		{
			if (this.readOnly != isReadOnly)
			{
				this.readOnly = isReadOnly;
				TH_OHInfo.RefreshBinding();
				TH_AirCFXInfo.RefreshBinding();
				TH_SeaCFXInfo.RefreshBinding();
				TH_ExportAirCFXInfo.RefreshBinding();
				TH_ExportSeaCFXInfo.RefreshBinding();
				TH_GlobalRateLevelInfo.RefreshBinding();
				TH_QuoteDateInfo.RefreshBinding();
				TH_QuoteEndDateInfo.RefreshBinding();

				foreach (RateEntryCollection entryCollection in EntryCollections.Values)
				{
					entryCollection.SetReadOnlyIncludingChildren(isReadOnly);
				}
				SummaryRateEntries.SetReadOnlyIncludingChildren(isReadOnly);
			}
		}

		#endregion

		#region Summary Collection - HasChangesChanged

		public override bool HasChanges
		{
			set
			{
				try
				{
					isInsideHasChangesSetter = true;
					hasChanges = value;
					base.HasChanges = value;
				}
				finally
				{
					isInsideHasChangesSetter = false;
				}
			}
			get
			{
				return isInsideHasChangesSetter ? hasChanges : base.HasChanges;
			}
		}
		bool isInsideHasChangesSetter;
		bool hasChanges;

		#endregion

		#region Show Related Rate Entries Properties

		#region Costing

		public ZBool ShowCosting
		{
			get { return fShowCosting && Env.Security.CostingRatesView.IsAllowed; }
			set
			{
				fShowCosting = value;
				ShowCostingInfo.RefreshBinding();
			}
		}

		ZBool fShowCosting;

		public bool ShowCosting_ReadOnly => !Env.Security.CostingRatesView.IsAllowed || !this.IsShowCosting;

		public ZPropertyInfo ShowCostingInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShowCosting); }
		}

		#endregion

		#region Company Tariff

		public ZBool ShowCompanyTariff
		{
			get { return fShowCompanyTariff && Env.Security.CompanyTariffRates.IsAllowed; }
			set
			{
				fShowCompanyTariff = value;
				ShowCompanyTariffInfo.RefreshBinding();
			}
		}

		ZBool fShowCompanyTariff;

		public bool ShowCompanyTariff_ReadOnly => !Env.Security.CompanyTariffRates.IsAllowed || !this.IsShowCompanyTariff;

		public ZPropertyInfo ShowCompanyTariffInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShowCompanyTariff); }
		}

		#endregion

		#region Client Rates

		public ZBool ShowClientRates
		{
			get { return fShowClientRates; }
			set
			{
				fShowClientRates = value;
				ShowClientRatesInfo.RefreshBinding();
			}
		}

		ZBool fShowClientRates;

		public bool ShowClientRates_ReadOnly => !IsShowClientRates;

		public ZPropertyInfo ShowClientRatesInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.ShowClientRates); }
		}

		#endregion

		internal BusinessObjectFactory ShowCostingCompanyTariffFactory
		{
			get
			{
				if (fShowCostingCompanyTariffFactory == null)
				{
					fShowCostingCompanyTariffFactory = new BusinessObjectFactory();
				}

				return fShowCostingCompanyTariffFactory;
			}
		}

		BusinessObjectFactory fShowCostingCompanyTariffFactory;

		#endregion

		#region Type Decider

		public static readonly RatingHeaderTypeDecider TypeDecider = new RatingHeaderTypeDecider();

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (!IsDeleted && (docManagerInfo == null || TH_RateType != lastRateTypeForDocManagerInfo))
				{
					var code = ZString.Empty;

					if (this.IsClientRate())
					{
						code = this.IsGlobal() ? DocManagerCodes.GlobalClientRate : DocManagerCodes.ClientRate;
					}
					else if (this.IsTariff())
					{
						code = this.IsGlobal() ? DocManagerCodes.GlobalCompanyTariff : DocManagerCodes.CompanyTariff;
					}
					else if (this.IsQuote())
					{
						code = this.IsOneOffQuote() ? DocManagerCodes.OneOffQuote : DocManagerCodes.Quotation;
					}
					else if (this.IsCosting())
					{
						code = this.IsGlobal() ? DocManagerCodes.GlobalCosting : DocManagerCodes.Costing;
					}

					docManagerInfo = new RatingDocManagerInfo(this, code);
					lastRateTypeForDocManagerInfo = TH_RateType;
				}

				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;
		ZString lastRateTypeForDocManagerInfo;

		#endregion

		#region Event Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		/// <summary>
		/// Enable changes to rate entries to be logged when the RatingHeader itself has not changed.
		/// Note, the base implementation will also return true if the header is loaded in a form since ZForm will set IsToplevel which has the same effect.
		/// So this only affects unit tests and functions that adds rate entries directly (bulk rate updater, imports, etc)
		/// </summary>
		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => CreateAutoLogIfOnlyChildrenChanged;
		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => CreateAutoLogIfOnlyChildrenChanged;
		public bool CreateAutoLogIfOnlyChildrenChanged { get; set; } = true;

		#endregion

		#region Rate Entry Type

		public Type RateEntryType
		{
			get { return this.IsQuote() ? typeof(QuoteEntry) : typeof(RateEntry); }
		}

		#endregion

		#region Pre-Fetch

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new RatingHeaderFetchStrategy(this);
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new RatingHeaderDocumentSupporter(this); }
		}

		#endregion

		#region Data Import

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		bool fIsImportingData;

		#endregion

		#region Testing Methods
#if DEBUG

		public RateEntry AddRateEntry(
			ZString category,
			string mode = "",
			string origin = "",
			string destination = "",
			string serviceLevel = "",
			string container = "",
			string commodity = "",
			bool removeLines = false,
			ZDate startDate = default,
			ZDate endDate = default,
			bool matchContainerRateClass = false,
			string unitType = "")
		{
			if (Env.CurrentCompany.IsGSTRegistered)
			{
				var rate = AccTaxRate.Helper
					.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
				rate?.SetRateNumerator_ForTestOnly(0);
				rate?.Factory.Save();
			}

			var collection = EntryCollections[category].LazyLoadingCollection;
			// Mimic how a RateEntry is created by the user on the GUI.
			var entry = (RateEntry)((ILegacyBusinessObjectCollectionInternals)collection).CreateNewBusinessObject();
			collection.SetupNewElementButDoNotAddIt(entry, true);

			if (!string.IsNullOrEmpty(mode))
			{
				entry.TI_Mode = mode;
			}
			else
			{
				entry.TI_Mode = RateEntryLookups.GetTransportModesByRateCategory(category)[0].Code;
			}

			entry.TI_OriginLRC = origin;
			entry.TI_DestinationLRC = destination;

			if (entry.IsCosting())
			{
				entry.TI_PL_NKCarrierServiceLevel = serviceLevel;
			}
			else
			{
				entry.TI_RS_NKServiceLevel_NI = serviceLevel;
			}

			if (!string.IsNullOrEmpty(unitType))
			{
				entry.TI_YardUnitType = unitType;
			}

			if (!string.IsNullOrEmpty(container))
			{
				entry.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container).PK;
			}

			if (!string.IsNullOrEmpty(commodity))
			{
				entry.TI_RH_NKCommodityCode = commodity;
			}

			if (matchContainerRateClass)
			{
				entry.TI_MatchContainerRateClass = true;
			}

			if (startDate != default)
			{
				entry.TI_RateStartDate = startDate;
			}

			if (endDate != default)
			{
				entry.TI_RateEndDate = endDate;
			}

			collection.Add(entry);

			if (removeLines)
			{
				entry.RateLines.RemoveAndDeleteAll();
			}

			return entry;
		}

		public RateEntry AddRateEntryViaReflection(ZString category, ZString mode, ZString origin, ZString destination) =>
			AddRateEntry(category, mode, origin, destination);

		public RateEntry AddRateEntryWithFlatRateLine(ZString rateCategory, ZString rateMode, ZString origin, ZString destination, ZString chargeCode, ZDecimal flatRateAmount, string currency = "", string container = "", string description = "", short? lineOrder = null, int? frequency = default, string frequencyUnit = default, string contractNumber = default, bool? matchContainerRateClass = default, string transitTime = default, string commodity = "", string unitFactor = "")
		{
			var rateEntry = AddRateEntry(rateCategory, rateMode, origin, destination, commodity: commodity);
			rateEntry.RateLines.RemoveAndDeleteAll();

			if (lineOrder != default)
			{
				rateEntry.TI_LineOrder = lineOrder.Value;
			}

			if (frequency != default)
			{
				rateEntry.TI_Frequency = frequency.Value;
			}

			if (frequencyUnit != default)
			{
				rateEntry.TI_FrequencyUnit = frequencyUnit;
			}

			if (contractNumber != default)
			{
				rateEntry.TI_ContractNumber = contractNumber;
			}

			if (matchContainerRateClass != default)
			{
				rateEntry.TI_MatchContainerRateClass = matchContainerRateClass.Value;
			}

			if (transitTime != default)
			{
				rateEntry.TI_TransitTime = transitTime;
			}

			rateEntry.AddFlatRateLine(chargeCode, flatRateAmount, currency, container, description, unitFactor);

			return rateEntry;
		}

		public RateEntry AddRateEntryWithFlatRateLine<T>(ZString rateCategory, ZString rateMode, ZString origin, ZString destination, ZString chargeCode, ZDecimal flatRateAmount, SchemaColumn schemaColumn, T value, string currency = "", string container = "", string description = "", short? lineOrder = null, int? frequency = default, string frequencyUnit = default, string contractNumber = default, bool? matchContainerRateClass = default, string unitFactor = default)
		{
			var rateEntry = AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, chargeCode, flatRateAmount, currency, container, description, lineOrder, frequency, frequencyUnit, contractNumber, matchContainerRateClass, unitFactor);
			rateEntry[schemaColumn] = value;
			return rateEntry;
		}

		public RateEntry AddRateEntryWithUnitRateLine(ZString rateCategory, ZString rateMode, ZString origin, ZString destination, ZString chargeCode, ZDecimal unitRateAmount, string unit, string currency = "", string container = "", string commodity = "")
		{
			var rateEntry = AddRateEntry(rateCategory, rateMode, origin, destination, container: container, commodity: commodity);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, unit, currencyCode: currency);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = unitRateAmount;

			return rateEntry;
		}

		public RateEntry AddRateEntryWithCMBRateLine(ZString rateCategory, ZString rateMode, ZString origin, ZString destination, ZString chargeCode, ZDecimal unitRateAmount, string unit, string currency = "", string container = "", string description = "", params (string breakValue, decimal rate)[] rateBreaks)
		{
			var rateEntry = AddRateEntry(rateCategory, rateMode, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			rateEntry.AddCMBRateLine(chargeCode, unit, rateBreaks);

			return rateEntry;
		}

#endif
		#endregion

		#region ICancellable

		public override string CanCancel()
		{
			return Res.GetString("e82d228d-b011-49cc-b3c8-1ac8a9cda4d9", "Record can't be marked as Inactive. It does not support Inactivating/Activating");
		}

		public override string CanReactivate()
		{
			return Res.GetString("b589cb09-952d-46b6-9c61-2766fd6d9474", "Record can't be marked as Active. It does not support Inactivating/Activating");
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return TH_QuoteNumber; }
		}

		#endregion

		public bool CanRollUpAndSort() => this.IsQuote();

		[BusinessObjectTestExclude]
		public AccCFXUpliftConfigurationCollection AccCFXConfigurations => accCFXConfigurations ?? (accCFXConfigurations = Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).AccCFXConfigurations);
		AccCFXUpliftConfigurationCollection accCFXConfigurations;

		public string InvalidReason => string.Empty;
		public static RatingHeader GetBO(IRatingHeader rate)
		{
			return rate as RatingHeader;        //ToDo: fix this
		}

		internal void AddRateEntryLogEvent(RateEntry rateEntry, Event eventType)
		{
			rateEntryLogEventMap[rateEntry] = eventType;

			var rateEntryLoggingService = Factory.ServiceContainer.GetAfterOnSavingService<RateEntryLoggingService>();
			if (rateEntryLoggingService == null)
			{
				rateEntryLoggingService = new RateEntryLoggingService();
				Factory.ServiceContainer.AddAfterOnSavingService(rateEntryLoggingService);
			}
			rateEntryLoggingService.Add(this);
		}

		/// <summary>
		/// Called after OnSaving has been called on all modified rate entries.
		/// At that point we can calculate a single StmALog record on the RatingHeader summarizing all changes.
		/// </summary>
		class RateEntryLoggingService : IAfterOnSavingBOProcessingService
		{
			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				// Parameter businessObjectsInOnSavingOrder will contain the RateEntry records that were modified
				// and many others. Since we only want the unique parent RatingHeaders
				// we can just maintain our own list.
				foreach (var ratingHeader in ratingHeaders)
				{
					ratingHeader.CreateRateEntryLog();
				}
				ratingHeaders.Clear();
			}

			internal void Add(RatingHeader ratingHeader)
				=> ratingHeaders.Add(ratingHeader);

			readonly HashSet<RatingHeader> ratingHeaders = new HashSet<RatingHeader>();
		}

		internal void CreateRateEntryLog()
		{
			if (rateEntryLogEventMap.Count == 0)
			{
				return;
			}

			int added = 0;
			int edited = 0;
			int deleted = 0;
			foreach (var item in rateEntryLogEventMap)
			{
				if (item.Value == AutoEvents.AddedARecordToTheSystem)
				{
					++added;
				}
				else if (item.Value == AutoEvents.EditedARecord)
				{
					++edited;
				}
				else
				{
					++deleted;
				}
			}

			rateEntryLogEventMap.Clear();
			var log = Logs.AutoCreatedLog;
			if (log != null)
			{
				log.UpdateReference($"{added}/{edited}/{deleted} entries added/edited/deleted"); // log reference in db can't be translated
			}
		}

		readonly Dictionary<RateEntry, Event> rateEntryLogEventMap = new Dictionary<RateEntry, Event>();
	}

	#region IXMLImportOrgCache Interface

	public interface IXMLImportOrgCache
	{
		Dictionary<ZString, OrgHeader> CachedOrgsForXMLImport
		{
			get;
		}
	}

	#endregion

	#region Rating Header Type Decider

	public class RatingHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			return RatingHeaderTypeDecider.FromString(row[RatingHeaderSchema.TH_RateType.Name].ToString());
		}

		public override Type GetTypeForBinding()
		{
			return typeof(RatingHeader);
		}

		public override Type GetTypeForNew()
		{
			return typeof(RatingHeader);
		}

		public static Type FromString(string headerCode)
		{
			return RatingHeader.TypesAndCodes.GetType(headerCode);
		}
	}

	#endregion
}
