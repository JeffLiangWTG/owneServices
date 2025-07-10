using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[DebuggerDisplay("P0_ProcessType: {P0_ProcessType} P0_Name: {P0_Name} P0_SubType1: {P0_SubType1} P0_SubType2: {P0_SubType2} P0_SubType3: {P0_SubType3} P0_SubType4: {P0_SubType4} P0_SubType5: {P0_SubType5}")]
	[TestExcludeWorkflowProviderHasTestCase]
	[ProvideMetaDataProperty("ReadOnly", MetaDataTypes.ReadOnly)]
	[CodeProperty(Schema.P0_Name), DescriptionProperty(Schema.P0_Description)]
	public class ProcessTaskTemplate : AutoProcessTaskTemplate,
		IWorkflowProvider,
		ITemplateCopyable,
		IProcessTaskTemplate,
		ICustomProcessTaskHandlerProvider,
		IAuditParent,
		IValidationToolParent
	{
		public ProcessTaskTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			HasChangesChanged += CheckPermissionAndRecordInvalidChanges;
			ShowWarningIfCancelled = true;
		}

		void CheckPermissionAndRecordInvalidChanges(object sender, HasChangesChangedEventArgs e)
		{
			if (reportChangesStackTrace == null)
			{
				if (Env.Security.WorkflowTaskTemplatesEdit.IsAllowed || (Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed && !P0_IsActive))
				{
					return;
				}

				if (!IsInDatabase && Env.Security.WorkflowTaskTemplatesNew.IsAllowed || isCreatingNewTemplate)
				{
					isCreatingNewTemplate = true;
					return;
				}

				reportChangesStackTrace = new StackTrace();
			}
		}
		bool isCreatingNewTemplate;
		StackTrace reportChangesStackTrace;

		#region Loader

		public new class Loader : BusinessObject.Loader, IProcessTaskTemplateLoader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ProcessTaskTemplate[] FindMatches(IWorkflowProviderCore workflowProvider, bool ignoreCache = false, bool includeUniversalTemplates = false, bool includeOnlyUniversalTemplates = false)
			{
				if (workflowProvider.WorkflowType.IsEmpty)
				{
					return Array.Empty<ProcessTaskTemplate>();
				}

				var workflowType = workflowProvider.WorkflowType;
				var workflowTypeQuery = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, workflowType);
				workflowTypeQuery.AddToFilter(ProcessTaskTemplateSchema.P0_IsActive, true);
				workflowTypeQuery.AddToFilter(ProcessTaskTemplateSchema.P0_IsPartialTemplate, false);
				var ranker = (ColumnValueRanker)workflowProvider.GetTemplateSelectionCriteria();
				ranker.Add(ProcessTaskTemplateSchema.P0_GC, GetCompanyPK(workflowProvider), ZGuid.Empty);
				ranker.Add(ProcessTaskTemplateSchema.P0_IsSystem, ZBool.False, ZBool.True);
				ranker.Add(ProcessTaskTemplateSchema.P0_IsUniversal, ZBool.False, ZBool.True);

				var result = ranker.GetBestMatches<ProcessTaskTemplate>(Factory, workflowTypeQuery, WorkflowDataRegistry.Instance.EnableTemplateApplicationInMemoryFiltering.Value, nameof(ProcessTaskTemplate.Loader) + workflowType, ignoreCache ? 0 : CacheDurationInMins);

				if (WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value)
				{
					var workflowProviderCreateTime = GetJobCreateTime(workflowProvider);

					result = result.Where(template =>
					{
						var isWithinStartDate = !template.P0_EffectiveStartDateUtc.IsValid || template.P0_EffectiveStartDateUtc <= workflowProviderCreateTime;
						var isWithinEndDate = !template.P0_EffectiveEndDateUtc.IsValid || template.P0_EffectiveEndDateUtc >= workflowProviderCreateTime;
						return isWithinStartDate && isWithinEndDate;
					}).ToArray();
				}

				if (includeOnlyUniversalTemplates)
				{
					return result.Where(t => t.P0_IsUniversal).ToArray();
				}
				else if (!includeUniversalTemplates)
				{
					return result.Where(t => !t.P0_IsUniversal).ToArray();
				}
				else
				{
					return result;
				}
			}

			static IBusiness GetRoot(IWorkflowProviderCore provider)
			{
				if (provider is IWorkflowProviderCollection workflowProvider)
				{
					var workflowItems = workflowProvider.WorkflowItems;
					return workflowItems.Parent;
				}
				else if (provider is IBusiness bizo)
				{
					return bizo;
				}
				else
				{
					ErrorReporter.ReportOnce("Unrecognized workflow provider heirarchy");
					return null;
				}
			}

			static ZDateTime GetJobCreateTime(IWorkflowProviderCore workflowProviderCore)
			{
				var root = GetRoot(workflowProviderCore);
				if (root is IAuditDetails audit && audit.SystemCreateTimeUtc.IsValid)
				{
					return audit.SystemCreateTimeUtc;
				}
				else if (root is BusinessObject bizo && bizo.IsInDatabase && root is IStmALogParent logParent)
				{
					var addedLog = logParent.Logs?.AddedLog;
					if (addedLog != null && !addedLog.IsDeleted && addedLog.SL_PostedTimeUtc.IsValid)
					{
						return addedLog.SL_PostedTimeUtc;
					}
				}

				return ZDateTime.UtcNow;
			}

			ZGuid GetCompanyPK(IWorkflowProviderCore workflowProvider)
			{
				var workflowProviderSpecifiedCompanyPK = workflowProvider as IWorkflowProviderTemplateCriteria;
				var specifiedCompanyPK = workflowProviderSpecifiedCompanyPK?.CompanyPK != null ? workflowProviderSpecifiedCompanyPK.CompanyPK : ZGuid.Empty;

				return specifiedCompanyPK.IsValid ? specifiedCompanyPK : Env.CurrentCompanyPK;
			}

			public static int CacheDurationInMins
			{
				get
				{
#if DEBUG
					if (Globals.IsTest)
					{
						return CacheDurationMinutes_ForTest.Value;
					}
#endif
					return 20;
				}
			}

#if DEBUG
			public static readonly Overridable<int> CacheDurationMinutes_ForTest = new Overridable<int>(0);
