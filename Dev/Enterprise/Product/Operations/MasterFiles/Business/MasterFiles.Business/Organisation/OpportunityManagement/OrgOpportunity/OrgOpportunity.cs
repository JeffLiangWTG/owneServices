using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.MasterFiles.Business
{
	[UserDefinedValues]
	[UniversalDataContext(DataContextType.Opportunity)]
	[CodeProperty(OrgOpportunity.Schema.P8_OpportunityID), DescriptionProperty(OrgOpportunity.Schema.P8_OpportunityDescription)]
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyIgnoreElement(OrgOpportunity.Schema.P8_OpportunityID)]
	public class OrgOpportunity : AutoOrgOpportunity,
		IOrgOpportunity,
		IDocManagerSupport,
		IWorkflowProvider,
		ICustomFieldProvider,
		ISalesRelationActivity,
		ISalesValueAssociatedEntity,
		IImportParentRelatedActivityInfoOnNew,
		IImportChildRelatedActivityInfoOnAttach,
		IImportChildRelatedActivityInfoOnDetach,
		ISalesRelatedBusinessObject,
		IDocumentSupportable,
		IJobNumber
	{
		public OrgOpportunity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract new class Schema : AutoOrgOpportunity.Schema
		{
			public const string P8_RecallDateLocal = "P8_RecallDateLocal";
			public const string P8_EstimatedCloseDateLocal = "P8_EstimatedCloseDateLocal";
			public const string P8_ClosedDateLocal = "P8_ClosedDateLocal";
			public const string P8_Calc_CompanyEstimatedValue = "P8_Calc_CompanyEstimatedValue";
			public const string P8_Calc_CommittedValue = "P8_Calc_CommittedValue";
			public const string P8_Calc_PipelineValue = "P8_Calc_PipelineValue";
			public const string P8_Calc_UnsuccessfulValue = "P8_Calc_UnsuccessfulValue";
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = Res.GetString("034353f1-c553-475b-a857-a2eb527917b7", "Opportunity");
				if (!IsDeleted && !P8_OpportunityID.IsEmpty)
				{
					result += " (" + P8_OpportunityID + ")";
				}

				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = P8_OpportunityID;

				if (Header != null)
				{
					result += " - " + Header.OH_Code;
				}

				if (!P8_OpportunityDescription.IsEmpty)
				{
					result += " - " + P8_OpportunityDescription;
				}

				return result;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			OriginalP8_RecallDate = P8_RecallDate;
		}

		ZDateTime OriginalP8_RecallDate;

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void OnSaving()
		{
			RelatedChildActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			RelatedParentActivityPivotCollection.RelinkRelatedSuperAndSubActivities();
			PopulateP8_OpportunityIDOnSaving();
			UpdateProspectiveSalesIfNeeded();
			base.OnSaving();
			EditOpportunityLog();
			AddStageProgress();
		}

		void AddStageProgress()
		{
			if (string.IsNullOrEmpty(P8_Stage))
			{
				return;
			}

			var auditDateTime = ZDateTimeOffset.Now;
			var statusChangedFromOpenToClosed = P8_StatusInfo.HasChanges
					&& !IsClosedStatus((ZString)P8_StatusInfo.OriginalValue)
					&& IsClosedStatus(P8_Status);

			if (IsInDatabase)
			{
				if (!P8_StageInfo.HasChanges)
				{
					if (statusChangedFromOpenToClosed)
					{
						UpdateStageProgressDateCompletedTime(auditDateTime);
					}
					return;
				}

				UpdateStageProgressDateCompletedTime(auditDateTime);
			}

			var stageList = OrganisationsDataRegistry.Instance.OpportunityStages.Value.GetActiveCodeDescriptionPairList();
			var stageDescription = stageList.GetDescriptionFromCode(P8_Stage);
			if (!string.IsNullOrEmpty(stageDescription))
			{
				var newItem = StageProgressCollection.AddNew();

				newItem.OSP_Stage = P8_Stage;
				if (stageDescription.Length > newItem.OSP_StageDescriptionInfo.MaxLength)
				{
					stageDescription = stageDescription.Substring(0, newItem.OSP_StageDescriptionInfo.MaxLength);
				}
				newItem.OSP_StageDescription = stageDescription;
				newItem.OSP_DateStarted = auditDateTime;

				if (statusChangedFromOpenToClosed)
				{
					newItem.OSP_DateCompleted = auditDateTime;
				}
			}
		}

		void UpdateStageProgressDateCompletedTime(ZDateTimeOffset auditTime)
		{
			var lastItem = StageProgressCollection.Where(p => p.OSP_Stage == (ZString)P8_StageInfo.OriginalValue && p.OSP_DateCompleted.IsEmpty).OrderBy(p => p.OSP_DateStarted).LastOrDefault();
			if (lastItem != null)
			{
				lastItem.OSP_DateCompleted = auditTime;
			}
		}

		void EditOpportunityLog()
		{
			if (!IsInDatabase)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Header.Logs.AddNew(Events.EditedARecord, ZString.Format("Opportunity {0} attached", P8_OpportunityID));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			else
			{
				if (P8_OHInfo.HasChanges)
				{
					ZGuid originalHeaderPK = (ZGuid)P8_OHInfo.OriginalValue;
					OrgHeader originalHeader = Factory.Load<OrgHeader>(originalHeaderPK);

					string originalHeaderCode = originalHeader == null ? string.Empty : originalHeader.OH_Code.ToString();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Header.Logs.AddNew(Events.EditedARecord, ZString.Format("Opportunity {0} attached from {1}", P8_OpportunityID, originalHeaderCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

					if (originalHeader != null)
					{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						originalHeader.Logs.AddNew(Events.EditedARecord, ZString.Format("Opportunity {0} moved to {1}", P8_OpportunityID, Header.OH_Code));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					}
				}

				if (P8_StatusInfo.HasChanges)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, ZString.Format("Opportunity Status updated from {0} ({1}) to {2} ({3})",
						(ZString)P8_StatusInfo.OriginalValue, StatusAutoClosed((ZString)P8_StatusInfo.OriginalValue),
						P8_Status, StatusAutoClosed(P8_Status)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		static string StatusAutoClosed(ZString status) => IsClosedStatus(status) ? (NoResString)"Closed" : (NoResString)"Open";

		void PopulateP8_OpportunityIDOnSaving()
		{
			if (!IsInDatabase && P8_OpportunityID.IsEmpty)
			{
				P8_OpportunityID = Env.NumberFountains.SalesOpportunityID.GetNextFormatted(Factory);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			RemoveOpportunityLog();
			StageProgressCollection.DeleteAll();
			base.Delete();
			AssociatedTradeLanesPivots.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			SalesValueAssociatedEntity.DeleteAllSalesValuePivots(this);
			ValueItems.RemoveAndDeleteAll();
		}

		void RemoveOpportunityLog()
		{
			if (IsInDatabase && Header != null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Header.Logs.AddNew(Events.EditedARecord, ZString.Format("Opportunity {0} deleted", P8_OpportunityID));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		public override bool CanDelete
		{
			get { return base.CanDelete && !AnyAgreementHasCommission; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("e6ccffa8-218e-4480-87c4-d976b32cf3b0", "This opportunity's commission agreement has already been used for a commission pay out."); }
		}

		ZBool AnyAgreementHasCommission
		{
			get { return ApprovedCommissionAgreements.Any(x => x.AlreadyHasCommissionLines); }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return Res.GetString("6cf7c4b8-571f-4500-ac78-7dd216496d13", "Opportunity {0}", P8_OpportunityID); }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				if (IsDeleted)
				{
					return base.BusinessObjectsWithRelatedEventsCore;
				}

				List<BusinessObject> objects = new List<BusinessObject>();
				objects.AddRange(WorkflowItems);
				objects.AddRange(ValueItems);

				var mainVersionAgreementsQuery = new ZQuery(OrgCommissionAgreementSchema.CA0_CA0_ParentVersion, null);
				mainVersionAgreementsQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_P8, PK);
				var mainVersionAgreements = Factory.Load<OrgCommissionAgreement>(mainVersionAgreementsQuery).Where(x => x.IsMainVersion()).ToArray();
				objects.AddRange(mainVersionAgreements);
				foreach (var agreement in mainVersionAgreements)
				{
					objects.AddRange(agreement.BusinessObjectsWithRelatedEvents);
				}

				return objects.ToArray();
			}
		}

		#endregion

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.P8_GC = GlbCompany.CurrentCompany.PK;
			base.P8_DateForExchangeRate = ZDateTime.Today;
			base.P8_RX_NKEstimatedValueCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#endregion

		#region Properties

		#region P8_GC

		protected bool P8_GC_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region P8_G0
		[List("Lookups.Campaigns")]
		public override ZGuid P8_G0
		{
			get
			{
				return base.P8_G0;
			}
			set
			{
				base.P8_G0 = value;
			}
		}
		#endregion

		#region P8_GS_NKPrimarySalesPerson

		[List("Lookups.PrimarySalesPersons")]
		public override ZString P8_GS_NKPrimarySalesPerson
		{
			get
			{
				return base.P8_GS_NKPrimarySalesPerson;
			}
			set
			{
				base.P8_GS_NKPrimarySalesPerson = value;
				RefreshCommissionAgreementsReadOnly();
			}
		}

		protected bool P8_GS_NKPrimarySalesPerson_ReadOnly
		{
			get
			{
				return
					IsInDatabase &&
					(!Env.Security.OpportunityManagementEditModifyStaffAssignment.IsAllowed ||
					(!P8_GS_NKPrimarySalesPersonInfo.OriginalValue.IsEmpty &&
					(ZString)P8_GS_NKPrimarySalesPersonInfo.OriginalValue != GlbStaff.CurrentUser.GS_Code &&
					!Env.Security.CommissionAgreementOverrideAny.IsAllowed &&
					CommissionAgreementsForEdit.Any()));
			}
		}

		#endregion

		#region P8_DateForExchangeRate

		public override ZDateTime P8_DateForExchangeRate
		{
			get { return base.P8_DateForExchangeRate; }
			set
			{
				if (base.P8_DateForExchangeRate != value)
				{
					base.P8_DateForExchangeRate = value;
					UpdateEstimatedValueIfCurrenyNotEmpty();
				}
			}
		}

		protected bool P8_DateForExchangeRate_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region P8_LostReason

		[List("Lookups.ActiveCloseReasonsByStatus")]
		public override ZString P8_LostReason
		{
			get
			{
				return base.P8_LostReason;
			}
			set
			{
				base.P8_LostReason = value;
			}
		}

		protected bool P8_LostReason_ReadOnly => !IsClosed;

		#endregion

		#region P8_PackageType

		[List("Lookups.ActiveExtraCategories")]
		public override ZString P8_PackageType
		{
			get { return base.P8_PackageType; }
			set { base.P8_PackageType = value; }
		}

		#endregion

		#region P8_OpportunityID

		[ReadOnly(true)]
		public override ZString P8_OpportunityID
		{
			get { return base.P8_OpportunityID; }
			set { base.P8_OpportunityID = value; }
		}

		#endregion

		#region P8_OpportunityType

		[List("Lookups.ActiveTypes")]
		public override ZString P8_OpportunityType
		{
			get { return base.P8_OpportunityType; }
			set { base.P8_OpportunityType = value; }
		}

		#endregion

		#region P8_Outcome
		[List("Lookups.ActiveOutcomes")]
		public override ZString P8_Outcome
		{
			get
			{
				return base.P8_Outcome;
			}
			set
			{
				base.P8_Outcome = value;
			}
		}
		#endregion

		#region P8_OA
		[List("Lookups.Addresses")]
		public override ZGuid P8_OA
		{
			get
			{
				return base.P8_OA;
			}
			set
			{
				if (value != P8_OA)
				{
					base.P8_OA = value;
					UpdateProcessTasks((processTask) => { processTask.P9_OA = P8_OA; });
				}
			}
		}
		#endregion

		#region P8_OC
		[List("Lookups.ActiveContacts")]
		public override ZGuid P8_OC
		{
			get
			{
				return base.P8_OC;
			}
			set
			{
				if (value != P8_OC)
				{
					base.P8_OC = value;
					UpdateProcessTasks((processTask) => { processTask.P9_OC = P8_OC; });
				}
			}
		}
		#endregion

		#region P8_OH
		[List("Lookups.Orgs")]
		public override ZGuid P8_OH
		{
			get
			{
				return base.P8_OH;
			}
			set
			{
				if (value != P8_OH)
				{
					base.P8_OH = value;
					P8_OC = ZGuid.Empty;
					P8_OA = (Header != null) ? Header.MainAddress.PK : ZGuid.Empty;

					if (Header != null)
					{
						P8_GS_NKPrimarySalesPerson = (Header.StaffAssignments.OverallSalesRepStaff != null) ? Header.StaffAssignments.OverallSalesRepStaff.GS_Code : ZString.Empty;
					}

					if (CommissionAgreementsForEditInitialized)
					{
						foreach (var agreement in commissionAgreementsForEdit)
						{
							agreement.RefreshCommissionRuleDefaultsOfAllNonSavedStaffRecipients();
						}
					}
				}
			}
		}

		#endregion

		#region P8_ClosedDateLocal

		public ZDateTime P8_ClosedDateLocal
		{
			get
			{
				return P8_ClosedDate.IsValid ? Env.Time.GetLocalTimeFromUtc(P8_ClosedDate.ToDateTime()) : P8_ClosedDate;
			}
			set
			{
				base.P8_ClosedDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public ZWrappedPropertyInfo P8_ClosedDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P8_ClosedDateLocal, x => P8_ClosedDateInfo); }
		}

		#endregion

		#region P8_EstimatedCloseDateLocal

		public ZDateTime P8_EstimatedCloseDateLocal
		{
			get
			{
				return P8_EstimatedCloseDate.IsValid ? Env.Time.GetLocalTimeFromUtc(P8_EstimatedCloseDate.ToDateTime()) : P8_EstimatedCloseDate;
			}
			set
			{
				base.P8_EstimatedCloseDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public ZWrappedPropertyInfo P8_EstimatedCloseDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P8_EstimatedCloseDateLocal, x => P8_EstimatedCloseDateInfo); }
		}

		#endregion

		#region P8_RecallDateLocal

		public ZDateTime P8_RecallDateLocal
		{
			get
			{
				return P8_RecallDate.IsValid ? Env.Time.GetLocalTimeFromUtc(P8_RecallDate.ToDateTime()) : P8_RecallDate;
			}
			set
			{
				base.P8_RecallDate = value.IsValid ? Env.Time.GetUtcFromLocalTime(value.ToDateTime()) : value;
			}
		}

		public ZWrappedPropertyInfo P8_RecallDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.P8_RecallDateLocal, x => P8_RecallDateInfo); }
		}

		#endregion

		#region P8_SystemCreateTimeLocal

		public ZDateTime P8_SystemCreateTimeLocal
		{
			get { return P8_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		#region P8_SystemLastEditTimeLocal

		public ZDateTime P8_SystemLastEditTimeLocal
		{
			get { return P8_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		#endregion

		void UpdateProcessTasks(Action<ProcessTask> updateAction)
		{
			foreach (ProcessTask task in WorkflowItems.Tasks)
			{
				updateAction(task);
			}
		}

		#region P8_RX_NKEstimatedValueCurrency
		[List("Lookups.EstimatedValueCurrencies")]
		public override ZString P8_RX_NKEstimatedValueCurrency
		{
			get
			{
				return base.P8_RX_NKEstimatedValueCurrency;
			}
			set
			{
				if (base.P8_RX_NKEstimatedValueCurrency != value)
				{
					base.P8_RX_NKEstimatedValueCurrency = value;
					UpdateEstimatedValueIfCurrenyNotEmpty();
					if (prospectiveSalesHeaderCollection != null && !IsValidationSuspended)
					{
						prospectiveSalesHeaderCollection.ValidateAllCurrencies();
					}
				}
			}
		}
		#endregion

		#region P8_Source
		[List("Lookups.ActiveSources")]
		public override ZString P8_Source
		{
			get
			{
				return base.P8_Source;
			}
			set
			{
				if (P8_Source != value)
				{
					base.P8_Source = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateSourceDetails();
					}
				}
			}
		}

		#endregion

		#region P8_Status

		[List("Lookups.ActiveStatuses")]
		public override ZString P8_Status
		{
			get { return base.P8_Status; }
			set
			{
				base.P8_Status = value;
				if (IsClosed)
				{
					P8_ClosedDate = ZDateTime.UtcNow;
				}
				else
				{
					P8_LostReason = ZString.Empty;
				}
			}
		}

		#endregion

		#region IsClosed

		public ZBool IsClosed => IsClosedStatus(P8_Status);

		static bool IsClosedStatus(ZString status)
		{
			return OrganisationsDataRegistry.Instance.OpportunityStatus.Value.GetBoolFromCode(status);
		}

		#endregion

		#region OverallDisposition

		public ZString OverallDisposition
		{
			get { return IsClosed ? OrgOpportunityOverallDispositionList.Codes.Closed : OrgOpportunityOverallDispositionList.Codes.Open; }
		}

		public ZString OverallDispositionDescription
		{
			get { return Lookups.OverallDispositions.GetDescriptionFromCode(OverallDisposition); }
		}

		#endregion

		#region P8_Stage

		[List("Lookups.ActiveStages")]
		public override ZString P8_Stage
		{
			get { return base.P8_Stage; }
			set { base.P8_Stage = value; }
		}

		#endregion

		#region P8_OC_AssignedOffice

		[List("Lookups.AssignedOffices")]
		public override ZGuid P8_OA_AssignedOffice
		{
			get
			{
				return base.P8_OA_AssignedOffice;
			}
			set
			{
				base.P8_OA_AssignedOffice = value;
			}
		}

		#endregion

		#region P8_OpportunityNotes_HTML
		public ZBlob P8_OpportunityNotes_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(P8_OpportunityNotes);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.P8_OpportunityNotes = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}
		#endregion

		#region Source Details

		[List("Lookups.ActiveSourceDetails")]
		public ZString SourceDetails
		{
			get { return P8_SourceDetails; }
			set
			{
				if (P8_SourceDetails != value)
				{
					CheckMaximumLength(SourceDetailsInfo, value);
					P8_SourceDetails = value;
					SourceDetailsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SourceDetailsInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(SourceDetails));
			}
		}

		public int SourceDetails_MaxLength
		{
			get
			{
				var details = Lookups.ActiveSourceDetails;
				if (details.Count == 0)
				{
					return Schema.P8_SourceDetailsMaxLength;
				}

				var maxLength = details.MaxCodeLength;
				return maxLength == 0 ? 5 : maxLength;
			}
		}

		public ZString SourceDetailsFree
		{
			get
			{
				return P8_SourceDetails;
			}
			set
			{
				P8_SourceDetails = value;
			}
		}

		public ZPropertyInfo SourceDetailsFreeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SourceDetailsFree), x => P8_SourceDetailsInfo); }
		}

		public ZString SourceDetailsDataFieldType
		{
			get
			{
				return Lookups.ActiveSourceDetails.Count > 0 ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);
			}
		}

		#endregion

		#region Type Description

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(P8_OpportunityType); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion

		#region Outcome Description

		public ZString OutcomeDescription
		{
			get { return Lookups.Outcomes.GetDescriptionFromCode(P8_Outcome); }
		}

		public ZPropertyInfo OutcomeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(OutcomeDescription)); }
		}

		#endregion

		#region Source Description

		public ZString SourceDescription
		{
			get { return Lookups.Sources.GetDescriptionFromCode(P8_Source); }
		}

		public ZPropertyInfo SourceDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SourceDescription)); }
		}

		#endregion

		#region Source Details Description

		public ZString SourceDetailsDescription
		{
			get
			{
				return OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetRelatedItemList(P8_Source).GetDescriptionFromCode(P8_SourceDetails);
			}
		}

		#endregion

		#region Status Description

		public ZString StatusDescription
		{
			get { return Lookups.Statuses.GetDescriptionFromCode(P8_Status); }
		}

		public ZPropertyInfo StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDescription)); }
		}

		#endregion

		#region Referring Organisation and Contact

		[List("Lookups.Orgs")]
		public override ZGuid P8_OH_ReferringOrganisation
		{
			get { return base.P8_OH_ReferringOrganisation; }
			set
			{
				if (base.P8_OH_ReferringOrganisation != value)
				{
					base.P8_OH_ReferringOrganisation = value;
					P8_OC_ReferringContact = ZGuid.Empty;
				}
			}
		}

		[List("Lookups.ContactsOfReferringOrg")]
		public override ZGuid P8_OC_ReferringContact
		{
			get { return base.P8_OC_ReferringContact; }
			set { base.P8_OC_ReferringContact = value; }
		}

		protected bool P8_OC_ReferringContact_ReadOnly
		{
			get { return ReferringOrganisation == null; }
		}

		#endregion

		#region Stage Description

		public ZString StageDescription
		{
			get { return Lookups.Stages.GetDescriptionFromCode(P8_Stage); }
		}

		public ZPropertyInfo StageDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(StageDescription)); }
		}

		#endregion

		#region Sales Person

		public ZString SalesPersonName
		{
			get { return PrimarySalesPerson != null ? PrimarySalesPerson.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo SalesPersonNameInfo
		{
			get { return GetZPropertyInfo(nameof(SalesPersonName)); }
		}

		#endregion

		#region AssignedOrg

		[List("Lookups.Orgs")]
		public ZGuid AssignedOrgPK
		{
			get
			{
				if (assignedOrgPK.IsEmpty && AssignedOffice != null)
				{
					assignedOrgPK = AssignedOffice.OA_OH;
				}

				return assignedOrgPK;
			}
			set
			{
				if (assignedOrgPK != value)
				{
					SetNonPersistentPropertyValue(AssignedOrgPKInfo, ref assignedOrgPK, value);

					if (!assignedOrgPK.IsEmpty && AssignedOrg != null)
					{
						P8_OA_AssignedOffice = AssignedOrg.MainAddress.PK;
					}
					else
					{
						P8_OA_AssignedOffice = ZGuid.Empty;
					}

					P8_OC_AssignedOfficeContact = ZGuid.Empty;

					if (!IsValidationSuspended)
					{
						Validation.ValidateAssignedOrgPK();
					}
				}
			}
		}
		ZGuid assignedOrgPK;

		public void ResetAssignedOrgPKCache()
		{
			assignedOrgPK = Guid.Empty;
		}

		public ZPropertyInfo AssignedOrgPKInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedOrgPK)); }
		}

		public OrgHeader AssignedOrg
		{
			get { return Factory.Load<OrgHeader>(AssignedOrgPK); }
		}

		#endregion

		#region P8_OC_AssignedOfficeContact

		public bool P8_OC_AssignedOfficeContact_ReadOnly
		{
			get { return P8_OA_AssignedOffice.IsEmpty; }
		}

		#endregion

		#region Extra Category

		public ZString ExtraCategoryDescription
		{
			get { return OrganisationsDataRegistry.Instance.ProductTypeLabel.Value; }
		}

		public ZPropertyInfo ExtraCategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ExtraCategoryDescription)); }
		}

		#endregion

		#region Product Type Description

		public ZString ProductTypeDescription
		{
			get { return Lookups.ExtraCategories.GetDescriptionFromCode(P8_PackageType); }
		}

		#endregion

		#region Client Contact

		public OrgContact ClientContact
		{
			get { return Factory.Load<OrgContact>(P8_OC); }
		}

		#endregion

		#region Value Types

		public ZString ValueTypes
		{
			get { return ZString.Join(", ", ValueItems.Cast<OrgOpportunityValue>().Select(v => v.PV_RevenueType).ToArray()); }
		}

		#endregion

		#region Opportunity Details

		public ZString OpportunityDetails
		{
			get { return ORtfTextUtil.RtfToText(P8_OpportunityNotes.ToUTF8()).Replace("\r\n", " "); }
		}

		#endregion

		#region Sales Team

		public SalesTeam SalesTeam
		{
			get
			{
				var primarySalesPerson = PrimarySalesPerson;
				if (primarySalesPerson == null)
				{
					return null;
				}

				var header = Header;
				if (header == null)
				{
					return null;
				}

				return primarySalesPerson.GetSalesTeamResponsibleFor(header.ClosestPort);
			}
		}

		#endregion

		[DecimalPlaces(2)]
		public override ZDecimal P8_EstimatedValue
		{
			get { return base.P8_EstimatedValue; }
			set { base.P8_EstimatedValue = value; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public bool P8_EstimatedValue_ReadOnly
		{
			get { return P8_EstimatedValue_ReadOnlyCore; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		protected virtual bool P8_EstimatedValue_ReadOnlyCore
		{
			get { return true; }
		}

		void UpdateEstimatedValueIfCurrenyNotEmpty()
		{
			if (!P8_RX_NKEstimatedValueCurrency.IsEmpty)
			{
				UpdateEstimatedValue();
			}
		}

		[DecimalPlaces(2)]
		public ZDecimal P8_Calc_CommittedValue => ProspectiveSalesHeaderCollection.CommittedValue;
		public ZPropertyInfo P8_Calc_CommittedValueInfo => GetZPropertyInfo(Schema.P8_Calc_CommittedValue);

		[DecimalPlaces(2)]
		public ZDecimal P8_Calc_PipelineValue => ProspectiveSalesHeaderCollection.PipelineValue;
		public ZPropertyInfo P8_Calc_PipelineValueInfo => GetZPropertyInfo(Schema.P8_Calc_PipelineValue);

		[DecimalPlaces(2)]
		public ZDecimal P8_Calc_UnsuccessfulValue => ProspectiveSalesHeaderCollection.UnsuccessfulValue;
		public ZPropertyInfo P8_Calc_UnsuccessfulValueInfo => GetZPropertyInfo(Schema.P8_Calc_UnsuccessfulValue);

		public virtual void UpdateEstimatedValue()
		{
			if (P8_RX_NKEstimatedValueCurrency.IsEmpty)
			{
				P8_RX_NKEstimatedValueCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}

			var newEstimatedValue = ValueItems.TotalValue * 12 + ProspectiveSalesHeaderCollection.TotalValue;
			if (newEstimatedValue != P8_EstimatedValue)
			{
				P8_EstimatedValue = newEstimatedValue;
			}
		}

		public void RefreshCalculatedEstimatedValues()
		{
			UpdateEstimatedValue();
			P8_EstimatedValueInfo.RefreshBinding();
			P8_Calc_CommittedValueInfo.RefreshBinding();
			P8_Calc_PipelineValueInfo.RefreshBinding();
			P8_Calc_UnsuccessfulValueInfo.RefreshBinding();
		}

		public override ZDecimal P8_RentalMultiplier
		{
			get { return base.P8_RentalMultiplier; }
			set
			{
				base.P8_RentalMultiplier = value;
				UpdateEstimatedValue();
			}
		}

		public ZString RentalMultiplierDescription
		{
			get { return OrganisationsDataRegistry.Instance.PotentialLabel.Value; }
		}

		public ZPropertyInfo RentalMultiplierDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RentalMultiplierDescription)); }
		}

		public ZString TotalDiscountDescription
		{
			get { return OrganisationsDataRegistry.Instance.CurrentLabel.Value; }
		}

		public ZPropertyInfo TotalDiscountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDiscountDescription)); }
		}

		#region P8_OH_ReadOnly

		protected bool P8_OH_ReadOnly
		{
			get { return p8_OH_ReadOnly; }
			private set { p8_OH_ReadOnly = value; }
		}

		public void SetOrgHeaderReadOnly(bool readOnly)
		{
			P8_OH_ReadOnly = readOnly;
		}

		bool p8_OH_ReadOnly;

		#endregion

		public ZString CloseReasonDescription
		{
			get { return Lookups.CloseReasons.GetDescriptionFromCode(P8_LostReason); }
		}

		public ZPropertyInfo CloseReasonDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CloseReasonDescription)); }
		}

		#region Close Certainty

		public ZString CloseCertaintyAsPercentageString
		{
			get { return (ZString)string.Concat(P8_CloseCertainty, "%"); }
		}

		#endregion

		#region Created From Inquiry

		public SalesEnquiry CreatedFromInquiry
		{
			get { return Factory.Load<SalesEnquiry>(P8_O1_Enquiry); }
		}

		#endregion

		#region RelatedCommunicationCollection

		public OrgSalesCallCollection RelatedCommunicationCollection
		{
			get
			{
				if (relatedCommunicationCollection == null)
				{
					relatedCommunicationCollection = new RelatedOrgSalesCallCollection(this);
				}

				return relatedCommunicationCollection;
			}
		}
		OrgSalesCallCollection relatedCommunicationCollection;

		#endregion

		#endregion

		#region Stage Progress

		public OrgOpportunityStageProgressCollection StageProgressCollection
		{
			get
			{
				if (stageProgressCollection == null)
				{
					stageProgressCollection = new OrgOpportunityStageProgressCollection(this);
				}
				return stageProgressCollection;
			}
		}

		OrgOpportunityStageProgressCollection stageProgressCollection;

		#endregion

		#region Trade Lane Integration

		public ISalesHeaderCollection ActualAndProspectiveSalesHeaderCollection
		{
			get
			{
				if (salesHeaderCollection == null)
				{
					salesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, true);
				}

				return salesHeaderCollection;
			}
		}
		ISalesHeaderCollection salesHeaderCollection;

		public ISalesHeaderCollection ProspectiveSalesHeaderCollection
		{
			get
			{
				if (prospectiveSalesHeaderCollection == null)
				{
					prospectiveSalesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, false);
				}
				return prospectiveSalesHeaderCollection;
			}
		}
		ISalesHeaderCollection prospectiveSalesHeaderCollection;

		#region Associated Trade Lanes Pivots

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgOpportunityTradeLanePivotCollection AssociatedTradeLanesPivots
		{
			get
			{
				if (fAssociatedTradeLanesPivots == null)
				{
					fAssociatedTradeLanesPivots = new OrgOpportunityTradeLanePivotCollection(this);
					RegisterEditableChildObject(fAssociatedTradeLanesPivots);
				}
				return fAssociatedTradeLanesPivots;
			}
		}
		OrgOpportunityTradeLanePivotCollection fAssociatedTradeLanesPivots;

		/// <summary>
		/// Duplicate values associated with other records, for when P8_OH changes.
		/// </summary>
		public void DuplicateSharedProspectiveSales()
		{
			var salesPivots = AssociatedTradeLanesPivots.Where(x => x.SVP_TradeTableCode == OrgSalesSchema.Constants.Prefix).ToArray();
			foreach (var pivot in salesPivots)
			{
				var tradeLane = pivot.SalesValue as OrgSales;
				if (tradeLane != null &&
					!tradeLane.IsActual &&
					tradeLane.SalesAssociationPivotCollectionGlobal.Any(x => x.SVP_ActivityId != PK))
				{
					var clonedLane = Factory.New<OrgSales>();
					clonedLane.CopyPersistentValuesFrom(tradeLane);
					clonedLane.OW_OH_Primary = P8_OH;
					AssociatedTradeLanesPivots.AddPivotFor(clonedLane);

					bool detailsHaveAssociations = tradeLane.Product.AllowedAssociationTargets.HasFlag(OrgSalesProductAssociationTarget.OrgTradeDetail);

					foreach (OrgTradeDetail detail in tradeLane.TradeDetails)
					{
						bool shouldClone = true;
						bool hasPivotForThis = false;
						if (detailsHaveAssociations)
						{
							foreach (var detailPivot in detail.SalesAssociationPivotCollectionGlobal.ToArray())
							{
								if (detailPivot.SVP_ActivityId == PK)
								{
									hasPivotForThis = true;
									detailPivot.Delete();
								}
							}
							shouldClone = hasPivotForThis;
						}

						if (shouldClone)
						{
							var clonedDetail = Factory.New<OrgTradeDetail>();
							clonedDetail.CopyPersistentValuesFrom(detail, new BusinessObjectCloneArgs(new[] { OrgTradeDetailSchema.Constants.PA_OW }));
							clonedDetail.ProspectDetail.CopyPersistentValuesFrom(detail.ProspectDetail, new BusinessObjectCloneArgs(new[] { OrgTradeProspectSchema.Constants.PAP_PA }));
							clonedDetail.CurrentProspectPeriod.CopyPersistentValuesFrom(detail.CurrentProspectPeriod, new BusinessObjectCloneArgs(new[] { OrgTradePeriodSchema.Constants.PAS_PA }));
							clonedDetail.UpdateProspectPeriods();
							clonedLane.TradeDetails.Add(clonedDetail);
							if (hasPivotForThis)
							{
								clonedDetail.SalesAssociationPivotCollectionGlobal.AddNew(this);
							}
						}
					}
					pivot.Delete();
					clonedLane.UpdateAllTradePeriods(P8_OH);
				}
			}

			ShouldRefreshAfterSave = true;
			DeleteOrphanedRecords();
		}

		void UpdateProspectiveSalesIfNeeded()
		{
			if (!IsInDatabase || !P8_OHInfo.HasChanges)
			{
				return;
			}

			var oldOrgPk = (ZGuid)P8_OHInfo.OriginalValue;
			var newOrgPk = P8_OH;
			if (oldOrgPk.IsValid && newOrgPk.IsValid && oldOrgPk != newOrgPk)
			{
				UpdateTradeLinesForChangedOrg(oldOrgPk, P8_OH);
			}
		}

		/// <summary>
		/// This opportunity is linked to an organization (P8_OH), and it's trade lanes are also linked to an organization.
		/// This ensure trade lanes are linked to the same organization, for when P8_OH changes.
		/// </summary>
		void UpdateTradeLinesForChangedOrg(ZGuid oldValue, ZGuid newValue)
		{
			foreach (var pivot in AssociatedTradeLanesPivots.Where(x => !x.IsDeleted).ToArray())
			{
				var tradeLane = pivot.SalesValue as OrgSales;
				if (tradeLane != null && !tradeLane.IsDeleted && tradeLane.OW_OH_Primary == oldValue)
				{
					if (tradeLane.SalesAssociationPivotCollectionGlobal.Any(x => !x.IsDeleted && x.SVP_ActivityId != PK))
					{
						// Remove links to shared trades
						foreach (OrgTradeDetail detail in tradeLane.TradeDetails)
						{
							foreach (var detailPivot in detail.SalesAssociationPivotCollectionGlobal.Where(x => !x.IsDeleted).ToArray())
							{
								if (detailPivot.SVP_ActivityId == PK)
								{
									detailPivot.Delete();
								}
							}
						}
						pivot.Delete();
					}
					else
					{
						tradeLane.OW_OH_Primary = newValue;
						tradeLane.UpdateAllTradePeriods(newValue);
					}
				}
			}
		}

		#endregion

		#region Binding

		void SynchroniseAssociatedTradeLanesPivots()
		{
			if (TradeProfileDescriptionList == null || Header == null)
			{
				return;
			}

			AssociatedTradeLanesPivots.DeleteObsoletePivots(Header.SalesCollection.ToArray<OrgSales>());
			foreach (ZBoolDescriptionPair pair in TradeProfileDescriptionList)
			{
				OrgSales tradeLane = (OrgSales)Header.SalesCollection.FindByPK(pair.PK);
				if (tradeLane == null)
				{
					continue;
				}

				if (pair.Value)
				{
					if (!AssociatedTradeLanesPivots.Contains(tradeLane))
					{
						AssociatedTradeLanesPivots.AddPivotFor(tradeLane);
					}
				}
				else
				{
					if (AssociatedTradeLanesPivots.Contains(tradeLane))
					{
						AssociatedTradeLanesPivots.DeletePivotFor(tradeLane);
					}
				}
			}
		}

		public ZBoolDescriptionPairList TradeProfileDescriptionList
		{
			get
			{
				if (fTradeProfileDescriptionList == null)
				{
					UpdateTradeProfileDescriptionList();
				}
				return fTradeProfileDescriptionList;
			}
		}
		ZBoolDescriptionPairList fTradeProfileDescriptionList;

		public void UpdateTradeProfileDescriptionList()
		{
			if (fTradeProfileDescriptionList != null)
			{
				SynchroniseAssociatedTradeLanesPivots();
			}
			else
			{
				fTradeProfileDescriptionList = new ZBoolDescriptionPairList();
				fTradeProfileDescriptionList.OnPairChanged += delegate
				{ HasChanges = true; };
			}
			fTradeProfileDescriptionList.Clear();

			if (Header != null)
			{
				foreach (OrgSales tradeLane in Header.SalesCollection.Cast<OrgSales>().Where(x => !x.IsDeleted && !x.IsActual).OrderBy(x => x.TradeLaneDetailedDescription))
				{
					fTradeProfileDescriptionList.AddNew(tradeLane.PK,
						tradeLane.TradeLaneDetailedDescription,
						AssociatedTradeLanesPivots.Contains(tradeLane));
				}
			}
		}

		#endregion

		#endregion

		#region Note Types

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.OpportunityFollowUpNote);
				return types;
			}
		}

		#endregion

		#region Related Business Objects

		#region Value Items

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgOpportunityValueCollection ValueItems
		{
			get
			{
				if (fValueItems == null)
				{
					fValueItems = GetNewValueItemsCollection();
					fValueItems.Load();
					RegisterEditableChildObject(fValueItems);
				}

				return fValueItems;
			}
		}
		OrgOpportunityValueCollection fValueItems;

		protected virtual OrgOpportunityValueCollection GetNewValueItemsCollection()
		{
			return new OrgOpportunityValueCollection(this);
		}

		#endregion

		#region CommissionAgreements

		public OrgCommissionAgreementCollection CommissionAgreements
		{
			get
			{
				if (commissionAgreements == null)
				{
					commissionAgreements = new OrgCommissionAgreementCollection(this);
				}

				return commissionAgreements;
			}
		}
		OrgCommissionAgreementCollection commissionAgreements;

		public ApprovedCommissionAgreementCollection ApprovedCommissionAgreements
		{
			get
			{
				if (approvedCommissionAgreements == null)
				{
					approvedCommissionAgreements = new ApprovedCommissionAgreementCollection(this);
				}

				return approvedCommissionAgreements;
			}
		}
		ApprovedCommissionAgreementCollection approvedCommissionAgreements;

		[ChildEditable]
		public CommissionAgreementForEditCollection CommissionAgreementsForEdit
		{
			get
			{
				if (commissionAgreementsForEdit == null)
				{
					commissionAgreementsForEdit = new CommissionAgreementForEditCollection(this);
					RefreshCommissionAgreementsReadOnly();
					commissionAgreementsForEdit.UpdateAgreementsReadOnlyProperty();
				}
				return commissionAgreementsForEdit;
			}
		}
		CommissionAgreementForEditCollection commissionAgreementsForEdit;

		internal bool CommissionAgreementsForEditInitialized
		{
			get { return commissionAgreementsForEdit != null; }
		}

		void RefreshCommissionAgreementsReadOnly()
		{
			if (CommissionAgreementsForEditInitialized)
			{
				if (CreateCommissionAgreementsAllowed)
				{
					RegisterEditableChildObject(commissionAgreementsForEdit);
				}
				else
				{
					UnRegisterEditableChildObject(commissionAgreementsForEdit);
				}
				CommissionAgreementsForEdit.UpdateAgreementsReadOnlyProperty();
			}
		}

		public ZBool CreateCommissionAgreementsAllowed
		{
			get
			{
				var isCurrentUserPrimarySalesPerson = P8_GS_NKPrimarySalesPerson == GlbStaff.CurrentUser.GS_Code;
				var isAllowedToEditCommissionAgreement = Env.Security.ApprovedCommissionAgreementEdit.IsAllowed || Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed;

				return isAllowedToEditCommissionAgreement && (!IsInDatabase || isCurrentUserPrimarySalesPerson || Env.Security.CommissionAgreementOverrideAny.IsAllowed);
			}
		}
		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new OrgOpportunityFetchStrategy(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (fDocManagerInfo == null)
				{
					fDocManagerInfo = new OrgOpportunityDocManagerInfo(this, Core.Constants.DocManagerCodes.OrgOpportunity);
				}
				return fDocManagerInfo;
			}
		}

		DocManagerInfo fDocManagerInfo;

		internal class OrgOpportunityDocManagerInfo : DocManagerInfo
		{
			public OrgOpportunityDocManagerInfo(OrgOpportunity opportunity, ZString code)
				: base(opportunity, code)
			{
			}

			protected override BusinessObject[] GetRelatedObjects()
			{
				return ((OrgOpportunity)BusinessEntity).WorkflowItems.ToArray();
			}
		}

		#endregion

		#region IWorkflowProvider

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		[ActionFieldFollow]
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

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fTasks == null)
				{
					fTasks = this.GetOrCreateProcessTaskCollection(() => new OpportunityProcessTasksCollection(this));
					RegisterEditableChildObject(fTasks);
				}
				return fTasks;
			}
		}
		ProcessTaskCollection fTasks;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return new OpportunityWorkflowDescriptor().Code; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			// Must match OpportunityWorkflowDescriptor.SubTypeInformation
			// and OpportunityFormCustomisationSettingProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_SubType1, P8_OpportunityType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, P8_Source, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, P8_PackageType, ZString.Empty);
			return result;
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region Calendar Reminders

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				P8_OpportunityID = ZString.Empty;
			}

			if (saveSucceeded && ShouldCreateRecallReminder)
			{
				var log = Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					var parameters = new Dictionary<string, string>
					{
						{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, P8_RecallDate.ToString("yyyyMMdd\\THHmmss\\Z") },
						{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, OriginalP8_RecallDate.ToString("yyyyMMdd\\THHmmss\\Z") }
					};
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = StmALog.GenerateEventReference($"Opportunity {P8_OpportunityID} Recall Date Updated", parameters);
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();
			}

			if (saveSucceeded)
			{
				OriginalP8_RecallDate = P8_RecallDate;

				P8_GS_NKPrimarySalesPersonInfo.RefreshBinding();
				RefreshCommissionAgreementsReadOnly();

				SourceOpportunityOrgPK = ZGuid.Empty;
				CopiedOrgTradePeriods.Clear();
				CopiedOrgTradeDetails.Clear();
			}

			if (ShouldRefreshAfterSave)
			{
				ShouldRefreshAfterSave = false;
				ActualAndProspectiveSalesHeaderCollection.Refresh();
				ProspectiveSalesHeaderCollection.Refresh();
				RefreshBinding();
			}
		}

		internal bool ShouldCreateRecallReminder
		{
			get { return P8_RecallDate != OriginalP8_RecallDate; }
		}

		public Reminder GetNewRecallReminder(ZDateTime initialRecallDate, ZDateTime newRecallDate)
		{
			Reminder result;
			var opportunityUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Opportunity, PK.ToGuid());
			var linkText = Res.GetString("c71e47da-7b31-412a-8fb3-3366d695a94b", "{0} - {1}", P8_OpportunityID, Header?.OH_FullName);
			var htmlLink = $"<a href='{opportunityUrl}'>{linkText}</a>";

			if (!newRecallDate.IsEmpty)
			{
				var htmlBody = WrapWithHtmlDeclaration(Res.GetString("b202e28c-c3ec-4d0e-b6a0-0a60e6fa3450", "Recall due for Opportunity {0}", htmlLink));
				result = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Utc, newRecallDate, newRecallDate, Res.GetString("4056ff3e-1706-4f8f-ac33-aaa8bdd175e0", "Recall due for Opportunity {0} - {1}", P8_OpportunityID, Header?.OH_FullNameTruncated), Res.GetString("b34694d5-49d6-481d-a9ec-c47a9ba767f5", "Recall due for Opportunity {0} - {1}", P8_OpportunityID, Header?.OH_FullName), htmlBody);
			}
			else
			{
				var htmlBody = WrapWithHtmlDeclaration(Res.GetString("5a17469b-55e2-4580-bb69-bee9c3305d34", "Recall has been canceled for Opportunity {0}", htmlLink));
				result = new Reminder(PK.ToString(), PK, new ZString(TableName), DateTimeKind.Utc, initialRecallDate, initialRecallDate, Res.GetString("0588369f-7bca-41bd-b1bc-ecf19626b08d", "Recall has been canceled for Opportunity {0} - {1}", P8_OpportunityID, Header?.OH_FullNameTruncated), Res.GetString("058ff044-2aaf-4978-9cc8-0d92b8820f5e", "Recall has been canceled for Opportunity {0} - {1}", P8_OpportunityID, Header?.OH_FullName), htmlBody);
				result.ReminderType = CargoWise.Services.Calendar.ReminderType.Cancellation;
			}

			if (PrimarySalesPerson != null && !PrimarySalesPerson.GS_EmailAddress.IsEmpty)
			{
				result.Recipients.Add(PrimarySalesPerson.GS_FullName, PrimarySalesPerson.GS_EmailAddress);
			}

			result.Location = Address != null ? Address.AddressAsASingleLineWithoutCompanyName : ZString.Empty;

			return result;
		}

		string WrapWithHtmlDeclaration(string innerHtmlBody) => FormattableString.Invariant($"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{innerHtmlBody}</BODY></HTML>");

		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.OpportunityManager; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Header; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return P8_OHInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return P8_OCInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { P8_OpportunityDescription, P8_PackageType, P8_OpportunityType, StatusDescription }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region ISalesRelationActivity

		public SalesRelationModel SalesRelationModel
		{
			get
			{
				if (salesRelationModel == null)
				{
					salesRelationModel = new SalesRelationModel(this);
					RegisterEditableChildObject(salesRelationModel);
				}

				return salesRelationModel;
			}
		}
		SalesRelationModel salesRelationModel;

		ISalesRelationModel ISalesRelationActivity.SalesRelationModel
		{
			get { return SalesRelationModel; }
		}

		ZString ISalesRelationActivity.ActivityNotePropertyName
		{
			get { return OrgOpportunitySchema.Constants.P8_OpportunityNotes; }
		}

		#endregion

		#region ISalesAssociatedEntity

		ZString ISalesValueAssociatedEntity.ID => P8_OpportunityID;
		ZString ISalesValueAssociatedEntity.EntityType => ((IRelatableActivity)this).ActivityType;
		ControllerID ISalesValueAssociatedEntity.ControllerID => ControllerIDs.Opportunity;
		ZGuid? ISalesValueAssociatedEntity.CompanyPk => P8_GC;
		ZString ISalesValueAssociatedEntity.CompanyCode => Company?.GC_Code ?? ZString.Empty;
		ZPropertyInfo ISalesValueAssociatedEntity.OrgPkInfo => P8_OHInfo;
		ZString ISalesValueAssociatedEntity.Summary => ((IRelatableActivity)this).Summary;
		ZString ISalesValueAssociatedEntity.ValueCurrency => P8_RX_NKEstimatedValueCurrency;
		public ZDateTime DateForExchangeRate => P8_DateForExchangeRate;

		ZDateTime ISalesValueAssociatedEntity.GetDateAssociatedToSalesValue(ISalesValue salesValue)
		{
			return P8_SystemCreateTimeLocal;
		}

		ZString ISalesValueAssociatedEntity.GetUserThatAssociatedToSalesValue(ISalesValue salesValue)
		{
			return P8_SystemCreateUser;
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var result = true;

			if (parentActivity is IGlbCompanyCampaign parentCampaign)
			{
				P8_G0 = parentCampaign.PK;
			}
			else if (parentActivity is IGlbCompanyCampaignItem parentCampaignItem)
			{
				P8_G0 = parentCampaignItem.G8_G0;

				var grandparentInquiry = ((IRelatableActivity)parentCampaignItem).RelatedParentActivityPivotCollection.Activities.OfType<SalesEnquiry>().FirstOrDefault();
				if (grandparentInquiry != null)
				{
					return ((IImportParentRelatedActivityInfoOnNew)this).ImportParentInfo(grandparentInquiry, deciderFactory);
				}
			}

			var parentInquiry = parentActivity as SalesEnquiry;
			if (parentInquiry != null)
			{
				if (parentInquiry.Header == null)
				{
					var setInquiryOrgDecider = deciderFactory.GetIfAvailable<IImportRelatedActivityLinkInquiryToOrganisationDecider>();
					if (setInquiryOrgDecider == null || !setInquiryOrgDecider.GetDecision(parentInquiry))
					{
						result = false;
					}
				}

				P8_Source = parentInquiry.O1_LeadSource;
				P8_SourceDetails = parentInquiry.O1_OpportunitySourceDetails;
				P8_GS_NKPrimarySalesPerson = parentInquiry.O1_GS_NKRepAssigned;
				P8_OpportunityNotes = parentInquiry.EnquiryNotesContent;
				P8_OH_ReferringOrganisation = parentInquiry.O1_OH_SourceOfLead;
				P8_OC_ReferringContact = parentInquiry.O1_OC_ReferringContact;

				if (parentInquiry.O1_LeadStatus != SalesEnquiryStatusCodeList.Codes.Converted)
				{
					if (Enquiry == null && result)
					{
						var yesNoDecider = deciderFactory.GetIfAvailable<IImportRelatedActivityYesNoDecider>();
						var shouldImport = yesNoDecider != null && yesNoDecider.GetDecision(ResString.GetMultilingualString("94ed4b2e-071c-4324-93ab-8aacd2452034", "Convert {0} to new {1}?", parentInquiry.HumanReadableName, HumanReadableName));
						if (shouldImport)
						{
							var parentInquiryInLocalFactory = Factory.Load<SalesEnquiry>(parentInquiry.PK);
							if (parentInquiryInLocalFactory != null)
							{
								parentInquiryInLocalFactory.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;
								P8_O1_Enquiry = parentInquiryInLocalFactory.PK;
							}
						}
						else
						{
							result = false;
						}
					}
				}
			}

			// P8_OH and P8_OC need to be set AFTER getting the result from setInquiryOrgDecider
			if (parentActivity.Client != null)
			{
				P8_OH = parentActivity.Client.PK;
			}

			if (parentActivity.Contact != null)
			{
				P8_OC = parentActivity.Contact.PK;
			}

			var parentCommunication = parentActivity as OrgSalesCall;
			if (parentCommunication != null)
			{
				P8_GS_NKPrimarySalesPerson = parentCommunication.OQ_GS_NKSalesRep;
				P8_OpportunityNotes = parentCommunication.OQ_SalesCallNotes;
				P8_OpportunityDescription = parentCommunication.OQ_CallSummary;
			}

			return result;
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnAttach

		bool IImportChildRelatedActivityInfoOnAttach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			ImportChildInfoOnAttach(childActivity);
			return true;
		}

		protected virtual void ImportChildInfoOnAttach(IRelatableActivity childActivity)
		{
			if (childActivity is IProject childProject)
			{
				childProject.WKP_P8_Opportunity = PK;
			}
		}

		#endregion

		#region IImportChildRelatedActivityInfoOnDetach

		bool IImportChildRelatedActivityInfoOnDetach.ImportChildInfo(IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			ImportChildInfoOnDetach(childActivity);
			return true;
		}

		protected virtual void ImportChildInfoOnDetach(IRelatableActivity childActivity)
		{
			if (childActivity is IProject childProject)
			{
				childProject.WKP_P8_Opportunity = ZGuid.Empty;
			}
		}

		#endregion

		#region ISalesRelatedBusinessObject Members

		public void AddFetchHintsForSalesEstimatedValueChange()
		{
			Factory.AddFetchHint(OrgOpportunityValueSchema.PV_P8, PK);
		}

		public void OnSalesEstimatedValueChange()
		{
			UpdateEstimatedValue();
		}
		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => P8_OpportunityID;

		#endregion

		public DocumentSupporter DocumentSupporter
		{
			get { return new OrgOpportunityDocumentSupporter(this); }
		}

		#region Copy OrgOpportunity

		public void OnCopyOrgOpportunity(OrgOpportunity sourceOpportunity)
		{
			SourceOpportunityOrgPK = sourceOpportunity?.P8_OH ?? ZGuid.Empty;
		}

		public void OnCopyOrgTradePeriod(OrgTradePeriod orgTradePeriod)
		{
			CopiedOrgTradePeriods.Add(orgTradePeriod);
		}

		public void OnCopyOrgTradeDetail(OrgTradeDetail orgTradeDetail)
		{
			CopiedOrgTradeDetails.Add(orgTradeDetail);
		}

		readonly List<OrgTradePeriod> CopiedOrgTradePeriods = new List<OrgTradePeriod>();

		readonly List<OrgTradeDetail> CopiedOrgTradeDetails = new List<OrgTradeDetail>();

		public ZGuid SourceOpportunityOrgPK { get; private set; }

		/// <summary>
		/// User can not edit the trade info before saving the Opportunity.
		/// So any unsaved OrgTradePeriod must be copied from other source opportunity.
		/// </summary>
		public bool HasUnsavedCopiedOrgTradePeriods =>
			!SourceOpportunityOrgPK.IsEmpty && !IsInDatabase && CopiedOrgTradePeriods.Any(x => !x.IsDeleted && !x.IsInDatabase);

		bool ShouldRefreshAfterSave { get; set; }

		void DeleteOrphanedRecords()
		{
			var newDetails = CopiedOrgTradeDetails.Where(x => !x.IsDeleted && !x.IsInDatabase).ToArray();

			if (newDetails.Any())
			{
				var pivotsQuery = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode, OrgTradeDetailSchema.Constants.Prefix);
				pivotsQuery.AddToFilter(OrgSalesValueAssociationPivotSchema.SVP_TradeId, newDetails.Select(x => x.PK));

				var linkedDetails = new HashSet<ZGuid>(Factory.Load<OrgSalesValueAssociationPivot>(pivotsQuery).Select(x => x.SVP_TradeId).Distinct());

				Array.ForEach(newDetails, (x) =>
				{
					if (!linkedDetails.Contains(x.PK))
					{
						x.Delete();
					}
				});
			}
		}

		#endregion
	}
}
