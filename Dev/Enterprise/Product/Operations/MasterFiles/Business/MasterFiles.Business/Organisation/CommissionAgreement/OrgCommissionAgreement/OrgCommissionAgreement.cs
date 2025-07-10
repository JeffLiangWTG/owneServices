using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty("AgreementId", nameof(OrgCommissionAgreement.BuildCodeQuery))]
	[DescriptionProperty("AgreementId")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgCommissionAgreement : AutoOrgCommissionAgreement, ICommissionAgreementRelated<OrgCommissionAgreement>
	{
		#region Schema

		public abstract new class Schema : AutoOrgCommissionAgreement.Schema
		{
			public const string IsApproved = "IsApproved";
			public const string IsDraft = "IsDraft";
			public const string AgreementId = "AgreementId";
			public const string EffectiveDate = "EffectiveDate";
			public const string Status = "Status";
			public const string StatusDescription = "StatusDescription";
		}

		#endregion

		#region Constructor

		public OrgCommissionAgreement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CA0_CA0_ParentVersion), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(CA0_LastApprovedDateUtc), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Validation

		protected override OrgCommissionAgreementValidation GetNewValidation()
		{
			return new OrgCommissionAgreementValidation(this, RunFromApprovalItem || ShouldAutoApproveOnSave);
		}

		#endregion

		#region Properties

		#region AgreementId

		public ZString AgreementId
		{
			get
			{
				var opportunity = Opportunity;
				var prefix = opportunity != null ? opportunity.P8_OpportunityID : ZString.Empty;
				return prefix + CA0_Name;
			}
		}

		public ZPropertyInfo AgreementIdInfo
		{
			get { return GetZPropertyInfo(Schema.AgreementId); }
		}

		public static ZQuery BuildCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgCommissionAgreement));

			var orgOpportunitySubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgCommissionAgreementSchema.CA0_P8);
			var sql = FormattableString.Invariant(
				$"{OrgOpportunitySchema.Constants.P8_OpportunityID}+{OrgCommissionAgreementSchema.Constants.CA0_Name} {comparisonOperator.ComparisonText(value)} @AgreementId");
			value = comparisonOperator.ValueForLiteralADO(value).ToString();

			var @params = new ZSqlParameterCollection();
			var agreementIdColumn = new SchemaStringColumn(CargoWise.Schema.Schema.GenericTableSchema, "AgreementId", 0, SqlDbType.VarChar, null, true, OrgOpportunitySchema.P8_OpportunityID.MaxLength + OrgCommissionAgreementSchema.CA0_Name.MaxLength);
			@params.Add(ZSqlParameter.New("@AgreementId", value, agreementIdColumn));

			orgOpportunitySubQuery.AddFilterAndZSQLParameterCollection(sql, @params);

			query.AddSubQuery(orgOpportunitySubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region CA0_CA0_ParentVersion

		public override ZGuid CA0_CA0_ParentVersion
		{
			get { return base.CA0_CA0_ParentVersion; }
			set
			{
				if (base.CA0_CA0_ParentVersion != value)
				{
					base.CA0_CA0_ParentVersion = value;
					RefreshProductItemsEditable();
				}
			}
		}

		#endregion

		#region CA0_CommissionTriggerType

		[List("Lookups.TriggerTypes")]
		public override ZString CA0_CommissionTriggerType
		{
			get { return base.CA0_CommissionTriggerType; }
			set
			{
				if (base.CA0_CommissionTriggerType != value)
				{
					base.CA0_CommissionTriggerType = value;
					if (value != OrgCommissionAgreementTriggerTypes.Codes.Manual)
					{
						CA0_EffectiveDate = ZDate.Empty;
					}

					StatusInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region CA0_CommissionBasis

		[List("Lookups.CommissionBasisType")]
		public override ZString CA0_CommissionBasis
		{
			get { return base.CA0_CommissionBasis; }
			set { base.CA0_CommissionBasis = value; }
		}

		#endregion

		#region CA0_CommissionStream

		[List("Lookups.CommissionBasisStreams_Active")]
		public override ZString CA0_CommissionStream
		{
			get { return base.CA0_CommissionStream; }
			set { base.CA0_CommissionStream = value; }
		}

		public ZString CA0_CommissionStreamDescription
		{
			get { return Lookups.CommissionBasisStreams_All.GetDescriptionFromCode(CA0_CommissionStream); }
		}

		#endregion

		#region CA0_EffectiveDate

		public override ZDate CA0_EffectiveDate
		{
			get { return base.CA0_EffectiveDate; }
			set
			{
				if (CA0_EffectiveDate != value)
				{
					base.CA0_EffectiveDate = value;
					StatusInfo.RefreshBinding();
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZDate EffectiveDate
		{
			get { return GetEffectiveDateForTriggerType(CA0_CommissionTriggerType); }
			set
			{
				if (CA0_CommissionTriggerType == OrgCommissionAgreementTriggerTypes.Codes.Manual)
				{
					CA0_EffectiveDate = value;
				}

				EffectiveDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EffectiveDateInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveDate); }
		}

		public ZDate GetEffectiveDateOriginalValue()
		{
			var originalTriggerType = (ZString)CA0_CommissionTriggerTypeInfo.OriginalValue;
			if (originalTriggerType == OrgCommissionAgreementTriggerTypes.Codes.Manual)
			{
				return (ZDate)CA0_EffectiveDateInfo.OriginalValue;
			}

			return GetEffectiveDateForTriggerType(originalTriggerType);
		}

		protected bool EffectiveDate_ReadOnly
		{
			get { return CA0_CommissionTriggerType != OrgCommissionAgreementTriggerTypes.Codes.Manual; }
		}

		protected virtual ZDate GetEffectiveDateForTriggerType(ZString triggerType)
		{
			if (triggerType == OrgCommissionAgreementTriggerTypes.Codes.Manual)
			{
				return CA0_EffectiveDate;
			}

			var customer = Customer;
			if (customer == null)
			{
				return ZDate.Empty;
			}

			switch (triggerType)
			{
				case CommissionTriggerTypes.Codes.ClientCommencedDate:
					return customer.MiscServ.OM_CMClientCommenced.Date;

				case CommissionTriggerTypes.Codes.FirstArInvoice:
					return GetFirstArInvoicePostDate(customer).Date;

				case CommissionTriggerTypes.Codes.EarliestRevRecog:
					return GetArInvoiceEarliestRecognitionDate(customer).Date;
			}

			return ZDate.Empty;
		}

		ZDateTime GetFirstArInvoicePostDate(OrgHeader header)
		{
			var arInvoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_OH, header.PK);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Opportunity.P8_GC);
			arInvoicesFilter.OrderBy = AccTransactionHeader.Schema.AH_PostDate;

			var arInvoices = Factory.LoadTop1<AccTransactionHeader>(arInvoicesFilter);
			if (arInvoices == null)
			{
				return ZDateTime.Empty;
			}
			else
			{
				return arInvoices.AH_PostDate;
			}
		}

		ZDateTime GetArInvoiceEarliestRecognitionDate(OrgHeader header)
		{
			var arInvoicesFilter = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_OH, header.PK);
			arInvoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Opportunity.P8_GC);

			var arInvoicesLineFilter = new ZDBOnlyQuery(typeof(AccTransactionLines));
			arInvoicesLineFilter.AddSubQuery(AccTransactionLinesSchema.AL_AH, arInvoicesFilter, JoinCondition.And);
			arInvoicesLineFilter.OrderBy = $"CASE WHEN {AccTransactionLinesSchema.AL_LineType.Name} " +
	$"IN ('{TransactionLineTypes.Revenue}', '{TransactionLineTypes.Cost}') " +
	$"THEN {AccTransactionLinesSchema.AL_ReverseDate.Name} ELSE {AccTransactionLinesSchema.AL_PostDate.Name} END";

			var arInvoiceLine = Factory.LoadTop1<AccTransactionLines>(arInvoicesLineFilter);
			return GetRecognitionDate(arInvoiceLine);
		}

		ZDateTime GetRecognitionDate(AccTransactionLines line)
		{
			if (line == null)
			{
				return ZDate.Empty;
			}

			if (line.AL_LineType == TransactionLineTypes.Revenue || line.AL_LineType == TransactionLineTypes.Cost)
			{
				return line.AL_ReverseDate;
			}
			else
			{
				return line.AL_PostDate;
			}
		}

		#endregion

		#region CA0_ExpiredDate

		public override ZDate CA0_ExpiredDate
		{
			get { return base.CA0_ExpiredDate; }
			set
			{
				if (base.CA0_ExpiredDate != value)
				{
					base.CA0_ExpiredDate = value;
					StatusInfo.RefreshBinding();
				}
			}
		}

		protected bool CA0_ExpiredDate_ReadOnly
		{
			get { return true; }
		}

		public ZBool IsExpired(ZDate date)
		{
			return (!CA0_ExpiredDate.IsEmpty) && (CA0_ExpiredDate <= date);
		}

		public void Expire(ZDate date)
		{
			CA0_ExpiredDate = date;
		}

		#endregion

		#region CA0_LastApprovedDateUtc

		public ZDateTime CA0_LastApprovedDateLocal
		{
			get { return CA0_LastApprovedDateUtc.ToLocalBranchTime(); }
			set { CA0_LastApprovedDateUtc = value.ToUniversalBranchTime(); }
		}

		#endregion

		#region CA0_Name

		protected bool CA0_Name_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region CA0_ReversedDateUtc

		public override ZDateTime CA0_ReversedDateUtc
		{
			get { return base.CA0_ReversedDateUtc; }
			set
			{
				if (base.CA0_ReversedDateUtc != value)
				{
					base.CA0_ReversedDateUtc = value;
					StatusInfo.RefreshBinding();
				}
			}
		}

		public ZBool IsReversed
		{
			get { return !CA0_ReversedDateUtc.IsEmpty; }
		}

		public void Reverse()
		{
			if (!IsReversed)
			{
				CA0_ReversedDateUtc = ZDateTime.UtcNow;
				RefreshProductItemsEditable();
				RefreshRecipientsEditable();
			}
		}

		#endregion

		#region CA0_SystemCreateTimeUtc

		public ZDateTime CA0_SystemCreateTimeLocal
		{
			get { return CA0_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region CA0_SystemLastEditTimeUtc

		public ZDateTime CA0_SystemLastEditTimeLocal
		{
			get { return CA0_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region AlreadyHasCommissionLines

		public ZBool AlreadyHasCommissionLines
		{
			get
			{
				var mainVersion = MainVersion;
				return !mainVersion.CA0_FirstUsageDateUtc.IsEmpty;
			}
		}

		#endregion

		#region CategorizationProperty

		#region CA0_OH_Customer

		[List("Lookups.Customers")]
		public override ZGuid CA0_OH_Customer
		{
			get { return base.CA0_OH_Customer; }
			set
			{
				var previousValue = base.CA0_OH_Customer;
				if (base.CA0_OH_Customer != value)
				{
					base.CA0_OH_Customer = value;

					DefaultTriggerTypeAndCommissionBasisFromSalesPersonCommissionRules();
					if (previousValue.IsEmpty)
					{
						AddPrimarySalesPersonAsRecipientIfNotExists();
					}

					OnCategorizationPropertyChanged();
					StatusInfo.RefreshBinding();
				}
			}
		}

		internal void DefaultTriggerTypeAndCommissionBasisFromSalesPersonCommissionRules()
		{
			if (!CA0_CommissionTriggerType.IsEmpty && !CA0_CommissionBasis.IsEmpty)
			{
				return;
			}

			var opportunity = Opportunity;
			if (opportunity == null)
			{
				return;
			}

			var customer = Customer;
			if (customer == null)
			{
				return;
			}

			var salesPerson = opportunity.PrimarySalesPerson;
			if (salesPerson == null)
			{
				return;
			}

			var localClient = opportunity.Header;
			var localPort = localClient != null ? localClient.ClosestPort : null;
			var details = ProductServiceSubModuleForCommisionRule;

			var itemArgs = new CommissionItemArgs(ZGuid.Empty, details.Product, details.Service, details.SubModule, ZDate.Empty, details.Mode, details.Origin, details.Destination);
			var responsibleRule = salesPerson.GetOverallCommissionRuleResponsibleFor(customer.ClosestPort, localPort, itemArgs);
			if (responsibleRule != null)
			{
				CA0_CommissionBasis = responsibleRule.CommissionBasis;
				CA0_CommissionTriggerType = responsibleRule.CommissionTriggerType;
			}
		}

		void AddPrimarySalesPersonAsRecipientIfNotExists()
		{
			var opportunity = Opportunity;
			if (opportunity != null && !opportunity.P8_GS_NKPrimarySalesPerson.IsEmpty)
			{
				if (!Recipients.Any(x => x.CAR_GS_NKStaff == opportunity.P8_GS_NKPrimarySalesPerson))
				{
					var salesPersonRecipient = Recipients.AddNew();
					salesPersonRecipient.CAR_GS_NKStaff = opportunity.P8_GS_NKPrimarySalesPerson;
				}
			}
		}

		#endregion

		#region AgreementItems

		public void NotifyAgreementItemsChanged()
		{
			if (!notifyChangedSuspended)
			{
				OnAgreementItemsChanged();
			}
		}

		void OnAgreementItemsChanged()
		{
			OnCategorizationPropertyChanged();
		}

		public IDisposable GetNotifyAgreementItemsChangedSuspender()
		{
			if (notifyChangedSuspended)
			{
				return null;
			}

			notifyChangedSuspended = true;
			return new DisposableAction(() =>
			{
				notifyChangedSuspended = false;
				OnAgreementItemsChanged();
			});
		}

		bool notifyChangedSuspended;

		#endregion

		void OnCategorizationPropertyChanged()
		{
			RefreshCommissionRuleDefaultsOfAllNonSavedStaffRecipients();
		}

		public void RefreshCommissionRuleDefaultsOfAllNonSavedStaffRecipients()
		{
			foreach (var recipient in Recipients)
			{
				if (
					!recipient.CAR_GS_NKStaff.IsEmpty &&
					(!recipient.CAR_IsCommissionRateOverriden || recipient.Rates.Count == 0))
				{
					recipient.SetDefaultsFromStaffCommissionRule();
				}
			}
		}

		#endregion

		#region IsDraft

		public ZBool IsDraft
		{
			get { return CA0_LastApprovedDateUtc.IsEmpty; }
		}

		public ZPropertyInfo IsDraftInfo
		{
			get { return GetZPropertyInfo(Schema.IsDraft); }
		}

		#endregion

		#region

		public bool IsDraftCreatedFromOrganisationMerge
		{
			get { return IsDraft && !CA0_CA0_ParentVersion.IsEmpty && CA0_ReversedDateUtc.IsEmpty && CA0_ExpiredDate.IsEmpty; }
		}

		#endregion

		#region IsApproved

		public ZBool IsApproved
		{
			get { return IsUncommittedDraft || (!IsDraft && !HasDraft); }
		}

		public ZPropertyInfo IsApprovedInfo
		{
			get { return GetZPropertyInfo(Schema.IsApproved); }
		}

		#endregion

		#region

		public bool IsEditable
		{
			get
			{
				if (!IsApproved && !Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed
					|| IsApproved && !Env.Security.ApprovedCommissionAgreementEdit.IsAllowed)
				{
					return false;
				}

				return
					!IsInDatabase ||
					(Opportunity.P8_GS_NKPrimarySalesPerson == GlbStaff.CurrentUser.GS_Code) ||
					Env.Security.CommissionAgreementOverrideAny.IsAllowed;
			}
		}

		#endregion

		#region OpportunityIsEffective

		public ZBool OpportunityIsEffective(ZGuid companyPk)
		{
			var opportunity = Opportunity;
			if (opportunity == null)
			{
				return false;
			}

			return OrganisationsDataRegistry.Instance.OpportunityStatus.GetValueWithoutFallback(companyPk.IsValid ? companyPk.ToGuid() : Guid.Empty, Guid.Empty, Guid.Empty).GetEffectiveAgreementFromCode(opportunity.P8_Status);
		}

		#endregion

		#region ProductServiceSubModuleForCommisionRule

		public CommissionRuleDetails ProductServiceSubModuleForCommisionRule
		{
			get
			{
				ZString product = CommissionRuleLookups.AnyProductsCode;
				ZString service = CommissionRuleLookups.AnyServicesCode;
				ZString subModule = CommissionRuleLookups.AnySubModulesCode;
				ZString mode = OrgCommissionAgreementItemLookups.AllModesCode;
				ZString origin = "";
				ZString destination = "";

				if (ProductItems.Count == 0)
				{
					product = ZString.Empty;
				}
				else if (ProductItems.Count == 1)
				{
					var productItem = ProductItems[0];
					product = productItem.CAI_Code;

					if (productItem.ChildServiceItems.Count == 0)
					{
						service = ZString.Empty;
					}
					else if (productItem.ChildServiceItems.Count == 1)
					{
						var serviceItem = productItem.ChildServiceItems[0];
						service = serviceItem.CAI_Code;

						if (serviceItem.ChildSubModuleItems.Count == 0)
						{
							subModule = ZString.Empty;
						}
						else if (serviceItem.ChildSubModuleItems.Count == 1)
						{
							var subModuleItem = serviceItem.ChildSubModuleItems[0];
							subModule = subModuleItem.CAI_Code;
						}
					}
					if (productItem.ConditionCollection.Count == 1)
					{
						mode = productItem.ConditionCollection[0].CIC_Mode;
						origin = productItem.ConditionCollection[0].CIC_RL_NKOrigin;
						destination = productItem.ConditionCollection[0].CIC_RL_NKDestination;
					}
				}

				return new CommissionRuleDetails(product, service, subModule, mode, origin, destination);
			}
		}

		public class CommissionRuleDetails
		{
			public CommissionRuleDetails(ZString product, ZString service, ZString subModule, ZString mode, ZString origin, ZString destination)
			{
				Product = product;
				Service = service;
				SubModule = subModule;
				Mode = mode;
				Origin = origin;
				Destination = destination;
			}

			public ZString Product { get; }
			public ZString Service { get; }
			public ZString SubModule { get; }
			public ZString Mode { get; }
			public ZString Origin { get; }
			public ZString Destination { get; }
		}

		#endregion

		#region Status

		public ZString Status
		{
			get { return GetStatus(ZDate.Today); }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		public ZString StatusDescription
		{
			get
			{
				var description = new OrgCommissionAgreementStatusList().GetDescriptionFromCode(Status);
				if (IsInQueue || ParentIsInQueue)
				{
					description += ", " + Res.GetString("779C6AA9-6959-4C7B-B2E0-750334A0792C", "Queued");
				}

				return description;
			}
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.StatusDescription, (x) => StatusInfo); }
		}

		public ZString GetStatus(ZDate date)
		{
			if (IsReversed)
			{
				return OrgCommissionAgreementStatusList.Codes.Reversed;
			}
			else if (IsExpired(date))
			{
				return OrgCommissionAgreementStatusList.Codes.Expired;
			}
			else if (EffectiveDate <= date)
			{
				return OrgCommissionAgreementStatusList.Codes.Active;
			}
			else
			{
				return OrgCommissionAgreementStatusList.Codes.Inactive;
			}
		}

		#endregion

		#endregion

		#region Approve

		public void Approve(string logMessage = "")
		{
			CA0_LastApprovedDateUtc = ZDateTime.UtcNow;
			Logs.AddNew(Events.StatusChange, $"Approved: {logMessage}");
		}

		#endregion

		#region Draft

		public override OrgCommissionAgreement ParentVersion
		{
			get { return UncommittedParentVersion ?? base.ParentVersion; }
		}

		void DetachFromParentVersion()
		{
			UncommittedParentVersion = null;
			CA0_CA0_ParentVersion = ZGuid.Empty;
		}

		public OrgCommissionAgreement MainVersion
		{
			get { return this.GetMainVersion(); }
		}

		public ZBool IsFirstDraft
		{
			get { return this.IsMainVersion() && IsDraft; }
		}

		public OrgCommissionAgreement CreateDraft(bool uncommitted = false)
		{
			var draft = Factory.New<OrgCommissionAgreement>();
			PopulateDraft(draft);

			if (!uncommitted)
			{
				draft.CA0_CA0_ParentVersion = PK;
			}
			((IBusinessObjectState)draft).ClearHasChangesIncludingChildren();
			if (uncommitted)
			{
				draft.UncommittedParentVersion = this;  // do this after ClearHasChangesIncludingChildren(), otherwise it will trigger UncommittedDraft_HasChangesChanged event
			}

			return draft;
		}

		protected virtual void PopulateDraft(OrgCommissionAgreement draft)
		{
			draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());
			draft.CA0_LastApprovedDateUtc = ZDateTime.Empty;
			DraftCommissionAgreementLogs.AddDraftLog(draft, this);

			foreach (var productItem in ProductItems.ToArray())
			{
				draft.ProductItems.Add(productItem.CreateDraft());
			}

			foreach (var recipient in Recipients.ToArray())
			{
				draft.Recipients.Add(recipient.CreateDraft());
			}
		}

		OrgCommissionAgreement MergeDraft()
		{
			var draft = this;
			var parentVersion = ParentVersion;
			if (parentVersion != null)
			{
				parentVersion.CopyValuesFromDraft(draft);

				DetachFromParentVersion();
				parentVersion.HasChanges = true;
				Delete();

				return parentVersion;
			}
			else
			{
				return this;
			}
		}

		protected virtual void CopyValuesFromDraft(OrgCommissionAgreement draft)
		{
			CopyPersistentValuesFrom(draft, GetDraftCloneArgs());

			var recipientsNotInDraft = new HashSet<OrgCommissionAgreementRecipient>(Recipients);
			foreach (var draftRecipient in draft.Recipients.ToArray())
			{
				recipientsNotInDraft.Remove(draftRecipient.MergeDraft());
			}
			foreach (var recipient in recipientsNotInDraft)
			{
				if (!recipient.CanDelete)
				{
					string agreementId = recipient.CommissionAgreement?.AgreementId;

					string messageForLog = string.Format(
						CultureInfo.InvariantCulture,
						(NoResString)"Commision recipient '{0}' for agreement '{1}' cannot be removed." +
						(NoResString)" Reason: {2}",
						recipient.PK,
						agreementId,
						recipient.ReasonForNotAbleToDelete
					);

					string messageForUser = CommissionAgreementMessageTemplates.GetCannotMergeDraftCommisionMessage(agreementId);

					throw new CommissionAgreementApprovalException(messageForLog, messageForUser);
				}
				recipient.Delete();
			}

			var productItemsNotInDraft = new HashSet<OrgCommissionAgreementItem>(ProductItems);
			foreach (var draftProductItem in draft.ProductItems.ToArray())
			{
				productItemsNotInDraft.Remove(draftProductItem.MergeDraft());
			}
			foreach (var productItem in productItemsNotInDraft)
			{
				productItem.Delete();
			}
		}

		BusinessObjectCloneArgs GetDraftCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[]
				{
					OrgCommissionAgreement.Schema.CA0_CA0_ParentVersion,
					OrgCommissionAgreement.Schema.CA0_FirstUsageDateUtc,
					OrgCommissionAgreement.Schema.CA0_LastApprovedDateUtc
				},
				true);
		}

		#region Approve / Disapprove

		public void ApproveDraft(string logMessage = "")
		{
			if (!IsDraft)
			{
				throw new InvalidOperationException("Must be draft");
			}

			var newVersion = MergeDraft();
			newVersion.Approve(logMessage);
		}

		public void DisapproveDraft()
		{
			if (!IsDraft)
			{
				throw new InvalidOperationException("Must be draft");
			}

			var parentVersion = ParentVersion;
			if (parentVersion != null)
			{
				DetachFromParentVersion();
				parentVersion.Logs.AddNew(Events.StatusChange, "Disapproved");
			}

			Delete();
		}

		public void NotifyHasChangedPreventingAutoApproval()
		{
			hasChangePreventingAutoApproval = true;
		}
		bool hasChangePreventingAutoApproval;

		#endregion

		#region UncommittedDraft

		public ZBool IsUncommittedDraft
		{
			get { return UncommittedParentVersion != null; }
		}

		void CommitUncommittedDraft()
		{
			if (IsUncommittedDraft)
			{
				CA0_CA0_ParentVersion = UncommittedParentVersion.PK;
				UncommittedParentVersion = null;
			}
		}

		internal OrgCommissionAgreement UncommittedParentVersion
		{
			get { return uncommittedParentVersion; }
			private set
			{
				if (uncommittedParentVersion != value)
				{
					uncommittedParentVersion = value;
					if (uncommittedParentVersion != null)
					{
						HasChangesChanged -= UncommittedDraft_HasChangesChanged;
						HasChangesChanged += UncommittedDraft_HasChangesChanged;
					}
					IsApprovedInfo.RefreshBinding();
					RefreshProductItemsEditable();
				}
			}
		}
		OrgCommissionAgreement uncommittedParentVersion;

		void UncommittedDraft_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (HasChanges)
			{
				HasChangesChanged -= UncommittedDraft_HasChangesChanged;
				CommitUncommittedDraft();
			}
		}

		#endregion

		#endregion

		#region Coverage

		#region CoversOverallItem

		public OrgCommissionAgreementItem GetItem(params Tuple<ZBool, ZString>[] itemPath)
		{
			OrgCommissionAgreementItem result = null;

			if (itemPath.Length > 0 && itemPath.Length < 4)
			{
				result = ProductItems.FirstOrDefault(x => x.CAI_IsInclude == itemPath[0].Item1 && x.CAI_Code == itemPath[0].Item2);
				if (result != null && itemPath.Length > 1)
				{
					result = result.ChildServiceItems.FirstOrDefault(x => x.CAI_IsInclude == itemPath[1].Item1 &&
																		  x.CAI_Code == itemPath[1].Item2);

					if (result != null && itemPath.Length > 2)
					{
						result = result.ChildSubModuleItems.FirstOrDefault(x => x.CAI_IsInclude == itemPath[2].Item1 && x.CAI_Code == itemPath[2].Item2);
					}
				}
			}

			return result;
		}

		public OrgCommissionAgreementItem FindDuplicate(OrgCommissionAgreementItem item)
		{
			if (item == null)
			{
				return null;
			}

			var topItem = item.TopLevelItem;

			if (!topItem.CanHaveConditions)
			{
				return GetItem(item.GetItemPath().ToArray());
			}

			var products = ProductItems.Where(p => p.CAI_IsInclude == topItem.CAI_IsInclude && p.CAI_Code == topItem.CAI_Code && p.CanHaveConditions);

			if (!topItem.ConditionCollection.Any())
			{
				return products.FirstOrDefault(p => !p.ConditionCollection.Any());
			}

			foreach (var cond in topItem.ConditionCollection)
			{
				var duplicate = products.FirstOrDefault(p => p.HasCondition(cond.CIC_Mode, cond.CIC_RL_NKOrigin, cond.CIC_RL_NKDestination));
				if (duplicate != null)
				{
					return duplicate;
				}
			}

			return null;
		}

		public IEnumerable<OrgCommissionAgreementItem> GetItemsThatAreCoveredBy(CommissionItemArgs itemArgs)
		{
			foreach (var productItem in ProductItems.Where(x => x.CAI_IsInclude))
			{
				if (productItem.CAI_Code == OrgCommissionAgreementItemLookups.AllProductsCode)
				{
					if (itemArgs.Product == OrgCommissionAgreementItemLookups.AllProductsCode)
					{
						yield return productItem;
					}
				}
				else if (productItem.CanHaveConditions && productItem.CAI_Code == itemArgs.Product)
				{
					if (productItem.ConditionCollection.Any(c =>
						(itemArgs.Mode == OrgCommissionAgreementItemLookups.AllModesCode || itemArgs.Mode == c.CIC_Mode)
						&& (itemArgs.Origin.IsEmpty || itemArgs.Origin == OrgCommissionAgreementItem.AllItemCode || c.CIC_RL_NKOrigin.StartsWith(itemArgs.Origin))
						&& (itemArgs.Destination.IsEmpty || itemArgs.Destination == OrgCommissionAgreementItem.AllItemCode || c.CIC_RL_NKDestination.StartsWith(itemArgs.Destination))
					))
					{
						yield return productItem;
					}
				}
				else if (itemArgs.Product == OrgCommissionAgreementItemLookups.AllProductsCode || itemArgs.Product == productItem.CAI_Code)
				{
					foreach (var serviceItem in productItem.ChildServiceItems.Where(x => x.CAI_IsInclude))
					{
						if (serviceItem.CAI_Code == OrgCommissionAgreementItemLookups.AllServicesCode)
						{
							if (itemArgs.Service == OrgCommissionAgreementItemLookups.AllServicesCode)
							{
								yield return serviceItem;
							}
						}
						else if (itemArgs.Service == OrgCommissionAgreementItemLookups.AllServicesCode || itemArgs.Service == serviceItem.CAI_Code)
						{
							foreach (var subModuleItem in serviceItem.ChildSubModuleItems.Where(x => x.CAI_IsInclude))
							{
								if (itemArgs.SubModule == OrgCommissionAgreementItemLookups.AllSubModulesCode || (subModuleItem.CAI_Code == itemArgs.SubModule))
								{
									yield return subModuleItem;
								}
							}
						}
					}
				}
			}
		}

		public IEnumerable<OrgCommissionAgreementItem> GetItemsThatCover(CommissionItemArgs itemArgs)
		{
			return GetItemsThatExcludes(itemArgs).Any() ? Enumerable.Empty<OrgCommissionAgreementItem>() : GetItemsThatIncludes(itemArgs);
		}

		IEnumerable<OrgCommissionAgreementItem> GetItemsThatIncludes(CommissionItemArgs itemArgs)
		{
			foreach (var productItem in ProductItems.Where(x => x.CAI_IsInclude))
			{
				if (productItem.CAI_Code == OrgCommissionAgreementItemLookups.AllProductsCode)
				{
					yield return productItem;
				}
				else if (productItem.CanHaveConditions && productItem.CAI_Code == itemArgs.Product)
				{
					if (productItem.ConditionCollection.Any(c =>
						(c.CIC_Mode == OrgCommissionAgreementItemLookups.AllModesCode || itemArgs.Mode == c.CIC_Mode)
						&& (c.CIC_RL_NKOrigin.IsEmpty || c.CIC_RL_NKOrigin == OrgCommissionAgreementItem.AllItemCode || itemArgs.Origin.StartsWith(c.CIC_RL_NKOrigin))
						&& (c.CIC_RL_NKDestination.IsEmpty || c.CIC_RL_NKDestination == OrgCommissionAgreementItem.AllItemCode || itemArgs.Destination.StartsWith(c.CIC_RL_NKDestination))
					))
					{
						yield return productItem;
					}
				}
				else if (itemArgs.Product != OrgCommissionAgreementItemLookups.AllProductsCode && productItem.CAI_Code == itemArgs.Product)
				{
					foreach (var serviceItem in productItem.ChildServiceItems.Where(x => x.CAI_IsInclude))
					{
						if (serviceItem.CAI_Code == OrgCommissionAgreementItemLookups.AllServicesCode)
						{
							yield return serviceItem;
						}
						else if (itemArgs.Service != OrgCommissionAgreementItemLookups.AllServicesCode && serviceItem.CAI_Code == itemArgs.Service)
						{
							foreach (var subModuleItem in serviceItem.ChildSubModuleItems.Where(x => x.CAI_IsInclude))
							{
								if (subModuleItem.CAI_Code == OrgCommissionAgreementItemLookups.AllSubModulesCode || subModuleItem.CAI_Code == itemArgs.SubModule)
								{
									yield return subModuleItem;
								}
							}
						}
					}
				}
			}
		}

		public IEnumerable<OrgCommissionAgreementItem> GetItemsThatExcludes(CommissionItemArgs itemArgs)
		{
			foreach (var productItem in ProductItems.Where(x => x.CAI_Code == itemArgs.Product))
			{
				if (!productItem.CAI_IsInclude)
				{
					yield return productItem;
				}
				else
				{
					foreach (var serviceItem in productItem.ChildServiceItems.Where(x => x.CAI_Code == itemArgs.Service))
					{
						if (!serviceItem.CAI_IsInclude)
						{
							yield return serviceItem;
						}
						else
						{
							foreach (var subModuleItem in serviceItem.ChildSubModuleItems.Where(x => x.CAI_Code == itemArgs.SubModule))
							{
								if (!subModuleItem.CAI_IsInclude)
								{
									yield return subModuleItem;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Security

		public ZBool IsViewRecipientsAdditionalInformationAllowed
		{
			get
			{
				return
					!IsInDatabase ||
					(Opportunity != null && Opportunity.P8_GS_NKPrimarySalesPerson == GlbStaff.CurrentUser.GS_Code) ||
					Env.Security.CommissionAgreementViewAny.IsAllowed ||
					CurrentLoginIsARecipient;
			}
		}

		#region Security Checkpoints

		public static SecurityCheckpoint ExpireAgreementSecurityCheckpoint
		{
			get { return Env.Security.CommissionAgreementOverrideAny; }
		}

		public static SecurityCheckpoint ReverseAgreementSecurityCheckpoint
		{
			get { return Env.Security.CommissionAgreementOverrideAny; }
		}

		#endregion

		#endregion

		#region Save

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsUncommittedDraft); }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ModifiedLogs.AddLogsOnFactorySaving();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsDraft && ShouldAutoApproveOnSave)
			{
				ApproveDraft((NoResString)"Auto Approved On Save");
			}
		}

		bool ShouldAutoApproveOnSave
		{
			get
			{
				if (IsInDatabase)
				{
					return false;
				}

				if (!OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.Value)
				{
					return false;
				}

				if (MainVersion.IsInDatabase)
				{
					if (hasChangePreventingAutoApproval)
					{
						return false;
					}

					return true;
				}
				else
				{
					var effectiveDate = EffectiveDate;
					return effectiveDate.IsEmpty || effectiveDate > ZDate.Today;
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			ModifiedLogs.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				hasChangePreventingAutoApproval = false;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			HasChangesChanged -= UncommittedDraft_HasChangesChanged;

			((IActiveBusinessObjectCollection)Recipients).Refresh();
			((IActiveBusinessObjectCollection)ProductItems).Refresh();
			Recipients.DeleteAll();
			ProductItems.DeleteAll();

			var parentVersion = ParentVersion;
			if (parentVersion != null)
			{
				parentVersion.Delete();
			}

			var queue = new OrgCommissionCalculationQueueCollection(Factory, this);
			queue.DeleteAll();

			base.Delete();
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !AlreadyHasCommissionLines; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				List<MultilingualString> msg = new List<MultilingualString>();
				var originalAgreement = MainVersion;
				var disableText = originalAgreement.IsExpired(ZDate.Today) || originalAgreement.IsReversed ? (MultilingualString)((NoResString)string.Empty) : ResString.GetMultilingualString("78582299-5629-4b05-ac40-2dcdb86af867", "If you wish to disable the agreement, select the \"Disable\" option from the context menu.");
				var reverseText = originalAgreement.IsReversed ? (MultilingualString)((NoResString)string.Empty) : ResString.GetMultilingualString("b2f62c47-d70a-4173-9c19-d0f2e5ec46ef", "If you wish to revert all commission payments, select the \"Reverse\" option from the context menu.");

				msg.Add(GetCannotDeleteBecauseAlreadyHasCommissionLinesErrorMessage());
				if (!disableText.IsEmpty)
				{
					msg.Add(disableText);
				}
				if (!reverseText.IsEmpty)
				{
					msg.Add(reverseText);
				}

				return MultilingualString.Join(" ", msg.ToArray());
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
					(ZString)Res.GetString("7a518518-4346-4d1b-9b10-6975552a73a4", "Commission Agreement {0}", AgreementId);
			}
		}

		#endregion

		#region Logs

		#region Related Event BusinessObjects

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(ProductItems.Where(x => x.IsMainVersion()));
				foreach (var item in ProductItems)
				{
					result.AddRange(item.BusinessObjectsWithRelatedEvents);
				}

				result.AddRange(Recipients.Where(x => x.IsMainVersion()));
				foreach (var recipient in Recipients)
				{
					result.AddRange(recipient.BusinessObjectsWithRelatedEvents);
				}

				var draft = Draft;
				if (draft != null)
				{
					result.AddRange(draft.BusinessObjectsWithRelatedEvents);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Modified Logs

		public OrgCommissionAgreementModifiedLogsManager ModifiedLogs
		{
			get { return modifiedLogsManager ?? (modifiedLogsManager = new OrgCommissionAgreementModifiedLogsManager(this)); }
		}
		OrgCommissionAgreementModifiedLogsManager modifiedLogsManager;

		#endregion

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new OrgCommissionAgreementUniqueIndexFailureHandler(this); }
		}

		#endregion

		#region RunFromApprovalItem

		public bool RunFromApprovalItem
		{
			get { return runFromApprovalItem; }
			set { runFromApprovalItem = value; }
		}
		bool runFromApprovalItem;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgCommissionAgreementFetchStrategy(this);
		}

		#endregion

		#region Related Business Objects

		#region Draft

		public OrgCommissionAgreement Draft
		{
			get { return Factory.LoadTop1<OrgCommissionAgreement>(new ZQuery(OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, PK)); }
		}

		public bool HasDraft
		{
			get { return Draft != null; }
		}

		#endregion

		#region Product Items

		[ChildEditable]
		public OrgCommissionAgreementItemCollection ProductItems
		{
			get
			{
				if (productItems == null)
				{
					productItems = new OrgCommissionAgreementItemCollection(this);
					RegisterEditableChildObject(productItems);
					RefreshProductItemsEditable();
				}

				return productItems;
			}
		}
		OrgCommissionAgreementItemCollection productItems;

		void RefreshProductItemsEditable()
		{
			if (productItems != null)
			{
				var expectedReadOnly = ReadOnly || IsReversed;
				SetupChildReadOnly(productItems, expectedReadOnly);
			}
		}

		#endregion

		#region Recipients

		[ChildEditable]
		public OrgCommissionAgreementRecipientCollection Recipients
		{
			get
			{
				if (recipients == null)
				{
					recipients = new OrgCommissionAgreementRecipientCollection(this);
					RegisterEditableChildObject(recipients);
					RefreshRecipientsEditable();
				}

				return recipients;
			}
		}
		OrgCommissionAgreementRecipientCollection recipients;

		void RefreshRecipientsEditable()
		{
			if (recipients != null)
			{
				var expectedReadOnly = ReadOnly || IsReversed;
				SetupChildReadOnly(recipients, expectedReadOnly);
			}
		}

		public ZBool CurrentLoginIsARecipient
		{
			get { return Recipients.Any(x => x.CAR_GS_NKStaff == GlbStaff.CurrentUser.GS_Code); }
		}

		#endregion

		void SetupChildReadOnly<T>(ActiveBusinessObjectCollection<T> collection, bool shouldBeReadOnly) where T : BusinessObject
		{
			if (collection.ReadOnly != shouldBeReadOnly)
			{
				collection.SetReadOnlyIncludingChildren(shouldBeReadOnly);
			}
		}

		#endregion

		#region Messages

		public MultilingualString GetCannotDeleteBecauseAlreadyHasCommissionLinesErrorMessage()
		{
			return ResString.GetMultilingualString("e3be7abf-711f-4d4a-b70f-261b7babeeaa", "You can not delete {0} because it has already been used for a commission pay out.", HumanReadableName);
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || IsReversed;
		}

		#endregion

		#region ICommissionAgreementRelated Members

		OrgCommissionAgreement ICommissionAgreementRelated<OrgCommissionAgreement>.CommissionAgreement
		{
			get { return this; }
		}

		#endregion

		#region CalculationQueue

		bool IsInQueue
		{
			get
			{
				var isInQueueQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, PK);
				return Factory.Load<OrgCommissionCalculationQueue>(isInQueueQuery).Length > 0;
			}
		}

		bool ParentIsInQueue
		{
			get
			{
				if (!CA0_CA0_ParentVersion.IsValid)
				{
					return false;
				}

				var parentInQueueQuery = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, CA0_CA0_ParentVersion);
				return Factory.Load<OrgCommissionCalculationQueue>(parentInQueueQuery).Length > 0;
			}
		}

		public void SendToCalculationQueue(ZDateTime fromDate, ZBool overwriteExistingValues)
		{
			Factory.Load<OrgCommissionCalculationQueue>(new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, PK)).DeleteAll();

			var queue = Factory.New<OrgCommissionCalculationQueue>();
			queue.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Approval;
			queue.CAQ_OverwriteExistingCommissions = overwriteExistingValues;
			queue.CAQ_MinimumInvoicePostedDate = fromDate;
			queue.CAQ_CA0 = PK;
		}

		#endregion

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (CA0_CommissionBasis.IsEmpty)
			{
				CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			}

			if (CA0_CommissionTriggerType.IsEmpty)
			{
				CA0_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);

			if (!IsDraft)
			{
				CA0_LastApprovedDateUtc = ZDateTime.UtcNow;
			}
		}

#endif
		#endregion
	}
}
