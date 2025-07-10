using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OverallStaffCommissionRule : AutoOverallStaffCommissionRule, ICommissionRule
	{
		#region Schema

		public abstract new class Schema : AutoOverallStaffCommissionRule.Schema
		{
			public const string SourceDescription = "SourceDescription";
		}

		#endregion

		#region Constructors

		public OverallStaffCommissionRule(GlbStaff staff, AccGroupCommissionRule groupRule)
			: base(groupRule.Factory)
		{
			Argument.NotNull(groupRule, "groupRule");

			using (SuspendSettingHasChanges())
			{
				StaffCode = staff.GS_Code;
			}

			BaseRule = groupRule;
			Source = OverallStaffCommissionRuleSource.Group;

			RegisterEditableChildObject(BaseRule);
			BaseRule.NotificationsChanged += BaseRule_NotificationsChanged;
		}

		public OverallStaffCommissionRule(AccStaffCommissionRule staffRule)
			: base(staffRule.Factory)
		{
			Argument.NotNull(staffRule, "staffRule");

			using (SuspendSettingHasChanges())
			{
				StaffCode = staffRule.ACM_GS_NKStaff;
			}

			BaseRule = staffRule;
			Source = OverallStaffCommissionRuleSource.Staff;

			RegisterEditableChildObject(BaseRule);
			BaseRule.NotificationsChanged += BaseRule_NotificationsChanged;
		}

		void BaseRule_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			if (BaseRule.IsDeleted)
			{
				BaseRule.NotificationsChanged -= BaseRule_NotificationsChanged;

				foreach (var parentCollection in ParentCollections.ToList())
				{
					parentCollection.RemoveAndDelete(this);
				}
			}
		}

		public readonly AccCommissionRule BaseRule;
		public readonly OverallStaffCommissionRuleSource Source;

		#endregion

		#region Properties

		#region Source

		public ZString SourceDescription
		{
			get
			{
				switch (Source)
				{
					case OverallStaffCommissionRuleSource.Staff:
						return ResString.GetMultilingualString("4704cb75-dd04-48a6-9d2e-7694ca9897f7", "Staff");

					case OverallStaffCommissionRuleSource.Group:
						return ResString.GetMultilingualString("521ebcab-ae88-44f7-9412-93dec57b636f", "Team");

					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo SourceDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SourceDescription); }
		}

		#endregion

		#region Status

		[List("Lookups.Statuses")]
		[BusinessObjectTestExclude]
		public override ZString Status
		{
			get
			{
				switch (Source)
				{
					case OverallStaffCommissionRuleSource.Staff:
						return StaffCommissionRuleStatusTypes.Codes.Enabled;

					case OverallStaffCommissionRuleSource.Group:
						return
							RuleDisable != null ? GroupCommissionRuleStatusTypes.Codes.Disabled :
							RuleOverride != null ? GroupCommissionRuleStatusTypes.Codes.Overridden :
							GroupCommissionRuleStatusTypes.Codes.Inherited;

					default:
						return ZString.Empty;
				}
			}
			set
			{
				if (Status != value)
				{
					if (Source == OverallStaffCommissionRuleSource.Group)
					{
						if (value == GroupCommissionRuleStatusTypes.Codes.Disabled)
						{
							RuleOverride = null;
							RuleDisable = Factory.New<AccCommissionRuleStaffDisable>();
						}
						else if (value == GroupCommissionRuleStatusTypes.Codes.Overridden)
						{
							var newRuleOverride = Factory.New<AccCommissionRuleStaffOverride>();
							using (newRuleOverride.GetValidationSuspender())
							{
								newRuleOverride.CRO_CommissionBasis = BaseRule.ACM_CommissionBasis;
								newRuleOverride.CRO_CommissionTriggerType = BaseRule.ACM_CommissionTriggerType;
							}

							foreach (var baseRuleRate in BaseRule.Rates)
							{
								var newRuleOverrideRate = newRuleOverride.Rates.AddNew();
								using (newRuleOverrideRate.GetValidationSuspender())
								{
									newRuleOverrideRate.ACT_CommissionType = baseRuleRate.ACT_CommissionType;
									newRuleOverrideRate.ACT_CommissionPercentage = baseRuleRate.ACT_CommissionPercentage;
									newRuleOverrideRate.ACT_RX_NKCommissionCurrency = baseRuleRate.ACT_RX_NKCommissionCurrency;
									newRuleOverrideRate.ACT_CommissionAmount = baseRuleRate.ACT_CommissionAmount;
									newRuleOverrideRate.ACT_CommissionPeriod = baseRuleRate.ACT_CommissionPeriod;
								}
							}

							RuleDisable = null;
							RuleOverride = newRuleOverride;
						}
						else
						{
							RuleDisable = null;
							RuleOverride = null;
						}
					}

					base.Status = value;
					RefreshRatesAsCollectionForBinding();
					RefreshBinding();
				}
			}
		}

		#endregion

		#region StaffCode

		public GlbStaff Staff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, StaffCode); }
		}

		#endregion

		#region GroupPk

		[List("Lookups.SalesTeams")]
		public override ZGuid GroupPk
		{
			get { return BaseRule.ACM_GG; }
			set
			{
				BaseRule.ACM_GG = value;
				GroupPkInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateGroupPk();
				}
			}
		}

		public GlbGroup Group
		{
			get { return BaseRule.Group; }
		}

		#endregion

		#region CompanyPk

		[List("Lookups.Companies")]
		public override ZGuid CompanyPk
		{
			get { return BaseRule.ACM_GC; }
			set
			{
				BaseRule.ACM_GC = value;
				CompanyPkInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCompanyPk();
				}
			}
		}

		protected bool CompanyPk_ReadOnly
		{
			get { return GroupPk.IsValid; }
		}

		public GlbCompany Company
		{
			get { return BaseRule.Company; }
		}

		#endregion

		#region Product

		[List("Lookups.Products")]
		public override ZString Product
		{
			get { return BaseRule.ACM_Product; }
			set
			{
				BaseRule.ACM_Product = value;
				ProductInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateProduct();
				}

				if (!CommissionRuleLookups.ProductSupportsTradeLane(Product))
				{
					Mode = OrgCommissionAgreementItem.AllItemCode;
					Origin = ZString.Empty;
					Destination = ZString.Empty;
				}
			}
		}

		#endregion

		#region Service

		[List("Lookups.Services")]
		public override ZString Service
		{
			get { return BaseRule.ACM_Service; }
			set
			{
				BaseRule.ACM_Service = value;
				ServiceInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateService();
				}
			}
		}

		#endregion

		#region SubModule

		[List("Lookups.SubModules")]
		public override ZString SubModule
		{
			get { return BaseRule.ACM_SubModule; }
			set
			{
				BaseRule.ACM_SubModule = value;
				SubModuleInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateSubModule();
				}
			}
		}

		#endregion

		#region Mode

		[List("Lookups.Modes")]
		public override ZString Mode
		{
			get { return BaseRule.ACM_Mode; }
			set
			{
				if (BaseRule != null)
				{
					BaseRule.ACM_Mode = value;
				}

				ModeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateMode();
				}
			}
		}

		public bool Mode_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(Product);
			}
		}

		#endregion

		#region Origin

		[List("Lookups.Locations")]
		public override ZString Origin
		{
			get { return BaseRule.ACM_NKOrigin; }
			set
			{
				BaseRule.ACM_NKOrigin = value;
				OriginInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateOrigin();
				}
			}
		}

		public bool Origin_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(Product);
			}
		}

		#endregion

		#region Destination

		[List("Lookups.Locations")]
		public override ZString Destination
		{
			get { return BaseRule.ACM_NKDestination; }
			set
			{
				BaseRule.ACM_NKDestination = value;
				DestinationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateDestination();
				}
			}
		}

		public bool Destination_ReadOnly
		{
			get
			{
				return !CommissionRuleLookups.ProductSupportsTradeLane(Product);
			}
		}

		#endregion

		#region StartDate

		public override ZDateTime StartDate
		{
			get { return BaseRule.ACM_StartDate; }
			set
			{
				BaseRule.ACM_StartDate = value.Date;
				StartDateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartDate();
				}
			}
		}

		ZDate ICommissionRule.StartDate
		{
			get { return StartDate.Date; }
		}

		#endregion

		#region EndDate

		public override ZDateTime EndDate
		{
			get { return BaseRule.ACM_EndDate; }
			set
			{
				BaseRule.ACM_EndDate = value.Date;
				EndDateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateEndDate();
				}
			}
		}

		ZDate ICommissionRule.EndDate
		{
			get { return EndDate.Date; }
		}

		#endregion

		#region IsCurrent

		public ZBool IsCurrent(ZDate targetDate)
		{
			return
				(StartDate.IsEmpty || StartDate <= targetDate) &&
				(EndDate.IsEmpty || EndDate >= targetDate);
		}

		#endregion

		#region Commission Rate

		ICommissionRuleRatesProvider OverallCommissionRatesProvider
		{
			get { return (ICommissionRuleRatesProvider)RuleOverride ?? BaseRule; }
		}

		#region CommissionBasis

		[List("Lookups.CommissionBasisType")]
		public override ZString CommissionBasis
		{
			get { return OverallCommissionRatesProvider.CommissionBasis; }
			set
			{
				OverallCommissionRatesProvider.CommissionBasis = value;
				CommissionBasisInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCommissionBasis();
				}
			}
		}

		#endregion

		#region CommissionTriggerType

		[List("Lookups.TriggerTypes")]
		public override ZString CommissionTriggerType
		{
			get { return OverallCommissionRatesProvider.CommissionTriggerType; }
			set
			{
				OverallCommissionRatesProvider.CommissionTriggerType = value;
				CommissionTriggerTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCommissionTriggerType();
				}
			}
		}

		#endregion

		#region Rates

		public AccCommissionRuleRateCollection BaseRuleRates
		{
			get
			{
				if (baseRuleRates == null)
				{
					baseRuleRates = new AccCommissionRuleRateCollection(BaseRule);
					if (Source == OverallStaffCommissionRuleSource.Group)
					{
						baseRuleRates.SetReadOnlyIncludingChildren(true);
					}
					else
					{
						RegisterEditableChildObject(baseRuleRates);
					}
				}

				return baseRuleRates;
			}
		}
		AccCommissionRuleRateCollection baseRuleRates;

		public AccCommissionRuleRateCollection Rates
		{
			get
			{
				if (RuleOverride != null)
				{
					return RuleOverride.Rates;
				}
				return BaseRuleRates;
			}
		}

		#region RatesAsCollectionForBiniding

		/// <summary>
		/// Bind to this property instead of 'Rates' so that it can automatically rebind to the correct AccCommissionRuleRateCollection whenever the 'Status' has been changed between Disabled/Inherrited/Overriden.
		/// </summary>
		public RatesWrapperCollection RatesAsCollectionForBinding
		{
			get
			{
				if (ratesAsCollectionForBinding == null)
				{
					ratesAsCollectionForBinding = new RatesWrapperCollection();
					RefreshRatesAsCollectionForBinding();
				}

				return ratesAsCollectionForBinding;
			}
		}
		RatesWrapperCollection ratesAsCollectionForBinding;

		void RefreshRatesAsCollectionForBinding()
		{
			if (ratesAsCollectionForBinding != null)
			{
				ratesAsCollectionForBinding.RemoveAll();
				ratesAsCollectionForBinding.Add(new RatesWrapper(Rates));
			}
		}

		public class RatesWrapper : NonPersistentBusinessObject
		{
			public RatesWrapper(AccCommissionRuleRateCollection inner)
			{
				Inner = inner;
			}

			public AccCommissionRuleRateCollection Inner { get; private set; }
		}

		public class RatesWrapperCollection : NonPersistentBusinessObjectCollection<RatesWrapper>
		{
			protected override bool AllowNewCore
			{
				get { return false; }
			}

			protected override bool AllowRemoveCore
			{
				get { return false; }
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotSupportedException();
			}
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region Related Business Objects

		#region Rule Disable

		public AutoAccCommissionRuleStaffDisable RuleDisable
		{
			get
			{
				if (ruleDisable == null || ruleDisable.IsDeleted || ruleDisable.ACD_ACM_ParentRule != BaseRule.PK)
				{
					if (ruleDisable != null)
					{
						UnRegisterEditableChildObject(ruleDisable);
					}

					var staffRuleDisableQuery = GetRuleDisableQuery();
					ruleDisable = Factory.LoadTop1<AccCommissionRuleStaffDisable>(staffRuleDisableQuery);
					if (ruleDisable != null)
					{
						RegisterEditableChildObject(ruleDisable);
					}
				}

				return ruleDisable;
			}
			set
			{
				if (RuleDisable != value)
				{
					if (RuleDisable != null)
					{
						if (!RuleDisable.IsDeleted)
						{
							RuleDisable.Delete();
						}

						UnRegisterEditableChildObject(RuleDisable);
					}

					ruleDisable = value;
					if (value != null)
					{
						value.ACD_GS_NKStaff = StaffCode;
						value.ACD_ACM_ParentRule = BaseRule.PK;
						RegisterEditableChildObject(value);
					}
				}
			}
		}
		AutoAccCommissionRuleStaffDisable ruleDisable;

		internal ZQuery GetRuleDisableQuery()
		{
			var query = new ZQuery(AccCommissionRuleStaffDisableSchema.ACD_GS_NKStaff, StaffCode);
			query.AddToFilter(AccCommissionRuleStaffDisableSchema.ACD_ACM_ParentRule, BaseRule.PK);
			return query;
		}

		#endregion

		#region Rule Override

		public AccCommissionRuleStaffOverride RuleOverride
		{
			get
			{
				if (ruleOverride == null || ruleOverride.IsDeleted || ruleOverride.CRO_ACM_ParentRule != BaseRule.PK)
				{
					if (ruleOverride != null)
					{
						UnRegisterEditableChildObject(ruleOverride);
					}

					var staffRuleOverrideQuery = GetRuleOverrideQuery();
					ruleOverride = Factory.LoadTop1<AccCommissionRuleStaffOverride>(staffRuleOverrideQuery);
					if (ruleOverride != null)
					{
						RegisterEditableChildObject(ruleOverride);
					}
				}

				return ruleOverride;
			}
			set
			{
				if (RuleOverride != value)
				{
					if (RuleOverride != null)
					{
						if (!RuleOverride.IsDeleted)
						{
							RuleOverride.Delete();
						}

						UnRegisterEditableChildObject(RuleOverride);
					}

					ruleOverride = value;
					if (value != null)
					{
						value.CRO_GS_NKStaff = StaffCode;
						value.CRO_ACM_ParentRule = BaseRule.PK;
						RegisterEditableChildObject(value);
					}
				}
			}
		}
		AccCommissionRuleStaffOverride ruleOverride;

		internal ZQuery GetRuleOverrideQuery()
		{
			var query = new ZQuery(AccCommissionRuleStaffOverrideSchema.CRO_GS_NKStaff, StaffCode);
			query.AddToFilter(AccCommissionRuleStaffOverrideSchema.CRO_ACM_ParentRule, BaseRule.PK);
			return query;
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return base.CanDelete && Source != OverallStaffCommissionRuleSource.Group; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var group = BaseRule.Group;
				var groupName = group != null ? group.HumanReadableName : ZString.Empty;
				return ResString.GetMultilingualString("3e0d4002-f227-4d9c-b72e-95f229f0feb9", "Can not delete this rule as it is inherited from {0}. If this rule does not apply to this staff, disable this rule instead.", groupName);
			}
		}

		public override void Delete()
		{
			BaseRule.Delete();
			base.Delete();
		}

		#endregion

		#region Fetch Strategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new OverallStaffCommissionRuleFetchStrategy(this);
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			if (MetaData.GetReadOnlyExcludingMethodProvider(this, property))
			{
				return true;
			}

			if (Source == OverallStaffCommissionRuleSource.Staff)
			{
				return false;
			}
			else if (Source == OverallStaffCommissionRuleSource.Group)
			{
				if (property.Name == Schema.Status)
				{
					return false;
				}

				var isOverrideableGroupProperty =
					(property.Name == Schema.CommissionBasis) ||
					(property.Name == Schema.CommissionTriggerType);

				if (Status == GroupCommissionRuleStatusTypes.Codes.Overridden && isOverrideableGroupProperty)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Lookups

		public OverallStaffCommissionRuleLookups Lookups
		{
			get { return GetNewLookups(); }
		}

		protected OverallStaffCommissionRuleLookups GetNewLookups()
		{
			return new OverallStaffCommissionRuleLookups(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("3a5fe199-50e0-4c92-bfdb-9e1f36f4c183", "Commission Rule"); }
		}

		#endregion

		#region Implementation

		public override bool IsInDatabase
		{
			get { return BaseRule.IsInDatabase; }
		}

		#endregion
	}
}
