using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[UniversalDataContext(DataContextType.GlobalRate)]
	[CodeProperty(CompanyTariff.Schema.CompanyTariffLevelString), DescriptionProperty(RatingHeaderSchema.Constants.TH_GlobalRateDescription)]
	[BusinessContext(BusinessContext.Rating)]
	public class CompanyTariff : RatingHeader, ICompanyTariff, IWorkflowProvider
	{
		#region Schema

		public new abstract class Schema : RatingHeader.Schema
		{
			public const string CompanyTariffLevelString = "CompanyTariffLevelString";
		}

		#endregion

		public CompanyTariff(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Code Property Attribute

		public ZString CompanyTariffLevelString
		{
			get { return TH_GlobalRateLevel.ToString(); }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!Factory.IsConstructingNullBusinessObject)
			{
				if (IsGlobalTariff)
				{
					TH_GC = ZGuid.Empty;
				}

				TH_GlobalRateLevel = NextTariffLevel;

				if (TH_GlobalRateLevel == 1)
				{
					TH_GlobalRateDescription = IsGlobalTariff
						? Res.GetString("03508f08-d213-4fb4-aa30-ed9b71612c53", "Global Base Tariff")
						: Res.GetString("589cd06c-6b27-4ddd-b3e8-44079350833e", "Base Company Tariff");
				}
				else
				{
					TH_GlobalRateDescription = IsGlobalTariff
						? Res.GetString("376b858f-e840-4637-8220-f4f39eae1ae6", "Global Tariff Level {0}", TH_GlobalRateLevel)
						: Res.GetString("56892c0c-f6c3-426c-9552-6085722ee291", "Company Tariff Level {0}", TH_GlobalRateLevel);
				}
			}
		}

		protected ZByte NextTariffLevel
		{
			get
			{
				var filter = new ZQuery(RatingHeaderSchema.TH_RateType, TH_RateType);
				AddCompanyFilter(filter);
				filter.OrderBy = AutoRatingHeader.Schema.TH_GlobalRateLevel + " desc";

				var tariff = Factory.LoadTop1<CompanyTariff>(filter);
				var result = tariff != null
					? tariff.TH_GlobalRateLevel + 1
					: 1;

				return (ZByte)result;
			}
		}

		void AddCompanyFilter(ZQuery filter)
		{
			if (Company == null)
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}
			else
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, Company.PK);
			}
		}

		protected virtual bool IsGlobalTariff
		{
			get { return false; }
		}

		#endregion

		#region DisplayInfo / RatingHeaderTypeDescription

		protected override string RatingHeaderTypeDescriptionCore => TH_GC.IsEmpty
			? Res.GetString("1e5d552e-9e41-4352-9c26-e4c7f4eab8d3", "Global Tariff")
			: Res.GetString("f7990ffe-ab43-4e1e-b57f-e0efe5a2e37d", "Company Tariff");

		public override ZString DisplayInfo()
		{
			var desc = GlobalRateDescriptionInCurrentLanguage;
			if (!string.IsNullOrEmpty(desc))
			{
				return desc;
			}
			else
			{
				return RatingHeaderTypeDescription + " " + TH_GlobalRateLevel;
			}
		}

		#endregion

		#region Validation

		protected override RatingHeaderValidation GetNewValidation() => new CompanyTariffValidation(this);

		#endregion

		internal CompanyTariffCodes CompanyTariffCodes
		{
			get
			{
				return new CompanyTariffCodes();
			}
		}

		#region Discount

		public override ZString SelectedFilterCategory
		{
			get { return base.SelectedFilterCategory; }
			set
			{
				base.SelectedFilterCategory = value;
				DiscountType = RatingConstants.RateCategory.IsValidEntryCategory(value) ? value : ZString.Empty;
			}
		}

		#region Discount Types List

		public CodeDescriptionPairList DiscountTypes
		{
			get { return CompanyTariffCodes; }
		}

		#endregion

		#region Discounts

		[ChildEditable(true)]
		public RateTariffDiscountCollection Discounts
		{
			get
			{
				if (fDiscounts == null)
				{
					fDiscounts = new RateTariffDiscountCollection(this, RatingConstants.RateCategory.GetRateCategories((RateType)~0, RateCategoryGroup.All));
					fDiscounts.Load();
					RegisterEditableChildObject(fDiscounts);
				}

				return fDiscounts;
			}
		}

		RateTariffDiscountCollection fDiscounts;

		#endregion

		#region Discount Service Levels

		public RefServiceLevelCollection DiscountServiceLevels
		{
			get
			{
				if (fDiscountServiceLevels == null)
				{
					fDiscountServiceLevels = new RefServiceLevelCollection(Factory);
				}
				return fDiscountServiceLevels;
			}
		}

		RefServiceLevelCollection fDiscountServiceLevels;

		#endregion

		#region Discount Service Level

		[BusinessObjectTestExclude] // because DiscountType is blank in the test
		[List("DiscountServiceLevels")]
		[MaxLength(3)]
		public ZString DiscountServiceLevel
		{
			get { return Discounts.GetServiceLevel(DiscountType); }
			set
			{
				Discounts.SetServiceLevel(DiscountType, value);
				OnInvalidateRateLines();
				DiscountServiceLevelInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DiscountServiceLevelInfo
		{
			get { return GetZPropertyInfo(nameof(DiscountServiceLevel)); }
		}

		#endregion

		#region Discount

		public ZDecimal Discount
		{
			get
			{
				return Discounts.GetDiscount(DiscountType);
			}
			set
			{
				Discounts.SetDiscount(DiscountType, value);
				OnInvalidateRateLines();
				DiscountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DiscountInfo
		{
			get { return GetZPropertyInfo(nameof(Discount)); }
		}

		#endregion

		#region Discount Type

		[List("DiscountTypes")]
		[MaxLength(3)]
		public ZString DiscountType
		{
			get { return fDiscountType; }
			set
			{
				CheckMaximumLength(DiscountTypeInfo, value);
				fDiscountType = value;
				DiscountTypeInfo.RefreshBinding();
			}
		}

		ZString fDiscountType;

		public ZPropertyInfo DiscountTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DiscountType)); }
		}

		#endregion

		#endregion

		#region Save Company Tariff

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsDeleted)
			{
				return;
			}

			Discounts.RemoveAndDeleteAll();

			base.Delete();
			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion

		#region Level One Tariff Reference

		public CompanyTariff LevelOneTariff
		{
			get
			{
				if (fLevelOneTariff == null)
				{
					if (this.IsLevelOneTariff())
					{
						fLevelOneTariff = this;
					}
					else
					{
						var filter = new ZQuery(RatingHeaderSchema.TH_GlobalRateLevel, SQLComparisonOperator.Equal, (byte)1);
						filter.AddToFilter(JoinCondition.And, RatingHeaderSchema.TH_RateType, SQLComparisonOperator.Equal, TH_RateType);
						AddCompanyFilter(filter);
						fLevelOneTariff = Factory.LoadTop1<CompanyTariff>(filter);
					}
				}
				return fLevelOneTariff;
			}
		}

		CompanyTariff fLevelOneTariff;

		#endregion

		#region GetTariffLevel

		internal static int GetLevel(OrgHeader organisation, GlbCompany company, ZString rateCategory, string rateMode, OrgRateTariffLevel.Directions direction, ZDate startDate, ZDate endDate)
		{
			var levelData = GetLevelData(organisation, company, rateCategory, rateMode, direction, startDate, endDate);
			return levelData != null ? (int)levelData.P7_TariffLevel : Env.Registry.GlobalTariffDefault;
		}

		internal static IEnumerable<int> GetLevelsForRequestedDirections(OrgHeader organisation, GlbCompany company, ZString rateCategory, string rateMode, OrgRateTariffLevel.Directions direction, ZDate startDate, ZDate endDate)
		{
			var results = new List<OrgRateTariffLevel>();

			if ((direction & OrgRateTariffLevel.Directions.IMP) != 0)
			{
				AddIfNotNull(results, GetLevelData(organisation, company, rateCategory, rateMode, OrgRateTariffLevel.Directions.IMP, startDate, endDate));
			}

			if ((direction & OrgRateTariffLevel.Directions.EXP) != 0)
			{
				AddIfNotNull(results, GetLevelData(organisation, company, rateCategory, rateMode, OrgRateTariffLevel.Directions.EXP, startDate, endDate));
			}

			if (results.Count == 0 && (direction & OrgRateTariffLevel.Directions.ALL) != 0)
			{
				AddIfNotNull(results, GetLevelData(organisation, company, rateCategory, rateMode, OrgRateTariffLevel.Directions.ALL, startDate, endDate));
			}

			return results.Select(orgRateTariffLevel => (int)orgRateTariffLevel.P7_TariffLevel).Distinct();
		}

		static void AddIfNotNull(List<OrgRateTariffLevel> results, OrgRateTariffLevel result)
		{
			if (result != null)
			{
				results.Add(result);
			}
		}

		internal static OrgRateTariffLevel GetLevelData(OrgHeader organisation, GlbCompany company, ZString rateCategory, string rateMode, OrgRateTariffLevel.Directions direction, ZDate startDate, ZDate endDate)
		{
			var levels = GetRelevantOrgLevels(organisation, company, rateCategory, rateMode, direction, startDate, endDate);
			var isFreight = RatingConstants.RateCategory.IsFreight(rateCategory);
			return GetLatestAndMostRelevantLevel(levels, rateMode, isFreight, endDate);
		}

		internal static IEnumerable<OrgRateTariffLevel> GetLevelsData(OrgHeader organisation, GlbCompany company, ZString rateCategory, string rateMode, OrgRateTariffLevel.Directions direction, ZDate startDate, ZDate endDate)
		{
			var levels = GetRelevantOrgLevels(organisation, company, rateCategory, rateMode, direction, startDate, endDate);

			if (levels.Any() && organisation == null)
			{
				ErrorReporter.ReportOnce("CompanyTariffGetLevel", string.Format(CultureInfo.InvariantCulture, "It is expected to have zero levels returned when organization is not specified."));
			}

			return levels;
		}

		/// <summary>
		/// Loads company tariffs for the provided organisation and company. Then fetches levels within the provided
		/// date range that are in anyway relevant to the parameters and their generalised versions,
		/// e.g FRT > DEF, IMP > ALL, LSE > AIR > ALL.
		/// </summary>
		/// <returns>
		/// A list of tariff levels that are in anyway relevant to the given parameters.
		/// </returns>
		internal static IEnumerable<OrgRateTariffLevel> GetRelevantOrgLevels(OrgHeader organisation, GlbCompany company, ZString rateCategory, string rateMode, OrgRateTariffLevel.Directions direction, ZDate startDate, ZDate endDate)
		{
			var applicableDirection = new CompanyTariffCodes().HasNoServiceDirection(rateCategory) ? OrgRateTariffLevel.Directions.ALL : direction;

			if (organisation != null)
			{
				var companyPK = company != null ? company.PK : GlbCompany.CurrentCompany.PK;
				var tariffLevelsCollection = new OrgRateTariffLevelCollection(organisation, companyPK);
				tariffLevelsCollection.LoadRelevant();

				if (!string.IsNullOrEmpty(rateMode))
				{
					var isFreight = RatingConstants.RateCategory.IsFreight(rateCategory);
					var generalizedMode = RatingConstants.RateMode.Generalize(rateMode, isFreight);
					return tariffLevelsCollection.GetRelevantLevels(GetLevelCode(rateCategory), applicableDirection.ToString(), rateMode, generalizedMode, startDate, endDate);
				}

				return tariffLevelsCollection.Where(level => level.P7_TariffType == OrgRateTariffLevel.DefaultTariffType && level.P7_Direction == nameof(OrgRateTariffLevel.Directions.ALL) && OrgRateTariffLevelCollection.IsOverlappingDateRange(level, startDate, endDate));
			}
			return Enumerable.Empty<OrgRateTariffLevel>();
		}

		internal static OrgRateTariffLevel GetLatestAndMostRelevantLevel(IEnumerable<OrgRateTariffLevel> levels, string rateMode, bool isFreight, ZDate endDate)
		{
			if (!levels.Any())
			{
				return null;
			}
			return levels.Aggregate((mostRelevant, level) => IsLatestLevelMoreRelevant(level, mostRelevant, rateMode, isFreight, endDate) ? level : mostRelevant);
		}

		static bool IsLatestLevelMoreRelevant(OrgRateTariffLevel level, OrgRateTariffLevel mostRelevant, string rateMode, bool isFreight, ZDate endDate)
		{
			if (IsLaterExpiry(level, mostRelevant, endDate))
			{
				return true;
			}
			if (IsLaterExpiry(mostRelevant, level, endDate))
			{
				return false;
			}

			return IsLevelMoreSpecific(level, mostRelevant, rateMode, isFreight);
		}

		static bool IsLaterExpiry(OrgRateTariffLevel level1, OrgRateTariffLevel level2, ZDate endDate)
		{
			var cappedLevel1Expiry = !endDate.IsEmpty && (level1.P7_ExpiryDate > endDate || level1.P7_ExpiryDate.IsEmpty) ? endDate : level1.P7_ExpiryDate;
			var cappedLevel2Expiry = !endDate.IsEmpty && (level2.P7_ExpiryDate > endDate || level2.P7_ExpiryDate.IsEmpty) ? endDate : level2.P7_ExpiryDate;

			if (cappedLevel2Expiry.IsEmpty)
			{
				return false;
			}

			if (!cappedLevel1Expiry.IsEmpty && cappedLevel1Expiry > cappedLevel2Expiry)
			{
				return true;
			}
			return cappedLevel1Expiry.IsEmpty;
		}

		static bool IsLevelMoreSpecific(OrgRateTariffLevel level1, OrgRateTariffLevel level2, string rateMode, bool isFreight)
		{
			if (IsTariffTypeMoreSpecific(level1, level2))
			{
				return true;
			}
			if (IsTariffTypeMoreSpecific(level2, level1))
			{
				return false;
			}

			if (IsRateModeMoreSpecific(level1, level2, rateMode, isFreight))
			{
				return true;
			}
			if (IsRateModeMoreSpecific(level2, level1, rateMode, isFreight))
			{
				return false;
			}

			if (IsDirectionMoreSpecific(level1, level2))
			{
				return true;
			}
			if (IsDirectionMoreSpecific(level2, level1))
			{
				return false;
			}

			return false;
		}

		static bool IsTariffTypeMoreSpecific(OrgRateTariffLevel level1, OrgRateTariffLevel level2) => level1.P7_TariffType != OrgRateTariffLevel.DefaultTariffType && level2.P7_TariffType == OrgRateTariffLevel.DefaultTariffType;

		static bool IsRateModeMoreSpecific(OrgRateTariffLevel level1, OrgRateTariffLevel level2, string rateMode, bool isFreight)
		{
			if (string.IsNullOrEmpty(rateMode))
			{
				return false;
			}

			if (level1.P7_Mode == rateMode && level2.P7_Mode != rateMode)
			{
				return true;
			}
			if (level1.P7_Mode != rateMode && level2.P7_Mode == rateMode)
			{
				return false;
			}

			var generalizedMode = RatingConstants.RateMode.Generalize(rateMode, isFreight);
			if (level1.P7_Mode == generalizedMode && level2.P7_Mode != generalizedMode)
			{
				return true;
			}
			if (level1.P7_Mode != generalizedMode && level2.P7_Mode == generalizedMode)
			{
				return false;
			}

			return false;
		}

		static bool IsDirectionMoreSpecific(OrgRateTariffLevel level1, OrgRateTariffLevel level2) => level1.P7_Direction != nameof(OrgRateTariffLevel.Directions.ALL) && level2.P7_Direction == nameof(OrgRateTariffLevel.Directions.ALL);

		internal static string GetLevelCode(ZString fCL_LCL)
		{
			var pair = new CompanyTariffCodes().GetCompanyTariffCodeDescription(fCL_LCL);
			return pair != null ? pair.Code : "";
		}

		#endregion

		#region ICompanyTariff Members

		CodeDescriptionPairList ICompanyTariff.CompanyTariffTypes
		{
			get
			{
				if (companyTariffTypes == null)
				{
					lock (companyTariffTypesLock)
					{
						if (companyTariffTypes == null)
						{
							var temp = new CodeDescriptionPairList();
							foreach (var entryCategory in RatingConstants.RateCategory.RateCategories)
							{
								var pair = CompanyTariffCodes.GetCompanyTariffCodeDescription(entryCategory);
								if (pair != null && !temp.ContainsCode(pair.Code))
								{
									temp.Add(pair);
								}
							}
							companyTariffTypes = temp;
						}
					}
				}

				return companyTariffTypes;
			}
		}

		CodeDescriptionPairList ICompanyTariff.GetCompanyTransportModes(ZString tariffType)
		{
			return RateEntryLookups.GetTransportModesByTariffType(tariffType);
		}

		CodeDescriptionPairList ICompanyTariff.GetApplicableDirections(ZString tariffType)
		{
			return RateEntryLookups.GetApplicableDirections(tariffType);
		}

		[ThreadStatic]
		static volatile CodeDescriptionPairList companyTariffTypes;

		static readonly object companyTariffTypesLock = new object();

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CompanyTariffProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		CompanyTariffProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CompanyTariffsWorkflowDescriptorCode; }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return IsAdditionalTariffOrOnlyTariff && base.CanDelete; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsAdditionalTariffOrOnlyTariff)
				{
					return base.ReasonForNotAbleToDelete;
				}

				return ResString.GetMultilingualString("d0761b6d-8a88-41e4-bd59-014f9e701bb2", "You cannot delete {0} when additional {1} Tariff(s) exist.", TH_GlobalRateDescriptionMultilingual, Published);
			}
		}

		bool IsAdditionalTariffOrOnlyTariff
		{
			get { return this.IsAdditionalTariff() || TariffCount == 1; }
		}

		int TariffCount
		{
			get
			{
				var filter = new ZQuery(RatingHeaderSchema.TH_GlobalRateLevel, SQLComparisonOperator.NotEqual, 0);
				AddCompanyFilter(filter);
				filter.AddToFilter(RatingHeaderSchema.TH_RateType, (ZString)TypesAndCodes.GetCode(GetType()));

				return Factory.GetDatabaseCount(typeof(RatingHeader), filter);
			}
		}

		#endregion
	}
}

