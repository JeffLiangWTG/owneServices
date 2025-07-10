using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// Generic Project
	/// </summary>
	[CodeProperty(WorkProjectSchema.Constants.WKP_ProjectNumber), DescriptionProperty(WorkProjectSchema.Constants.WKP_Summary)]
	[UniversalDataContext(DataContextType.Project)]
	[MetadataContext(MetadataContext.Project)]
	[UniversalCopyWithExtendedEntities]
	[UserDefinedValues]
	[System.Diagnostics.DebuggerDisplay("PK = {PK}, Number = {WKP_ProjectNumber}")]
	public class Project : AutoWorkProject
		, IProject
		, IWorkflowProvider
		, IJobInvoicingPlugIn
		, IEDocsProvider
		, ICustomFieldProvider
		, ISendEmailSource
		, IWorkItemRelatedItem
		, IWorkTaskRelatedItemSource
		, ISalesRelationActivity
		, IImportParentRelatedActivityInfoOnNew
		, IUniversalXMLNoteParent
		, IConversationProvider
	{
		public Project(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoWorkProject.Schema
		{
			public const string CreatedDateAsText = "CreatedDateAsText";
			public const string ClosedOrDeferredDateAsText = "ClosedOrDeferredDateAsText";
			public const string TypeDescription = "TypeDescription";
			public const string SubtypeDescription = "SubtypeDescription";
			public const string ModuleDescription = "ModuleDescription";
			public const string PriorityDescription = "PriorityDescription";
			public const string ReleaseSequenceName = "ReleaseSequenceName";
			public const string ReleaseSequencePosition = "ReleaseSequencePosition";
			public const string ReleaseSequenceInvestment = "ReleaseSequenceInvestment";
			public const string ReleaseSequenceValue = "ReleaseSequenceValue";
			public const string ReleaseSequenceDateAsText = "ReleaseSequenceDateAsText";
		}

		#endregion

		#region Name

		public static ResourceString SingularName
		{
			get { return ResString.GetMultilingualString("A01D60C3-6963-4EDB-BEA9-C1B3F6C8A8F0", "Project"); }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = SingularName;
				if (!IsDeleted && !WKP_ProjectNumber.IsEmpty)
				{
					result += " " + WKP_ProjectNumber;
				}
				return result;
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = WKP_ProjectNumber;

				if (ClientOrganisation != null)
				{
					result += " - " + ClientOrganisation.OH_Code;
				}

				if (!string.IsNullOrEmpty(WKP_Summary))
				{
					result += " - " + WKP_Summary;
				}

				return result;
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AddToLog(Res.GetString("21EE97A0-D223-42E4-BF1A-99313F7870DC", "Project Created"));
		}

		#region Save / Delete

		public override void OnSaving()
		{
			SetJobNumberIfRequired();
			base.OnSaving();
			LogChanges();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				WKP_ProjectNumber = "";
			}
		}

		void LogChanges()
		{
			if (WKP_StatusInfo.HasChanges)
			{
				Logs.AddNew(AutoEvents.StatusChange, WKP_Status);
			}
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			RelatedChildActivityPivotCollection.DeleteAll();
			RelatedParentActivityPivotCollection.DeleteAll();
			ExternalEntityLinkHelper.DeleteLinksForBusinessObject(this);

			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public void SetJobNumberIfRequired()
		{
			if (!IsInDatabase)
			{
				PopulateFormattedNumberPropertyIfRequired(WKP_ProjectNumberInfo, JobNumberFountain);
			}
		}

		protected virtual INumberFountainProxy JobNumberFountain
		{
			get { return Env.NumberFountains.ProjectNo; }
		}

		#endregion

		#region Properties
		public bool WKP_GS_NKProjectManager_ReadOnly
		{
			get { return IsInDatabase && !Env.Security.ProjectEditModifyStaffAssignment.IsAllowed; }
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		ScheduleDeactivator ScheduleDeactivator => new ScheduleDeactivator();

		#region WKP_ProjectNumber

		[ActionField(ReadOnly = true)]
		public override ZString WKP_ProjectNumber
		{
			get { return base.WKP_ProjectNumber; }
			set { base.WKP_ProjectNumber = value; }
		}

		#endregion

		#region Assigned To

		[List("Lookups.Staff")]
		public ZString AssignedToCode
		{
			get
			{
				var result = CurrentTaskAssignedToCode;
				if (result.IsEmpty)
				{
					var items = WorkflowItems.Tasks;
					if (items.Count > 0)
					{
						result = items.Cast<ProcessTask>().OrderByDescending(x => x.P9_Sequence).First().P9_GS_NKAssignedStaffMember;
					}
				}
				return result;
			}
		}

		public GlbStaff AssignedToStaff
		{
			get { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AssignedToCode); }
		}

		#endregion

		#region Current Task

		[List("Lookups.Staff")]
		public ZString CurrentTaskAssignedToCode
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_GS_NKAssignedStaffMember : ZString.Empty;
			}
		}

		[List("Lookups.Staff")]
		public ZString CurrentOrNextTaskAssignedToCode
		{
			get
			{
				var task = CurrentOrNextTask;
				return task != null ? task.P9_GS_NKAssignedStaffMember : ZString.Empty;
			}
		}

		public ZString CurrentOrNextTaskAssignedToCodeAndName
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentOrNextTask;
				if (task != null)
				{
					var staff = task.AssignedStaffMember;
					if (staff != null)
					{
						result = task.P9_GS_NKAssignedStaffMember + "  " + staff.GS_FullName;
					}
					else
					{
						result = task.P9_GS_NKAssignedStaffMember;
					}
				}
				return result;
			}
		}

		public GlbStaff CurrentTaskAssignedTo
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.AssignedStaffMember : null;
			}
		}

		public ZGuid CurrentTaskAssignedToGroupPK
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_GG_AssignedGroup : ZGuid.Empty;
			}
		}

		public ZPropertyInfo CurrentTaskAssignedToGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTaskAssignedToGroupPK)); }
		}

		public GlbGroup CurrentTaskAssignedToGroup
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.AssignedGroup : null;
			}
		}

		public ProcessTask CurrentTask
		{
			get
			{
				if (currentTask == null)
				{
					currentTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentStartableTask();
					}
					);
				}
				return currentTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentTask;

		public ProcessTask CurrentOrNextTask
		{
			get
			{
				if (currentOrNextTask == null)
				{
					currentOrNextTask = new CachedProperty<ProcessTask>(Factory, delegate
					{
						return TaskFinder.FindCurrentOrNextStartableTask();
					}
					);
				}
				return currentOrNextTask.Value;
			}
		}
		CachedProperty<ProcessTask> currentOrNextTask;

		CurrentTaskFinder TaskFinder => taskFinder ?? (taskFinder = new CurrentTaskFinder(this));
		CurrentTaskFinder taskFinder;

		#endregion

		#region Status

		[List("Lookups.StatusList")]
		public override ZString WKP_Status
		{
			get { return base.WKP_Status; }
			set
			{
				if (base.WKP_Status != value)
				{
					string originalValue = base.WKP_Status;
					base.WKP_Status = value;
					if (value == ProcessTaskStatusCodeList.Codes.Closed || value == ProcessTaskStatusCodeList.Codes.Cancelled)
					{
						WKP_ClosedDate = ZDateTime.UtcNow;
					}
					else
					{
						WKP_ClosedDate = ZDateTime.Empty;
					}

					AddToLogStatusChange(originalValue, value);
				}
			}
		}

		public void AddToLogStatusChange(string originalValue, string value)
		{
			if (!suspendAddToLogStatusChange)
			{
				if ((originalValue == ProcessTaskStatusCodeList.Codes.Closed && value != ProcessTaskStatusCodeList.Codes.Cancelled) ||
					(originalValue == ProcessTaskStatusCodeList.Codes.Cancelled && value != ProcessTaskStatusCodeList.Codes.Closed))
				{
					AddToLog(ReOpenedDescription);
				}
				else if (value == ProcessTaskStatusCodeList.Codes.Closed || value == ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					AddToLog(Lookups.CloseList.GetDescriptionFromCode(value));
				}
			}
		}

		string ReOpenedDescription
		{
			get { return Res.GetString("C1C694FA-F024-48F5-A17C-56F8862C5317", "Re-Opened"); }
		}

		IDisposable SuspendAddToLogStatusChange()
		{
			suspendAddToLogStatusChange = true;
			return new DisposableAction(delegate
			{ suspendAddToLogStatusChange = false; });
		}

		bool suspendAddToLogStatusChange;

		[List("Lookups.StatusList")]
		public ZString CurrentTaskStatus
		{
			get
			{
				var task = CurrentTask;
				return task != null ? task.P9_Status : ZString.Empty;
			}
		}

		[List("Lookups.StatusList")]
		public ZString CurrentOrNextTaskStatus
		{
			get
			{
				var task = CurrentOrNextTask;
				return task != null ? task.P9_Status : ZString.Empty;
			}
		}

		public ZString OverallTaskStatusCode
		{
			get
			{
				ZString result = ZString.Empty;

				var task = CurrentOrNextTask;
				if (task == null)
				{
					if (WorkflowItems.AllTasksCancelled)
					{
						result = ProcessTaskStatusCodeList.Codes.Cancelled;
					}
					else if (WorkflowItems.Tasks.Count > 0)
					{
						result = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}
				else
				{
					result = task.P9_Status;
				}
				return result;
			}
		}

		public ZString OverallTaskStatusDescription
		{
			get
			{
				ZString statusCode = OverallTaskStatusCode;
				return !statusCode.IsEmpty ? new ZString(Lookups.StatusList.GetDescriptionFromCode(statusCode)) : ZString.Empty;
			}
		}

		public ZString OverallTaskStatusCodeAndDescription
		{
			get
			{
				ZString statusCode = OverallTaskStatusCode;
				return !statusCode.IsEmpty ? new ZString(statusCode + "  " + Lookups.StatusList.GetDescriptionFromCode(statusCode)) : ZString.Empty;
			}
		}

		#endregion

		#region Information

		#region WorkProject Type

		[List("Lookups.ActiveTypes")]
		public override ZString WKP_Type
		{
			get { return base.WKP_Type; }
			set
			{
				if (base.WKP_Type != value)
				{
					base.WKP_Type = value;
					UpdateOtherCategories(CategoryDepth.Type);
				}
			}
		}

		public ZString TypeDescription
		{
			get { return Lookups.AllTypes.GetDescriptionFromCode(WKP_Type); }
		}

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.TypeDescription); }
		}

		enum CategoryDepth
		{
			Type = 0,
			SubType = 1,
			Module = 2,
			Priority = 3
		}

		void UpdateOtherCategories(CategoryDepth depth)
		{
			if (inUpdateOtherCategories)
			{
				return;
			}

			inUpdateOtherCategories = true;

			try
			{
				using (GetValidationSuspender())
				{
					if (depth < CategoryDepth.SubType &&
					!WKP_SubType.IsEmpty &&
					!Lookups.ActiveSubtypes.ContainsCode(WKP_SubType))
					{
						WKP_SubType = "";
					}

					if (depth < CategoryDepth.Module &&
						!WKP_Module.IsEmpty &&
						!Lookups.ActiveModules.ContainsCode(WKP_Module))
					{
						WKP_Module = "";
					}

					if (depth < CategoryDepth.Priority &&
						!WKP_Priority.IsEmpty &&
						!Lookups.ActivePriorities.ContainsCode(WKP_Priority))
					{
						WKP_Priority = "";
					}
				}
			}
			finally
			{
				inUpdateOtherCategories = false;
			}
		}

		bool inUpdateOtherCategories;

		#endregion

		#region WorkProject Subtype

		[List("Lookups.ActiveSubtypes")]
		public override ZString WKP_SubType
		{
			get { return base.WKP_SubType; }
			set
			{
				if (base.WKP_SubType != value)
				{
					base.WKP_SubType = value;
					UpdateOtherCategories(CategoryDepth.SubType);
				}
			}
		}

		public ZString SubtypeDescription
		{
			get { return Lookups.AllSubtypes.GetDescriptionFromCode(WKP_SubType); }
		}

		public ZPropertyInfo SubtypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SubtypeDescription); }
		}

		#endregion

		#region WorkProject Module

		[List("Lookups.ActiveModules")]
		public override ZString WKP_Module
		{
			get { return base.WKP_Module; }
			set
			{
				if (base.WKP_Module != value)
				{
					base.WKP_Module = value;
					UpdateOtherCategories(CategoryDepth.Module);
				}
			}
		}

		public ZString ModuleDescription
		{
			get { return Lookups.AllModules.GetDescriptionFromCode(WKP_Module); }
		}

		public ZPropertyInfo ModuleDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ModuleDescription); }
		}

		#endregion

		#region Priorities

		[List("Lookups.ActivePriorities")]
		public override ZString WKP_Priority
		{
			get { return base.WKP_Priority; }
			set { base.WKP_Priority = value; }
		}

		public ZString PriorityDescription
		{
			get { return Lookups.AllPriorities.GetDescriptionFromCode(WKP_Priority); }
		}

		public ZPropertyInfo PriorityDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.PriorityDescription); }
		}

		#endregion

		#endregion

		#region Project Created / Closed Date / Deferred Date

		public ZString CreatedDateAsText
		{
			get { return WKP_SystemCreateTimeUtc.IsValid ? new ZDateTime(Env.Time.GetLocalTimeFromUtc(WKP_SystemCreateTimeUtc.ToDateTime())).ToShortDateString() : string.Empty; }
		}

		public ZPropertyInfo CreatedDateAsTextInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CreatedDateAsText, x => WKP_SystemCreateTimeUtcInfo); }
		}

		public override ZDateTime WKP_ClosedDate
		{
			get { return base.WKP_ClosedDate; }
			set
			{
				base.WKP_ClosedDate = value;
				ClosedOrDeferredDateAsTextInfo.RefreshBinding();
			}
		}

		public ZString ClosedDateAsText
		{
			get { return WKP_ClosedDate.IsValid ? new ZDateTime(Env.Time.GetLocalTimeFromUtc(WKP_ClosedDate.ToDateTime())).ToShortDateString() : string.Empty; }
		}

		public ZString ClosedOrDeferredDateAsText
		{
			get
			{
				return !HasDeferred ? ClosedDateAsText : DeferredDateAsText;
			}
		}

		public ZPropertyInfo ClosedOrDeferredDateAsTextInfo
		{
			get { return GetZPropertyInfo(nameof(ClosedOrDeferredDateAsText)); }
		}

		public ZString ClosedOrDeferredLabel
		{
			get
			{
				if (HasDeferred)
				{
					if (DeferredDateMet)
					{
						return Res.GetString("776B57E4-D159-496F-8348-0FA734FEF892", "Def. Date Met");
					}
					else
					{
						return Res.GetString("FB0F0F15-F387-41E9-8309-D584DE884CC6", "Deferred Until");
					}
				}
				else
				{
					return Res.GetString("410DAD98-4348-42D6-AE62-B420915E9274", "Project Closed");
				}
			}
		}

		public ZDateTime DeferredDateLocal
		{
			get
			{
				return HasJobWorkflow ? JobWorkflow.DoNotStartBeforeDateLocal : ZDateTime.Empty;
			}
		}

		// When BMS is always enabled, JobWorkflow can still be null unless a BMSystem for Projects is set up
		public bool HasJobWorkflow
		{
			get { return ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled && JobWorkflow != null; }
		}

		public ZString DeferredDateAsText
		{
			get { return DeferredDateLocal.IsValid ? DeferredDateLocal.ToShortDateString() : string.Empty; }
		}

		public ZBool HasDeferred
		{
			get { return WKP_ClosedDate.IsEmpty && DeferredDateLocal.IsValid; }
		}

		public ZBool DeferredDateMet
		{
			get { return DeferredDateLocal.IsValid && !DeferredDateLocal.IsInTheFutureDatePartOnly; }
		}

		public IProcessJobHeader JobWorkflow
		{
			get
			{
				if (jobWorkflow == null)
				{
					jobWorkflow = ProcessJobHeaderProvider.GetForParent(this, Factory, addDefaultProcessHeaderIfNone: false);
				}
				return jobWorkflow;
			}
		}

		IProcessJobHeader jobWorkflow;

		#endregion

		#region Release Sequencing

		public ZString ReleaseSequenceName => JobWorkflow?.ReleaseSequenceName ?? ZString.Empty;
		public ZInt ReleaseSequencePosition => JobWorkflow?.ReleaseSequencePosition ?? ZInt.Zero;
		public ZInt ReleaseSequenceValue => JobWorkflow?.ReleaseSequenceValue ?? ZInt.Zero;

		public ZInt ReleaseSequenceInvestment => JobWorkflow?.ReleaseSequenceInvestment ?? ZInt.Zero;
		public ZDateTime ReleaseSequenceDate => HighestReleaseSequence != null ? JobWorkflow.FH_AgreedDeliveryDate : ZDateTime.Empty;

		public ZString ReleaseSequenceDateAsText => ReleaseSequenceDate.IsValid ? Env.Time.GetLocalTimeFromUtc(ReleaseSequenceDate.ToDateTime()).ToString(DateTimeFormatStrings.LongTimeFormat, CultureInfo.CurrentCulture) : string.Empty;

		public IBMReleaseSequence HighestReleaseSequence => JobWorkflow?.HighestReleaseSequence;

		public ZPropertyInfo ReleaseSequenceNameInfo => GetZPropertyInfo(Schema.ReleaseSequenceName);
		public ZPropertyInfo ReleaseSequencePositionInfo => GetZPropertyInfo(Schema.ReleaseSequencePosition);
		public ZPropertyInfo ReleaseSequenceValueInfo => GetZPropertyInfo(Schema.ReleaseSequenceValue);
		public ZPropertyInfo ReleaseSequenceInvestmentInfo => GetZPropertyInfo(Schema.ReleaseSequenceInvestment);
		public ZPropertyInfo ReleaseSequenceDateAsTextInfo => GetZPropertyInfo(Schema.ReleaseSequenceDateAsText);

		#endregion

		#region Opportunity PK

		[List("Lookups.Opportunities")]
		public override ZGuid WKP_P8_Opportunity
		{
			get { return base.WKP_P8_Opportunity; }
			set
			{
				base.WKP_P8_Opportunity = value;
			}
		}

		public bool WKP_P8_Opportunity_ReadOnly
		{
			get { return true; }
		}

		#endregion

		[List("Lookups.Staff")]
		public override ZString WKP_SystemCreateUser
		{
			get { return base.WKP_SystemCreateUser; }
			set
			{
				base.WKP_SystemCreateUser = value;
			}
		}

		[List("Lookups.Clients")]
		public virtual ZGuid ClientOrganisationPK
		{
			get
			{
				if (clientOrganisationPK.IsEmpty)
				{
					clientOrganisationPK = ClientAddress != null ? ClientAddress.OA_OH : ZGuid.Empty;
				}
				return clientOrganisationPK;
			}
			set
			{
				if (clientOrganisationPK != value)
				{
					clientOrganisationPK = value;
					if (!value.IsEmpty && !WKP_OC_Contact.IsEmpty && Contact.OC_OH != value)
					{
						WKP_OC_Contact = ZGuid.Empty;
					}
					ClientOrganisationPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ClientOrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(ClientOrganisationPK)); }
		}

		ZGuid clientOrganisationPK;

		public OrgHeader ClientOrganisation
		{
			get { return Factory.Load<OrgHeader>(ClientOrganisationPK); }
		}

		public override ZGuid WKP_OA_ClientAddress
		{
			get { return base.WKP_OA_ClientAddress; }
			set
			{
				base.WKP_OA_ClientAddress = value;
				ClientOrganisationPK = ClientAddress != null ? ClientAddress.OA_OH : ZGuid.Empty;

				if (ClientAddress != null && ClientAddress.Header != null && !ClientAddress.Header.ARSettlementGroupPK.IsEmpty)
				{
					using (Job job = new Job.Loader(this).Load())
					{
						if (job != null)
						{
							job.LocalChargesPK = ClientAddress.Header.ARSettlementGroupPK;
						}
					}
				}
			}
		}

		#region Contact Phone

		public ZString ContactPhoneForDisplay
		{
			get { return GetPhoneWithFallbackForDisplay(Contact); }
		}

		public ZString ContactPhone
		{
			get { return GetPhoneWithFallback(Contact); }
		}

		public ZPropertyInfo ContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(ContactPhone)); }
		}

		protected ZString GetPhoneWithFallbackForDisplay(OrgContact contact)
		{
			var result = GetPhoneWithFallback(contact);
			if (!result.IsEmpty)
			{
				var directContactPhoneLabel = Res.GetString("74d86b03-a390-41e4-96f3-700bae6e10c3", "Dir:") + " ";
				var branchOfficePhoneLabel = Res.GetString("d97871e6-c957-45f7-9982-b2c49b3312bf", "Off:") + " ";
				var phoneType = !contact.OC_Phone.IsEmpty ? directContactPhoneLabel : branchOfficePhoneLabel;
				result = string.Concat(phoneType, result);
			}
			return result;
		}

		protected ZString GetPhoneWithFallback(OrgContact contact)
		{
			ZString result = ZString.Empty;
			if (contact != null)
			{
				result = !contact.OC_Phone.IsEmpty
					? contact.OC_Phone
					: GetFirstNotEmptyPhoneFromAddress(contact.BranchAddress, ClientAddress, contact.Header.MainAddress);
			}
			return result;
		}

		ZString GetFirstNotEmptyPhoneFromAddress(params OrgAddress[] addresses)
		{
			var addressWithPhone = addresses.FirstOrDefault(address => address != null && !address.OA_Phone.IsEmpty);
			return addressWithPhone != null ? addressWithPhone.OA_Phone : ZString.Empty;
		}

		#endregion

		#region Contact Email

		[List("Lookups.ContactList")]
		public override ZGuid WKP_OC_Contact
		{
			get { return base.WKP_OC_Contact; }
			set { base.WKP_OC_Contact = value; }
		}

		public ZString ContactEmail
		{
			get { return GetEmailWithFallback(Contact); }
		}

		public ZPropertyInfo ContactEmailInfo
		{
			get { return GetZPropertyInfo(nameof(ContactEmail)); }
		}

		protected ZString GetEmailWithFallback(OrgContact contact)
		{
			ZString result = "";

			if (contact != null)
			{
				if (!contact.OC_Email.IsEmpty)
				{
					result = contact.OC_Email;
				}
				else if (contact.Header != null)
				{
					result = contact.Header.MainAddress.OA_Email;
				}
			}

			return result;
		}

		#endregion

		#region Technician Organisation

		[List("Lookups.TechnicalContacts")]
		public override ZGuid WKP_OC_TechnicalContact
		{
			get { return base.WKP_OC_TechnicalContact; }
			set { base.WKP_OC_TechnicalContact = value; }
		}

		[List("Lookups.Clients")]
		public ZGuid TechnicianOrganisationPK
		{
			get
			{
				if (technicianOrganisationPK.IsEmpty)
				{
					technicianOrganisationPK = TechnicalContact != null ? TechnicalContact.OC_OH : ZGuid.Empty;
				}
				return technicianOrganisationPK;
			}
			set
			{
				if (technicianOrganisationPK != value)
				{
					technicianOrganisationPK = value;
					if (!value.IsEmpty && !WKP_OC_TechnicalContact.IsEmpty && TechnicalContact.OC_OH != value)
					{
						WKP_OC_TechnicalContact = ZGuid.Empty;
					}
					TechnicianOrganisationPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TechnicianOrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(TechnicianOrganisationPK)); }
		}

		ZGuid technicianOrganisationPK;

		public OrgHeader TechnicianOrganisation
		{
			get { return Factory.Load<OrgHeader>(TechnicianOrganisationPK); }
		}

		#endregion

		#region Technical Contact Phone

		public ZString TechnicalContactPhone
		{
			get { return GetPhoneWithFallback(TechnicalContact); }
		}

		public ZPropertyInfo TechnicalContactPhoneInfo
		{
			get { return GetZPropertyInfo(nameof(TechnicalContactPhone)); }
		}

		public ZString TechnicalContactPhoneForDisplay
		{
			get { return GetPhoneWithFallbackForDisplay(TechnicalContact); }
		}

		#endregion

		#region WKP_Details_HTML

		public ZBlob WKP_Details_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(WKP_Details);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.WKP_Details = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		#endregion

		#endregion

		#region Actions

		public virtual void Close(ZString closeType, ZString comment)
		{
			using (SuspendAddToLogStatusChange())
			{
				if (WKP_Status != ProcessTaskStatusCodeList.Codes.Cancelled && WKP_Status != ProcessTaskStatusCodeList.Codes.Closed)
				{
					if (closeType != ProcessTaskStatusCodeList.Codes.Cancelled || ScheduleDeactivator.ConfirmCancellationAndMaybeDeactivateCopySchedules(this))
					{
						WKP_Status = closeType;
						AddToLog(Lookups.CloseList.GetDescriptionFromCode(closeType) + " - " + comment);

						foreach (ProcessTask task in WorkflowItems.Tasks.ToList())
						{
							if (task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed)
							{
								if (task.P9_GS_NKAssignedStaffMember.IsEmpty && task.P9_GG_AssignedGroup.IsEmpty && closeType != ProcessTaskStatusCodeList.Codes.Cancelled)
								{
									task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
								}

								if (closeType == ProcessTaskStatusCodeList.Codes.Cancelled)
								{
									task.CancelAndSuspendValidationOnTaskCancellation();
								}
								else
								{
									task.P9_Status = closeType;
								}
							}
						}
					}
				}
			}
		}

		public virtual void ReOpen(ZString comment)
		{
			using (SuspendAddToLogStatusChange())
			{
				WKP_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				AddToLog(ReOpenedDescription + " - " + comment);
				foreach (ProcessTask task in WorkflowItems.Tasks.ToList())
				{
					if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled || task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					}
					WKP_ClosedDate = ZDateTime.Empty;
				}
			}
		}

		#endregion

		#region Log

		public void AddToLog(ZString comment)
		{
			if (!comment.Trim().IsEmpty)
			{
				ZString newLog = LogWithTimeStampAndCommentor(comment);
				LogText = newLog + LogText;
				LogTextInfo.RefreshBinding();
			}
		}

		public int CommentMaxLength
		{
			get
			{
				// The added 20 at the end represents appended comments such as the following:
				// - 'Closed - '
				// - 'Cancelled - '
				// - 'Re-Opened - '
				// Any changes to this should be reflected in:
				// - See See Enterprise.ProcessManagement.Business.ProjectTest.TestCommentMaxLength
				int remainingLength = LogText_MaxLength - (LogWithTimeStampAndCommentor(string.Empty).Length + LogText.Length + 20);
				return remainingLength >= 0 ? remainingLength : 0;
			}
		}