#endif

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(ProcessTaskTemplate);
			}

			#region IProcessTaskTemplateLoader Members

			[DebuggerStepThrough]
			IProcessTaskTemplateMatches IProcessTaskTemplateLoader.FindMatches(IWorkflowProviderCore workflowProvider, bool ignoreCache, bool includeUniversalTemplates, bool includeOnlyUniversalTemplates)
			{
				return new ProcessTaskTemplateMatches(FindMatches(workflowProvider, ignoreCache, includeUniversalTemplates, includeOnlyUniversalTemplates));
			}

			IProcessTaskTemplate IProcessTaskTemplateLoader.FindTemplateForScreenLayout(IWorkflowProviderCore host, bool ignoreCache)
			{
				ProcessTaskTemplate[] templates = FindMatches(host, ignoreCache, false, false);
				var result = templates.FirstOrDefault(t => !t.P0_IsScreenLayoutFallback);
				if (result == null) //if no templates are available. this can happen if you deactivate the most general template, like Shipment - All - All
				{
					//if we already made default template, retrieve it
					ZQuery defaultTemplateQuery = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, host.WorkflowType);
					defaultTemplateQuery.FetchOnlyFromLocalCache = true;
					var candidates = Factory.Load<ProcessTaskTemplate>(defaultTemplateQuery);
					foreach (ProcessTaskTemplate candidate in candidates)
					{
						if (candidate.IsNonPersistant)
						{
							result = candidate;
							break;
						}
					}

					if (result == null)
					{
						result = Factory.New<ProcessTaskTemplate>();
						result.IsNonPersistant = true;
						result.SuspendValidation();
						result.P0_ProcessType = host.WorkflowType; // this loads the basic default setup specified in FormCustomisationSettingsProvider overrides (i.e. ForwardingShipmentFormCustomisationSettingsProvider)
					}

					IProcessTaskTemplateUpdatable processTaskTemplateUpdatable = host as IProcessTaskTemplateUpdatable;
					if (processTaskTemplateUpdatable != null)
					{
						processTaskTemplateUpdatable.Update(result);
					}
				}
				return result;
			}

			#endregion
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			P0_GC = GlbCompany.CurrentCompany.PK;
		}

		public override void Delete()
		{
			if (!IsDeleted && P0_IsSystem)
			{
				throw new CannotDeleteException("Cannot delete a system-defined workflow template");
			}

			WorkflowItems.RemoveAndDeleteAll();
			ProcessHeaders.DeleteAll();
			TemplateTriggers.DeleteAll();
			ReleaseGroupRules.DeleteAll();
			ProcessTemplateValidationActions.DeleteAll();
			ProcessTemplateValidations.DeleteAll();

			base.Delete();
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && !IsNonPersistant; }
		}

		public bool IsCloningTasks { get; set; }

		public IDisposable CloningTemplate(ProcessTaskTemplate cloningTemplate)
		{
			cloningTemplate.IsCloningTasks = true;

			return new DisposableAction(() =>
			{
				cloningTemplate.IsCloningTasks = false;
			});
		}

		/// <summary>
		/// This property is a terrible, terrible hack and should not be used. Will be removed with WI00092810. And yet...
		/// </summary>
		public bool IsNonPersistant { get; set; }

		protected override ZString HumanReadableNameCore
		{
			get { return base.HumanReadableNameCore + " - " + P0_Name; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			P0_Name = "Template" + GetHashCode();
		}
#endif

		#endregion

		#region Related Business Objects

		#region Task Provider

#if DEBUG
		virtual
#endif
 public WorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				WorkflowDescriptor workflowDescriptor;
				using (((IBusinessObjectInternals)this).SuppressReportRowDeletedError()) // Even if this template is deleted, we may want to know its workflow descriptor
				{
					workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(P0_ProcessType);
				}

				if (workflowDescriptor != null)
				{
					workflowDescriptor.LastProcessTaskTemplate = this;
				}

				return workflowDescriptor;
			}
		}

		public ProcessTemplateSubType[] WorkflowSubTypeInformation
		{
			get { return WorkflowDescriptor != null ? WorkflowDescriptor.SubTypeInformation : Array.Empty<ProcessTemplateSubType>(); }
		}

		#endregion

		#region Field Customisation Settings

		[ChildEditable(true)]
		public GenCustomColumnDefinitionCollection GenCustomColumnDefinitions
		{
			get
			{
				if (genCustomColumnDefinitions == null)
				{
					genCustomColumnDefinitions = new GenCustomColumnDefinitionCollection(this);
					if (P0_IsSystem)
					{
						genCustomColumnDefinitions.SetReadOnlyIncludingChildren(true);
					}
					RegisterEditableChildObject(genCustomColumnDefinitions);
				}
				return genCustomColumnDefinitions;
			}
		}
		GenCustomColumnDefinitionCollection genCustomColumnDefinitions;

		ICustomColumnDefinition[] IProcessTaskTemplate.CustomColumnDefinitions => GenCustomColumnDefinitions.ToArray();

		#endregion

		#region Form Customisation Settings

		public FormCustomisationSettings FormCustomisationSettings
		{
			get { return formCustomisationSettings ?? GetNewFormCustomisationSettings(); }
		}
		FormCustomisationSettings formCustomisationSettings;

		FormCustomisationSettings GetNewFormCustomisationSettings()
		{
			using (SuspendSettingHasChangesIncludingChildren())
			{
				formCustomisationSettings = GetNewFormCustomisationSettingsCore();
				if (!IsDeleted)
				{
					SetFormCustomisationSettingsReadOnly(P0_IsSystem);
				}

				return formCustomisationSettings;
			}
		}

		void SetFormCustomisationSettingsReadOnly(bool readOnly)
		{
			if (formCustomisationSettings != null)
			{
				FormCustomisationSettings.DisplayTabsView.SetReadOnlyIncludingChildren(readOnly);
				FormCustomisationSettings.DisplayFieldsView.SetReadOnlyIncludingChildren(readOnly);
			}
		}

		protected virtual FormCustomisationSettings GetNewFormCustomisationSettingsCore() => new FormCustomisationSettings(this);

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (formCustomisationSettings != null &&
				(FormCustomisationSettings.DisplayFields.HasChanges || FormCustomisationSettings.DisplayTabs.HasChanges) &&
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed)
			{
				formCustomisationSettings.SaveState();
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (reportChangesStackTrace != null)
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"Template [{0}] was modified but user [{1}] has no permission.", P0_Description, GlbStaff.CurrentUser?.GS_FullName);
				Globals.Message.ShowDeveloperErrorOnce("ec280abf-3497-4ad0-bc58-8b9fac16c608", reportChangesStackTrace.ToString(), message);
				reportChangesStackTrace = null;
			}

			if (P0_IsUniversal && P0_TriggerFallbackMethod == FallbackTypeList.Codes.EmptyFallback)
			{
				throw new NotSupportedException("Cannot save a universal template with EFB trigger fallback method as this is not supported");
			}

			if (WorkflowDescriptor != null && !WorkflowDescriptor.SupportsTasks)
			{
				WorkflowItems.Tasks.RemoveAndDeleteAll();
			}
		}

		#endregion

		#region Process Headers

		[ChildEditable]
		public IProcessHeaderCollection ProcessHeaders
		{
			get
			{
				if (processHeaders == null)
				{
					processHeaders = ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetForTemplate(this);

					UpdateWorkflowsCollection();

					if (P0_IsSystem)
					{
						processHeaders.SetCountedReadOnlyIncludingChildren(true);
					}

					RegisterEditableChildObject(processHeaders);
				}

				return processHeaders;
			}
		}

		IProcessHeaderCollection processHeaders;

		#endregion

		#region Tasks

		[ChildEditable]
		public ProcessTaskCollectionView TasksExcludingCompletionStatements
		{
			get
			{
				if (tasksExcludingCompletionStatements == null)
				{
					tasksExcludingCompletionStatements = new ProcessTaskExcludingCompletionStatementCollectionView(WorkflowItems);
					RegisterEditableChildObject(tasksExcludingCompletionStatements);
				}

				return tasksExcludingCompletionStatements;
			}
		}

		ProcessTaskCollectionView tasksExcludingCompletionStatements;

		#endregion

		#region Completion Statements

		[ChildEditable]
		public ProcessTaskCollectionView CompletionStatementTasks
		{
			get
			{
				if (completionStatementTasks == null)
				{
					completionStatementTasks = new ProcessTaskCompletionStatementCollectionView(this);
					RegisterEditableChildObject(completionStatementTasks);
				}

				return completionStatementTasks;
			}
		}

		ProcessTaskCollectionView completionStatementTasks;

		#endregion

		#region ProcessHeaderLinks

		[ChildEditable]
		public IProcessHeaderLinkCollection ProcessHeaderLinks
		{
			get
			{
				if (processHeaderLinks == null)
				{
					processHeaderLinks = ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetLinkCollection(this);

					if (P0_IsSystem)
					{
						processHeaderLinks.SetCountedReadOnlyIncludingChildren(true);
					}

					RegisterEditableChildObject(processHeaderLinks);
				}

				return processHeaderLinks;
			}
		}

		IProcessHeaderLinkCollection processHeaderLinks;

		#endregion

		#region Template Triggers

		[ChildEditable]
		public ITemplateTriggerCollection TemplateTriggers
		{
			get
			{
				if (templateTriggers == null)
				{
					templateTriggers = ObjectFactory.Get<ITemplateTriggerCollection>(nameof(ITemplateTriggerCollection), this);

					if (P0_IsSystem)
					{
						templateTriggers.SetCountedReadOnlyIncludingChildren(true);
					}

					RegisterEditableChildObject(templateTriggers);
				}

				return templateTriggers;
			}
		}

		ITemplateTriggerCollection templateTriggers;

		#endregion

		#region Release Group Rules

		[ChildEditable]
		public IProcessTemplateReleaseGroupRuleCollection ReleaseGroupRules
		{
			get
			{
				if (releaseGroupRules == null)
				{
					releaseGroupRules = ObjectFactory.Get<IProcessTemplateReleaseGroupRuleCollection>(nameof(IProcessTemplateReleaseGroupRuleCollection), this);
					RegisterEditableChildObject(releaseGroupRules);

					if (P0_IsSystem)
					{
						releaseGroupRules.SetReadOnlyIncludingChildren(true);
					}
				}

				return releaseGroupRules;
			}
		}

		IProcessTemplateReleaseGroupRuleCollection releaseGroupRules;

		#endregion

		#endregion

		#region Properties

		#region P0_LoadPortCountry
		[List("Lookups.Locations")]
		public override ZString P0_LoadPortCountry
		{
			get
			{
				return base.P0_LoadPortCountry;
			}
			set
			{
				base.P0_LoadPortCountry = value;
			}
		}
		#endregion

		#region P0_GE
		[List("Lookups.Departments")]
		public override ZGuid P0_GE
		{
			get
			{
				return base.P0_GE;
			}
			set
			{
				base.P0_GE = value;
			}
		}
		#endregion

		#region P0_OH_Client

		[List("ClientList")]
		public override ZGuid P0_OH_Client
		{
			get
			{
				return base.P0_OH_Client;
			}
			set
			{
				base.P0_OH_Client = value;
			}
		}

		public OrgHeaderCollection ClientList
		{
			get
			{
				OrgHeaderCollection result = null;

				var clientListProvider = WorkflowDescriptor != null ? WorkflowDescriptor.ClientListProvider : null;

				if (clientListProvider != null)
				{
					result = clientListProvider(this);
				}

				return result ?? Lookups.Clients;
			}
		}

		#endregion

		#region PO_WW

		[List("Lookups.Warehouses")]
		public override ZGuid P0_WW
		{
			get
			{
				return base.P0_WW;
			}
			set
			{
				base.P0_WW = value;
			}
		}

		#endregion

		#region P0_FS_BufferManagementSystem

		[List("Lookups.BMSystems")]
		public override ZGuid P0_FS_BufferManagementSystem
		{
			get { return base.P0_FS_BufferManagementSystem; }
			set { base.P0_FS_BufferManagementSystem = value; }
		}

		#endregion

		#region Global Template

		/// <summary>
		/// Ie, accessible from all companies
		/// </summary>
		public ZBool GlobalTemplate
		{
			get { return P0_GC.IsEmpty; }
			set
			{
				P0_GC = value ? ZGuid.Empty : GlbCompany.CurrentCompany.PK;
				GlobalTemplateInfo.RefreshBinding();

				foreach (ProcessTask task in WorkflowItems.Tasks)
				{
					task.Validation.ValidateP9_ShareTasksForAllCompanies();
				}
			}
		}

		public ZPropertyInfo GlobalTemplateInfo
		{
			get { return GetZPropertyInfo(nameof(GlobalTemplate)); }
		}

		public bool HasNonSharedTasks
		{
			get
			{
				return TasksExcludingCompletionStatements
					.Cast<ProcessTask>()
					.Any(x => !x.P9_ShareTasksForAllCompanies);
			}
		}

		public bool ShouldWarnAboutNonSharedTasks
		{
			get { return GlobalTemplate && (WorkflowDescriptor?.AreTasksCompanySpecific ?? false); }
		}

		#endregion

		#region P0_DischargePortCountry
		[List("Lookups.Locations")]
		public override ZString P0_DischargePortCountry
		{
			get
			{
				return base.P0_DischargePortCountry;
			}
			set
			{
				base.P0_DischargePortCountry = value;
			}
		}
		#endregion

		#region P0_SubType1

		[List("Lookups.List1")]
		public override ZString P0_SubType1
		{
			get { return base.P0_SubType1; }
			set
			{
				if (P0_SubType1 != value)
				{
					base.P0_SubType1 = value;
				}
			}
		}

		#endregion

		#region P0_SubType2
		[List("Lookups.List2")]
		public override ZString P0_SubType2
		{
			get { return base.P0_SubType2; }
			set
			{
				if (P0_SubType2 != value)
				{
					base.P0_SubType2 = value;
				}
			}
		}

		#endregion

		#region P0_SubType3
		[List("Lookups.List3")]
		public override ZString P0_SubType3
		{
			get { return base.P0_SubType3; }
			set
			{
				if (P0_SubType3 != value)
				{
					base.P0_SubType3 = value;
				}
			}
		}

		#endregion

		#region P0_SubType4

		[List("Lookups.List4")]
		public override ZString P0_SubType4
		{
			get { return base.P0_SubType4; }
			set
			{
				if (P0_SubType4 != value)
				{
					base.P0_SubType4 = value;
				}
			}
		}

		#endregion

		#region P0_SubType5

		[List("Lookups.List5")]
		public override ZString P0_SubType5
		{
			get { return base.P0_SubType5; }
			set
			{
				if (P0_SubType5 != value)
				{
					base.P0_SubType5 = value;
				}
			}
		}

		#endregion

		#region P0_ProcessType

		[List("Lookups.WorkflowTypeList")]
		public override ZString P0_ProcessType
		{
			get { return base.P0_ProcessType; }
			set
			{
				if (base.P0_ProcessType != value)
				{
					UpdateCompletionStatementTaskTypes(value);

					base.P0_ProcessType = value;

					foreach (ProcessTask workflowItem in WorkflowItems)
					{
						workflowItem.MarkAsNeedingValidation();
						workflowItem.RefreshWorkflowType();
					}

					RefreshBinding();
					DischargePortExistsInfo.RefreshBinding();
					LoadPortExistsInfo.RefreshBinding();
					ClientExistsInfo.RefreshBinding();
					WarehouseExistsInfo.RefreshBinding();
					BranchExistsInfo.RefreshBinding();
					DepartmentExistsInfo.RefreshBinding();
					SubType1ExistsInfo.RefreshBinding();
					SubType2ExistsInfo.RefreshBinding();
					SubType3ExistsInfo.RefreshBinding();
					SubType4ExistsInfo.RefreshBinding();
					SubType5ExistsInfo.RefreshBinding();
					SubType1LabelInfo.RefreshBinding();
					SubType2LabelInfo.RefreshBinding();
					SubType3LabelInfo.RefreshBinding();
					SubType4LabelInfo.RefreshBinding();
					SubType5LabelInfo.RefreshBinding();
					Port1NameInfo.RefreshBinding();
					Port2NameInfo.RefreshBinding();
					ClientNameInfo.RefreshBinding();
					WarehouseNameInfo.RefreshBinding();
					MilestoneTemplateHintCaptionInfo.RefreshBinding();

					ClearCriteriasIfRequired();
					UpdateWorkflowsCollection();

					FormCustomisationSettings.Reset();

					if (!IsValidationSuspended)
					{
						Validation.ValidateP0_IsUniversal();
						ValidateSelectionCriteria();
					}
				}
			}
		}

		void UpdateCompletionStatementTaskTypes(ZString newProcessType)
		{
			var taskType = ProcessTask.GetCompletionStatementTaskType(newProcessType);
			if (!string.IsNullOrEmpty(taskType))
			{
				foreach (ProcessTask completionStatement in CompletionStatementTasks.ToArray())
				{
					completionStatement.P9_Type = taskType;
				}
			}
		}

		void ClearCriteriasIfRequired()
		{
			if (!DischargePortExists && !P0_DischargePortCountry.IsEmpty)
			{
				P0_DischargePortCountry = ZString.Empty;
				P0_DischargePortCountryInfo.RefreshBinding();
			}

			if (!LoadPortExists && !P0_LoadPortCountry.IsEmpty)
			{
				P0_LoadPortCountry = ZString.Empty;
				P0_LoadPortCountryInfo.RefreshBinding();
			}

			if (!ClientExists && !P0_OH_Client.IsEmpty)
			{
				P0_OH_Client = ZGuid.Empty;
				P0_OH_ClientInfo.RefreshBinding();
			}

			if (!WarehouseExists && !P0_WW.IsEmpty)
			{
				P0_WW = ZGuid.Empty;
				P0_WWInfo.RefreshBinding();
			}

			if (!BranchExists && !P0_GB.IsEmpty)
			{
				P0_GB = ZGuid.Empty;
				P0_GBInfo.RefreshBinding();
			}

			if (!DepartmentExists && !P0_GE.IsEmpty)
			{
				P0_GE = ZGuid.Empty;
				P0_GEInfo.RefreshBinding();
			}

			if (!SubType1Exists && !P0_SubType1.IsEmpty)
			{
				P0_SubType1 = ZString.Empty;
				P0_SubType1Info.RefreshBinding();
			}

			if (!SubType2Exists && !P0_SubType2.IsEmpty)
			{
				P0_SubType2 = ZString.Empty;
				P0_SubType2Info.RefreshBinding();
			}

			if (!SubType3Exists && !P0_SubType3.IsEmpty)
			{
				P0_SubType3 = ZString.Empty;
				P0_SubType3Info.RefreshBinding();
			}

			if (!SubType4Exists && !P0_SubType4.IsEmpty)
			{
				P0_SubType4 = ZString.Empty;
				P0_SubType4Info.RefreshBinding();
			}

			if (!SubType5Exists && !P0_SubType5.IsEmpty)
			{
				P0_SubType5 = ZString.Empty;
				P0_SubType5Info.RefreshBinding();
			}
		}

		void UpdateWorkflowsCollection()
		{
			ObjectFactory.Get<IProcessHeaderCollectionProvider>().EnsureJobHeaderPresentWhenRequired(this);
		}

		#endregion

		#region P0_IsSystem

		public override ZBool P0_IsSystem
		{
			get { return base.P0_IsSystem; }
			set
			{
				if (base.P0_IsSystem != value)
				{
					base.P0_IsSystem = value;
					WorkflowItems.SetReadOnlyIncludingChildren(value);
					ReleaseGroupRules.SetReadOnlyIncludingChildren(value);
					GenCustomColumnDefinitions.SetReadOnlyIncludingChildren(value);
					ProcessHeaders.SetCountedReadOnlyIncludingChildren(true);
					SetFormCustomisationSettingsReadOnly(value);
				}
			}
		}

		#endregion

		#region P0_CustomFieldFallback

		[List("Lookups.CustomFieldFallbackTypes")]
		public override ZString P0_CustomFieldFallback
		{
			get => base.P0_CustomFieldFallback;
			set => base.P0_CustomFieldFallback = value;
		}

		#endregion

		#region P0_TaskFallbackMethod

		[List("Lookups.FallbackTypes")]
		public override ZString P0_TaskFallbackMethod
		{
			get { return base.P0_TaskFallbackMethod; }
			set { base.P0_TaskFallbackMethod = value; }
		}

		#endregion

		#region P0_MilestoneFallbackMethod

		[List("Lookups.FallbackTypes")]
		public override ZString P0_MilestoneFallbackMethod
		{
			get { return base.P0_MilestoneFallbackMethod; }
			set { base.P0_MilestoneFallbackMethod = value; }
		}

		#endregion

		#region P0_TriggerFallbackMethod

		[List("Lookups.FallbackTypes")]
		public override ZString P0_TriggerFallbackMethod
		{
			get { return base.P0_TriggerFallbackMethod; }
			set { base.P0_TriggerFallbackMethod = value; }
		}

		#endregion

		#region P0_ValidationFallbackMethod

		[List("Lookups.FallbackTypes")]
		public override ZString P0_ValidationFallbackMethod
		{
			get { return base.P0_ValidationFallbackMethod; }
			set { base.P0_ValidationFallbackMethod = value; }
		}

		#endregion

		#region P0_ReleaseGroupFallbackMethod

		[List("Lookups.ReleaseGroupFallbackMethods")]
		public override ZString P0_ReleaseGroupFallbackMethod
		{
			get => base.P0_ReleaseGroupFallbackMethod;
		}

		#endregion

		#region WorkflowTypeDescription

		public ZString WorkflowTypeDescription
		{
			get { return Lookups.WorkflowTypeList.GetDescriptionFromCode(P0_ProcessType); }
		}

		public ZPropertyInfo WorkflowTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(WorkflowTypeDescription)); }
		}

		#endregion

		#region SubTypeExists

		public ZBool SubType1Exists
		{
			get { return SubTypeExists(1); }
		}

		public ZPropertyInfo SubType1ExistsInfo
		{
			get { return GetZPropertyInfo(nameof(SubType1Exists)); }
		}

		public ZBool SubType2Exists
		{
			get { return SubTypeExists(2); }
		}

		public ZPropertyInfo SubType2ExistsInfo
		{
			get { return GetZPropertyInfo(nameof(SubType2Exists)); }
		}

		public ZBool SubType3Exists
		{
			get { return SubTypeExists(3); }
		}

		public ZPropertyInfo SubType3ExistsInfo
		{
			get { return GetZPropertyInfo(nameof(SubType3Exists)); }
		}

		public ZBool SubType4Exists
		{
			get { return SubTypeExists(4); }
		}

		public ZPropertyInfo SubType4ExistsInfo
		{
			get { return GetZPropertyInfo(nameof(SubType4Exists)); }
		}

		public ZBool SubType5Exists
		{
			get { return SubTypeExists(5); }
		}

		public ZPropertyInfo SubType5ExistsInfo
		{
			get { return GetZPropertyInfo(nameof(SubType5Exists)); }
		}

		bool SubTypeExists(int listNumber)
		{
			return WorkflowDescriptor?.SubTypeInformation?.Length > listNumber - 1;
		}

		#endregion

		#region SubTypeLabels

		public ZString SubType1Label
		{
			get { return GetLabel(1); }
		}

		public ZPropertyInfo SubType1LabelInfo
		{
			get { return GetZPropertyInfo(nameof(SubType1Label)); }
		}

		public ZString SubType2Label
		{
			get { return GetLabel(2); }
		}

		public ZPropertyInfo SubType2LabelInfo
		{
			get { return GetZPropertyInfo(nameof(SubType2Label)); }
		}

		public ZString SubType3Label
		{
			get { return GetLabel(3); }
		}

		public ZPropertyInfo SubType3LabelInfo
		{
			get { return GetZPropertyInfo(nameof(SubType3Label)); }
		}

		public ZString SubType4Label
		{
			get { return GetLabel(4); }
		}

		public ZPropertyInfo SubType4LabelInfo
		{
			get { return GetZPropertyInfo(nameof(SubType4Label)); }
		}

		public ZString SubType5Label
		{
			get { return GetLabel(5); }
		}

		public ZPropertyInfo SubType5LabelInfo
		{
			get { return GetZPropertyInfo(nameof(SubType5Label)); }
		}

		ZString GetLabel(int listNumber)
		{
			ZString result = ZString.Empty;
			if (WorkflowDescriptor != null && WorkflowDescriptor.SubTypeInformation.Length > listNumber - 1)
			{
				result = WorkflowDescriptor.SubTypeInformation[listNumber - 1].Description + " (" + listNumber + ")";
			}
			return result;
		}

		#endregion

		#region SubTypeDescriptions

		public ZString SubType1Description
		{
			get
			{
				return GetDescriptionFromCode(Lookups.List1, P0_SubType1);
			}
		}

		public ZPropertyInfo SubType1DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubType1Description)); }
		}

		public ZString SubType2Description
		{
			get
			{
				return GetDescriptionFromCode(Lookups.List2, P0_SubType2);
			}
		}

		public ZPropertyInfo SubType2DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubType2Description)); }
		}

		public ZString SubType3Description
		{
			get
			{
				return GetDescriptionFromCode(Lookups.List3, P0_SubType3);
			}
		}

		public ZPropertyInfo SubType3DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubType3Description)); }
		}

		public ZString SubType4Description
		{
			get
			{
				return GetDescriptionFromCode(Lookups.List4, P0_SubType4);
			}
		}

		public ZPropertyInfo SubType4DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubType4Description)); }
		}

		public ZString SubType5Description
		{
			get
			{
				return GetDescriptionFromCode(Lookups.List5, P0_SubType5);
			}
		}

		public ZPropertyInfo SubType5DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SubType5Description)); }
		}

		string GetDescriptionFromCode(IList list, ZString code)
		{
			if (list is ICodeDescriptionPairList)
			{
				var listAsCodeDescList = list as ReadOnlyCodeDescriptionPairList;
				if (listAsCodeDescList != null)
				{
					return listAsCodeDescList.GetDescriptionFromCode(code);
				}
			}
			else
			{
				var collection = list as IActiveBusinessObjectCollection;
				if (collection != null)
				{
					return collection.DescriptionFromCode(code);
				}
			}
			return "";
		}

		#endregion

		#region SubTypeIsMandatory

		public bool SubType1IsMandatory
		{
			get { return SubTypeIsMandatory(1); }
		}

		public bool SubType2IsMandatory
		{
			get { return SubTypeIsMandatory(2); }
		}

		public bool SubType3IsMandatory
		{
			get { return SubTypeIsMandatory(3); }
		}

		public bool SubType4IsMandatory
		{
			get { return SubTypeIsMandatory(4); }
		}

		public bool SubType5IsMandatory
		{
			get { return SubTypeIsMandatory(5); }
		}

		bool SubTypeIsMandatory(int listNumber)
		{
			bool result = false;

			if (!P0_IsPartialTemplate && WorkflowDescriptor != null && WorkflowDescriptor.SubTypeInformation.Length > listNumber - 1)
			{
				result = WorkflowDescriptor.SubTypeInformation[listNumber - 1].IsListRequired;
			}

			return result;
		}

		#endregion

		#region Client Exists

		public ZBool ClientExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresClient; }
		}

		public ZPropertyInfo ClientExistsInfo
		{
			get { return GetZPropertyInfo(nameof(ClientExists)); }
		}

		public ZString ClientName
		{
			get
			{
				return WorkflowDescriptor != null
					? (P0_ProcessType == WorkflowDescriptors.APInvoiceCode || P0_ProcessType == WorkflowDescriptors.AccDraftInvoiceCode)
					? WorkflowDescriptor.CreditorName
					: WorkflowDescriptor.ClientName
					: ZString.Empty;
			}
		}

		public ZPropertyInfo ClientNameInfo
		{
			get { return GetZPropertyInfo(nameof(ClientName)); }
		}

		#endregion

		#region WarehouseExists

		public ZBool WarehouseExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresWarehouse; }
		}

		public ZPropertyInfo WarehouseExistsInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseExists)); }
		}

		public ZString WarehouseName
		{
			get
			{
				return WorkflowDescriptor != null ? WorkflowDescriptor.WarehouseName : ZString.Empty;
			}
		}

		public ZPropertyInfo WarehouseNameInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseName)); }
		}

		#endregion

		#region Port 1

		public ZBool LoadPortExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresPort1; }
		}

		public ZPropertyInfo LoadPortExistsInfo
		{
			get { return GetZPropertyInfo(nameof(LoadPortExists)); }
		}

		public ZString Port1Name
		{
			get { return WorkflowDescriptor != null ? WorkflowDescriptor.Port1Name : ZString.Empty; }
		}

		public ZPropertyInfo Port1NameInfo
		{
			get { return GetZPropertyInfo(nameof(Port1Name)); }
		}

		#endregion

		#region Port 2

		public ZBool DischargePortExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresPort2; }
		}

		public ZPropertyInfo DischargePortExistsInfo
		{
			get { return GetZPropertyInfo(nameof(DischargePortExists)); }
		}

		public ZString Port2Name
		{
			get { return WorkflowDescriptor != null ? WorkflowDescriptor.Port2Name : ZString.Empty; }
		}

		public ZPropertyInfo Port2NameInfo
		{
			get { return GetZPropertyInfo(nameof(Port2Name)); }
		}

		#endregion

		#region Branch Exists

		public ZBool BranchExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresBranch; }
		}

		public ZPropertyInfo BranchExistsInfo
		{
			get { return GetZPropertyInfo(nameof(BranchExists)); }
		}

		#endregion

		#region Department Exists

		public ZBool DepartmentExists
		{
			get { return WorkflowDescriptor != null && WorkflowDescriptor.RequiresDepartment; }
		}

		public ZPropertyInfo DepartmentExistsInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentExists)); }
		}

		#endregion

		#region P0_IsActive

		[ReadOnlyMember(nameof(shouldIsActiveBeReadonly))]
		public override ZBool P0_IsActive
		{
			get { return base.P0_IsActive; }
			set
			{
				base.P0_IsActive = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateP0_IsUniversal();
					Validation.ValidateP0_IsActive();
					ValidateSelectionCriteria();
				}
			}
		}

		bool shouldIsActiveBeReadonly => Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed && !Env.Security.WorkflowTaskTemplatesEdit.IsAllowed;
		#endregion

		#region P0_IsPartialTemplate

		public override ZBool P0_IsPartialTemplate
		{
			get { return base.P0_IsPartialTemplate; }
			set
			{
				base.P0_IsPartialTemplate = value;

				if (!IsValidationSuspended)
				{
					ValidateSelectionCriteria();
				}
			}
		}

		void ValidateSelectionCriteria()
		{
			Validation.ValidateP0_GB();
			Validation.ValidateP0_GE();
			Validation.ValidateP0_WW();
			Validation.ValidateP0_LoadPortCountry();
			Validation.ValidateP0_DischargePortCountry();
			Validation.ValidateP0_OA_Address();
			Validation.ValidateP0_OH_Client();
			Validation.ValidateP0_SubType1();
			Validation.ValidateP0_SubType2();
			Validation.ValidateP0_SubType3();
			Validation.ValidateP0_SubType4();
			Validation.ValidateP0_SubType5();
			if (WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.Value)
			{
				Validation.ValidateP0_EffectiveStartDateUtc();
				Validation.ValidateP0_EffectiveEndDateUtc();
			}
		}

		#endregion

		#region P0_IsScreenLayoutFallback

		public override ZBool P0_IsScreenLayoutFallback
		{
			get => base.P0_IsScreenLayoutFallback;
			set
			{
				base.P0_IsScreenLayoutFallback = value;
				formCustomisationSettings?.DisplayFields.RefreshBindingIncludingChildren();
				formCustomisationSettings?.DisplayTabs.RefreshBindingIncludingChildren();
			}
		}

		#endregion

		#region P0_IsUniversal

		public override ZBool P0_IsUniversal
		{
			get { return base.P0_IsUniversal; }
			set
			{
				base.P0_IsUniversal = value;

				if (!IsValidationSuspended)
				{
					ValidateSelectionCriteria();
					Validation.ValidateP0_TriggerFallbackMethod();
				}
			}
		}

		#endregion

		#region MilestoneTemplateHintCaption

		public ZString MilestoneTemplateHintCaption
		{
			get
			{
				if (P0_IsUniversal)
				{
					return Res.GetString("5acda161-551b-4942-8d3b-bac6ded72a57", "Changes made to triggers on a Universal Template take effect immediately on all jobs of this Process Type that match any Template Conditions. Inactive triggers do not appear on jobs.");
				}
				else
				{
					return WorkflowDescriptor?.MilestoneTemplateHintCaption ?? ZString.Empty;
				}
			}
		}

		public ZPropertyInfo MilestoneTemplateHintCaptionInfo
		{
			get { return GetZPropertyInfo(nameof(MilestoneTemplateHintCaption)); }
		}

		#endregion

		#endregion

		#region ReadOnly

		protected bool GetReadOnly(PropertyDescriptor property)
		{
			bool result = (P0_IsSystem && property.Name != ProcessTaskTemplateSchema.Constants.P0_IsActive);
			result = result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return result;
		}

		#endregion

		#region Template Logic

		public bool CanFallBackForTasks
		{
			get
			{
				switch (P0_TaskFallbackMethod)
				{
					case FallbackTypeList.Codes.NeverFallback:
						return false;
					case FallbackTypeList.Codes.AlwaysFallback:
						return true;
					case FallbackTypeList.Codes.EmptyFallback:
						return WorkflowItems.Tasks.GetItemsToCreateFromTemplate(this).Count == 0;
					default:
						return false;
				}
			}
		}

		public EntityTypeSupportedToken SupportsTemplateEntityType(TemplateEntityType entityType)
		{
			if (P0_IsUniversal && entityType != TemplateEntityType.Triggers)
			{
				return EntityTypeSupportedToken.NotSupported(ResString.GetMultilingualString("6a7d05da-2e1a-4ea3-9792-d6691ae7b300", "Not available for Universal Templates"));
			}

			if (P0_IsPartialTemplate && (entityType == TemplateEntityType.CustomFields || entityType == TemplateEntityType.ScreenLayout))
			{
				return EntityTypeSupportedToken.NotSupported(ResString.GetMultilingualString("7660350C-535D-4402-BB4C-9C3EDAE1D506", "Not available for Partial Templates"));
			}

			var descriptor = WorkflowDescriptor;
			var supported = false;

			if (descriptor != null)
			{
				switch (entityType)
				{
					case TemplateEntityType.ReleaseGroupRules:
						supported = descriptor.SupportsReleaseGroupRules;
						if (supported)
						{
							return SupportsTemplateEntityType(TemplateEntityType.Workflows);
						}
						break;

					case TemplateEntityType.Workflows:
						if (!IsProcessTypeDefinedForBufferManagementSystem)
						{
							return EntityTypeSupportedToken.NotSupported(ResString.GetMultilingualString("692f8e80-009a-4807-915b-63e87bf11d10", "There is no Buffer Management System defined for this Process Type"));
						}
						supported = descriptor.SupportsTasks;
						break;

					case TemplateEntityType.Tasks:
					case TemplateEntityType.CompletionStatements:
						supported = descriptor.SupportsTasks;
						break;

					case TemplateEntityType.Triggers:
					case TemplateEntityType.Milestones:
						supported = descriptor.SupportsEventTracking;

						if (!P0_IsUniversal && descriptor.OnlySupportEventTrackingForUniversalTemplates)
						{
							supported = false;
						}

						break;

					case TemplateEntityType.ScreenLayout:
						supported = descriptor.SupportsScreenLayout;
						break;

					case TemplateEntityType.CustomFields:
						supported = descriptor.SupportsCustomFields;
						break;

					case TemplateEntityType.ValidationTool:
						if (GlobalTemplate && !descriptor.ValidationToolSettings.IsValidationRulesAvailableForGlobalTemplates)
						{
							return EntityTypeSupportedToken.NotSupported(ResString.GetMultilingualString("4b3be1bd-d1fd-4991-9536-7479527add26", "Not available for Global Templates"));
						}
						supported = descriptor.ValidationToolSettings.SupportsValidationRules;
						break;
				}
			}

			return supported ? EntityTypeSupportedToken.Supported : EntityTypeSupportedToken.NotSupported(ResString.GetMultilingualString("fedd6597-8df6-4b17-ba37-7487cd92df75", "Not available for this Workflow Type"));
		}

		bool IsProcessTypeDefinedForBufferManagementSystem
		{
			get { return ProcessJobHeaderProvider.SupportsPAVE(P0_ProcessType, Factory); }
		}

		#endregion

		#region IWorkflowProvider

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

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fTasks == null)
				{
					fTasks = this.GetOrCreateProcessTaskCollection(() =>
					{
						var collection = new TemplateProcessTaskCollection(this);
						if (P0_IsSystem)
						{
							collection.SetReadOnlyIncludingChildren(true);
						}
						return collection;
					});
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

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			throw new NotSupportedException();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return ""; }
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return (ProcessTaskTemplate)Clone();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newTemplate = (ProcessTaskTemplate)base.CloneInternal(args);
			var newName = ResString.GetMultilingualString("38B93B1F-B0A5-492E-A5EC-B477F84F1FE2", "{0} - Copy", P0_Name);
			if (((string)newName).Length <= ProcessTaskTemplateSchema.P0_Name.MaxLength)
			{
				newTemplate.P0_Name = newName;
			}

			if (Env.Security.WorkflowTaskTemplateCopyInactive.IsAllowed && !EnvProxy.Instance.Security.FindOrCreateCopyCheckpoint(Env.Security.WorkflowTaskTemplates).IsAllowed)
			{
				newTemplate.P0_IsActive = false;
			}

			if (this.P0_IsUniversal)
			{
				return CloneUniversalTemplate(newTemplate);
			}
			else
			{
				return CloneTemplate(newTemplate);
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var list = new List<string>(base.GetPropertiesToExcludeFromCloning());
			list.Add(ProcessTaskTemplateSchema.Constants.P0_IsSystem);
			return list;
		}

		BusinessObject CloneUniversalTemplate(ProcessTaskTemplate newTemplate)
		{
			using (newTemplate.GetValidationSuspender())
			using (TemplateTriggers.SuspendListChanged())
			{
				foreach (var universalTrigger in TemplateTriggers.Cast<IUniversalTemplateTrigger>())
				{
					newTemplate.TemplateTriggers.Add(universalTrigger.Clone());
				}
			}

			newTemplate.MarkAsNeedingValidationIncludingChildren();
			return newTemplate;
		}

		BusinessObject CloneTemplate(ProcessTaskTemplate newTemplate)
		{
			var jobHeader = newTemplate.ProcessHeaders.OfType<IProcessJobHeader>().SingleOrDefault();
			var sourceTargetWorkflows = ProcessHeaders.CloneWorkflowsAndLinksForTemplates(jobHeader, newTemplate.ProcessHeaders);
			var sourceTargetTasks = new Dictionary<ProcessTask, ProcessTask>();

			using (CloningTemplate(newTemplate))
			{
				foreach (ProcessTask task in WorkflowItems)
				{
					var clonedTask = TemplateProcessTaskCopier.Clone(task, task.GetType());
					sourceTargetTasks.Add(task, clonedTask);

					clonedTask.P9_ParentTableCode = ProcessTaskTemplateSchema.Constants.Prefix;
					clonedTask.P9_ParentID = newTemplate.PK;

					var workflow = task.ProcessHeader;
					var clonedWorkflow = workflow != null && sourceTargetWorkflows.ContainsKey(workflow) ? sourceTargetWorkflows[task.ProcessHeader] : null;
					if (clonedWorkflow != null)
					{
						clonedTask.P9_FH_ProcessHeader = clonedWorkflow.PK;
					}

					clonedTask.RefreshWorkflowType();
					newTemplate.WorkflowItems.SetDefaultsForNewTask(clonedTask, false);
					newTemplate.WorkflowItems.Add(clonedTask);

					foreach (var notification in task.ProcessTaskNotifications)
					{
						var clonedNotification = (ProcessTaskNotification)notification.Clone();
						clonedNotification.PQ_P9 = clonedTask.PK;
						clonedNotification.PQ_EmailText = notification.PQ_EmailText;
						clonedTask.ProcessTaskNotifications.Add(clonedNotification);
					}
				}
			}

			foreach (GenCustomColumnDefinition custom in GenCustomColumnDefinitions)
			{
				var clonedCustom = custom.Clone(custom.GetType());
				clonedCustom.XC_ParentID = newTemplate.PK;
				clonedCustom.XC_ParentTableCode = newTemplate.TablePrefix;
			}

			newTemplate.FormCustomisationSettings.UpdateFieldsFrom(FormCustomisationSettings);

			ReleaseGroupRules.CloneRulesAndCategoriesAndMappings(newTemplate.ReleaseGroupRules);

			return newTemplate;
		}

		#endregion

		#region ICustomProcessTaskHandlerProvider

		IProcessTaskHandler ICustomProcessTaskHandlerProvider.GetHandler(IStmALog log)
		{
			return new NullProcessTaskHandler(); // Do not fire workflow against Templates.
		}

		#endregion

		#region IProcessTaskTemplate Members

		IFormCustomisationSettings IProcessTaskTemplate.FormCustomisationSettings
		{
			get { return FormCustomisationSettings; }
		}

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(ProcessTasksSchema.P9_ParentID, ProcessTasksSchema.P9_TaskID);
			}
		}

		#endregion

		#region Validation Rules
		[ChildEditable(true)]
		public ProcessTemplateValidationActionCollection ProcessTemplateValidationActions
		{
			get
			{
				if (processTemplateValidationActions is null)
				{
					processTemplateValidationActions = new ProcessTemplateValidationActionCollection(this);
					RegisterEditableChildObject(processTemplateValidationActions);
				}

				return processTemplateValidationActions;
			}
		}
		ProcessTemplateValidationActionCollection processTemplateValidationActions;

		[ChildEditable(true)]
		public ProcessTemplateValidationCollection ProcessTemplateValidations
		{
			get
			{
				if (processTemplateValidations is null)
				{
					processTemplateValidations = new ProcessTemplateValidationCollection(this);
					RegisterEditableChildObject(processTemplateValidations);
				}

				return processTemplateValidations;
			}
		}
		ProcessTemplateValidationCollection processTemplateValidations;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	using System.Text;
	using System.Threading;
	using CargoWise.Common;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Schema;
	using Enterprise.Integration;
	using Enterprise.Security;
	using Enterprise.Warehouse.Integration;

	#region Test Objects

	public class DummyWorkflowDescriptor : WorkflowDescriptor, IEventPublisher, IEventSubscriptionAgent, IWorkflowParentWithLines
	{
		#region Construction

		protected DummyWorkflowDescriptor()
		{
			if (DummyBusinessObject.TypeDecider.TypeForLoadOverride == null)
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);
			}
		}

		public static DummyWorkflowDescriptor Instance
		{
			get
			{
				if (!instance.IsOverriden)
				{
					instance.Value = new DummyWorkflowDescriptor();
				}
				return instance.Value;
			}
		}

		public static bool IsActive => instance.IsOverriden;

		static readonly Overridable<DummyWorkflowDescriptor> instance = new Overridable<DummyWorkflowDescriptor>(null);

		protected override Dictionary<ZString, SecurityCheckpoint> GetWorkflowTriggerActionTypeSecurityCheckPoints()
		{
			var result = base.GetWorkflowTriggerActionTypeSecurityCheckPoints();
			result.Add(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, Env.Security.ImporterSecurityFilingMessaging);
			return result;
		}

		#endregion

		#region Defaults From

		protected internal override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString defaultsFrom)
		{
			var trueResult = customDefaultsFromDate == null
				? base.GetScheduleDateTimeAndLocationForTimezone(defaultedFromDateProvider, defaultsFrom)
				: customDefaultsFromDate(defaultedFromDateProvider, defaultsFrom);

			return trueResult;
		}

		public (ZDateTime, RefUNLOCO) GetDefaultsFromDateBase(IDefaultedFromDateProvider defaultedFromDateProvider, ZString defaultsFrom)
		{
			return base.GetScheduleDateTimeAndLocationForTimezone(defaultedFromDateProvider, defaultsFrom);
		}

		public void SetCustomDefaultsFromDate(Func<IDefaultedFromDateProvider, ZString, (ZDateTime, RefUNLOCO)> customDefaultsFromDate)
		{
			this.customDefaultsFromDate = customDefaultsFromDate;
		}

		Func<IDefaultedFromDateProvider, ZString, (ZDateTime, RefUNLOCO)> customDefaultsFromDate;

		#endregion

		#region Default Trigger Conditions

		protected override void SetDefaultTriggerConditionsCore(IMilestoneDateDefaultable defaultable, BusinessObject parent)
		{
			if (defaultable.TriggerEventCode == Events.AuthorisationWithdrawnCode)
			{
				defaultable.TriggerCondition = "ABC";
				defaultable.TriggerConditionValue = "BOT";
			}
		}

		#endregion

		#region Company-Specific Tasks

		public void SetAreTasksCompanySpecific(bool value)
		{
			areTasksCompanySpecific = value;
		}

		public override bool AreTasksCompanySpecific
		{
			get { return areTasksCompanySpecific; }
		}
		bool areTasksCompanySpecific;

		#endregion

		#region Client List

		public override Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProvider
		{
			get { return ClientListProviderExposed ?? base.ClientListProvider; }
		}

		public Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProviderExposed { get; set; }

		#endregion

		#region RequiresClient

		public override bool RequiresClient
		{
			get { return ClientNeeded; }
		}

		public ZBool ClientNeeded
		{
			get { return clientNeeded; }
			set { clientNeeded = value; }
		}
		bool clientNeeded;

		public ZPropertyInfo ClientNeededInfo
		{
			get { return GetZPropertyInfo(nameof(ClientNeeded)); }
		}

		#endregion

		#region RequiresWarehouse

		public override bool RequiresWarehouse
		{
			get { return WarehouseNeeded; }
		}

		public ZBool WarehouseNeeded
		{
			get { return warehouseNeeded; }
			set { warehouseNeeded = value; }
		}
		bool warehouseNeeded;

		public ZPropertyInfo WarehouseNeededInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseNeeded)); }
		}

		#endregion

		#region WarehouseType

		public override WarehouseCollectionType WarehouseType
		{
			get { return WarehouseTypeNeeded; }
		}

		public WarehouseCollectionType WarehouseTypeNeeded { get; set; }

		public ZPropertyInfo WarehouseTypeInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseTypeNeeded)); }
		}

		#endregion

		#region RequiresPort1

		public override bool RequiresPort1
		{
			get { return Port1Needed; }
		}

		public ZBool Port1Needed
		{
			get { return port1Needed; }
			set { port1Needed = value; }
		}
		bool port1Needed;

		public ZPropertyInfo Port1NeededInfo
		{
			get { return GetZPropertyInfo(nameof(Port1Needed)); }
		}

		#endregion

		#region RequiresPort2

		public override bool RequiresPort2
		{
			get { return Port2Needed; }
		}

		public ZBool Port2Needed
		{
			get { return port2Needed; }
			set { port2Needed = value; }
		}
		bool port2Needed;

		public ZPropertyInfo Port2NeededInfo
		{
			get { return GetZPropertyInfo(nameof(Port2Needed)); }
		}

		#endregion

		#region RequiresBranch

		public override bool RequiresBranch
		{
			get { return BranchNeeded; }
		}

		public ZBool BranchNeeded
		{
			get { return branchNeeded; }
			set { branchNeeded = value; }
		}
		bool branchNeeded;

		public ZPropertyInfo BranchNeededInfo
		{
			get { return GetZPropertyInfo(nameof(BranchNeeded)); }
		}

		#endregion

		#region RequiresDepartment

		public override bool RequiresDepartment
		{
			get { return DepartmentNeeded; }
		}

		public ZBool DepartmentNeeded
		{
			get { return departmentNeeded; }
			set { departmentNeeded = value; }
		}
		bool departmentNeeded;

		public ZPropertyInfo DepartmentNeededInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentNeeded)); }
		}

		#endregion

		#region SupportsEventTracking

		public override bool SupportsEventTracking
		{
			get { return EventTrackingSupported; }
		}
		public bool EventTrackingSupported = true;

		public override bool OnlySupportEventTrackingForUniversalTemplates
		{
			get { return EventTrackingSupportedForUniversalTemplates; }
		}
		public bool EventTrackingSupportedForUniversalTemplates;

		protected internal override bool SupportsWorkflowTriggerActionUniversalEventXML => SupportsWorkflowTriggerActionUniversalEventXML_Override.IsOverriden ? SupportsWorkflowTriggerActionUniversalEventXML_Override.Value : base.SupportsWorkflowTriggerActionUniversalEventXML;

		public readonly Overridable<bool> SupportsWorkflowTriggerActionUniversalEventXML_Override = new Overridable<bool>();

		#endregion

		#region SupportsTasks

		public override bool SupportsTasks
		{
			get { return TasksSupported; }
		}
		public bool TasksSupported = true;

		public override bool ShouldHideTasksTabOnJobs => ShouldHideTasksTabOnJobs_Exposed ?? base.ShouldHideTasksTabOnJobs;

		public bool? ShouldHideTasksTabOnJobs_Exposed { get; set; }

		#endregion

		#region SupportsScreenLayout

		public override bool SupportsScreenLayout
		{
			get { return ScreenLayoutSupported; }
		}
		public bool ScreenLayoutSupported = true;

		#endregion

		#region SupportsCustomFields

		public override bool SupportsCustomFields => CustomFieldsSupported;

		public bool CustomFieldsSupported = true;

		#endregion

		#region Validation Rules

		public void SetValidationToolSettings(ValidationToolSettings settings)
		{
			var validationToolSettings = typeof(WorkflowDescriptor).GetField("validationToolSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			validationToolSettings.SetValue(this, settings);
		}

		#endregion

		#region Customs Messaging

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			return WorkflowTriggerActionTypeConstants.ValidateForCustomsMessagingCodes;
		}

		#endregion

		#region DocumentBusinessContext

		public void SetDocumentBusinessContext(BusinessContext[] documentBusinessContext)
		{
			this.documentBusinessContext = documentBusinessContext;
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return documentBusinessContext ?? base.DocumentBusinessContext; }
		}
		BusinessContext[] documentBusinessContext;

		#endregion

		#region Workflow Trigger Actions

		protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			base.AddToMessageRecipientPartyList(messageTriggerParties, bizObj, partyType);

			if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(GlbCompany.CurrentCompany.OrgProxy, ZString.Empty));
			}
		}

		public void AddToWorkflowTriggerFieldColumnList(SchemaColumn column)
		{
			var propCollection1 = PropertyChangeSubscriptionList.workflowTriggerPropertyNamesThatMayBeLogged.Value.ToList();
			var propCollection2 = PropertyChangeSubscriptionList.propertyNamesThatMayBeLogged.Value.ToList();

			workflowTriggerFieldColumnList.Add(column);
			propCollection1.Add(column.Name);
			propCollection2.Add(column.Name);

			PropertyChangeSubscriptionList.workflowTriggerPropertyNamesThatMayBeLogged.Value = propCollection1;
			PropertyChangeSubscriptionList.propertyNamesThatMayBeLogged.Value = propCollection2;
		}

		readonly List<SchemaColumn> workflowTriggerFieldColumnList = new List<SchemaColumn>();

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return workflowTriggerFieldColumnList.ToArray();
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			string description = base.GetFieldColumnDescription(factory, fieldColumn);
			return description.Length > 0 ? description : fieldColumn.Name.Replace("Z0_", "");
		}

		public CodeDescriptionPairList WorkflowTriggerActionTypeList = new CodeDescriptionPairList();

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			return WorkflowTriggerActionTypeList;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			return SupportedMessageRecipientPartiesForSpecificActionExposed;
		}

		public MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificActionExposed;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return SupportedMessageRecipientPartiesExposed;
		}

		public MessageRecipientPartyType SupportedMessageRecipientPartiesExposed;

		protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient)
		{
			return SupportedTriggerPartyServicesExposed ?? base.SupportedTriggerPartyServicesCore(recipient);
		}

		public ZString[] SupportedTriggerPartyServicesExposed;

		public int WorkflowTriggerActionRunCount;
		public IProcessor WorkflowTriggerActionProcessorOverride;
		public Func<BusinessObject, ProcessTaskNotification, IQueuedLog, IProcessor> WorkflowTriggerActionGetter;

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog log)
		{
			if (WorkflowTriggerActionGetter != null)
			{
				var result = WorkflowTriggerActionGetter(source.Job, source.Action, log);
				if (result != null)
				{
					return result;
				}
			}

			return base.GetWorkflowTriggerActionCore(source, log);
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			return WorkflowTriggerActionProcessorOverride ?? new DummyWorkflowTriggerProcessor(this, source.Action);
		}

		protected override WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			WorkflowTriggerNotification processor = base.GetWorkflowTriggerForNotificationEmail(modes, action, parent, logProvider);
			processor.ExtraDataSubstitution = ExtraDataSubstitutionForNotificationEmail;
			return processor;
		}

		public TriggerActionCommunicationModeSubstitutor.ExtraDataSubstitutionDelegate ExtraDataSubstitutionForNotificationEmail { get; set; }

		class DummyWorkflowTriggerProcessor : IProcessor
		{
			public DummyWorkflowTriggerProcessor(DummyWorkflowDescriptor owner, ProcessTaskNotification action)
			{
				this.owner = owner;
				this.action = action;
			}

			void IProcessor.Process(INotifications notifications, CancellationToken token)
			{
				owner.WorkflowTriggerActionRunCount++;
				action.Parent.Description = "Trigger Fired";
			}

			readonly DummyWorkflowDescriptor owner;
			readonly ProcessTaskNotification action;
		}

		#endregion

		#region Identification

		public override string Code
		{
			get { return WorkflowDescriptors.DummyWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("57b3dc09-0169-43dc-96b4-c41ed67d6d23", "Dummy Task Provider"); }
		}

		Type workflowProviderType = typeof(DummyWithWorkflow);
		public override Type WorkflowProviderType => workflowProviderType;
		public Type OverriddenWorkflowProviderType { set => workflowProviderType = value; }

		public override ZString MilestoneTemplateHintCaption
		{
			get { return MilestoneTemplateHintCaptionOverride ?? base.MilestoneTemplateHintCaption; }
		}

		public string MilestoneTemplateHintCaptionOverride { get; set; }

		public override ControllerID ControllerID => OverriddenControllerID;

		public ControllerID OverriddenControllerID { get; set; }

		#endregion

		#region Sub Types

		public List<ProcessTemplateSubType> SubTypes
		{
			get
			{
				if (subTypes == null)
				{
					subTypes = new List<ProcessTemplateSubType>();
				}
				return subTypes;
			}
		}
		List<ProcessTemplateSubType> subTypes;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypes == null)
				{
					CodeDescriptionPairList list1 = new CodeDescriptionPairList();
					list1.AddPair("COL", "Colombia");

					CodeDescriptionPairList list2 = new CodeDescriptionPairList();
					list2.AddPair("AUS", "Australia");
					list2.AddPair("CHN", "China");

					CodeDescriptionPairList list3 = new CodeDescriptionPairList();
					list3.AddPair("USA", "United States of America");

					CodeDescriptionPairList list4 = new CodeDescriptionPairList();
					list4.AddPair("RUS", "Russia");
					list4.AddPair("GBR", "Great Britain");

					var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(new BusinessObjectFactory());
					var dummyBusinessObject = collection.AddNew();
					dummyBusinessObject.Z0_Code = "MON";
					dummyBusinessObject.Z0_Description = "The Moon";

					SubTypes.Add(new ProcessTemplateSubType("South America", list1, true));
					SubTypes.Add(new ProcessTemplateSubType("Asia", list2));
					SubTypes.Add(new ProcessTemplateSubType("North America", list3, true));
					SubTypes.Add(new ProcessTemplateSubType("Europe", list4));
					SubTypes.Add(new ProcessTemplateSubType("Celestial Bodies", collection));
				}
				return SubTypes.ToArray();
			}
		}

		#endregion

		#region Form Customisation

		public Func<FormCustomisationSettingsProvider> GetFormCustomisationSettingsProviderImplementation { get; set; }

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return GetFormCustomisationSettingsProviderImplementation != null ? GetFormCustomisationSettingsProviderImplementation() : new DummyFormCustomisationSettingsProvider();
		}

		#endregion

		#region Event Context

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new DummyEventDataModel(businessObject);
		}

		protected override bool IsRelatedEntityInContextCore(BusinessObject entity, BusinessObject relatedEntity, IEnumerable<WorkflowEventContextPair> contextPath)
		{
			return IsRelatedEntityInContextForTest ?? base.IsRelatedEntityInContextCore(entity, relatedEntity, contextPath);
		}

		public bool? IsRelatedEntityInContextForTest { get; set; }

		protected override IEnumerable<WorkflowEventContextPair> GetFollowingContextStepsCore(IEnumerable<WorkflowEventContextPair> currentContextPath)
		{
			return FollowingContextStepsForTest ?? base.GetFollowingContextStepsCore(currentContextPath);
		}

		public IEnumerable<WorkflowEventContextPair> FollowingContextStepsForTest { get; set; }

		#endregion

		#region IEventPublisher

		public string PublisherTableName
		{
			get
			{
				return DummyBizoSchema.Constants.TableName;
			}
		}

		public string GetSubscriptionsQuery()
		{
			return getSubscriptionsQueryExpectations ?? "select null, null";
		}

		public IEnumerable<string> GetPublisherTableNames()
		{
			yield return PublisherTableName;
		}

		public void ExpectGetSubscriptionsQuery(string queryToReturn)
		{
			getSubscriptionsQueryExpectations = queryToReturn;
		}

		string getSubscriptionsQueryExpectations;

		#endregion

		#region IEventSubscriptionAgent

		public IEnumerable<IStmALogParent> GetLogParentsToFireWorkflowForEvent(IStmEventSubscription subscription, IStmALog evnt)
		{
			var subscriptionInfo = new SubscriptionInfo(subscription, evnt);
			getLogParentsToFireWorkflowForEventCalls.Add(subscriptionInfo);
			return logParentsLookup[subscriptionInfo];
		}

		public void ExpectLogParentsToFireWorkflowForEvent(IStmEventSubscription subscription, IStmALog evnt, IEnumerable<IStmALogParent> expectedRelatedParents)
		{
			var subscriptionInfo = new SubscriptionInfo(subscription, evnt);
			getLogParentsToFireWorkflowForEventExpectations.Add(subscriptionInfo);
			logParentsLookup.Add(subscriptionInfo, expectedRelatedParents);
		}

		public void VerifyGetLogParentsToFireWorkflowForEventCalls()
		{
			var failedExpectations = getLogParentsToFireWorkflowForEventExpectations.Where(e => !getLogParentsToFireWorkflowForEventCalls.Any(c => c.Equals(e))).ToArray();
			var notExpectedCalls = getLogParentsToFireWorkflowForEventCalls.Where(c => !getLogParentsToFireWorkflowForEventExpectations.Any(e => c.Equals(e))).ToArray();

			var errorMessage = new StringBuilder();

			if (failedExpectations.Length > 0)
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine("The following method calls were expected but not called:");

				for (var i = 0; i < failedExpectations.Length; i++)
				{
					errorMessage.AppendLine(string.Format("GetLogParentsToFireWorkflowForEvent(subscription[PK='{0}'], event[Reference='{1}']", failedExpectations[i].Subscription.PK, failedExpectations[i].Log.SL_Reference));
				}
			}

			if (notExpectedCalls.Length > 0)
			{
				errorMessage.AppendLine();
				errorMessage.AppendLine("The following method were called but not expected:");

				for (var i = 0; i < notExpectedCalls.Length; i++)
				{
					errorMessage.AppendLine(string.Format("GetLogParentsToFireWorkflowForEvent(subscription[PK='{0}'], event[Reference='{1}']", notExpectedCalls[i].Subscription.PK, notExpectedCalls[i].Log.SL_Reference));
				}
			}

			Assertion.AssertEquals(errorMessage.ToString(), 0, errorMessage.Length);

			getLogParentsToFireWorkflowForEventExpectations.Clear();
			getLogParentsToFireWorkflowForEventCalls.Clear();
		}

		readonly List<SubscriptionInfo> getLogParentsToFireWorkflowForEventExpectations = new List<SubscriptionInfo>();
		readonly List<SubscriptionInfo> getLogParentsToFireWorkflowForEventCalls = new List<SubscriptionInfo>();
		readonly Dictionary<SubscriptionInfo, IEnumerable<IStmALogParent>> logParentsLookup = new Dictionary<SubscriptionInfo, IEnumerable<IStmALogParent>>();

		class SubscriptionInfo
		{
			public SubscriptionInfo(IStmEventSubscription subscription, IStmALog log)
			{
				Subscription = subscription;
				Log = log;
			}

			public IStmEventSubscription Subscription { get; }
			public IStmALog Log { get; }

			public override bool Equals(object obj)
			{
				var other = (SubscriptionInfo)obj;
				return Subscription.PK == other.Subscription.PK && Log.SL_Reference == other.Log.SL_Reference;
			}

			public override int GetHashCode() => Subscription.PK.GetHashCode() ^ Log.SL_Reference.GetHashCode();
		}

		#endregion

		#region IWorkflowParentWithLines

		public IEnumerable<string> SupportedTriggerLineTypes
		{
			get;
			set;
		}

		#endregion

		#region IRootTypeProvider

		protected override IEnumerable<Type> MacroTypesCore(ProcessTaskNotification action) => base.MacroTypesCore(action).Append(ExtraMacroBizo.Value?.GetType()).WhereNotNull();

		protected override IEnumerable<BusinessObject> MacroRootsCore(ProcessTaskNotification action) => base.MacroRootsCore(action).Append(ExtraMacroBizo.Value).WhereNotNull();

		public Overridable<BusinessObject> ExtraMacroBizo { get; } = new Overridable<BusinessObject>(null);

		#endregion

		public bool SupportsReapplyTemplatesMenuItemExposed { get; set; }
		protected override bool SupportsReapplyTemplatesMenuItemCore => SupportsReapplyTemplatesMenuItemExposed;
	}

	public class DummyEventDataModel : BusinessObjectEventDataModel<BusinessObject>
	{
		public DummyEventDataModel(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		public string Origin
		{
			get
			{
				return "UAIEV";
			}
		}
	}

	class DummyFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[] { "SubType1", "RelatedDummyWithTasksClient" };
		}
	}

	#endregion
}

#endif
#endregion
