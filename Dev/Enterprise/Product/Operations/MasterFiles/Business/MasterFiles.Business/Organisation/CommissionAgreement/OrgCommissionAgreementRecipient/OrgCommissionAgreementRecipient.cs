using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DependentBusinessObject(typeof(OrgCommissionAgreement), "Recipients")]
	public class OrgCommissionAgreementRecipient : AutoOrgCommissionAgreementRecipient, ICommissionAgreementRelated<OrgCommissionAgreementRecipient>
	{
		#region Schema

		public new abstract class Schema : AutoOrgCommissionAgreementRecipient.Schema
		{
			public const string SharePercentage = "SharePercentage";
		}

		#endregion

		#region Constructor

		public OrgCommissionAgreementRecipient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CAR_IsCommissionRateOverriden = true;
			CAR_Share = 1;
		}

		#endregion

		#region Properties

		#region CAR_GS_NKStaff

		[List("Lookups.SalesReps")]
		public override ZString CAR_GS_NKStaff
		{
			get { return base.CAR_GS_NKStaff; }
			set
			{
				if (base.CAR_GS_NKStaff != value)
				{
					base.CAR_OH_Party = ZGuid.Empty;
					base.CAR_GS_NKStaff = value;

					SetDefaultsFromStaffCommissionRule();

					if (!IsValidationSuspended)
					{
						Validation.ValidateCAR_OH_Party();
					}
				}
			}
		}

		protected bool CAR_GS_NKStaff_ReadOnly
		{
			get { return !base.CAR_OH_Party.IsEmpty || AnyRateHasCommissionLine; }
		}

		OverallStaffCommissionRule GetResponsibleStaffCommissionRule()
		{
			var agreement = CommissionAgreement;
			if (agreement == null)
			{
				return null;
			}

			var productServiceModule = agreement.ProductServiceSubModuleForCommisionRule;
			var itemArgs = new CommissionItemArgs(ZGuid.Empty,
				productServiceModule.Product, productServiceModule.Service, productServiceModule.SubModule, ZDate.Empty,
				productServiceModule.Mode, productServiceModule.Origin, productServiceModule.Destination);

			return GetResponsibleStaffCommissionRule(itemArgs);
		}

		public OverallStaffCommissionRule GetResponsibleStaffCommissionRule(CommissionItemArgs itemArgs)
		{
			var staff = Staff;
			if (staff == null)
			{
				return null;
			}

			var agreement = CommissionAgreement;
			if (agreement == null)
			{
				return null;
			}

			var opportunity = agreement.Opportunity;
			if (opportunity == null)
			{
				return null;
			}

			var customer = agreement.Customer;
			var customerUnloco = customer != null ? customer.ClosestPort : null;

			var localClient = opportunity.Header;
			var localClientUnloco = localClient != null ? localClient.ClosestPort : null;

			return staff.GetOverallCommissionRuleResponsibleFor(customerUnloco, localClientUnloco, itemArgs);
		}

		public void SetDefaultsFromStaffCommissionRule()
		{
			if (!Rates.Any(x => x.HasCommissionLine))
			{
				base.CAR_IsCommissionRateOverriden = false;

				var responsibleRule = GetResponsibleStaffCommissionRule();
				ImportRates(responsibleRule);
				RefreshRatesReadOnly();
			}
		}

		public ZBool HasResponsibleStaffCommissionRule
		{
			get { return GetResponsibleStaffCommissionRule() != null; }
		}

		#endregion

		#region CAR_OH_Party

		public override ZGuid CAR_OH_Party
		{
			get
			{
				var staff = Staff;
				if (staff != null && !IgnorePartyOrgFromStaff)
				{
					return GetStaffOrgProxyPk(staff);
				}

				return base.CAR_OH_Party;
			}
			set
			{
				if (base.CAR_OH_Party != value)
				{
					base.CAR_GS_NKStaff = ZString.Empty;
					base.CAR_OH_Party = value;

					Validation.ValidateCAR_GS_NKStaff();
				}
			}
		}

		public static ZGuid GetStaffOrgProxyPk(GlbStaff staff)
		{
			var homeBranch = staff.HomeBranch;
			if (homeBranch == null)
			{
				return ZGuid.Empty;
			}

			var company = homeBranch.Company;
			if (company == null)
			{
				return ZGuid.Empty;
			}

			return company.GC_OH_OrgProxy;
		}

		protected bool CAR_OH_Party_ReadOnly
		{
			get { return !base.CAR_GS_NKStaff.IsEmpty || AnyRateHasCommissionLine; }
		}

		#endregion

		#region Code

		public ZString Code
		{
			get
			{
				if (!CAR_GS_NKStaff.IsEmpty)
				{
					return CAR_GS_NKStaff;
				}
				else if (!CAR_OH_Party.IsEmpty)
				{
					var party = Party;
					return party != null ? party.OH_Code : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region CAR_IsCommissionValueOverriden

		public override ZBool CAR_IsCommissionRateOverriden
		{
			get { return base.CAR_IsCommissionRateOverriden; }
			set
			{
				base.CAR_IsCommissionRateOverriden = value;
				if (!value)
				{
					var responsibleRule = GetResponsibleStaffCommissionRule();
					ImportRates(responsibleRule);
				}

				RefreshRatesReadOnly();
			}
		}

		#endregion

		#region CAR_CommissionType

		[List("Lookups.CommissionTypes")]
		public override ZString CAR_CommissionType
		{
			get { return base.CAR_CommissionType; }
			set
			{
				if (base.CAR_CommissionType != value)
				{
					base.CAR_CommissionType = value;

					if (value.IsEmpty)
					{
						CAR_Share = 0;
					}
					else
					{
						RefreshSharePercentageBindingInfoForAllAgreementRecipients();
					}

					foreach (var rate in Rates)
					{
						CommissionRuleHelper.SetCommissionTypeDefaults(rate);
					}
				}
			}
		}

		protected bool CAR_CommissionType_ReadOnly
		{
			get { return !IsEditRatesAllowed || !CAR_IsCommissionRateOverriden; }
		}

		#endregion

		#region CAR_Share

		public override ZByte CAR_Share
		{
			get { return base.CAR_Share; }
			set
			{
				var previousShare = CAR_Share;
				if (base.CAR_Share != value)
				{
					base.CAR_Share = value;

					if (previousShare == 0 && !CAR_IsCommissionRateOverriden && Rates.Count == 0 && value > 0)
					{
						var responsibleRule = GetResponsibleStaffCommissionRule();
						if (responsibleRule != null)
						{
							ImportRates(responsibleRule);
						}
					}

					RefreshSharePercentageBindingInfoForAllAgreementRecipients();
				}
			}
		}

		protected bool CAR_Share_ReadOnly
		{
			get { return !CAR_IsCommissionRateOverriden && Rates.Count == 0 && !HasResponsibleStaffCommissionRule; }
		}

		#endregion

		#region CAR_EndDate

		protected bool CAR_EndDate_ReadOnly
		{
			get { return !IsEditRatesAllowed; }
		}

		#endregion

		#region Name

		public ZString Name
		{
			get
			{
				if (!CAR_GS_NKStaff.IsEmpty)
				{
					var staff = Staff;
					return staff != null ? staff.GS_FullName : ZString.Empty;
				}
				else if (!CAR_OH_Party.IsEmpty)
				{
					var party = Party;
					return party != null ? party.OH_FullName : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region SharePercentage

		[DecimalPlaces(2)]
		public ZDecimal SharePercentage
		{
			get
			{
				var recipientsOfSameAgreement = Factory.Load<OrgCommissionAgreementRecipient>(new ZQuery(OrgCommissionAgreementRecipientSchema.CAR_CA0, CAR_CA0));
				var agreementRecipientTotalShares = recipientsOfSameAgreement.Where(x => x.CAR_CommissionType == CAR_CommissionType).Sum(x => x.CAR_Share);
				if (agreementRecipientTotalShares == 0)
				{
					return 0;
				}

				return 100d * CAR_Share / agreementRecipientTotalShares;
			}
		}

		public ZPropertyInfo SharePercentageInfo
		{
			get { return GetZPropertyInfo(Schema.SharePercentage); }
		}

		void RefreshSharePercentageBindingInfoForAllAgreementRecipients()
		{
			SharePercentageInfo.RefreshBinding();
			var agreement = CommissionAgreement;
			if (agreement != null)
			{
				foreach (var recipient in agreement.Recipients.Where(x => x != this))
				{
					recipient.SharePercentageInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#endregion

		#region Ignore Party Org from Staff

		int ignorePartyOrgFromStaffIndex;
		bool IgnorePartyOrgFromStaff => ignorePartyOrgFromStaffIndex > 0;

		sealed class IgnorePartyOrgFromStaffSetter : IDisposable
		{
			public IgnorePartyOrgFromStaffSetter(OrgCommissionAgreementRecipient recipient)
			{
				this.recipient = recipient;
				recipient.ignorePartyOrgFromStaffIndex++;
			}

			readonly OrgCommissionAgreementRecipient recipient;

			public void Dispose()
			{
				recipient.ignorePartyOrgFromStaffIndex--;
			}
		}

		IDisposable SetIgnorePartyOrgFromStaff()
		{
			return new IgnorePartyOrgFromStaffSetter(this);
		}

		#endregion

		#region Draft

		public OrgCommissionAgreementRecipient ParentVersion
		{
			get
			{
				if (!parentVersionInitialized)
				{
					parentVersionInitialized = true;
					var parentVersionPk = DraftCommissionAgreementLogs.GetParentVersionPk(this);
					if (!parentVersionPk.IsEmpty)
					{
						parentVersion = Factory.Load<OrgCommissionAgreementRecipient>(parentVersionPk);
					}
				}

				return parentVersion;
			}
			private set
			{
				parentVersionInitialized = true;
				parentVersion = value;
				DraftCommissionAgreementLogs.AddDraftLog(this, value);
			}
		}
		OrgCommissionAgreementRecipient parentVersion;
		bool parentVersionInitialized;

		internal OrgCommissionAgreementRecipient CreateDraft()
		{
			var draft = Factory.New<OrgCommissionAgreementRecipient>();
			using (SetIgnorePartyOrgFromStaff())
			{
				draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());
			}
			draft.ParentVersion = this;

			foreach (var rate in Rates.ToArray())
			{
				draft.Rates.Add(rate.CreateDraft());
			}

			return draft;
		}

		internal OrgCommissionAgreementRecipient MergeDraft()
		{
			try
			{
				isMerging = true;

				var draft = this;
				var parentVersion = ParentVersion;
				if (parentVersion != null)
				{
					using (SetIgnorePartyOrgFromStaff())
					{
						parentVersion.CopyPersistentValuesFrom(this, GetDraftCloneArgs());
					}

					var parentVersionRatesNotInDraft = new HashSet<OrgCommissionAgreementRecipientRate>(parentVersion.Rates);
					foreach (var draftRate in draft.Rates.ToArray())
					{
						parentVersionRatesNotInDraft.Remove(draftRate.MergeDraft());
					}
					foreach (var parentVersionRate in parentVersionRatesNotInDraft)
					{
						if (!parentVersionRate.CanDelete)
						{
							string agreementId = parentVersionRate.CommissionAgreement?.AgreementId;

							string messageForLog = string.Format(
								CultureInfo.InvariantCulture,
								(NoResString)"Commision rate '{0}' for agreement '{1}' cannot be removed." +
								(NoResString)" Reason: {2}",
								parentVersionRate.PK,
								agreementId,
								parentVersionRate.ReasonForNotAbleToDelete
							);

							string messageForUser = CommissionAgreementMessageTemplates.GetCannotMergeDraftCommisionMessage(agreementId);

							throw new CommissionAgreementApprovalException(messageForLog, messageForUser);
						}
						parentVersionRate.Delete();
					}

					parentVersion.HasChanges = true;
					Delete();
					return parentVersion;
				}
				else
				{
					CAR_CA0 = CommissionAgreement.MainVersion.PK;
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
			return new BusinessObjectCloneArgs(new[] { OrgCommissionAgreementRecipient.Schema.CAR_CA0 }, true);
		}

		#endregion

		#region Emails

		public IEnumerable<ZString> NotificationEmailAddresses
		{
			get
			{
				if (!CAR_GS_NKStaff.IsEmpty)
				{
					var staff = Staff;
					if (staff != null && !staff.GS_EmailAddress.IsEmpty)
					{
						yield return staff.GS_EmailAddress;
					}
				}
				else if (!CAR_OH_Party.IsEmpty)
				{
					var party = Party;
					if (party != null)
					{
						foreach (OrgContact contact in party.Contacts)
						{
							// checking IsCommissionAgreementRecipientContact last because it is expensive
							if (!contact.OC_Email.IsEmpty && contact.IsCommissionAgreementRecipientContact)
							{
								yield return contact.OC_Email;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Security

		ZBool CurrentLoginUserIsOpportunitySalesPerson
		{
			get
			{
				var agreement = CommissionAgreement;
				if (agreement == null)
				{
					return false;
				}

				var opportunity = agreement.Opportunity;
				if (opportunity == null)
				{
					return false;
				}

				return opportunity.P8_GS_NKPrimarySalesPerson == GlbStaff.CurrentUser.GS_Code;
			}
		}

		public ZBool IsViewRatesAllowed
		{
			get
			{
				return
					Env.Security.CommissionAgreementViewAny.IsAllowed ||
					CAR_GS_NKStaff == GlbStaff.CurrentUser.GS_Code ||
					(CAR_GS_NKStaff.IsEmpty && CurrentLoginUserIsOpportunitySalesPerson);
			}
		}

		public ZBool IsEditRatesAllowed
		{
			get
			{
				return
					Env.Security.CommissionAgreementOverrideAny.IsAllowed ||
					(CAR_GS_NKStaff.IsEmpty && CurrentLoginUserIsOpportunitySalesPerson);
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
					(ZString)Res.GetString("5b1281ec-44d7-4da2-b993-f53611d23e44", "Commission Agreement Entity ({0})", Code);
			}
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
			((IActiveBusinessObjectCollection)Rates).Refresh();
			Rates.DeleteAll();

			if (!isMerging)
			{
				ModifiedLogs.AddDetachedLogOnDelete();
			}
			base.Delete();
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !AnyRateHasCommissionLine; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("67727909-1650-4a08-bb32-eec0e48b4862", "Wolf Pack members cannot be deleted once commissions have been generated. To exclude this Wolf Pack Member from this agreement, set its share to 0, and then re-approve the agreement."); }
		}

		ZBool AnyRateHasCommissionLine
		{
			get
			{
				var mainVersion = this.GetMainVersion();
				return mainVersion.Rates.Any(x => x.HasCommissionLine);
			}
		}

		#endregion

		#region Logs

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(Rates.Where(x => x.IsMainVersion()));

				return result.ToArray();
			}
		}

		internal OrgCommissionAgreementRecipientModifiedLogsManager ModifiedLogs
		{
			get { return modifiedLogsManager ?? (modifiedLogsManager = new OrgCommissionAgreementRecipientModifiedLogsManager(this)); }
		}
		OrgCommissionAgreementRecipientModifiedLogsManager modifiedLogsManager;

		#endregion

		#region Related Business Objects

		#region Rates

		[ChildEditable]
		public OrgCommissionAgreementRecipientRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new OrgCommissionAgreementRecipientRateCollection(this);
					RefreshRatesReadOnly();
				}

				return rates;
			}
		}
		OrgCommissionAgreementRecipientRateCollection rates;

		void RefreshRatesReadOnly()
		{
			if (rates != null)
			{
				var expectedRatesEditable = IsEditRatesAllowed && CAR_IsCommissionRateOverriden;
				SetupChildEditable(rates, expectedRatesEditable);
			}
		}

		void ImportRates(ICommissionRuleRatesProvider newCommissionRuleRatesProvider)
		{
			if (Rates.Any(x => x.HasCommissionLine))
			{
				throw new InvalidOperationException("Tried to import rates while existing rates have already been used for commission");
			}

			Rates.DeleteAll();

			if (newCommissionRuleRatesProvider != null && newCommissionRuleRatesProvider.Rates.Any())
			{
				var firstRate = newCommissionRuleRatesProvider.Rates.First();
				CAR_CommissionType = firstRate.ACT_CommissionType;

				foreach (var rateRule in newCommissionRuleRatesProvider.Rates)
				{
					var newRate = Rates.AddNew();
					using (newRate.GetValidationSuspender())
					{
						newRate.CAT_CommissionPercentage = rateRule.ACT_CommissionPercentage;
						newRate.CAT_CommissionAmount = rateRule.ACT_CommissionAmount;
						newRate.CAT_RX_NKCommissionCurrency = rateRule.ACT_RX_NKCommissionCurrency;
						newRate.CAT_CommissionPeriod = rateRule.ACT_CommissionPeriod;
					}
				}
			}
			else
			{
				CAR_CommissionType = ZString.Empty;
				CAR_Share = 0;
			}
		}

		#endregion

		void SetupChildEditable<T>(ActiveBusinessObjectCollection<T> collection, bool shouldBeEditable) where T : BusinessObject
		{
			if (collection.ReadOnly == shouldBeEditable)
			{
				collection.SetReadOnlyIncludingChildren(!shouldBeEditable);
			}

			if (shouldBeEditable)
			{
				if (!IsRegisteredEditableChildObject(collection))
				{
					RegisterEditableChildObject(collection);
				}
			}
			else
			{
				if (IsRegisteredEditableChildObject(collection))
				{
					UnRegisterEditableChildObject(collection);
				}
			}
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (CAR_GS_NKStaff.IsEmpty && CAR_OH_Party.IsEmpty)
			{
				CAR_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