#if DEBUG
		public
#endif
 ZString LogWithTimeStampAndCommentor(ZString comment)
		{
			return ZDateTime.Now.ToLongTimeString() + " " + GlbStaff.CurrentUser.GS_Code + " - " + comment + "\r\n" + ZString.Replicate('-', 118) + "\r\n";
		}

		public ZString LogText
		{
			get
			{
				if (!isLogNoteTextLoaded)
				{
					logNoteText = LogNote.ST_NoteText;
					isLogNoteTextLoaded = true;
				}
				return logNoteText;
			}
			set
			{
				if (value.Length > LogNote.ST_NoteTextInfo.MaxLength)
				{
					value = value.Left(LogNote.ST_NoteTextInfo.MaxLength);
				}

				SetNonPersistentPropertyValue(LogTextInfo, ref logNoteText, value);
				using (IsSettingHasChangesSuspended ? LogNote.SuspendSettingHasChanges() : null)
				{
					LogNote.ST_NoteText = value;
				}
			}
		}

		bool isLogNoteTextLoaded;
		ZString logNoteText;

		public bool LogText_ReadOnly
		{
			get { return true; }
		}

		public int LogText_MaxLength
		{
			get { return PredefinedNoteTypes.Instance.ProjectLog.TextOnlyMaxLength; }
		}

		public ZPropertyInfo LogTextInfo
		{
			get { return GetZPropertyInfo(nameof(LogText)); }
		}

		StmNote LogNote
		{
			get
			{
				if (logNote == null || logNote.IsDeleted)
				{
					logNote = LoadOrCreateNote(PredefinedNoteTypes.Instance.ProjectLog);
				}
				return logNote;
			}
		}

		StmNote logNote;

		StmNote LoadOrCreateNote(PredefinedNoteType noteType)
		{
			using (SuspendSettingHasChanges())
			{
				StmNote note = null;
				StmNote[] foundNotes = Notes.FindByDescription(noteType.Description);
				if (foundNotes.Length > 0)
				{
					note = foundNotes[0];
				}
				else
				{
					note = CreateNote(noteType);
				}

				RegisterEditableChildObject(note);

				return note;
			}
		}

		StmNote CreateNote(PredefinedNoteType noteType)
		{
			StmNote result;

			using (SuspendSettingHasChanges())
			using (Notes.SuspendSettingHasChanges())
			{
				result = Notes.AddNew();
				result.SuspendValidation();
				result.HasChanges = false;
				using (result.SuspendSettingHasChanges())
				{
					result.ST_Description = noteType.Description;
					result.ST_IsCustomDescription = false;
					result.ST_NoteType = noteType.DefaultVisibility.ToString();
				}
			}

			return result;
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ProjectFetchStrategy(this);
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete => new JobHeader.Loader(this).Load() is null;

		public override MultilingualString ReasonForNotAbleToDelete => (NoResString)"Projects with Job Headers may not be deleted.";

		#endregion

		#region IDocumentSupportable

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new ProjectDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport

		public virtual DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Project)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsProvider

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region ICustomFieldProvider

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region IWorkflowProvider

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
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
		public ProjectProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(CreateProjectProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProjectProcessTaskCollection workflowItems;

		protected virtual ProjectProcessTaskCollection CreateProjectProcessTaskCollection()
		{
			var result = new ProjectProcessTaskCollection(this);
			return result;
		}

		public virtual IColumnValueRanker GetTemplateSelectionCriteria()
		{
			// Must match ProjectWorkflowDescriptor.SubTypeInformation
			// and ProjectFormCustomisationSettingsProvider.GetPropertiesThatAffectWorkflow
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, WKP_Type, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, WKP_SubType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType3, WKP_Module, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType4, WKP_Priority, ZString.Empty);
			return result;
		}

		public ZString WorkflowType
		{
			get { return JobInvoicingConsumerTypes.Project.Code; }
		}

		#endregion

		#region IJobInvoicingPlugIn

		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = CreateProjectInvoicingSupporter()); }
		}
		IJobInvoicingSupporter invoicingSupporter;

		protected virtual IJobInvoicingSupporter CreateProjectInvoicingSupporter()
		{
			return new ProjectInvoicingSupporter(this);
		}

		#endregion

		#region IJobHeaderParent

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			SetJobNumberIfRequired();
		}

		public string JobNumber
		{
			get { return WKP_ProjectNumber; }
		}

		#endregion

		#region ISendEmailSource

		protected virtual string GetEmailSubject()
		{
			return Res.GetString("A9C82C25-C4A2-4716-82BD-C464BA0A9C64", "Project: {0}", WKP_ProjectNumber);
		}

		public string EmailSubject
		{
			get { return GetEmailSubject(); }
		}

		public string TemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.Project; }
		}

		AddressBookSelection ISendEmailSource.GetAddressBookSelection()
		{
			return GetAddressBookSelection();
		}

		protected virtual AddressBookSelection GetAddressBookSelection()
		{
			AddressBookSelection result = new AddressBookSelection();
			result.AddRecipient(ClientOrganisation);
			result.AddRecipient(TechnicianOrganisation);
			return result;
		}

		public string DefaultFromDisplayName
		{
			get { return GetDefaultFromDisplayName(); }
		}

		public string OverridingDefaultFromEmailAddress
		{
			get { return GetDefaultFromEmailAddress(); }
		}

		protected virtual string GetDefaultFromDisplayName()
		{
			return GlbStaff.CurrentUser.GS_FullName;
		}

		protected virtual string GetDefaultFromEmailAddress()
		{
			return ProcessManagementRegistry.Instance.ProjectDefaultFromEmailAddress.Value;
		}

		public Type DocWrapperType
		{
			get { return GetDocWrapperType(); }
		}

		protected virtual Type GetDocWrapperType()
		{
			return null;
		}

		#endregion

		#region Related Items / IWorkTaskRelatedItemSource

		[ActionFieldFollow(false)]
		public WorkTaskRelatedItemCollection RelatedItems => relatedItems ?? (relatedItems = CreateAndLoadRelatedItems());
		WorkTaskRelatedItemCollection relatedItems;

		protected virtual WorkTaskRelatedItemCollection CreateAndLoadRelatedItems()
		{
			using (SuspendSettingHasChanges())
			{
				var result = new WorkTaskRelatedItemGenPivotCollection(this);
				LoadRelatedItems(result);

				return result;
			}
		}

		protected virtual void LoadRelatedItems(WorkTaskRelatedItemGenPivotCollection collection)
		{
			collection.Load();
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public ZBool ShowOnlyNonClosedItems
		{
			get => !FilteredRelatedItems.IncludeAllItems;
			set => FilteredRelatedItems.IncludeAllItems = !value;
		}

		[ActionFieldFollow(false)]
		public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems
		{
			get
			{
				if (filteredRelatedItems == null)
				{
					filteredRelatedItems = new FilteredWorkTaskRelatedItemCollection(RelatedItems);
					filteredRelatedItems.IncludeAllItems = false;
				}
				return filteredRelatedItems;
			}
		}
		FilteredWorkTaskRelatedItemCollection filteredRelatedItems;

		public virtual IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules
		{
			get
			{
				yield return WorkTaskRelatedItemModuleInfo.Project(Factory, false);
				yield return WorkTaskRelatedItemModuleInfo.WorkItem(Factory);
			}
		}

		public virtual void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
		{
		}

		public ZBool ShouldAddRelatedItemAsParent { get; set; }

		#endregion

		#region IWorkTaskRelatedItem Members

		public ZString ClientName => ClientOrganisation != null ? ClientOrganisation.OH_FullName : ZString.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.ClientNameInfo => GetZPropertyInfo(nameof(ClientName));

		public ZString ClientCode => ClientOrganisation != null ? ClientOrganisation.OH_Code : ZString.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.ClientCodeInfo => GetZPropertyInfo(nameof(ClientCode));

		public ZString Type => WorkTaskRelatedItemTypes.Project;
		ZPropertyInfo IWorkTaskRelatedItem.TypeInfo => GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.Type");

		public ZString Number => WKP_ProjectNumber;
		ZPropertyInfo IWorkTaskRelatedItem.NumberInfo => WKP_ProjectNumberInfo;

		public ZString StatusDescription
		{
			get
			{
				var status = WKP_Status;
				if (status.IsEmpty)
				{
					status = OverallTaskStatusCode;
				}
				return Lookups.StatusList.GetDescriptionFromCode(status);
			}
		}

		ZPropertyInfo IWorkTaskRelatedItem.StatusDescriptionInfo => GetZPropertyInfo(nameof(StatusDescription));

		public ZString AssignedStaffCode => AssignedToCode;
		ZPropertyInfo IWorkTaskRelatedItem.AssignedStaffCodeInfo => GetZPropertyInfo(nameof(AssignedStaffCode));

		public ZString ItemDescription => WKP_Summary;
		ZPropertyInfo IWorkTaskRelatedItem.ItemDescriptionInfo => WKP_SummaryInfo;

		public ZString Criticality => WKP_Priority;
		ZPropertyInfo IWorkTaskRelatedItem.CriticalityInfo => WKP_PriorityInfo;

		ControllerID IWorkTaskRelatedItem.ControllerID => ControllerIDs.Project;

		public ZString SelectionCriterion1 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllTypes, WKP_Type);
		public ZPropertyInfo SelectionCriterion1Info => GetZPropertyInfo(nameof(SelectionCriterion1));

		public ZString SelectionCriterion2 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllSubtypes, WKP_SubType);
		public ZPropertyInfo SelectionCriterion2Info => GetZPropertyInfo(nameof(SelectionCriterion2));

		public ZString SelectionCriterion3 => WorkTaskRelatedItemHelper.GetSelectionCriterionString(Lookups.AllModules, WKP_Module);
		public ZPropertyInfo SelectionCriterion3Info => GetZPropertyInfo(nameof(SelectionCriterion3));

		public ZString SelectionCriterion4 => string.Empty;
		public ZPropertyInfo SelectionCriterion4Info => GetZPropertyInfo(nameof(SelectionCriterion4));

		public ZString SelectionCriterion5 => string.Empty;
		public ZPropertyInfo SelectionCriterion5Info => GetZPropertyInfo(nameof(SelectionCriterion5));

		public ZString Source => ZString.Empty;
		ZPropertyInfo IWorkTaskRelatedItem.SourceInfo => GetZPropertyInfo("Enterprise.ProcessManagement.Business.IWorkTaskRelatedItem.Source");

		public ZBool IsClosedOrCancelled => WKP_Status == ProcessTaskStatusCodeList.Codes.Closed;

		Type IWorkTaskRelatedItem.PivotCollectionType => typeof(GenPivotCollection);

		#endregion

		#region IWorkItemRelatedItem Members

		public bool OnRelatedWorkItemClosed(WorkItem workItem)
		{
			return true;
		}

		public void OnRelatedWorkItemReOpened(WorkItem workItem)
		{
		}

		public void OnWorkItemAdded(WorkItem workItem)
		{
		}

		public void OnWorkItemRemoved(WorkItem workItem)
		{
		}

		#endregion

		#region IRelatableActivity

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.Projects; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return ClientOrganisation; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return ClientOrganisationPKInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return Contact; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return WKP_OC_ContactInfo.HasChanges; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { WKP_Type, WKP_Summary, WKP_Status, ClientCode }.Where(x => !x.IsEmpty)); }
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
			get { return WorkProjectSchema.Constants.WKP_Details; }
		}

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				WKP_OA_ClientAddress = parentActivity.Client?.MainAddress?.PK ?? ZGuid.Empty;
			}

			if (parentActivity.Contact != null)
			{
				WKP_OC_Contact = parentActivity.Contact?.PK ?? ZGuid.Empty;
			}

			var quoteParent = parentActivity as QuotedBooking;
			if (quoteParent != null)
			{
				WKP_OC_Contact = quoteParent.ClientDocAddress?.Contact?.PK ?? ZGuid.Empty;
			}

			var orgParent = parentActivity as OrgOpportunity;
			if (orgParent != null)
			{
				WKP_P8_Opportunity = orgParent.PK;
				WKP_Summary = orgParent.P8_OpportunityDescription;
				WKP_OA_ClientAddress = orgParent.Header?.MainAddress?.PK ?? ZGuid.Empty;
				WKP_OC_Contact = orgParent.P8_OC;
			}

			var inqParent = parentActivity as OrgColdCallRegister;
			if (inqParent != null)
			{
				WKP_OA_ClientAddress = inqParent.O1_OA_LinkedAddress;
				WKP_OC_Contact = inqParent.O1_OC_LinkedContact;
			}

			var campaignItemParent = parentActivity as GlbCompanyCampaignItem;
			if (campaignItemParent != null)
			{
				WKP_OA_ClientAddress = campaignItemParent.ClientOrg?.MainAddress?.PK ?? ZGuid.Empty;
				WKP_OC_Contact = campaignItemParent.RecipientAsOrgContact?.PK ?? ZGuid.Empty;
			}

			return true;
		}

		#endregion

		#region eConversation

		public JobConversation Conversation => !IsInDatabase ? null : conversation ?? (conversation = GetOrCreateConversation());
		JobConversation conversation;

		protected virtual JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		#endregion

		#region IConversationProvider Members

		JobConversation IConversationProvider.eConversation => Conversation;

		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.Project;

		ControllerID IConversationProvider.ParentController => ControllerIDs.Project;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();

		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		string IConversationProvider.EmailSubjectContentOverride => default;

		string IConversationProvider.FromAddressOverride => default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		void IConversationProvider.RunConversationUpdateActionBeforeSaving() { }

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			WKP_ProjectNumber = ZString.Empty;
			var client = Factory.NewWithValidTestData<OrgHeader>();
			WKP_OA_ClientAddress = client.MainAddress.PK;
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			WKP_OC_Contact = contact.PK;
			WKP_Summary = "Summary for test";
		}
#endif
		#endregion
	}
}
