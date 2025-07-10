using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(OrgCommissionAgreementRecipient), "Rates")]
	public class OrgCommissionAgreementRecipientRate : AutoOrgCommissionAgreementRecipientRate, ICommissionRateOverridable, ICommissionAgreementRelated<OrgCommissionAgreementRecipientRate>
	{
		#region Schema

		public new abstract class Schema : AutoOrgCommissionAgreementRecipientRate.Schema
		{
			public const string CAT_CommissionStartDate = "CAT_CommissionStartDate";
			public const string CAT_CommissionEndDate = "CAT_CommissionEndDate";
		}

		#endregion

		#region Constructors

		public OrgCommissionAgreementRecipientRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Properties

		#region CAT_CommissionPercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal CAT_CommissionPercentage
		{
			get { return base.CAT_CommissionPercentage; }
			set { base.CAT_CommissionPercentage = value; }
		}

		protected bool CAT_CommissionPercentage_ReadOnly
		{
			get { return !this.IsPercentageCommissionType(); }
		}

		#endregion

		#region CAT_CommissionAmount

		[DecimalPlaces(nameof(CommissionAmountDecimalPlaces))]
		public override ZDecimal CAT_CommissionAmount
		{
			get { return base.CAT_CommissionAmount; }
			set { base.CAT_CommissionAmount = value; }
		}

		protected bool CAT_CommissionAmount_ReadOnly
		{
			get { return !this.IsAmountCommissionType(); }
		}

		public int CommissionAmountDecimalPlaces => CommissionCurrency?.Decimals ?? LocalDecimals;

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#endregion

		#region CAT_RX_NKCommissionCurrency

		[List("Lookups.CommissionCurrencies")]
		public override ZString CAT_RX_NKCommissionCurrency
		{
			get { return base.CAT_RX_NKCommissionCurrency; }
			set { base.CAT_RX_NKCommissionCurrency = value; }
		}

		protected bool CAT_RX_NKCommissionCurrency_ReadOnly
		{
			get { return !this.IsCurrencyCommissionType(); }
		}

		#endregion

		#region CAT_CommissionPeriod

		[List("Lookups.EnabledCommissionPeriods")]
		public override ZString CAT_CommissionPeriod
		{
			get { return base.CAT_CommissionPeriod; }
			set
			{
				var previousPeriod = CAT_CommissionPeriod;
				if (base.CAT_CommissionPeriod != value)
				{
					base.CAT_CommissionPeriod = value;

					if (value.IsEmpty)
					{
						CAT_CommissionStartDateOverride = GetDefaultStartDate(previousPeriod);
						CAT_CommissionEndDateOverride = GetDefaultEndDate(previousPeriod);
					}
					else
					{
						CAT_CommissionStartDateOverride = ZDate.Empty;
						CAT_CommissionEndDateOverride = ZDate.Empty;
					}
				}
			}
		}

		#endregion

		#region CAT_CommissionStartDate

		ZDate GetDefaultStartDate(ZString periodCode)
		{
			var agreement = CommissionAgreement;
			if (agreement == null)
			{
				return ZDate.Empty;
			}

			var effectiveDate = agreement.EffectiveDate;
			if (effectiveDate.IsEmpty || !effectiveDate.IsValid)
			{
				return effectiveDate;
			}

			var commissionPeriod = OrganisationsDataRegistry.Instance.CommissionPeriodList.Value.FindByCode(periodCode);
			if (commissionPeriod == null)
			{
				return ZDate.Empty;
			}

			return effectiveDate.AddMonths(commissionPeriod.Start);
		}

		public ZDate CAT_CommissionStartDate
		{
			get
			{
				return CAT_CommissionPeriod.IsEmpty ? CAT_CommissionStartDateOverride : GetDefaultStartDate(CAT_CommissionPeriod);
			}
			set
			{
				CAT_CommissionStartDateOverride = value;
				CAT_CommissionStartDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateCAT_CommissionStartDate();
				}
			}
		}

		public ZPropertyInfo CAT_CommissionStartDateInfo
		{
			get { return GetZPropertyInfo(Schema.CAT_CommissionStartDate); }
		}

		public ZDate GetCAT_CommissionStartDateOriginalValue()
		{
			return CAT_CommissionPeriodInfo.OriginalValue.IsEmpty ? (ZDate)CAT_CommissionStartDateOverrideInfo.OriginalValue : GetDefaultStartDate((ZString)CAT_CommissionPeriodInfo.OriginalValue);
		}

		protected bool CAT_CommissionStartDate_ReadOnly
		{
			get { return !CAT_CommissionPeriod.IsEmpty; }
		}

		#endregion

		#region CAT_CommissionEndDate

		ZDate GetDefaultEndDate(ZString periodCode)
		{
			var agreement = CommissionAgreement;
			if (agreement == null)
			{
				return ZDate.Empty;
			}

			var effectiveDate = agreement.EffectiveDate;
			if (effectiveDate.IsEmpty || !effectiveDate.IsValid)
			{
				return effectiveDate;
			}

			var commissionPeriod = OrganisationsDataRegistry.Instance.CommissionPeriodList.Value.FindByCode(periodCode);
			if (commissionPeriod == null)
			{
				return ZDate.Empty;
			}

			return commissionPeriod.End != 0 ? effectiveDate.AddMonths(commissionPeriod.End).AddDays(-1) : ZDate.Empty;
		}

		public ZDate CAT_CommissionEndDate
		{
			get
			{
				return CAT_CommissionPeriod.IsEmpty ? CAT_CommissionEndDateOverride : GetDefaultEndDate(CAT_CommissionPeriod);
			}
			set
			{
				CAT_CommissionEndDateOverride = value;
				CAT_CommissionEndDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateCAT_CommissionEndDate();
				}
			}
		}

		public ZPropertyInfo CAT_CommissionEndDateInfo
		{
			get { return GetZPropertyInfo(Schema.CAT_CommissionEndDate); }
		}

		public ZDate GetCAT_CommissionEndDateOriginalValue()
		{
			return CAT_CommissionPeriodInfo.OriginalValue.IsEmpty ? (ZDate)CAT_CommissionEndDateOverrideInfo.OriginalValue : GetDefaultEndDate((ZString)CAT_CommissionPeriodInfo.OriginalValue);
		}

		protected bool CAT_CommissionEndDate_ReadOnly
		{
			get { return !CAT_CommissionPeriod.IsEmpty; }
		}

		#endregion

		public bool CommissionDateCovered(ZDate date)
		{
			return
				(CAT_CommissionStartDate.IsEmpty || CAT_CommissionStartDate <= date) &&
				(CAT_CommissionEndDate.IsEmpty || CAT_CommissionEndDate >= date);
		}

		#endregion

		#region Draft

		public OrgCommissionAgreementRecipientRate ParentVersion
		{
			get
			{
				if (!parentVersionInitialized)
				{
					parentVersionInitialized = true;
					var parentVersionPk = DraftCommissionAgreementLogs.GetParentVersionPk(this);
					if (!parentVersionPk.IsEmpty)
					{
						parentVersion = Factory.Load<OrgCommissionAgreementRecipientRate>(parentVersionPk);
					}
				}

				return parentVersion;
			}
			set
			{
				parentVersionInitialized = true;
				parentVersion = value;
				DraftCommissionAgreementLogs.AddDraftLog(this, value);
			}
		}
		OrgCommissionAgreementRecipientRate parentVersion;
		bool parentVersionInitialized;

		internal OrgCommissionAgreementRecipientRate CreateDraft()
		{
			var draft = Factory.New<OrgCommissionAgreementRecipientRate>();
			draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());
			draft.ParentVersion = this;

			return draft;
		}

		internal OrgCommissionAgreementRecipientRate MergeDraft()
		{
			try
			{
				isMerging = true;

				var parentVersion = ParentVersion;
				if (parentVersion != null)
				{
					parentVersion.CopyPersistentValuesFrom(this, GetDraftCloneArgs());

					parentVersion.HasChanges = true;
					Delete();
					return parentVersion;
				}
				else
				{
					CAT_CAR = CommissionAgreementRecipient.GetMainVersion().PK;
					return this;
				}
			}
			finally
			{
				isMerging = false;
			}
		}
		bool isMerging;

		BusinessObjectCloneArgs GetDraftCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[] { OrgCommissionAgreementRecipientRate.Schema.CAT_CAR }, true);
		}

		#endregion

		#region Save

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !this.IsUncommittedDraft()); }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ModifiedLogs.AddLogsOnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			ModifiedLogs.OnFactorySaved(saveSucceeded);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!isMerging)
			{
				ModifiedLogs.AddDetachedLogOnDelete();
			}

			base.Delete();
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !HasCommissionLine; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("8c0e598e-8f5e-4485-8b2f-a4cba0351ed7", "This rate has already been used for a commission pay out."); }
		}

		public ZBool HasCommissionLine
		{
			get
			{
				var mainVersion = this.GetMainVersion();
				var hasCommissionLineQuery = new ZQuery(AccCommissionLineSchema.CL0_CAT, mainVersion.PK);
				return Factory.LoadTop1<IAccCommissionLine>(hasCommissionLineQuery) != null;
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return IsDeleted ?
					base.HumanReadableNameCore :
					(ZString)Res.GetString("2dfde323-65eb-4483-a3ff-bdaf6ac42fb2", "Commission Agreement Entity Rate ({0}: {1} - {2})", CommissionAgreementRecipient != null ? CommissionAgreementRecipient.Code : ZString.Empty, CAT_CommissionStartDate.ToShortDateString(), CAT_CommissionEndDate.ToShortDateString());
			}
		}

		#endregion

		#region Logs

		internal OrgCommissionAgreementRecipientRateModifiedLogsManager ModifiedLogs
		{
			get { return modifiedLogsManager ?? (modifiedLogsManager = new OrgCommissionAgreementRecipientRateModifiedLogsManager(this)); }
		}
		OrgCommissionAgreementRecipientRateModifiedLogsManager modifiedLogsManager;

		#endregion

		#region Related Business Objects

		public OrgCommissionAgreement CommissionAgreement
		{
			get
			{
				var recipient = CommissionAgreementRecipient;
				if (recipient == null)
				{
					return null;
				}

				return recipient.CommissionAgreement;
			}
		}

		#endregion

		#region ICommissionRateOverridable Members

		GlbCompany ICommissionRateOverridable.Company
		{
			get { return null; }
		}

		ZString ICommissionRateOverridable.CommissionType
		{
			get
			{
				var recipient = CommissionAgreementRecipient;
				if (recipient != null)
				{
					return recipient.CAR_CommissionType;
				}

				return ZString.Empty;
			}
			set
			{
				var recipient = CommissionAgreementRecipient;
				if (recipient != null)
				{
					recipient.CAR_CommissionType = value;
				}
			}
		}

		ZDecimal ICommissionRateOverridable.CommissionPercentage
		{
			get { return CAT_CommissionPercentage; }
			set { CAT_CommissionPercentage = value; }
		}

		ZDecimal ICommissionRateOverridable.CommissionAmount
		{
			get { return CAT_CommissionAmount; }
			set { CAT_CommissionAmount = value; }
		}

		ZString ICommissionRateOverridable.CommissionCurrency
		{
			get { return CAT_RX_NKCommissionCurrency; }
			set { CAT_RX_NKCommissionCurrency = value; }
		}

		ZString ICommissionRateOverridable.CommissionPeriod
		{
			get { return CAT_CommissionPeriod; }
			set { CAT_CommissionPeriod = value; }
		}

		#endregion
	}
}
